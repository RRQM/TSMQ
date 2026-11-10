<template>
  <div class="diagnostics-grid">
    <aside class="publish-pane"><el-card><template #header><div class="section-title"><span class="section-icon"><el-icon><Position /></el-icon></span><div><strong>{{ t('发送调试消息') }}</strong><p>{{ t('直接向 Broker 发布测试消息') }}</p></div></div></template><el-form label-position="top" @submit.prevent="publish"><el-form-item :label="t('主题')"><el-input v-model="publishForm.Topic" placeholder="devices/test/telemetry" /></el-form-item><el-form-item :label="t('编码')"><el-radio-group v-model="publishForm.IsBase64"><el-radio-button :value="false">{{ t('文本 / JSON') }}</el-radio-button><el-radio-button :value="true">Base64</el-radio-button></el-radio-group></el-form-item><el-form-item :label="t('负载')"><el-input v-model="publishForm.Payload" type="textarea" :rows="9" placeholder="{ &#10;  &quot;message&quot;: &quot;hello&quot;&#10;}" class="code-input" /></el-form-item><div class="publish-options"><el-form-item label="QoS"><el-select v-model="publishForm.Qos" style="width:85px"><el-option v-for="qos in [0,1,2]" :key="qos" :value="qos" :label="String(qos)" /></el-select></el-form-item><span>{{ payloadBytes }} B <span>/ 64 KiB</span></span></div><el-button v-permission="'MqttAccessService.PublishMqttMessage'" type="primary" :loading="publishing" :disabled="!publishForm.Topic" native-type="submit" class="publish-submit"><el-icon><Position /></el-icon>{{ t('发送消息') }}</el-button></el-form><div class="publish-note"><el-icon><InfoFilled /></el-icon><span>{{ t('调试发布使用管理权限，不受设备主题 ACL 限制。') }}</span></div></el-card><div class="diagnostic-hint"><el-icon><Lock /></el-icon><div><strong>{{ t('按需采集，安心排障') }}</strong><p>{{ t('负载默认不采集，可在访问控制中开启。每条最多保留 4 KiB。') }}</p><router-link to="/mqtt-access" v-permission="'MqttAccessService.GetMqttAccessPolicy'">{{ t('前往访问控制') }} →</router-link></div></div></aside>
    <el-card class="event-pane"><template #header><div class="actions"><div class="section-title"><span class="section-icon mint"><el-icon><DataLine /></el-icon></span><div><strong>{{ t('连接与消息诊断') }}</strong><p>{{ t('最近事件') }} <span class="count-badge">{{ events.length }}</span><span class="stream-state"><span class="status-dot" :class="{ paused }" />{{ paused ? t('已暂停') : t('实时更新') }}</span></p></div></div><div class="event-actions"><button class="icon-button bordered" @click="paused = !paused" :title="paused ? t('继续刷新') : t('暂停刷新')" :aria-label="paused ? t('继续刷新') : t('暂停刷新')"><el-icon><VideoPlay v-if="paused" /><VideoPause v-else /></el-icon></button><button class="icon-button bordered" @click="exportEvents" :title="t('导出')" :aria-label="t('导出')"><el-icon><Download /></el-icon></button><button class="icon-button bordered" @click="clear" :title="t('清空视图')" :aria-label="t('清空视图')"><el-icon><Delete /></el-icon></button></div></div></template><el-form inline class="filters" @submit.prevent="reload"><el-form-item><el-input v-model="topicInput" :placeholder="t('主题过滤器')" prefix-icon="Search" clearable /></el-form-item><el-form-item><el-input v-model="clientInput" :placeholder="t('客户端 ID')" clearable /></el-form-item><el-form-item><el-button :loading="fetching" native-type="submit">{{ t('查询') }}</el-button></el-form-item><el-form-item><el-select v-model="format" style="width:90px"><el-option :label="t('文本')" value="text" /><el-option label="JSON" value="json" /><el-option label="Hex" value="hex" /></el-select></el-form-item></el-form>
    <el-table :data="events" max-height="580" row-key="Id" :empty-text="t('等待连接与消息事件…')"><el-table-column type="expand"><template #default="{ row }"><div class="event-detail"><div><span>{{ t('客户端 ID') }}</span><code>{{ row.ClientId || '—' }}</code><span>{{ t('用户名') }}</span><code>{{ row.UserName || '—' }}</code></div><p>{{ t(row.Detail) }}</p><pre class="payload">{{ payloadText(row) }}</pre></div></template></el-table-column><el-table-column :label="t('时间')" width="105"><template #default="{ row }"><span class="mono">{{ new Date(row.Timestamp).toLocaleTimeString(locale, { hour12: false }) }}</span></template></el-table-column><el-table-column :label="t('类型')" width="155"><template #default="{ row }"><el-tag :type="row.Kind.includes('Denied') || row.Kind.includes('Rejected') ? 'danger' : row.Kind === 'Message' ? 'success' : 'info'">{{ t(kindNames[row.Kind] || row.Kind) }}</el-tag></template></el-table-column><el-table-column prop="Topic" :label="t('主题')" min-width="170" show-overflow-tooltip><template #default="{ row }"><span class="mono">{{ row.Topic || '—' }}</span></template></el-table-column><el-table-column prop="ClientId" :label="t('客户端 ID')" min-width="130" show-overflow-tooltip /><el-table-column :label="t('负载')" width="95"><template #default="{ row }"><span class="mono">{{ row.PayloadLength }} B</span>{{ row.Truncated ? '…' : '' }}</template></el-table-column><el-table-column :label="t('说明')" min-width="180" show-overflow-tooltip><template #default="{ row }">{{ t(row.Detail) }}</template></el-table-column></el-table><div class="record-footer"><span>{{ t('最多保留 1000 条事件 · 重启后清空') }}</span><span>{{ t('每 2 秒刷新') }}</span></div></el-card>
  </div>
</template>

<script setup lang="ts">
import { t, useLocale } from '@/locales'
const { locale } = useLocale()
import { computed, onMounted, onUnmounted, reactive, ref } from 'vue'
import { ElMessage } from 'element-plus'
import { mqttAccessApi, type DiagnosticEvent } from '@/api/mqtt-access'
const publishForm = reactive({ Topic: '', Payload: '', IsBase64: false, Qos: 0, Retain: false })
const payloadBytes = computed(() => new TextEncoder().encode(publishForm.Payload).length)
const events = ref<DiagnosticEvent[]>([]), topicInput = ref(''), clientInput = ref(''), format = ref('text')
const paused = ref(false), fetching = ref(false), publishing = ref(false)
const kindNames: Record<string, string> = { Connected: '连接通过', ConnectionRejected: '连接拒绝', Disconnected: '连接断开', AccessDenied: '权限拒绝', Message: '收到消息', DebugPublish: '调试发布', Subscribed: '订阅通过', Discarded: '消息丢弃', PolicyChanged: '策略变更' }
let cursor = 0, topic = '', client = '', timer: ReturnType<typeof setTimeout> | undefined, stopped = false, generation = 0
async function fetchEvents() {
  if (fetching.value || stopped) return
  const current = generation
  fetching.value = true
  try {
    const res = await mqttAccessApi.diagnostics({ AfterId: cursor, TopicFilter: topic, ClientId: client, Limit: 200 })
    if (stopped || current !== generation) return
    cursor = res.LastId
    events.value = [...events.value, ...res.Events].slice(-1000)
  } catch { paused.value = true } finally { fetching.value = false }
}
async function poll() { if (!paused.value) await fetchEvents(); if (!stopped) timer = setTimeout(poll, 2000) }
async function reload() { generation++; topic = topicInput.value; client = clientInput.value; cursor = 0; events.value = []; await fetchEvents() }
function clear() { events.value = [] }
async function publish() {
  publishing.value = true
  try { ElMessage.success((await mqttAccessApi.publish(publishForm)).Message); await fetchEvents() } catch { /* 请求层展示错误 */ }
  finally { publishing.value = false }
}
function payloadText(event: DiagnosticEvent) {
  if (!event.PayloadBase64) return event.PayloadLength ? t('未采集消息内容') : t('无负载')
  const bytes = Uint8Array.from(atob(event.PayloadBase64), c => c.charCodeAt(0))
  if (format.value === 'hex') return Array.from(bytes, b => b.toString(16).padStart(2, '0')).join(' ')
  const text = new TextDecoder().decode(bytes)
  if (format.value === 'json') { try { return JSON.stringify(JSON.parse(text), null, 2) } catch { return text } }
  return text
}
function exportEvents() {
  const url = URL.createObjectURL(new Blob([JSON.stringify(events.value, null, 2)], { type: 'application/json' }))
  const a = document.createElement('a'); a.href = url; a.download = 'mqtt-diagnostics.json'; a.click(); URL.revokeObjectURL(url)
}
onMounted(poll)
onUnmounted(() => { stopped = true; generation++; clearTimeout(timer) })
</script>

<style scoped>
.diagnostics-grid { display:grid;grid-template-columns:300px minmax(0,1fr);gap:22px;align-items:start; } .section-title { display:flex;align-items:center;gap:10px; } .section-icon { width:33px;height:33px;border-radius:9px;display:grid;place-items:center;background:var(--accent-soft);color:var(--accent);font-size:18px;flex-shrink:0; } .section-icon.mint { color:var(--mint);background:var(--mint-soft); } .section-title strong { font-size:12px;font-weight:600; } .section-title p { font-size:9px;color:var(--muted);font-weight:400;margin-top:6px; } .publish-submit { width:100%; } .code-input :deep(textarea) { font-family:'Consolas',monospace;font-size:12px;line-height:1.8;padding:12px;background:var(--surface-soft); } .publish-options { display:flex;justify-content:space-between;align-items:center;margin-top:10px; } .publish-options > span { font-size:9px;color:var(--muted); } .publish-options > span > span { opacity:.5; } .publish-note { display:flex;gap:8px;color:var(--muted);font-size:9px;line-height:1.8;border-top:1px solid var(--line);padding-top:17px;margin-top:20px; } .publish-note .el-icon { flex-shrink:0;margin-top:3px; } .diagnostic-hint { display:flex;gap:12px;padding:22px 5px;color:var(--muted); } .diagnostic-hint > .el-icon { font-size:19px;color:var(--accent);margin-top:2px; } .diagnostic-hint strong { color:var(--ink);font-size:11px;font-weight:500; } .diagnostic-hint p { font-size:10px;line-height:1.8;margin:9px 0; } .diagnostic-hint a { font-size:10px;color:var(--accent); } .actions { display:flex;justify-content:space-between;gap:12px;align-items:center;flex-wrap:wrap; } .event-actions { display:flex;gap:6px; } .bordered { border:1px solid var(--line); } .stream-state { margin-left:12px;display:inline-flex;gap:5px;align-items:center;color:var(--mint); } .status-dot.paused { background:var(--muted); } .filters :deep(.el-input) { width:165px; } .event-detail { padding:18px 25px;background:var(--surface-soft); } .event-detail > div { display:flex;gap:10px;align-items:center;flex-wrap:wrap;color:var(--muted);font-size:10px; } .event-detail code { color:var(--ink); } .event-detail p { font-size:10px;color:var(--muted);margin-top:12px; } .payload { background:var(--surface);padding:15px;border:1px solid var(--line);border-radius:8px;white-space:pre-wrap;overflow-wrap:anywhere;font:11px/1.8 'Consolas',monospace;max-height:330px;overflow:auto; }
@media(max-width:1250px) { .diagnostics-grid { grid-template-columns:260px minmax(0,1fr);gap:16px; } } @media(max-width:1000px) { .diagnostics-grid { grid-template-columns:minmax(0,1fr); } .publish-pane :deep(.el-form) { display:grid;grid-template-columns:1fr 1fr;gap:0 20px;align-items:center; } .publish-pane :deep(.el-form-item:nth-child(3)) { grid-column:1/-1; } .publish-pane :deep(textarea) { min-height:100px!important;height:100px; } .diagnostic-hint { display:none; } } @media(max-width:600px) { .publish-pane :deep(.el-form) { display:block; } }
</style>
