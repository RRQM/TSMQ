<template>
  <div class="dashboard">
    <PageIntro eyebrow="WORKSPACE OVERVIEW" :title="t('统计概览')" :description="t('每一次连接，每一条消息，尽在掌握。')">
      <span class="live-pill" :class="{ offline: error || paused }"><span class="status-dot" />{{ error ? t('连接异常') : paused ? t('已暂停') : t('实时更新') }}</span>
      <button class="icon-button bordered" @click="paused = !paused" :aria-label="paused ? t('继续刷新') : t('暂停刷新')"><el-icon><VideoPlay v-if="paused" /><VideoPause v-else /></el-icon></button>
      <el-button :loading="loading" @click="refresh"><el-icon><Refresh /></el-icon>{{ t('刷新数据') }}</el-button>
    </PageIntro>
    <el-alert v-if="error" :title="t('暂时无法获取统计数据，正在重试。当前保留最后一次成功的数据。')" type="warning" :closable="false" class="dashboard-error" />
    <section class="metric-grid">
      <article v-for="(metric, index) in metrics" :key="metric.label" class="metric-card" :style="{ '--metric-color': metric.color, '--metric-soft': metric.soft, animationDelay: index * 65 + 'ms' }">
        <div class="metric-heading"><span class="metric-icon"><el-icon><component :is="metric.icon" /></el-icon></span><span>{{ t(metric.label) }}</span><el-icon class="metric-info" :title="t('当前 Broker 的实际统计值')"><InfoFilled /></el-icon></div>
        <div class="metric-number"><AnimatedNumber :value="metric.value" /><span class="metric-unit">{{ t(metric.unit) }}</span></div>
        <div class="metric-bottom"><span><span class="tiny-dot" />{{ t(metric.caption) }}</span><svg class="sparkline" viewBox="0 0 90 24" aria-hidden="true"><polyline :points="sparkline(metric.key)" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" /></svg></div>
      </article>
    </section>
    <div class="dashboard-middle">
      <section class="panel traffic-panel"><div class="panel-header"><div><h2>{{ t('消息流量') }}<span class="panel-label">{{ t('趋势分析') }}</span></h2><p>{{ granularity === 'realtime' ? t('每 5 秒采样 · 区间消息增量') : t('历史快照 · 区间消息增量') }}</p></div><div class="segmented"><button v-for="period in periods" :key="period.value" :class="{ active: granularity === period.value }" @click="setPeriod(period.value)">{{ t(period.label) }}</button></div></div><div class="chart-legend"><span><i style="background:#8c77e5" />{{ t('接收消息') }}</span><span><i style="background:#40bfa5" />{{ t('转发消息') }}</span><span><i style="background:#ed8b9b" />{{ t('丢弃消息') }}</span><span class="chart-unit">{{ t('条 / 采样区间') }}</span></div><div ref="messageEl" class="message-chart" v-loading="historyLoading" /><div class="chart-foot"><span>{{ t('统计范围') }}<b>{{ t(periods.find(p => p.value === granularity)?.label || '') }}</b></span><span>{{ t('最近更新') }} {{ updatedAt }}</span></div></section>
      <section class="panel health-panel"><div class="panel-header"><div><h2>{{ t('投递观测') }}</h2><p>{{ t('消息丢弃与运行状态') }}</p></div><span class="subtle-icon"><el-icon><Odometer /></el-icon></span></div><div class="health-ring" :style="{ '--ring-progress': discardPercent + '%', '--ring-color': stats.DiscardedTotal ? 'var(--danger)' : 'var(--mint)' }"><div><span>{{ t('丢弃占比') }}</span><strong>{{ discardPercent.toFixed(1) }}<small>%</small></strong><span class="ring-label">{{ stats.DiscardedTotal ? t('需要关注') : t('暂无丢弃') }}</span></div></div><div class="health-breakdown"><div v-for="cause in causes" :key="cause.label"><span><i :style="{ background: cause.color }" />{{ t(cause.label) }}</span><strong>{{ number(cause.value) }}</strong></div></div><div class="health-total"><span>{{ t('总丢弃消息') }}</span><strong>{{ number(stats.DiscardedTotal) }}</strong></div></section>
    </div>
    <div class="dashboard-bottom">
      <section class="panel throughput-panel"><div class="panel-header"><div><h2>{{ t('数据吞吐') }}</h2><p>{{ t('接收与转发的数据量') }}</p></div><span class="panel-label">BYTES</span></div><div class="throughput-summary"><div><span><i class="legend-line purple" />{{ t('累计接收') }}</span><strong>{{ bytes(stats.TotalBytesReceived) }}</strong></div><div><span><i class="legend-line mint" />{{ t('累计转发') }}</span><strong>{{ bytes(stats.TotalBytesForwarded) }}</strong></div></div><div ref="bytesEl" class="bytes-chart" /></section>
      <section class="panel topology-panel"><div class="panel-header"><div><h2>{{ t('消息网络') }}</h2><p>{{ t('共享 Broker · 跨协议互通') }}</p></div><span class="live-pill small"><span class="status-dot" />MQTT</span></div><div class="topology"><svg class="topology-lines" viewBox="0 0 360 170" preserveAspectRatio="none" aria-hidden="true"><path d="M55 42 C110 42 100 85 180 85 S250 42 305 42 M55 130 C110 130 100 85 180 85 S250 130 305 130" /><circle v-if="!paused && !error && stats.OnlineClients > 0" r="3" class="packet"><animateMotion dur="4s" repeatCount="indefinite" path="M55 42 C110 42 100 85 180 85 S250 130 305 130" /></circle></svg><div class="topology-edge"><span class="node"><el-icon><Monitor /></el-icon></span><small>TCP</small><span class="node"><el-icon><Connection /></el-icon></span><small>WebSocket</small></div><div class="broker-node"><ConsoleLogo /><strong>TSMQ Broker</strong><span>{{ number(stats.OnlineClients) }} {{ t('个在线连接') }}</span></div><div class="topology-edge"><span class="node"><el-icon><Share /></el-icon></span><small>{{ t('订阅者') }}</small><span class="node"><el-icon><MessageBox /></el-icon></span><small>{{ t('主题') }}</small></div></div><div class="network-stats"><div><strong>{{ number(stats.RunningServers) }}</strong><span>{{ t('运行监听器') }}</span></div><div><strong>{{ number(stats.ActiveTopics) }}</strong><span>{{ t('活跃主题') }}</span></div><div><strong>{{ uptime }}</strong><span>{{ t('运行时长') }}</span></div></div></section>
      <section class="panel quick-panel"><div class="panel-header"><div><h2>{{ t('工作台') }}</h2><p>{{ t('常用操作，触手可及') }}</p></div><el-icon class="subtle-icon"><Grid /></el-icon></div><div class="quick-links"><router-link v-for="link in quickLinks" :key="link.path" :to="link.path"><span class="quick-icon" :class="link.color"><el-icon><component :is="link.icon" /></el-icon></span><div><strong>{{ t(link.title) }}</strong><small>{{ t(link.description) }}</small></div><el-icon><ArrowRight /></el-icon></router-link></div><div class="quick-foot"><el-icon><Clock /></el-icon><span>{{ t('启动于') }} {{ startedAt }}</span></div></section>
    </div>
  </div>
</template>
<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref, reactive, watch } from 'vue'
import * as echarts from 'echarts/core'
import { LineChart } from 'echarts/charts'
import { GridComponent, TooltipComponent, GraphicComponent } from 'echarts/components'
import { CanvasRenderer } from 'echarts/renderers'
import { mqttServerApi, type MqttDashboardStatsResponse, type MqttStatsSnapshot } from '@/api'
import { useUserStore } from '@/stores/user'
import { usePreferences } from '@/composables/preferences'
import { t } from '@/locales'
import PageIntro from '@/components/PageIntro.vue'
import ConsoleLogo from '@/components/ConsoleLogo.vue'
import AnimatedNumber from '@/components/AnimatedNumber.vue'
echarts.use([LineChart, GridComponent, TooltipComponent, GraphicComponent, CanvasRenderer])
const { theme, locale } = usePreferences(), user = useUserStore()
const stats = reactive({ OnlineClients: 0, ActiveTopics: 0, RunningServers: 0, TotalReceived: 0, TotalForwarded: 0, TotalBytesReceived: 0, TotalBytesForwarded: 0, DiscardedTotal: 0, DiscardedQueueFull: 0, DiscardedExpired: 0, DiscardedPublishFailed: 0, ServerStartedAt: '', SnapshotAt: '' })
const loading = ref(false), historyLoading = ref(false), error = ref(false), paused = ref(false), granularity = ref('realtime'), now = ref(Date.now())
const messageEl = ref<HTMLElement>(), bytesEl = ref<HTMLElement>()
const samples = ref<(typeof stats & { sampledAt: number })[]>([]), snapshots = ref<MqttStatsSnapshot[]>([])
let messageChart: echarts.ECharts | undefined, bytesChart: echarts.ECharts | undefined, timer: ReturnType<typeof setTimeout>, clock: ReturnType<typeof setInterval>, resize: ResizeObserver, disposed = false, historySequence = 0
const periods = [{ label: '实时', value: 'realtime' }, { label: '1 小时', value: 'minute' }, { label: '24 小时', value: 'hour' }, { label: '30 天', value: 'day' }, { label: '全部', value: 'all' }]
const metrics = computed(() => [
  { label: '在线客户端', unit: '个', caption: '当前活跃连接', value: stats.OnlineClients, key: 'OnlineClients', icon: 'Connection', color: 'var(--accent)', soft: 'var(--accent-soft)' },
  { label: '活跃主题', unit: '个主题', caption: '已注册订阅主题', value: stats.ActiveTopics, key: 'ActiveTopics', icon: 'Share', color: 'var(--mint)', soft: 'var(--mint-soft)' },
  { label: '累计接收消息', unit: '条', caption: 'Broker 接收总量', value: stats.TotalReceived, key: 'TotalReceived', icon: 'Bottom', color: '#d9a251', soft: 'color-mix(in srgb,#d9a251 10%,var(--surface))' },
  { label: '累计转发消息', unit: '条', caption: '订阅队列转发总量', value: stats.TotalForwarded, key: 'TotalForwarded', icon: 'Top', color: '#629edd', soft: 'color-mix(in srgb,#629edd 10%,var(--surface))' }
])
const causes = computed(() => [{ label: '队列已满', value: stats.DiscardedQueueFull, color: '#a18ae5' }, { label: '消息过期', value: stats.DiscardedExpired, color: '#e4b66b' }, { label: '发送失败', value: stats.DiscardedPublishFailed, color: '#ed8b9b' }])
const discardPercent = computed(() => stats.TotalReceived > 0 ? Math.min(100, stats.DiscardedTotal / stats.TotalReceived * 100) : 0)
const updatedAt = computed(() => stats.SnapshotAt ? new Date(stats.SnapshotAt).toLocaleTimeString(locale.value, { hour12: false }) : '—')
const startedAt = computed(() => stats.ServerStartedAt ? new Date(stats.ServerStartedAt).toLocaleString(locale.value, { month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit', hour12: false }) : '—')
const uptime = computed(() => { if (!stats.ServerStartedAt) return '—'; const seconds = Math.max(0, Math.floor((now.value - new Date(stats.ServerStartedAt).getTime()) / 1000)); return seconds > 86400 ? `${Math.floor(seconds / 86400)}d ${Math.floor(seconds % 86400 / 3600)}h` : `${Math.floor(seconds / 3600)}h ${Math.floor(seconds % 3600 / 60)}m` })
const quickLinks = computed(() => [
  { path: '/mqtt-servers', title: '管理监听器', description: '配置服务与连接凭据', icon: 'Connection', color: 'purple', permission: 'MqttServerManagerService.GetMqttServerList' },
  { path: '/mqtt-access', title: '访问控制', description: '管理认证与主题权限', icon: 'Lock', color: 'mint', permission: 'MqttAccessService.GetMqttAccessPolicy' },
  { path: '/mqtt-diagnostics', title: '消息排障', description: '追踪事件，发送调试消息', icon: 'Search', color: 'amber', permission: 'MqttAccessService.GetMqttDiagnostics' }
].filter(link => user.hasPermission(link.permission)))
function number(value: number) { return new Intl.NumberFormat(locale.value, { notation: value >= 1000000 ? 'compact' : 'standard', maximumFractionDigits: 1 }).format(value) }
function bytes(value: number) { if (!value) return '0 B'; const index = Math.min(3, Math.floor(Math.log(value) / Math.log(1024))); return `${(value / 1024 ** index).toFixed(index ? 1 : 0)} ${['B', 'KiB', 'MiB', 'GiB'][index]}` }
function sparkline(key: string) { const values = samples.value.map(sample => Number((sample as unknown as Record<string, unknown>)[key] || 0)); if (values.length < 2) return '0,20 90,20'; const max = Math.max(...values), min = Math.min(...values); return values.map((v, i) => `${i / (values.length - 1) * 90},${max === min ? 19 : 21 - (v - min) / (max - min) * 18}`).join(' ') }
function renderCharts() {
  if (!messageChart || !bytesChart) return
  const css = getComputedStyle(document.documentElement), muted = css.getPropertyValue('--muted').trim(), line = css.getPropertyValue('--line').trim(), surface = css.getPropertyValue('--surface').trim(), ink = css.getPropertyValue('--ink').trim()
  const data = granularity.value === 'realtime' ? samples.value : snapshots.value
  const times = data.map(s => new Date(s.SnapshotAt).toLocaleString(locale.value, granularity.value === 'day' || granularity.value === 'all' ? { month: 'short', day: 'numeric' } : { hour: '2-digit', minute: '2-digit', second: granularity.value === 'realtime' ? '2-digit' : undefined, hour12: false }))
  const delta = (key: keyof MqttDashboardStatsResponse) => data.map((s, i) => i === 0 ? 0 : Math.max(0, Number(s[key as keyof typeof s]) - Number(data[i - 1]![key as keyof typeof s])))
  const base = { animationDuration: matchMedia('(prefers-reduced-motion: reduce)').matches ? 0 : 450, grid: { left: 52, right: 24, top: 18, bottom: 30 }, tooltip: { trigger: 'axis', backgroundColor: surface, borderColor: line, textStyle: { color: ink, fontSize: 11 } }, xAxis: { type: 'category', data: times, boundaryGap: false, axisTick: { show: false }, axisLine: { show: false }, axisLabel: { color: muted, fontSize: 9, hideOverlap: true, margin: 16 } }, yAxis: { type: 'value', minInterval: 1, axisLabel: { color: muted, fontSize: 9 }, splitLine: { lineStyle: { color: line, type: 'dashed' } } } }
  const series = (name: string, key: keyof MqttDashboardStatsResponse, color: string) => ({ name: t(name), type: 'line', smooth: .35, showSymbol: data.length <= 1, symbolSize: 5, data: delta(key), lineStyle: { width: 2, color }, itemStyle: { color }, areaStyle: { color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [{ offset: 0, color: color + '33' }, { offset: 1, color: color + '00' }]) } })
  messageChart.setOption({ ...base, series: [series('接收消息', 'TotalReceived', '#8c77e5'), series('转发消息', 'TotalForwarded', '#40bfa5'), series('丢弃消息', 'DiscardedTotal', '#ed8b9b')], graphic: data.length ? [] : [{ type: 'text', left: 'center', top: 'middle', style: { text: t('暂无采样数据'), fill: muted, fontSize: 12 } }] }, true)
  bytesChart.setOption({ ...base, grid: { ...base.grid, top: 8, bottom: 26 }, series: [series('接收字节', 'TotalBytesReceived', '#8c77e5'), series('转发字节', 'TotalBytesForwarded', '#40bfa5')] }, true)
}
async function refresh() {
  if (loading.value || disposed) return
  loading.value = true
  try { const result = await mqttServerApi.getDashboardStats() as unknown as MqttDashboardStatsResponse; if (disposed) return; Object.assign(stats, result); error.value = false; samples.value = [...samples.value, { ...stats, sampledAt: Date.now() }].slice(-40); renderCharts() } catch { error.value = true } finally { loading.value = false }
}
async function setPeriod(period: string) {
  granularity.value = period; const sequence = ++historySequence
  if (period === 'realtime') { historyLoading.value = false; renderCharts(); return }
  if (!user.hasPermission('MqttServerManagerService.GetDashboardStatsHistory')) { snapshots.value = []; renderCharts(); return }
  historyLoading.value = true
  try { const result = await mqttServerApi.getDashboardStatsHistory({ Granularity: period }) as unknown as { Snapshots?: MqttStatsSnapshot[] }; if (sequence === historySequence && !disposed) { snapshots.value = result.Snapshots || []; renderCharts() } } catch { if (sequence === historySequence && !disposed) { snapshots.value = []; renderCharts() } } finally { if (sequence === historySequence) historyLoading.value = false }
}
async function poll() { if (!paused.value && !document.hidden) await refresh(); if (!disposed) timer = setTimeout(poll, 5000) }
watch([theme, locale], renderCharts)
onMounted(() => { messageChart = echarts.init(messageEl.value!); bytesChart = echarts.init(bytesEl.value!); resize = new ResizeObserver(() => { messageChart?.resize(); bytesChart?.resize() }); resize.observe(messageEl.value!); resize.observe(bytesEl.value!); renderCharts(); poll(); clock = setInterval(() => { now.value = Date.now() }, 1000) })
onUnmounted(() => { disposed = true; clearTimeout(timer); clearInterval(clock); resize?.disconnect(); messageChart?.dispose(); bytesChart?.dispose() })
</script>
<style scoped>
.metric-grid { display: grid; grid-template-columns: repeat(4,minmax(0,1fr)); gap: 18px; margin-bottom: 23px; }
.metric-card { padding: 20px 21px 15px; border-radius: 12px; border: 1px solid var(--line); background: var(--surface); animation: reveal .45s both; transition: transform .25s,box-shadow .25s; overflow: hidden; }
.metric-card:hover { transform: translateY(-3px); box-shadow: var(--shadow); }
.metric-heading { display: flex; align-items: center; gap: 9px; font-size: 11px; color: var(--el-text-color-regular); }
.metric-icon { width: 31px; height: 31px; display: grid; place-items: center; border-radius: 8px; color: var(--metric-color); background: var(--metric-soft); font-size: 17px; }
.metric-info { margin-left: auto; color: var(--el-border-color); font-size: 12px; }
.metric-number { font-size: 33px; font-weight: 650; letter-spacing: -1.2px; padding: 17px 0 12px; font-variant-numeric: tabular-nums; }
.metric-unit { font-size: 10px; font-weight: 400; color: var(--muted); margin-left: 8px; }
.metric-bottom { display: flex; align-items: center; justify-content: space-between; gap: 8px; font-size: 9px; color: var(--muted); height: 25px; }
.metric-bottom > span { display: flex; align-items: center; gap: 5px; white-space: nowrap; }
.tiny-dot { width: 4px; height: 4px; border-radius: 50%; background: var(--metric-color); }
.sparkline { width: 70px; height: 25px; color: var(--metric-color); }
.dashboard-middle { display: grid; grid-template-columns: minmax(0,2.45fr) minmax(250px,1fr); gap: 22px; margin-bottom: 22px; }
.dashboard-bottom { display: grid; grid-template-columns: 1.12fr 1.15fr 1fr; gap: 22px; }
.panel { animation: reveal .5s .1s both; } .panel-label { font-size: 8px; border-radius: 4px; padding: 3px 6px; background: var(--surface-soft); color: var(--muted); font-weight: 400; margin-left: 9px; }
.segmented { display: flex; padding: 3px; background: var(--surface-soft); border: 1px solid var(--line); border-radius: 7px; }
.segmented button { font-size: 9px; padding: 5px 9px; background: none; border: 0; border-radius: 5px; color: var(--muted); white-space: nowrap; }
.segmented button.active { background: var(--surface); color: var(--accent); box-shadow: 0 1px 4px #0000000b; }
.chart-legend { display: flex; align-items: center; gap: 18px; font-size: 9px; color: var(--muted); padding: 25px 23px 3px; }
.chart-legend > span { display: flex; align-items: center; gap: 6px; } .chart-legend i,.health-breakdown i { width: 6px; height: 6px; border-radius: 50%; display: inline-block; }
.chart-unit { margin-left: auto; }
.message-chart { height: 249px; } .chart-foot { display: flex; justify-content: space-between; border-top: 1px solid var(--line); padding: 13px 23px; color: var(--muted); font-size: 9px; } .chart-foot b { color: var(--el-text-color-regular); margin-left: 9px; font-weight: 400; }
.live-pill { display: inline-flex; align-items: center; gap: 7px; color: var(--mint); background: var(--mint-soft); font-size: 10px; padding: 7px 10px; border-radius: 6px; white-space: nowrap; } .live-pill .status-dot { animation: pulse 2.5s infinite; } .live-pill.offline { color: var(--muted); background: var(--surface-soft); } .live-pill.offline .status-dot { background: var(--muted); animation: none; } .bordered { border: 1px solid var(--line); background: var(--surface); }
.subtle-icon { color: var(--muted); font-size: 16px; } .health-ring { width: 146px; height: 146px; margin: 23px auto 20px; border-radius: 50%; background: conic-gradient(var(--ring-color) var(--ring-progress),var(--mint-soft) 0); padding: 9px; transform: rotate(-25deg); position: relative; } .health-ring::before { content: ''; position: absolute; inset: 0; border-radius: 50%; border: 1px solid var(--line); }
.health-ring > div { border-radius: 50%; height: 100%; background: var(--surface); display: flex; flex-direction: column; align-items: center; justify-content: center; transform: rotate(25deg); } .health-ring span { font-size: 9px; color: var(--muted); } .health-ring strong { font-size: 30px; margin: 5px 0; font-weight: 600; letter-spacing: -.7px; } .health-ring small { font-size: 14px; margin-left: 2px; } .health-ring .ring-label { color: var(--mint); font-size: 8px; }
.health-breakdown { padding: 0 24px; } .health-breakdown > div { display: flex; justify-content: space-between; align-items: center; padding: 6px 0; font-size: 10px; color: var(--muted); } .health-breakdown > div > span { display: flex; gap: 8px; align-items: center; } .health-breakdown strong { color: var(--ink); font-weight: 500; } .health-total { display: flex; justify-content: space-between; margin: 13px 24px 0; padding: 13px 0; border-top: 1px solid var(--line); font-size: 10px; color: var(--muted); } .health-total strong { color: var(--ink); }
.throughput-summary { display: flex; gap: 32px; padding: 22px 23px 12px; } .throughput-summary > div > span { color: var(--muted); font-size: 9px; display: flex; align-items: center; gap: 5px; } .throughput-summary strong { display: block; margin-top: 8px; font-size: 20px; font-weight: 550; letter-spacing: -.5px; } .legend-line { display: inline-block; width: 8px; height: 2px; } .legend-line.purple { background: #8c77e5; } .legend-line.mint { background: #40bfa5; } .bytes-chart { height: 166px; }
.topology { height: 175px; margin: 10px 20px 0; display: flex; justify-content: space-between; align-items: center; position: relative; } .topology-lines { position: absolute; inset: 0; width: 100%; height: 100%; } .topology-lines path { fill: none; stroke: var(--el-border-color); stroke-width: 1.2; stroke-dasharray: 4 4; } .packet { fill: var(--mint); } .topology-edge { position: relative; display: flex; flex-direction: column; align-items: center; width: 50px; gap: 5px; } .node { background: var(--surface-soft); border: 1px solid var(--line); border-radius: 8px; width: 33px; height: 30px; display: grid; place-items: center; color: var(--muted); } .topology-edge small { font-size: 8px; color: var(--muted); margin-bottom: 8px; }
.broker-node { position: relative; padding: 13px 10px; background: var(--surface); border: 1px solid color-mix(in srgb,var(--accent) 30%,var(--line)); box-shadow: 0 0 0 5px var(--accent-soft); border-radius: 12px; display: flex; flex-direction: column; align-items: center; gap: 9px; width: 120px; } .broker-node strong { font-size: 11px; font-weight: 600; } .broker-node > span { color: var(--muted); font-size: 8px; } .broker-node :deep(.brand-mark) { width: 30px; height: 30px; border-radius: 8px; } .broker-node :deep(svg) { width: 24px; height: 24px; }
.network-stats { display: flex; border-top: 1px solid var(--line); margin: 5px 20px 0; padding: 16px 0; } .network-stats > div { flex: 1; text-align: center; border-right: 1px solid var(--line); } .network-stats > div:last-child { border: 0; } .network-stats strong { font-size: 16px; display: block; font-weight: 600; } .network-stats span { color: var(--muted); font-size: 8px; display: block; margin-top: 6px; }
.quick-links { padding: 15px 21px; } .quick-links a { display: flex; align-items: center; gap: 10px; padding: 11px 0; border-bottom: 1px solid var(--line); transition: transform .2s; } .quick-links a:last-child { border: 0; } .quick-links a:hover { transform: translateX(4px); } .quick-icon { width: 32px; height: 33px; border-radius: 8px; display: grid; place-items: center; font-size: 16px; flex-shrink: 0; } .quick-icon.purple { color: var(--accent); background: var(--accent-soft); } .quick-icon.mint { color: var(--mint); background: var(--mint-soft); } .quick-icon.amber { color: #d9a251; background: color-mix(in srgb,#d9a251 10%,var(--surface)); } .quick-links strong { font-size: 10px; font-weight: 550; display: block; } .quick-links small { font-size: 8px; color: var(--muted); display: block; margin-top: 5px; } .quick-links a > .el-icon { margin-left: auto; color: var(--muted); font-size: 10px; } .quick-foot { display: flex; gap: 7px; align-items: center; font-size: 8px; color: var(--muted); border-top: 1px solid var(--line); padding: 13px 23px; } .dashboard-error { margin-bottom: 20px; }
@media(min-width:1600px) { .message-chart { height: 300px; } .health-ring { margin: 30px auto; width: 170px; height: 170px; } .bytes-chart { height: 195px; } .topology { height: 205px; } .quick-links a { padding: 16px 0; } }
@media(max-width:1150px) { .metric-grid { gap: 12px; } .metric-card { padding: 16px; } .sparkline { width: 45px; } .dashboard-middle { grid-template-columns: minmax(0,2fr) minmax(230px,1fr); gap: 15px; } .dashboard-bottom { grid-template-columns: 1fr 1fr; gap: 15px; } .quick-panel { grid-column: 1/-1; } .quick-links { display: flex; gap: 25px; } .quick-links a { flex: 1; border: 0; } .panel-header { padding-left: 18px; padding-right: 18px; } .segmented button { padding: 5px 6px; } }
@media(max-width:900px) { .metric-grid { grid-template-columns: repeat(2,minmax(0,1fr)); } .traffic-panel .panel-header { align-items: flex-start; flex-direction: column; } .message-chart { height: 256px; } .chart-unit { display: none!important; } }
@media(max-width:600px) { .dashboard-middle,.dashboard-bottom { grid-template-columns: minmax(0,1fr); } .health-panel { display: grid; grid-template-columns: 1fr 1fr; } .health-panel .panel-header { grid-column: 1/-1; } .health-ring { grid-row: 2/4; width: 125px; height: 125px; } .health-breakdown { padding-top: 20px; } .health-total { margin-top: 3px; } .quick-panel { grid-column: auto; } .quick-links { display: block; } .metric-number { font-size: 30px; } .metric-heading { font-size: 10px; } .metric-icon { width: 26px; height: 26px; } .metric-info { display: none; } .metric-bottom { font-size: 8px; } .sparkline { width: 40px; } .chart-foot { font-size: 8px; } }
</style>
