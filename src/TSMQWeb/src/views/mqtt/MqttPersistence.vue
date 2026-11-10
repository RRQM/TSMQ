<template>
  <div class="persistence-page">
    <el-card v-loading="loading" class="policy-card">
      <template #header><div class="section-title"><el-icon><Document /></el-icon><strong>{{ t('主题消息持久化策略') }}</strong></div></template>
      <el-alert type="info" :closable="false" :title="t('匹配主题过滤器的消息将保存完整原始负载，不受诊断采集的 4 KiB 限制。过期消息清理每小时执行。')" />
      <el-form label-position="top" class="policy-form" @submit.prevent="save">
        <div>
          <el-form-item :label="t('启用消息持久化')"><el-switch v-model="policy.Enabled" :disabled="!canSave || !policyLoaded || saving" /></el-form-item>
          <el-form-item :label="t('保留天数（1–3650）')"><el-input-number v-model="policy.RetentionDays" :min="1" :max="3650" :precision="0" :disabled="!canSave || !policyLoaded || saving" /></el-form-item>
        </div>
        <el-form-item :label="t('主题过滤器（每行一条，最多 500 条）')">
          <el-input v-model="filtersText" type="textarea" :rows="6" :disabled="!canSave || !policyLoaded || saving" :placeholder="t('输入主题过滤器，支持 + 和 # 通配符')" class="mono-input" />
          <p class="guidance">{{ t('+ 匹配单层主题；# 匹配多层主题，必须独占最后一层。空规则列表不保存任何消息。') }}</p>
        </el-form-item>
        <div class="policy-actions"><el-button :loading="loading" :disabled="saving" @click="loadPolicy">{{ t('重新加载策略') }}</el-button><el-button v-permission="'MqttPersistenceService.SavePolicy'" type="primary" native-type="submit" :loading="saving" :disabled="!policyLoaded">{{ t('保存策略') }}</el-button></div>
      </el-form>
    </el-card>

    <el-card class="history-card">
      <template #header><div class="section-title"><el-icon><Search /></el-icon><strong>{{ t('历史消息查询') }}</strong></div></template>
      <el-alert v-if="!canQuery" type="warning" :closable="false" :title="t('当前账号没有历史消息查询权限。')" />
      <el-form label-position="top" class="query-form" @submit.prevent="search">
        <el-form-item :label="t('主题过滤器')"><el-input v-model="query.TopicFilter" clearable :disabled="!canQuery || fetching" :placeholder="t('留空查询全部主题，支持 +/#')" /></el-form-item>
        <el-form-item :label="t('客户端 ID')"><el-input v-model="query.ClientId" clearable :disabled="!canQuery || fetching" :placeholder="t('留空查询所有客户端')" /></el-form-item>
        <el-form-item :label="t('时间范围')"><el-date-picker v-model="timeRange" type="datetimerange" :start-placeholder="t('开始时间')" :end-placeholder="t('结束时间')" :disabled="!canQuery || fetching" /></el-form-item>
        <el-form-item :label="t('每页条数')"><el-select v-model="query.Limit" :disabled="!canQuery || fetching"><el-option v-for="size in [20, 50, 100]" :key="size" :value="size" :label="String(size)" /></el-select></el-form-item>
        <el-form-item><el-button v-permission="'MqttPersistenceService.QueryMessages'" type="primary" native-type="submit" :loading="fetching">{{ t('查询最新消息') }}</el-button></el-form-item>
      </el-form>
      <el-table v-loading="fetching" :data="messages" row-key="Id" :empty-text="t('暂无历史消息，请调整筛选条件后查询。')">
        <el-table-column prop="Id" label="ID" width="90" />
        <el-table-column :label="t('时间')" min-width="180"><template #default="{ row }">{{ new Date(row.Timestamp).toLocaleString(locale, { hour12: false }) }}</template></el-table-column>
        <el-table-column prop="Topic" :label="t('主题')" min-width="200" show-overflow-tooltip />
        <el-table-column prop="ClientId" :label="t('客户端 ID')" min-width="150" show-overflow-tooltip />
        <el-table-column prop="UserName" :label="t('用户名')" min-width="120" show-overflow-tooltip />
        <el-table-column prop="Qos" label="QoS" width="65" />
        <el-table-column :label="t('保留消息')" width="95"><template #default="{ row }">{{ row.Retain ? t('是') : t('否') }}</template></el-table-column>
        <el-table-column :label="t('负载长度')" width="110"><template #default="{ row }">{{ row.PayloadLength }} B</template></el-table-column>
        <el-table-column :label="t('操作')" width="110" fixed="right"><template #default="{ row }"><el-button v-permission="'MqttPersistenceService.QueryMessages'" link type="primary" @click="showPayload(row)">{{ t('查看负载') }}</el-button></template></el-table-column>
      </el-table>
      <div class="pagination"><span>{{ t('按消息 ID 倒序显示') }} · {{ t('第 {page} 页', { page: pageIndex + 1 }) }}</span><div><el-button :disabled="!canQuery || fetching || pageIndex === 0" @click="previousPage">{{ t('上一页') }}</el-button><el-button :disabled="!canQuery || fetching || !hasMore || !messages.length" @click="nextPage">{{ t('下一页') }}</el-button></div></div>
    </el-card>

    <el-dialog v-model="payloadOpen" :title="t('完整消息负载')" width="min(850px, 94vw)">
      <template v-if="selectedMessage">
        <p class="payload-meta">{{ selectedMessage.Topic }} · {{ selectedMessage.PayloadLength }} B</p>
        <el-radio-group v-model="payloadFormat"><el-radio-button value="base64">Base64</el-radio-button><el-radio-button value="utf8">UTF-8</el-radio-button></el-radio-group>
        <p class="guidance">{{ t('二进制负载请使用 Base64 查看；UTF-8 仅用于有效文本，不会修改原始数据。') }}</p>
        <pre class="payload">{{ payloadText }}</pre>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, onUnmounted, reactive, ref } from 'vue'
import { ElMessage } from 'element-plus'
import { t, useLocale } from '@/locales'
import { useUserStore } from '@/stores/user'
import { mqttPersistenceApi, type MessageQuery, type PersistedMessage, type PersistencePolicy } from '@/api/mqtt-persistence'

const { locale } = useLocale()
const userStore = useUserStore()
const canSave = computed(() => userStore.hasPermission('MqttPersistenceService.SavePolicy'))
const canQuery = computed(() => userStore.hasPermission('MqttPersistenceService.QueryMessages'))
const loading = ref(false), saving = ref(false), fetching = ref(false), policyLoaded = ref(false)
const policy = reactive<PersistencePolicy>({ Enabled: false, TopicFilters: [], RetentionDays: 30 })
const filtersText = ref('')
const query = reactive({ TopicFilter: '', ClientId: '', Limit: 50 })
const timeRange = ref<[Date, Date] | null>(null)
const messages = ref<PersistedMessage[]>([]), hasMore = ref(false), pageIndex = ref(0)
const cursors = ref<number[]>([0])
const payloadOpen = ref(false), selectedMessage = ref<PersistedMessage | null>(null), payloadFormat = ref('base64')
let activeQuery: Omit<MessageQuery, 'BeforeId'> | null = null
let stopped = false

function validTopicFilter(filter: string) {
  if (!filter || filter.includes('\0') || new TextEncoder().encode(filter).length > 65535) return false
  const levels = filter.split('/')
  return levels.every((level, index) => (!level.includes('+') || level === '+') && (!level.includes('#') || (level === '#' && index === levels.length - 1)))
}

async function loadPolicy() {
  if (loading.value || saving.value) return
  loading.value = true
  try {
    const result = await mqttPersistenceApi.getPolicy()
    if (stopped) return
    Object.assign(policy, result.Policy)
    filtersText.value = result.Policy.TopicFilters.join('\n')
    policyLoaded.value = true
  } catch { policyLoaded.value = false }
  finally { loading.value = false }
}

async function save() {
  if (!canSave.value || !policyLoaded.value || saving.value) return
  const filters = filtersText.value.split(/\r?\n/).map(filter => filter.trim()).filter(Boolean)
  if (filters.length > 500) return void ElMessage.warning(t('主题过滤器最多 500 条。'))
  if (filters.some(filter => !validTopicFilter(filter))) return void ElMessage.warning(t('主题过滤器无效：+ 必须独占一层，# 必须独占最后一层。'))
  if (!Number.isInteger(policy.RetentionDays) || policy.RetentionDays < 1 || policy.RetentionDays > 3650) return void ElMessage.warning(t('保留天数必须为 1 到 3650 的整数。'))
  saving.value = true
  try {
    const result = await mqttPersistenceApi.savePolicy({ Enabled: policy.Enabled, TopicFilters: filters, RetentionDays: policy.RetentionDays })
    policy.TopicFilters = filters
    filtersText.value = filters.join('\n')
    ElMessage.success(result.Message || t('策略保存成功'))
  } catch {} finally { saving.value = false }
}

async function fetchPage(index: number, beforeId: number, criteria: Omit<MessageQuery, 'BeforeId'>) {
  if (!canQuery.value || fetching.value) return
  fetching.value = true
  try {
    const result = await mqttPersistenceApi.queryMessages({ ...criteria, BeforeId: beforeId })
    if (stopped) return
    messages.value = result.Messages
    hasMore.value = result.HasMore
    pageIndex.value = index
    if (index === 0) cursors.value = [0]
    else cursors.value = [...cursors.value.slice(0, index), beforeId]
    activeQuery = criteria
  } catch {} finally { fetching.value = false }
}

async function search() {
  if (!canQuery.value || fetching.value) return
  const topic = query.TopicFilter.trim()
  if (topic && !validTopicFilter(topic)) return void ElMessage.warning(t('主题过滤器无效：+ 必须独占一层，# 必须独占最后一层。'))
  const range = timeRange.value
  if (range && (!Number.isFinite(range[0].getTime()) || !Number.isFinite(range[1].getTime()) || range[0] > range[1])) return void ElMessage.warning(t('请选择有效的时间范围，开始时间不能晚于结束时间。'))
  await fetchPage(0, 0, { TopicFilter: topic, ClientId: query.ClientId.trim(), Limit: query.Limit, ...(range ? { StartTime: range[0].toISOString(), EndTime: range[1].toISOString() } : {}) })
}

async function previousPage() {
  const index = pageIndex.value - 1
  const cursor = cursors.value[index]
  if (activeQuery && index >= 0 && cursor !== undefined) await fetchPage(index, cursor, activeQuery)
}

async function nextPage() {
  const last = messages.value[messages.value.length - 1]
  if (activeQuery && hasMore.value && last) await fetchPage(pageIndex.value + 1, last.Id, activeQuery)
}

function showPayload(message: PersistedMessage) {
  if (!canQuery.value) return
  selectedMessage.value = message
  payloadFormat.value = 'base64'
  payloadOpen.value = true
}

const payloadText = computed(() => {
  const payload = selectedMessage.value?.PayloadBase64 || ''
  if (!payload) return t('无负载')
  if (payloadFormat.value === 'base64') return payload
  try {
    return new TextDecoder('utf-8', { fatal: true }).decode(Uint8Array.from(atob(payload), char => char.charCodeAt(0)))
  } catch { return t('负载不是有效的 UTF-8 文本，请切换到 Base64 查看原始数据。') }
})

onMounted(() => { void loadPolicy(); if (canQuery.value) void search() })
onUnmounted(() => { stopped = true })
</script>

<style scoped>
.persistence-page { display:grid;gap:22px; }
.section-title { display:flex;align-items:center;gap:10px; }
.section-title .el-icon { color:var(--accent);font-size:18px; }
.policy-form { display:grid;grid-template-columns:220px minmax(0,1fr);gap:20px;margin-top:22px; }
.policy-actions { grid-column:1/-1;display:flex;justify-content:flex-end;gap:10px;border-top:1px solid var(--line);padding-top:18px; }
.guidance { font-size:11px;color:var(--muted);line-height:1.8;margin-top:10px; }
.mono-input :deep(textarea), .payload { font-family:Consolas,monospace; }
.query-form { display:flex;align-items:flex-end;gap:14px;flex-wrap:wrap;margin-top:12px; }
.query-form :deep(.el-form-item) { min-width:130px; }
.query-form :deep(.el-select) { width:100px; }
.query-form :deep(.el-date-editor) { max-width:100%; }
.pagination { display:flex;align-items:center;justify-content:space-between;gap:12px;flex-wrap:wrap;margin-top:20px; }
.pagination > span, .payload-meta { font-size:11px;color:var(--muted);overflow-wrap:anywhere; }
.payload-meta { margin-bottom:16px; }
.payload { white-space:pre-wrap;overflow-wrap:anywhere;max-height:450px;overflow:auto;background:var(--surface-soft);border:1px solid var(--line);border-radius:8px;padding:16px;font-size:12px;line-height:1.8; }
@media(max-width:650px) { .policy-form { grid-template-columns:minmax(0,1fr); } .query-form { display:block; } .query-form :deep(.el-date-editor) { width:100%;min-width:0; } }
</style>
