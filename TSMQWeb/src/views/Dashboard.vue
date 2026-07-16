<template>
  <div class="dashboard">
    <!-- 顶部汇总卡片 -->
    <el-row :gutter="16">
      <el-col :xs="12" :sm="8" :md="4" v-for="card in summaryCards" :key="card.label">
        <el-card shadow="hover" :class="['stat-card', card.colorClass]">
          <div class="stat-inner">
            <el-icon class="stat-icon"><component :is="card.icon" /></el-icon>
            <div class="stat-info">
              <div class="stat-value">{{ card.format ? card.format(card.value) : formatNumber(card.value) }}</div>
              <div class="stat-label">{{ card.label }}</div>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- 图表粒度控制栏 -->
    <el-row style="margin-top: 16px">
      <el-col :span="24">
        <el-card>
          <div class="chart-toolbar">
            <span class="toolbar-title">统计图表</span>
            <el-radio-group v-model="granularity" size="small" @change="onGranularityChange">
              <el-radio-button value="realtime">实时</el-radio-button>
              <el-radio-button value="minute">按分钟</el-radio-button>
              <el-radio-button value="hour">按小时</el-radio-button>
              <el-radio-button value="day">按天</el-radio-button>
              <el-radio-button value="all">全部</el-radio-button>
            </el-radio-group>
            <el-tag size="small" :type="polling ? 'success' : 'info'" style="margin-left: 8px">
              {{ granularity === 'realtime' ? (polling ? '实时轮询中' : '已暂停') : '历史数据' }}
            </el-tag>
            <el-button v-if="granularity !== 'realtime'" size="small" type="primary" plain
              :loading="historyLoading" @click="handleRefresh" style="margin-left: 8px">
              刷新
            </el-button>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- 中部图表：消息趋势 + 丢弃分布 -->
    <el-row :gutter="16" style="margin-top: 16px">
      <el-col :span="16">
        <el-card>
          <template #header>
            <div class="card-header-row">
              <span v-if="granularity === 'realtime'">消息趋势（最近 {{ MAX_HISTORY }} 次采样，每 {{ POLL_INTERVAL / 1000 }}s 一次）</span>
              <span v-else-if="granularity === 'minute'">消息增量（最近1小时，每分钟）</span>
              <span v-else-if="granularity === 'hour'">消息增量（最近24小时，每小时）</span>
              <span v-else-if="granularity === 'day'">消息增量（最近30天，每天）</span>
              <span v-else>消息增量（全量历史，每天）</span>
              <el-tag size="small" :type="polling ? 'success' : 'info'">{{ granularity === 'realtime' ? (polling ? '轮询中' : '已暂停') : '历史' }}</el-tag>
            </div>
          </template>
          <div ref="msgChartEl" style="height: 280px"></div>
        </el-card>
      </el-col>
      <el-col :span="8">
        <el-card>
          <template #header>丢弃原因分布</template>
          <div ref="discardPieEl" style="height: 280px"></div>
        </el-card>
      </el-col>
    </el-row>

    <!-- 下部图表：数据吞吐 + 服务器信息 -->
    <el-row :gutter="16" style="margin-top: 16px">
      <el-col :span="16">
        <el-card>
          <template #header>
            <span v-if="granularity === 'realtime'">数据吞吐量（字节，最近 {{ MAX_HISTORY }} 次采样）</span>
            <span v-else-if="granularity === 'minute'">数据吞吐增量（字节，最近1小时，每分钟）</span>
            <span v-else-if="granularity === 'hour'">数据吞吐增量（字节，最近24小时，每小时）</span>
            <span v-else-if="granularity === 'day'">数据吞吐增量（字节，最近30天，每天）</span>
            <span v-else>数据吞吐增量（字节，全量历史，每天）</span>
          </template>
          <div ref="bytesChartEl" style="height: 240px"></div>
        </el-card>
      </el-col>
      <el-col :span="8">
        <el-card>
          <template #header>服务器信息</template>
          <el-descriptions :column="1" border size="small">
            <el-descriptions-item label="运行中服务器">
              <el-tag type="success">{{ stats.RunningServers }}</el-tag>
            </el-descriptions-item>
            <el-descriptions-item label="在线客户端">{{ stats.OnlineClients }}</el-descriptions-item>
            <el-descriptions-item label="活跃主题">{{ stats.ActiveTopics }}</el-descriptions-item>
            <el-descriptions-item label="启动时间">{{ formatDate(stats.ServerStartedAt) }}</el-descriptions-item>
            <el-descriptions-item label="运行时长">{{ uptime }}</el-descriptions-item>
            <el-descriptions-item label="总接收消息">{{ formatNumber(stats.TotalReceived) }}</el-descriptions-item>
            <el-descriptions-item label="总转发消息">{{ formatNumber(stats.TotalForwarded) }}</el-descriptions-item>
            <el-descriptions-item label="总丢弃消息">
              <el-text :type="stats.DiscardedTotal > 0 ? 'danger' : 'success'">{{ formatNumber(stats.DiscardedTotal) }}</el-text>
            </el-descriptions-item>
            <el-descriptions-item label="总接收流量">{{ formatBytes(stats.TotalBytesReceived) }}</el-descriptions-item>
            <el-descriptions-item label="总转发流量">{{ formatBytes(stats.TotalBytesForwarded) }}</el-descriptions-item>
            <el-descriptions-item label="最新快照">{{ formatDate(stats.SnapshotAt) }}</el-descriptions-item>
          </el-descriptions>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, onUnmounted, nextTick } from 'vue'
import * as echarts from 'echarts/core'
import { LineChart, BarChart, PieChart } from 'echarts/charts'
import {
  GridComponent, TooltipComponent, LegendComponent,
  TitleComponent, DataZoomComponent
} from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'
import {
  Connection, Monitor, MessageBox, ArrowUpBold,
  ArrowDownBold, Warning, Timer, Cpu
} from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import { mqttServerApi } from '@/api/index'

echarts.use([
  LineChart, BarChart, PieChart,
  GridComponent, TooltipComponent, LegendComponent,
  TitleComponent, DataZoomComponent,
  CanvasRenderer
])

const POLL_INTERVAL = 5000
const MAX_HISTORY = 20

const msgChartEl = ref(null)
const discardPieEl = ref(null)
const bytesChartEl = ref(null)

let msgChart = null
let discardPie = null
let bytesChart = null
let pollTimer = null   // 实时模式图表刷新 timer
let statsTimer = null  // 始终运行，刷新服务器信息面板

const polling = ref(false)
const granularity = ref('realtime')
const historyLoading = ref(false)
const historySnapshots = ref([])

const stats = reactive({
  OnlineClients: 0, ActiveTopics: 0, RunningServers: 0,
  TotalReceived: 0, TotalForwarded: 0,
  TotalBytesReceived: 0, TotalBytesForwarded: 0,
  DiscardedTotal: 0, DiscardedQueueFull: 0,
  DiscardedExpired: 0, DiscardedPublishFailed: 0,
  ServerStartedAt: '', SnapshotAt: ''
})

// 历史记录
const history = reactive({
  times: [],
  received: [],
  forwarded: [],
  discarded: [],
  bytesIn: [],
  bytesOut: []
})

// 当前图表数据（实时用 history 累计值，历史用快照增量）
const chartData = computed(() => {
  if (granularity.value === 'realtime') {
    return {
      times: [...history.times],
      received: [...history.received],
      forwarded: [...history.forwarded],
      discarded: [...history.discarded],
      bytesIn: [...history.bytesIn],
      bytesOut: [...history.bytesOut]
    }
  }
  const snaps = historySnapshots.value
  if (snaps.length === 0) {
    return { times: [], received: [], forwarded: [], discarded: [], bytesIn: [], bytesOut: [] }
  }
  // 历史模式显示每个时间段的增量（delta），而非累计值
  // 这样流量停止的区间会归零，不会显示为一条无意义的平线
  const delta = (key) => snaps.map((s, i) => i === 0 ? 0 : Math.max(0, s[key] - snaps[i - 1][key]))
  return {
    times: snaps.map(s => formatAxisTime(s.SnapshotAt, granularity.value)),
    received: delta('TotalReceived'),
    forwarded: delta('TotalForwarded'),
    discarded: delta('DiscardedTotal'),
    bytesIn: delta('TotalBytesReceived'),
    bytesOut: delta('TotalBytesForwarded')
  }
})

const summaryCards = computed(() => [
  { label: '在线客户端', value: stats.OnlineClients, icon: 'Connection', colorClass: 'card-blue' },
  { label: '活跃主题', value: stats.ActiveTopics, icon: 'MessageBox', colorClass: 'card-teal' },
  { label: '运行服务器', value: stats.RunningServers, icon: 'Monitor', colorClass: 'card-green' },
  { label: '总接收消息', value: stats.TotalReceived, icon: 'ArrowDownBold', colorClass: 'card-orange' },
  { label: '总转发消息', value: stats.TotalForwarded, icon: 'ArrowUpBold', colorClass: 'card-purple' },
  { label: '总丢弃消息', value: stats.DiscardedTotal, icon: 'Warning', colorClass: 'card-red',
    format: v => v > 0 ? formatNumber(v) : '0' }
])

const uptime = computed(() => {
  if (!stats.ServerStartedAt) return '-'
  const diff = Date.now() - new Date(stats.ServerStartedAt).getTime()
  const h = Math.floor(diff / 3600000)
  const m = Math.floor((diff % 3600000) / 60000)
  const s = Math.floor((diff % 60000) / 1000)
  if (h > 0) return `${h}h ${m}m ${s}s`
  if (m > 0) return `${m}m ${s}s`
  return `${s}s`
})

const formatNumber = v => {
  if (v === undefined || v === null) return '0'
  if (v >= 1e9) return (v / 1e9).toFixed(1) + 'B'
  if (v >= 1e6) return (v / 1e6).toFixed(1) + 'M'
  if (v >= 1e3) return (v / 1e3).toFixed(1) + 'K'
  return String(v)
}

const formatBytes = v => {
  if (!v) return '0 B'
  if (v >= 1073741824) return (v / 1073741824).toFixed(2) + ' GB'
  if (v >= 1048576) return (v / 1048576).toFixed(2) + ' MB'
  if (v >= 1024) return (v / 1024).toFixed(2) + ' KB'
  return v + ' B'
}

const formatDate = d => {
  if (!d) return '-'
  return new Date(d).toLocaleString('zh-CN')
}

const formatTime = d => {
  if (!d) return ''
  const dt = new Date(d)
  return `${String(dt.getHours()).padStart(2,'0')}:${String(dt.getMinutes()).padStart(2,'0')}:${String(dt.getSeconds()).padStart(2,'0')}`
}

/**
 * 根据粒度格式化坐标轴标签
 */
const formatAxisTime = (isoStr, gran) => {
  if (!isoStr) return ''
  const dt = new Date(isoStr)
  const pad = n => String(n).padStart(2, '0')
  if (gran === 'minute') {
    return `${pad(dt.getHours())}:${pad(dt.getMinutes())}`
  }
  if (gran === 'hour') {
    return `${pad(dt.getMonth()+1)}/${pad(dt.getDate())} ${pad(dt.getHours())}:00`
  }
  // day / all
  return `${dt.getFullYear()}/${pad(dt.getMonth()+1)}/${pad(dt.getDate())}`
}

const pushHistory = (s) => {
  const t = formatTime(s.SnapshotAt || new Date().toISOString())
  history.times.push(t)
  history.received.push(s.TotalReceived)
  history.forwarded.push(s.TotalForwarded)
  history.discarded.push(s.DiscardedTotal)
  history.bytesIn.push(s.TotalBytesReceived)
  history.bytesOut.push(s.TotalBytesForwarded)
  if (history.times.length > MAX_HISTORY) {
    history.times.shift(); history.received.shift(); history.forwarded.shift()
    history.discarded.shift(); history.bytesIn.shift(); history.bytesOut.shift()
  }
}

const fetchStats = async () => {
  try {
    const res = await mqttServerApi.getDashboardStats()
    Object.assign(stats, res)
    pushHistory(res)
    if (granularity.value === 'realtime') {
      updateCharts()
    }
  } catch (e) {
    // 静默失败，不打扰用户
  }
}

const fetchHistory = async () => {
  if (granularity.value === 'realtime') return
  historyLoading.value = true
  try {
    const res = await mqttServerApi.getDashboardStatsHistory({ Granularity: granularity.value })
    historySnapshots.value = res.Snapshots || []
    updateCharts()
  } catch (e) {
    ElMessage.error('获取历史数据失败')
  } finally {
    historyLoading.value = false
  }
}

// 历史模式下刷新按钮：同时刷新历史图表和服务器信息面板
const handleRefresh = async () => {
  await Promise.all([fetchHistory(), fetchStats()])
}

const onGranularityChange = async (val) => {
  if (val === 'realtime') {
    polling.value = true
    // 启动实时图表专用 timer
    if (!pollTimer) {
      pollTimer = setInterval(fetchStats, POLL_INTERVAL)
    }
    updateCharts()
  } else {
    polling.value = false
    // 停止实时图表 timer，服务器信息由 statsTimer 继续刷新
    if (pollTimer) {
      clearInterval(pollTimer)
      pollTimer = null
    }
    await fetchHistory()
  }
}

const updateMsgChart = () => {
  if (!msgChart) return
  const d = chartData.value
  msgChart.setOption({
    tooltip: { trigger: 'axis', axisPointer: { type: 'cross' } },
    legend: { data: ['接收', '转发', '丢弃'], bottom: 0 },
    grid: { left: 60, right: 20, top: 20, bottom: 40 },
    xAxis: { type: 'category', data: d.times, boundaryGap: false },
    yAxis: { type: 'value', minInterval: 1 },
    series: [
      { name: '接收', type: 'line', smooth: true, data: d.received,
        itemStyle: { color: '#409eff' }, areaStyle: { opacity: 0.1 } },
      { name: '转发', type: 'line', smooth: true, data: d.forwarded,
        itemStyle: { color: '#67c23a' }, areaStyle: { opacity: 0.1 } },
      { name: '丢弃', type: 'line', smooth: true, data: d.discarded,
        itemStyle: { color: '#f56c6c' }, areaStyle: { opacity: 0.1 } }
    ]
  }, true)
}

const updateDiscardPie = () => {
  if (!discardPie) return
  const total = stats.DiscardedTotal
  discardPie.setOption({
    tooltip: { trigger: 'item', formatter: '{b}: {c} ({d}%)' },
    legend: { orient: 'vertical', right: 10, top: 'center' },
    series: [{
      type: 'pie', radius: ['45%', '70%'], center: ['40%', '50%'],
      avoidLabelOverlap: false,
      label: { show: false },
      emphasis: { label: { show: true, fontSize: 14, fontWeight: 'bold' } },
      data: total === 0
        ? [{ name: '暂无丢弃', value: 1, itemStyle: { color: '#e8f5e9' } }]
        : [
            { name: '队列已满', value: stats.DiscardedQueueFull, itemStyle: { color: '#f56c6c' } },
            { name: '消息过期', value: stats.DiscardedExpired, itemStyle: { color: '#e6a23c' } },
            { name: '发送失败', value: stats.DiscardedPublishFailed, itemStyle: { color: '#909399' } }
          ]
    }]
  })
}

const updateBytesChart = () => {
  if (!bytesChart) return
  const d = chartData.value
  bytesChart.setOption({
    tooltip: {
      trigger: 'axis',
      formatter: params => {
        return params[0].axisValue + '<br/>' +
          params.map(p => `${p.marker}${p.seriesName}: ${formatBytes(p.value)}`).join('<br/>')
      }
    },
    legend: { data: ['接收字节', '转发字节'], bottom: 0 },
    grid: { left: 80, right: 20, top: 20, bottom: 40 },
    xAxis: { type: 'category', data: d.times },
    yAxis: {
      type: 'value', minInterval: 1,
      axisLabel: { formatter: v => formatBytes(v) }
    },
    series: [
      { name: '接收字节', type: 'bar', data: d.bytesIn,
        itemStyle: { color: '#409eff' }, barMaxWidth: 30 },
      { name: '转发字节', type: 'bar', data: d.bytesOut,
        itemStyle: { color: '#67c23a' }, barMaxWidth: 30 }
    ]
  }, true)
}

const updateCharts = () => {
  updateMsgChart()
  updateDiscardPie()
  updateBytesChart()
}

const initCharts = () => {
  if (msgChartEl.value) {
    msgChart = echarts.init(msgChartEl.value)
    updateMsgChart()
  }
  if (discardPieEl.value) {
    discardPie = echarts.init(discardPieEl.value)
    updateDiscardPie()
  }
  if (bytesChartEl.value) {
    bytesChart = echarts.init(bytesChartEl.value)
    updateBytesChart()
  }
}

const handleResize = () => {
  msgChart?.resize(); discardPie?.resize(); bytesChart?.resize()
}

onMounted(async () => {
  await nextTick()
  initCharts()
  await fetchStats()
  polling.value = true
  // statsTimer 始终运行，保持服务器信息面板实时更新
  statsTimer = setInterval(fetchStats, POLL_INTERVAL)
  // realtime 模式下 pollTimer 与 statsTimer 复用同一个
  pollTimer = statsTimer
  window.addEventListener('resize', handleResize)
})

onUnmounted(() => {
  if (statsTimer) clearInterval(statsTimer)
  if (pollTimer && pollTimer !== statsTimer) clearInterval(pollTimer)
  msgChart?.dispose(); discardPie?.dispose(); bytesChart?.dispose()
  window.removeEventListener('resize', handleResize)
})
</script>

<style scoped>
.dashboard { width: 100%; }

.stat-card {
  border-left: 4px solid transparent;
  margin-bottom: 0;
}
.card-blue { border-left-color: #409eff; }
.card-teal { border-left-color: #00bcd4; }
.card-green { border-left-color: #67c23a; }
.card-orange { border-left-color: #e6a23c; }
.card-purple { border-left-color: #9c27b0; }
.card-red { border-left-color: #f56c6c; }

.stat-inner {
  display: flex;
  align-items: center;
  gap: 12px;
}

.stat-icon {
  font-size: 36px;
  opacity: 0.8;
}

.card-blue .stat-icon { color: #409eff; }
.card-teal .stat-icon { color: #00bcd4; }
.card-green .stat-icon { color: #67c23a; }
.card-orange .stat-icon { color: #e6a23c; }
.card-purple .stat-icon { color: #9c27b0; }
.card-red .stat-icon { color: #f56c6c; }

.stat-value {
  font-size: 22px;
  font-weight: 700;
  color: #303133;
  line-height: 1.2;
}

.stat-label {
  font-size: 12px;
  color: #909399;
  margin-top: 4px;
}

.card-header-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.chart-toolbar {
  display: flex;
  align-items: center;
  gap: 8px;
}

.toolbar-title {
  font-weight: 600;
  font-size: 14px;
  color: #303133;
  margin-right: 8px;
}
</style>
