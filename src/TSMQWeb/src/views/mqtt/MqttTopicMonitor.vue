<template>
  <div class="mqtt-topic-monitor">
    <el-card>
      <template #header>
        <div class="card-header">
          <span>{{ t('MQTT 主题订阅') }}</span>
          <div class="header-actions">
            <el-button
              type="primary"
              :disabled="selectedTopics.length === 0"
              @click="handleStartMonitor"
            >
              <el-icon><DataAnalysis /></el-icon> {{ t('监控所选 (') }}{{ selectedTopics.length }})
            </el-button>
            <el-button @click="loadTopics" :loading="topicLoading">
              <el-icon><Refresh /></el-icon> {{ t('刷新') }} </el-button>
          </div>
        </div>
      </template>

      <CollectionToolbar v-model="query" :title="t('已注册主题')" :total="topicList.length" icon="Share" />
      <el-table
        :data="filteredRows"
        border
        stripe
        v-loading="topicLoading"
        @selection-change="handleTopicSelectionChange"
      >
        <el-table-column type="selection" width="50" align="center" />
        <el-table-column prop="Topic" :label="t('主题')" min-width="200" show-overflow-tooltip />
        <el-table-column prop="SubscriberCount" :label="t('订阅者数量')" width="120" align="center" />
      </el-table>
    </el-card>

    <el-card v-if="monitoringTopics.length > 0" style="margin-top: 16px;">
      <template #header>
        <div class="card-header">
          <span>{{ t('主题监控统计（自动刷新：') }}{{ pollingInterval / 1000 }} {{ t('秒）') }}</span>
          <div class="header-actions">
            <el-tag type="success" effect="dark"> {{ t('监控中') }} {{ monitoringTopics.length }} {{ t('个主题') }} </el-tag>
            <el-button v-permission="'MqttTopicManagerService.ResetMqttTopicStats'" @click="handleResetStats" :loading="resetLoading">
              <el-icon><RefreshLeft /></el-icon> {{ t('重置计数') }} </el-button>
            <el-button type="danger" @click="handleStopMonitor">
              <el-icon><CircleClose /></el-icon> {{ t('停止监控') }} </el-button>
          </div>
        </div>
      </template>

      <el-table
        :data="statsData"
        border
        stripe
        v-loading="statsLoading"
      >
        <el-table-column prop="topic" :label="t('主题')" min-width="200" show-overflow-tooltip />
        <el-table-column prop="subscriberCount" :label="t('订阅者数量')" width="120" align="center" />
        <el-table-column prop="inflowCount" :label="t('流入总量')" width="120" align="center" />
        <el-table-column prop="outflowCount" :label="t('流出总量')" width="120" align="center" />
        <el-table-column :label="t('流入速率(条/秒)')" width="150" align="center">
          <template #default="{ row }">
            <el-tag :type="row.inflowRate > 0 ? 'success' : 'info'">
              {{ row.inflowRate.toFixed(1) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column :label="t('流出速率(条/秒)')" width="150" align="center">
          <template #default="{ row }">
            <el-tag :type="row.outflowRate > 0 ? 'warning' : 'info'">
              {{ row.outflowRate.toFixed(1) }}
            </el-tag>
          </template>
        </el-table-column>
      </el-table>
    </el-card>
  </div>
</template>

<script setup>
import CollectionToolbar from '@/components/CollectionToolbar.vue'
import { t, useLocale } from '@/locales'
const { locale } = useLocale()
import { computed, ref, onMounted, onUnmounted } from 'vue'
import { ElMessage } from 'element-plus'
import { RefreshLeft } from '@element-plus/icons-vue'
import { mqttTopicApi } from '@/api/index'

const topicLoading = ref(false)
const statsLoading = ref(false)
const resetLoading = ref(false)
const topicList = ref([])
const selectedTopics = ref([])
const monitoringTopics = ref([])
const statsData = ref([])
const pollingInterval = 2000

let pollingTimer = null
const prevSnapshot = ref({})

onMounted(() => {
  loadTopics()
})

onUnmounted(() => {
  stopPolling()
})

const loadTopics = async () => {
  topicLoading.value = true
  try {
    const res = await mqttTopicApi.getTopicList()
    topicList.value = res.Topics || []
  } catch (error) {
    ElMessage.error(error.message || t("加载主题列表失败"))
  } finally {
    topicLoading.value = false
  }
}

const handleTopicSelectionChange = rows => {
  selectedTopics.value = rows
}

const handleStartMonitor = () => {
  const topics = selectedTopics.value.map(t => t.Topic)
  if (topics.length === 0) return

  const existing = new Set(monitoringTopics.value)
  topics.forEach(t => existing.add(t))
  monitoringTopics.value = [...existing]

  prevSnapshot.value = {}
  startPolling()
}

const handleResetStats = async () => {
  resetLoading.value = true
  try {
    await mqttTopicApi.resetTopicStats({ Topics: monitoringTopics.value })
    prevSnapshot.value = {}
    ElMessage.success(t("计数已重置"))
    await fetchStats()
  } catch (error) {
    ElMessage.error((error instanceof Error ? error.message : null) || t("重置失败"))
  } finally {
    resetLoading.value = false
  }
}

const handleStopMonitor = () => {
  monitoringTopics.value = []
  statsData.value = []
  prevSnapshot.value = {}
  stopPolling()
}

const startPolling = () => {
  stopPolling()
  fetchStats()
  pollingTimer = setInterval(fetchStats, pollingInterval)
}

const stopPolling = () => {
  if (pollingTimer !== null) {
    clearInterval(pollingTimer)
    pollingTimer = null
  }
}

const fetchStats = async () => {
  if (monitoringTopics.value.length === 0) return
  statsLoading.value = true
  try {
    const res = await mqttTopicApi.getTopicStats({ Topics: monitoringTopics.value })
    const now = Date.now()
    const rawStats = res.Stats || []

    statsData.value = rawStats.map(s => {
      const prev = prevSnapshot.value[s.Topic]
      let inflowRate = 0
      let outflowRate = 0

      if (prev) {
        const dt = (now - prev.time) / 1000
        if (dt > 0) {
          inflowRate = (s.InflowCount - prev.inflow) / dt
          outflowRate = (s.OutflowCount - prev.outflow) / dt
        }
      }

      prevSnapshot.value[s.Topic] = {
        inflow: s.InflowCount,
        outflow: s.OutflowCount,
        time: now
      }

      return {
        topic: s.Topic,
        subscriberCount: s.SubscriberCount,
        inflowCount: s.InflowCount,
        outflowCount: s.OutflowCount,
        inflowRate: Math.max(0, inflowRate),
        outflowRate: Math.max(0, outflowRate)
      }
    })
  } catch (error) {
    ElMessage.error(error.message || t("获取统计失败"))
  } finally {
    statsLoading.value = false
  }
}
const query = ref('')
const filteredRows = computed(() => topicList.value.filter(row => [row.Topic].some(value => String(value || '').toLowerCase().includes(query.value.toLowerCase()))))
</script>

<style scoped>
.mqtt-topic-monitor {
  width: 100%;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.header-actions {
  display: flex;
  align-items: center;
  gap: 8px;
}
</style>
