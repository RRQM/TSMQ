<template>
  <div>
    <el-card>
      <template #header>发送调试消息</template>
      <el-form label-width="90px">
        <el-form-item label="主题"><el-input v-model="publishForm.Topic" placeholder="devices/test/telemetry" /></el-form-item>
        <el-form-item label="负载"><el-input v-model="publishForm.Payload" type="textarea" :rows="3" /></el-form-item>
        <el-form-item label="编码"><el-radio-group v-model="publishForm.IsBase64"><el-radio-button :value="false">文本 / JSON</el-radio-button><el-radio-button :value="true">Base64</el-radio-button></el-radio-group><span class="hint">最多 64 KiB</span></el-form-item>
        <el-form-item label="QoS"><el-select v-model="publishForm.Qos" style="width: 100px"><el-option v-for="qos in [0, 1, 2]" :key="qos" :value="qos" :label="String(qos)" /></el-select></el-form-item>
        <el-form-item><el-button v-permission="'MqttAccessService.PublishMqttMessage'" type="primary" :loading="publishing" @click="publish">发送消息</el-button></el-form-item>
      </el-form>
    </el-card>
    <el-card class="events">
      <template #header><div class="actions"><span>连接与消息诊断</span><div><el-button @click="paused = !paused">{{ paused ? '继续刷新' : '暂停刷新' }}</el-button><el-button @click="exportEvents">导出</el-button><el-button @click="clear">清空视图</el-button></div></div></template>
      <el-alert title="最近 1000 条事件保存在内存，重启清空。消息内容需在访问控制页开启采集；单条最多展示 4 KiB。" type="info" :closable="false" />
      <el-form inline class="filters" @submit.prevent="reload">
        <el-form-item label="主题过滤器"><el-input v-model="topicInput" placeholder="留空包含连接事件" clearable /></el-form-item>
        <el-form-item label="客户端 ID"><el-input v-model="clientInput" clearable /></el-form-item>
        <el-form-item><el-button :loading="fetching" @click="reload">查询</el-button></el-form-item>
        <el-form-item label="内容格式"><el-select v-model="format" style="width: 100px"><el-option v-for="f in ['文本', 'JSON', 'Hex']" :key="f" :label="f" :value="f" /></el-select></el-form-item>
      </el-form>
      <el-table :data="events" border max-height="560" row-key="Id">
        <el-table-column type="expand"><template #default="{ row }"><pre class="payload">{{ payloadText(row) }}</pre></template></el-table-column>
        <el-table-column label="时间" width="180"><template #default="{ row }">{{ new Date(row.Timestamp).toLocaleString() }}</template></el-table-column>
        <el-table-column label="类型" width="135"><template #default="{ row }"><el-tag :type="row.Kind.includes('Denied') || row.Kind.includes('Rejected') ? 'danger' : 'info'">{{ kindNames[row.Kind] || row.Kind }}</el-tag></template></el-table-column>
        <el-table-column prop="ClientId" label="客户端 ID" min-width="130" show-overflow-tooltip />
        <el-table-column prop="UserName" label="用户名" width="110" />
        <el-table-column prop="Topic" label="主题" min-width="180" show-overflow-tooltip />
        <el-table-column prop="Qos" label="QoS" width="65" />
        <el-table-column label="负载" width="100"><template #default="{ row }">{{ row.PayloadLength }} B{{ row.Truncated ? '（截断）' : '' }}</template></el-table-column>
        <el-table-column prop="Detail" label="说明" min-width="230" show-overflow-tooltip />
      </el-table>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { onMounted, onUnmounted, reactive, ref } from 'vue'
import { ElMessage } from 'element-plus'
import { mqttAccessApi, type DiagnosticEvent } from '@/api/mqtt-access'
const publishForm = reactive({ Topic: '', Payload: '', IsBase64: false, Qos: 0, Retain: false })
const events = ref<DiagnosticEvent[]>([]), topicInput = ref(''), clientInput = ref(''), format = ref('文本')
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
  if (!event.PayloadBase64) return event.PayloadLength ? '未采集消息内容' : '无负载'
  const bytes = Uint8Array.from(atob(event.PayloadBase64), c => c.charCodeAt(0))
  if (format.value === 'Hex') return Array.from(bytes, b => b.toString(16).padStart(2, '0')).join(' ')
  const text = new TextDecoder().decode(bytes)
  if (format.value === 'JSON') { try { return JSON.stringify(JSON.parse(text), null, 2) } catch { return text } }
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
.events { margin-top: 20px; }
.actions { display: flex; justify-content: space-between; align-items: center; gap: 12px; }
.filters { margin-top: 16px; }
.hint { margin-left: 12px; color: var(--el-text-color-secondary); }
.payload { white-space: pre-wrap; overflow-wrap: anywhere; padding: 0 24px; }
</style>
