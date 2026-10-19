<template>
  <el-card v-loading="loading">
    <template #header><div class="actions"><span>MQTT 访问控制</span><el-button v-permission="'MqttAccessService.SetMqttAccessPolicy'" type="primary" :loading="saving" @click="save">保存策略</el-button></div></template>
    <el-alert title="保存策略会断开现有 MQTT 客户端并清除旧订阅，客户端需重新连接和订阅。" type="warning" :closable="false" />
    <el-form label-width="150px" class="settings">
      <el-form-item label="验证账号密码"><el-switch v-model="policy.AuthenticationEnabled" /><span class="hint">开启后仅允许连接授权用户登录</span></el-form-item>
      <el-form-item label="启用主题 ACL"><el-switch v-model="policy.AclEnabled" /><span class="hint">拒绝规则优先；未匹配允许规则则拒绝</span></el-form-item>
      <el-form-item label="采集消息内容"><el-switch v-model="policy.CapturePayload" /><span class="hint">诊断页可查看最近消息内容，每条最多 4 KiB</span></el-form-item>
    </el-form>
    <div class="actions"><span>主题规则（用户名和客户端 ID 留空表示全部）</span><el-button v-permission="'MqttAccessService.SetMqttAccessPolicy'" @click="addRule">添加规则</el-button></div>
    <el-table :data="policy.Rules" border class="rules">
      <el-table-column label="用户名" min-width="130"><template #default="{ row }"><el-input v-model="row.UserName" placeholder="所有用户" /></template></el-table-column>
      <el-table-column label="客户端 ID" min-width="140"><template #default="{ row }"><el-input v-model="row.ClientId" placeholder="所有客户端" /></template></el-table-column>
      <el-table-column label="主题过滤器" min-width="190"><template #default="{ row }"><el-input v-model="row.TopicFilter" placeholder="devices/+/telemetry" /></template></el-table-column>
      <el-table-column label="操作" width="140"><template #default="{ row }"><el-select v-model="row.Action"><el-option label="发布和订阅" value="Both" /><el-option label="发布" value="Publish" /><el-option label="订阅" value="Subscribe" /></el-select></template></el-table-column>
      <el-table-column label="结果" width="110"><template #default="{ row }"><el-switch v-model="row.Allow" inline-prompt active-text="允许" inactive-text="拒绝" /></template></el-table-column>
      <el-table-column width="80"><template #default="{ $index }"><el-button v-permission="'MqttAccessService.SetMqttAccessPolicy'" link type="danger" @click="policy.Rules.splice($index, 1)">删除</el-button></template></el-table-column>
    </el-table>
    <el-alert class="rules" type="info" :closable="false" title="支持 + 和 #。订阅范围必须完整包含在允许规则中，不能与拒绝规则重叠。暂不支持 $share 共享订阅。" />
    <el-divider>测试已保存策略</el-divider>
    <el-form inline>
      <el-form-item label="用户名"><el-input v-model="testForm.UserName" /></el-form-item>
      <el-form-item label="客户端 ID"><el-input v-model="testForm.ClientId" /></el-form-item>
      <el-form-item label="主题"><el-input v-model="testForm.Topic" /></el-form-item>
      <el-form-item><el-select v-model="testForm.Action" style="width: 100px"><el-option label="发布" value="Publish" /><el-option label="订阅" value="Subscribe" /></el-select></el-form-item>
      <el-form-item><el-button v-permission="'MqttAccessService.TestMqttAccess'" :loading="testing" @click="test">测试</el-button></el-form-item>
      <el-form-item v-if="testResult !== null"><el-tag :type="testResult ? 'success' : 'danger'">{{ testResult ? '允许' : '拒绝' }}</el-tag></el-form-item>
    </el-form>
  </el-card>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { mqttAccessApi, type AccessPolicy } from '@/api/mqtt-access'
const loading = ref(false), saving = ref(false), testing = ref(false)
const policy = reactive<AccessPolicy>({ AuthenticationEnabled: true, AclEnabled: false, CapturePayload: false, Rules: [] })
const testForm = reactive({ UserName: '', ClientId: '', Topic: '', Action: 'Publish' })
const testResult = ref<boolean | null>(null)
onMounted(async () => {
  loading.value = true
  try { Object.assign(policy, (await mqttAccessApi.getPolicy()).Policy) } catch { /* 请求层展示错误 */ }
  finally { loading.value = false }
})
function addRule() { policy.Rules.push({ UserName: '', ClientId: '', TopicFilter: '', Action: 'Both', Allow: true }) }
async function save() {
  try { await ElMessageBox.confirm('保存后现有 MQTT 连接将断开，是否继续？', '更新访问策略', { type: 'warning' }) } catch { return }
  saving.value = true
  try { ElMessage.success((await mqttAccessApi.savePolicy(policy)).Message); testResult.value = null } catch { /* 请求层展示错误 */ }
  finally { saving.value = false }
}
async function test() {
  testing.value = true
  try { testResult.value = (await mqttAccessApi.test(testForm)).Allowed } catch { testResult.value = null }
  finally { testing.value = false }
}
</script>

<style scoped>
.actions { display: flex; justify-content: space-between; align-items: center; gap: 12px; }
.settings, .rules { margin-top: 20px; }
.hint { margin-left: 12px; color: var(--el-text-color-secondary); }
</style>
