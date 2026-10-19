<template>
  <div class="mqtt-server-manager">
    <div class="summary-strip"><div><el-icon><Connection /></el-icon><span><strong>{{ serverList.length }}</strong><small>{{ t('已配置监听器') }}</small></span></div><div><el-icon><VideoPlay /></el-icon><span><strong>{{ serverList.filter(s => s.IsRunning).length }}</strong><small>{{ t('运行监听器') }}</small></span></div><div><el-icon><Lock /></el-icon><span><strong>{{ serverList.filter(s => s.SslCertId).length }}</strong><small>{{ t('TLS 加密监听器') }}</small></span></div></div><el-tabs v-model="activeTab" class="workspace-tabs">
      <el-tab-pane :label="t('服务器列表')" name="servers">
        <el-card>
          <template #header>
            <div class="card-header">
              <span>{{ t('MQTT 服务器管理') }}</span>
              <el-button v-permission="'MqttServerManagerService.AddMqttServer'" type="primary" @click="handleAdd">
                <el-icon><Plus /></el-icon> {{ t('添加服务器') }} </el-button>
            </div>
          </template>
          <el-alert type="info" :closable="false" style="margin-bottom: 16px"
            :title="t('所有 MQTT 服务器监听共享同一个 Broker，消息在不同协议之间互通。')" />
          <CollectionToolbar v-model="query" :title="t('服务器列表')" :total="serverList.length" icon="Connection" /><el-table :data="filteredServers" v-loading="loading">
            <el-table-column prop="Id" label="ID" width="140" show-overflow-tooltip><template #default="{ row }"><span class="mono">{{ row.Id }}</span></template></el-table-column>
            <el-table-column :label="t('类型')" width="120" align="center">
              <template #default="{ row }">
                <el-tag :type="isTcpType(row.Type) ? 'primary' : 'warning'">
                  {{ isTcpType(row.Type) ? 'TCP' : 'WebSocket' }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column :label="t('监听地址')" min-width="180">
              <template #default="{ row }">
                <div class="entity-cell"><span class="entity-avatar"><el-icon><Connection v-if="isTcpType(row.Type)" /><Link v-else /></el-icon></span><span class="mono">{{ row.Host }}:{{ row.Port }}</span></div>
                <span v-if="!isTcpType(row.Type)" class="path-text">{{ row.Path }}</span>
              </template>
            </el-table-column>
            <el-table-column :label="t('状态')" width="100" align="center">
              <template #default="{ row }">
                <el-tag :type="row.IsRunning ? 'success' : 'info'">
                  {{ row.IsRunning ? t('运行中') : t('已停止') }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column :label="t('SSL 证书')" width="150" show-overflow-tooltip>
              <template #default="{ row }">
                <el-tag v-if="row.SslCertName" type="success" size="small">
                  <el-icon><Lock /></el-icon> {{ row.SslCertName }}
                </el-tag>
                <el-text v-else type="info" size="small">{{ t('无') }}</el-text>
              </template>
            </el-table-column>
            <el-table-column prop="CreatedAt" :label="t('创建时间')" width="180" show-overflow-tooltip>
              <template #default="{ row }">{{ formatDate(row.CreatedAt) }}</template>
            </el-table-column>
            <el-table-column :label="t('操作')" width="200" fixed="right" align="center">
              <template #default="{ row }">
                <el-button v-permission="'MqttServerManagerService.StartMqttServer'" v-if="!row.IsRunning" link type="success" @click="handleStart(row)">{{ t('启动') }}</el-button>
                <el-button v-permission="'MqttServerManagerService.StopMqttServer'" v-if="row.IsRunning" link type="warning" @click="handleStop(row)">{{ t('停止') }}</el-button>
                <el-button v-permission="'MqttServerManagerService.RemoveMqttServer'" link type="danger" @click="handleRemove(row)">{{ t('移除') }}</el-button>
              </template>
            </el-table-column>
          </el-table>
        </el-card>
      </el-tab-pane>
      <el-tab-pane :label="t('连接授权')" name="auth" v-if="user.hasPermission('MqttServerManagerService.GetMqttAuthUsers')">
        <el-card>
          <template #header>
            <div class="card-header">
              <span>{{ t('连接授权用户管理') }}</span>
              <el-button v-permission="'MqttServerManagerService.AddMqttAuthUser'" type="primary" @click="handleAddAuthUser">
                <el-icon><Plus /></el-icon> {{ t('添加用户') }} </el-button>
            </div>
          </template>
          <el-alert type="info" :closable="false" style="margin-bottom: 16px"
            :title="t('在「MQTT访问控制」页设置是否验证账号密码。此处维护允许连接的用户；修改认证用户会断开现有连接。')" />
          <el-table :data="authUserList" v-loading="authLoading">
            <el-table-column prop="UserName" :label="t('用户名')" min-width="160" />
            <el-table-column prop="Remark" :label="t('备注')" min-width="200" show-overflow-tooltip>
              <template #default="{ row }">{{ row.Remark || '-' }}</template>
            </el-table-column>
            <el-table-column prop="CreatedAt" :label="t('创建时间')" width="180">
              <template #default="{ row }">{{ formatDate(row.CreatedAt) }}</template>
            </el-table-column>
            <el-table-column :label="t('操作')" width="160" fixed="right" align="center">
              <template #default="{ row }">
                <el-button v-permission="'MqttServerManagerService.UpdateMqttAuthUser'" link type="primary" @click="handleEditAuthUser(row)">{{ t('编辑') }}</el-button>
                <el-button v-permission="'MqttServerManagerService.DeleteMqttAuthUser'" link type="danger" @click="handleDeleteAuthUser(row)">{{ t('删除') }}</el-button>
              </template>
            </el-table-column>
          </el-table>
        </el-card>
      </el-tab-pane>
      <el-tab-pane :label="t('Broker 设置')" name="broker" v-if="user.hasPermission('MqttServerManagerService.GetMqttBrokerSettings')">
        <el-card>
          <template #header>
            <div class="card-header">
              <span>{{ t('MQTT Broker 参数设置') }}</span>
              <el-button v-permission="'MqttServerManagerService.SetMqttBrokerSettings'" type="primary" @click="handleSaveBrokerSettings" :loading="brokerSaving">{{ t('保存设置') }}</el-button>
            </div>
          </template>
          <el-alert type="info" :closable="false" style="margin-bottom: 16px"
            :title="t('修改后立即对新会话和现有 Broker 生效，无需重启服务器。')" />
          <el-form :model="brokerForm" label-width="180px" v-loading="brokerLoading" style="max-width: 600px">
            <el-form-item :label="t('消息队列容量')">
              <el-input-number v-model="brokerForm.MessageCapacity" :min="0" style="width: 200px" />
              <span class="form-hint">{{ t('每个离线会话最多缓存的消息数，0 表示不限制') }}</span>
            </el-form-item>
            <el-form-item :label="t('消息过期时长（秒）')">
              <el-input-number v-model="brokerForm.MessageExpirySeconds" :min="0" style="width: 200px" />
              <span class="form-hint">{{ t('队列中消息的最大存活时间，0 表示不过期') }}</span>
            </el-form-item>
            <el-form-item :label="t('会话过期时长（秒）')">
              <el-input-number v-model="brokerForm.SessionExpirySeconds" :min="0" style="width: 200px" />
              <span class="form-hint">{{ t('离线会话的保留时长，0 表示不自动清除') }}</span>
            </el-form-item>
          </el-form>
        </el-card>
      </el-tab-pane>
      <el-tab-pane :label="t('丢弃统计')" name="discard" v-if="user.hasPermission('MqttServerManagerService.GetMqttDiscardedStats')">
        <el-card>
          <template #header>
            <div class="card-header">
              <span>{{ t('消息丢弃统计') }}</span>
              <div>
                <el-button @click="loadDiscardedStats" :loading="discardLoading">{{ t('刷新') }}</el-button>
                <el-button v-permission="'MqttServerManagerService.ResetMqttDiscardedStats'" type="danger" @click="handleResetDiscardedStats" :loading="discardResetting" style="margin-left: 8px">{{ t('重置统计') }}</el-button>
              </div>
            </div>
          </template>
          <el-alert type="info" :closable="false" style="margin-bottom: 16px"
            :title="t('统计自上次重置以来被丢弃的消息数量及原因。')" />
          <el-descriptions :column="2" border v-loading="discardLoading">
            <el-descriptions-item :label="t('总丢弃数')">
              <el-text type="danger" size="large">{{ discardStats.Total }}</el-text>
            </el-descriptions-item>
            <el-descriptions-item :label="t('上次重置时间')">
              {{ formatDate(discardStats.LastResetAt) }}
            </el-descriptions-item>
            <el-descriptions-item :label="t('队列已满')">{{ discardStats.QueueFull }}</el-descriptions-item>
            <el-descriptions-item :label="t('消息过期')">{{ discardStats.Expired }}</el-descriptions-item>
            <el-descriptions-item :label="t('发送失败')">{{ discardStats.PublishFailed }}</el-descriptions-item>
          </el-descriptions>
        </el-card>
      </el-tab-pane>
      <el-tab-pane :label="t('SSL 证书')" name="ssl" v-if="user.hasPermission('MqttServerManagerService.GetMqttSslCerts')">
        <el-card>
          <template #header>
            <div class="card-header">
              <span>{{ t('SSL 证书管理') }}</span>
              <el-button v-permission="'MqttServerManagerService.AddMqttSslCert'" type="primary" @click="handleAddCert">
                <el-icon><Plus /></el-icon> {{ t('上传证书') }} </el-button>
            </div>
          </template>
          <el-alert type="info" :closable="false" style="margin-bottom: 16px"
            :title="t('上传 SSL/TLS 证书后，可在添加服务器时选择使用，实现加密连接。')" />
          <el-table :data="sslCertList" v-loading="sslLoading">
            <el-table-column prop="Name" :label="t('名称')" min-width="150" />
            <el-table-column :label="t('类型')" width="80" align="center">
              <template #default="{ row }">
                <el-tag :type="row.Type === 0 ? 'primary' : 'success'" size="small">
                  {{ row.Type === 0 ? 'PFX' : 'PEM' }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="Subject" :label="t('主题 (Subject)')" min-width="200" show-overflow-tooltip />
            <el-table-column prop="Thumbprint" :label="t('指纹')" min-width="180" show-overflow-tooltip />
            <el-table-column :label="t('有效期')" width="240">
              <template #default="{ row }">
                <span>{{ formatDate(row.NotBefore) }} ~ {{ formatDate(row.NotAfter) }}</span>
              </template>
            </el-table-column>
            <el-table-column prop="CreatedAt" :label="t('上传时间')" width="180">
              <template #default="{ row }">{{ formatDate(row.CreatedAt) }}</template>
            </el-table-column>
            <el-table-column :label="t('操作')" width="100" fixed="right" align="center">
              <template #default="{ row }">
                <el-button v-permission="'MqttServerManagerService.DeleteMqttSslCert'" link type="danger" @click="handleDeleteCert(row)">{{ t('删除') }}</el-button>
              </template>
            </el-table-column>
          </el-table>
        </el-card>
      </el-tab-pane>
    </el-tabs>

    <el-dialog v-model="dialogVisible" :title="t('添加 MQTT 服务器')" width="500px">
      <el-form :model="form" :rules="formRules" ref="formRef" label-width="110px">
        <el-form-item :label="t('服务器类型')" prop="Type">
          <el-radio-group v-model="form.Type">
            <el-radio :value="0">TCP</el-radio>
            <el-radio :value="1">WebSocket</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item :label="t('监听地址')" prop="Host">
          <el-input v-model="form.Host" placeholder="0.0.0.0" />
        </el-form-item>
        <el-form-item :label="t('监听端口')" prop="Port">
          <el-input-number v-model="form.Port" :min="1" :max="65535" style="width: 100%" />
        </el-form-item>
        <el-form-item v-if="form.Type === 1" :label="t('WebSocket路径')" prop="Path">
          <el-input v-model="form.Path" placeholder="/mqtt" />
        </el-form-item>
        <el-form-item :label="t('SSL 证书')" prop="SslCertId">
          <el-select v-model="form.SslCertId" :placeholder="t('不使用 SSL')" clearable style="width: 100%">
            <el-option v-for="cert in sslCertList" :key="cert.Id" :label="cert.Name" :value="cert.Id">
              <span>{{ cert.Name }}</span>
              <span style="float: right; color: #909399; font-size: 12px; margin-left: 8px;">
                {{ cert.Type === 0 ? 'PFX' : 'PEM' }}
              </span>
            </el-option>
          </el-select>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">{{ t('取消') }}</el-button>
        <el-button type="primary" @click="handleSubmit" :loading="submitting">{{ t('确定') }}</el-button>
      </template>
    </el-dialog>

    <el-dialog v-model="certDialogVisible" :title="t('上传 SSL 证书')" width="560px">
      <el-form :model="certForm" :rules="certFormRules" ref="certFormRef" label-width="110px">
        <el-form-item :label="t('证书名称')" prop="Name">
          <el-input v-model="certForm.Name" :placeholder="t('请输入证书名称')" />
        </el-form-item>
        <el-form-item :label="t('证书类型')" prop="Type">
          <el-radio-group v-model="certForm.Type">
            <el-radio :value="0">PFX / P12</el-radio>
            <el-radio :value="1">PEM</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item v-if="certForm.Type === 0" :label="t('PFX 文件')" prop="CertData">
          <el-upload :auto-upload="false" :show-file-list="false" accept=".pfx,.p12"
            :on-change="handlePfxFileChange">
            <el-button>{{ t('选择 PFX 文件') }}</el-button>
            <span v-if="certForm.CertData" style="margin-left: 8px; color: #67c23a">{{ t('已选择文件') }}</span>
          </el-upload>
        </el-form-item>
        <el-form-item v-if="certForm.Type === 0" :label="t('证书密码')" prop="Password">
          <el-input v-model="certForm.Password" :placeholder="t('无密码可留空')" type="password" show-password />
        </el-form-item>
        <el-form-item v-if="certForm.Type === 1" :label="t('证书文件 (.crt)')" prop="CertData">
          <el-upload :auto-upload="false" :show-file-list="false" accept=".crt,.pem,.cer"
            :on-change="handlePemCertFileChange">
            <el-button>{{ t('选择证书文件') }}</el-button>
            <span v-if="certForm.CertData" style="margin-left: 8px; color: #67c23a">{{ t('已选择文件') }}</span>
          </el-upload>
        </el-form-item>
        <el-form-item v-if="certForm.Type === 1" :label="t('私钥文件 (.key)')" prop="KeyData">
          <el-upload :auto-upload="false" :show-file-list="false" accept=".key,.pem"
            :on-change="handlePemKeyFileChange">
            <el-button>{{ t('选择私钥文件') }}</el-button>
            <span v-if="certForm.KeyData" style="margin-left: 8px; color: #67c23a">{{ t('已选择文件') }}</span>
          </el-upload>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="certDialogVisible = false">{{ t('取消') }}</el-button>
        <el-button type="primary" @click="handleCertSubmit" :loading="certSubmitting">{{ t('上传') }}</el-button>
      </template>
    </el-dialog>

    <el-dialog v-model="authDialogVisible" :title="t(authDialogTitle)" width="460px">
      <el-form :model="authForm" :rules="authFormRules" ref="authFormRef" label-width="100px">
        <el-form-item :label="t('用户名')" prop="UserName">
          <el-input v-model="authForm.UserName" :placeholder="t('请输入用户名')" :disabled="authEditMode" />
        </el-form-item>
        <el-form-item :label="t('密码')" prop="Password">
          <el-input v-model="authForm.Password" :placeholder="authEditMode ? t('留空保持原密码') : t('请输入密码')" type="password" show-password />
        </el-form-item>
        <el-form-item :label="t('备注')" prop="Remark">
          <el-input v-model="authForm.Remark" :placeholder="t('可选备注信息')" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="authDialogVisible = false">{{ t('取消') }}</el-button>
        <el-button type="primary" @click="handleAuthSubmit" :loading="authSubmitting">{{ t('确定') }}</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { t, useLocale } from '@/locales'
const { locale } = useLocale()
import { computed, ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { useUserStore } from '@/stores/user'
import CollectionToolbar from '@/components/CollectionToolbar.vue'
import { mqttServerApi } from '@/api/index'

const user = useUserStore()
const query = ref('')
const filteredServers = computed(() => serverList.value.filter(s => [s.Id, s.Host, String(s.Port), s.Type].some(v => String(v).toLowerCase().includes(query.value.toLowerCase()))))
const activeTab = ref('servers')
const loading = ref(false)
const submitting = ref(false)
const serverList = ref([])
const dialogVisible = ref(false)
const formRef = ref(null)

const authLoading = ref(false)
const authUserList = ref([])
const authDialogVisible = ref(false)
const authDialogTitle = ref('添加授权用户')
const authEditMode = ref(false)
const authSubmitting = ref(false)
const authFormRef = ref(null)

const brokerLoading = ref(false)
const brokerSaving = ref(false)
const brokerForm = reactive({ MessageCapacity: 0, MessageExpirySeconds: 0, SessionExpirySeconds: 0 })

const discardLoading = ref(false)
const discardResetting = ref(false)
const discardStats = reactive({ Total: 0, QueueFull: 0, Expired: 0, PublishFailed: 0, LastResetAt: '' })

const sslLoading = ref(false)
const sslCertList = ref([])
const certDialogVisible = ref(false)
const certSubmitting = ref(false)
const certFormRef = ref(null)
const certForm = reactive({ Name: '', Type: 0, CertData: '', KeyData: '', Password: '' })

const form = reactive({ Type: 0, Host: '0.0.0.0', Port: 1883, Path: '/mqtt', SslCertId: null })
const authForm = reactive({ UserName: '', Password: '', Remark: '' })

const formRules = computed(() => ({
  Port: [{ required: true, message: t("请输入端口号"), trigger: 'blur' }],
  Path: [{
    validator: (_, value, callback) => {
      if (form.Type === 1 && !value) callback(new Error(t("请输入 WebSocket 路径")))
      else callback()
    },
    trigger: 'blur'
  }]
}))

const authFormRules = computed(() => ({
  UserName: [{ required: true, message: t("请输入用户名"), trigger: 'blur' }],
  Password: [{ validator: (_, value, callback) => { if (!authEditMode.value && !value) callback(new Error(t("请输入密码"))); else callback() }, trigger: 'blur' }]
}))

const certFormRules = computed(() => ({
  Name: [{ required: true, message: t("请输入证书名称"), trigger: 'blur' }],
  CertData: [{ required: true, message: t("请选择证书文件"), trigger: 'change' }],
  KeyData: [{
    validator: (_, value, callback) => {
      if (certForm.Type === 1 && !value) callback(new Error(t("请选择私鑰文件")))
      else callback()
    },
    trigger: 'change'
  }]
}))

onMounted(() => { loadData(); if (user.hasPermission('MqttServerManagerService.GetMqttAuthUsers')) loadAuthUsers(); if (user.hasPermission('MqttServerManagerService.GetMqttBrokerSettings')) loadBrokerSettings(); if (user.hasPermission('MqttServerManagerService.GetMqttDiscardedStats')) loadDiscardedStats(); if (user.hasPermission('MqttServerManagerService.GetMqttSslCerts')) loadSslCerts() })

const loadData = async () => {
  loading.value = true
  try {
    const res = await mqttServerApi.getList()
    serverList.value = res.Servers || []
  } catch (e) {
    ElMessage.error(e.message || t("加载失败"))
  } finally {
    loading.value = false
  }
}

const loadAuthUsers = async () => {
  authLoading.value = true
  try {
    const res = await mqttServerApi.getAuthUsers()
    authUserList.value = res.Users || []
  } catch (e) {
    ElMessage.error(e.message || t("加载授权用户失败"))
  } finally {
    authLoading.value = false
  }
}

const handleAdd = () => {
  Object.assign(form, { Type: 0, Host: '0.0.0.0', Port: 1883, Path: '/mqtt', SslCertId: null })
  dialogVisible.value = true
}

const handleSubmit = async () => {
  if (!formRef.value) return
  await formRef.value.validate(async valid => {
    if (!valid) return
    submitting.value = true
    try {
      await mqttServerApi.addServer({
        Type: form.Type === 0 ? 'Tcp' : 'WebSocket',
        Host: form.Host, Port: form.Port,
        Path: form.Type === 1 ? form.Path : undefined,
        SslCertId: form.SslCertId || undefined
      })
      ElMessage.success(t("添加成功"))
      dialogVisible.value = false
      loadData()
    } catch (e) {
      ElMessage.error(e.message || t("添加失败"))
    } finally {
      submitting.value = false
    }
  })
}

const handleStart = async row => {
  try {
    await mqttServerApi.startServer({ ServerId: row.Id })
    ElMessage.success(t("启动成功"))
    loadData()
  } catch (e) { ElMessage.error(e.message || t("启动失败")) }
}

const handleStop = async row => {
  try {
    await ElMessageBox.confirm(t("确定要停止该 MQTT 服务器吗？"), t("提示"), {confirmButtonText: t('确定'), cancelButtonText: t('取消'),  type: 'warning' })
    await mqttServerApi.stopServer({ ServerId: row.Id })
    ElMessage.success(t("停止成功"))
    loadData()
  } catch (e) { if (e !== 'cancel') ElMessage.error(e.message || t("停止失败")) }
}

const handleRemove = async row => {
  try {
    await ElMessageBox.confirm(t("确定要移除该 MQTT 服务器配置吗？"), t("提示"), {confirmButtonText: t('确定'), cancelButtonText: t('取消'),  type: 'warning' })
    await mqttServerApi.removeServer({ ServerId: row.Id })
    ElMessage.success(t("移除成功"))
    loadData()
  } catch (e) { if (e !== 'cancel') ElMessage.error(e.message || t("移除失败")) }
}

const handleAddAuthUser = () => {
  authEditMode.value = false
  authDialogTitle.value = '添加授权用户'
  Object.assign(authForm, { UserName: '', Password: '', Remark: '' })
  authDialogVisible.value = true
}

const handleEditAuthUser = row => {
  authEditMode.value = true
  authDialogTitle.value = '编辑授权用户'
  Object.assign(authForm, { UserName: row.UserName, Password: '', Remark: row.Remark || '' })
  authDialogVisible.value = true
}

const handleAuthSubmit = async () => {
  if (!authFormRef.value) return
  await authFormRef.value.validate(async valid => {
    if (!valid) return
    authSubmitting.value = true
    try {
      if (authEditMode.value) {
        await mqttServerApi.updateAuthUser({ UserName: authForm.UserName, Password: authForm.Password, Remark: authForm.Remark || undefined })
        ElMessage.success(t("更新成功"))
      } else {
        await mqttServerApi.addAuthUser({ UserName: authForm.UserName, Password: authForm.Password, Remark: authForm.Remark || undefined })
        ElMessage.success(t("添加成功"))
      }
      authDialogVisible.value = false
      loadAuthUsers()
    } catch (e) {
      ElMessage.error(e.message || t("操作失败"))
    } finally {
      authSubmitting.value = false
    }
  })
}

const handleDeleteAuthUser = async row => {
  try {
    await ElMessageBox.confirm(t("确定要删除授权用户 \"{p0}\" 吗？", { p0: row.UserName }), t("提示"), {confirmButtonText: t('确定'), cancelButtonText: t('取消'),  type: 'warning' })
    await mqttServerApi.deleteAuthUser({ UserName: row.UserName })
    ElMessage.success(t("删除成功"))
    loadAuthUsers()
  } catch (e) { if (e !== 'cancel') ElMessage.error(e.message || t("删除失败")) }
}

const loadBrokerSettings = async () => {
  brokerLoading.value = true
  try {
    const res = await mqttServerApi.getBrokerSettings()
    Object.assign(brokerForm, {
      MessageCapacity: res.MessageCapacity ?? 0,
      MessageExpirySeconds: res.MessageExpirySeconds ?? 0,
      SessionExpirySeconds: res.SessionExpirySeconds ?? 0
    })
  } catch (e) {
    ElMessage.error(e.message || t("加载 Broker 设置失败"))
  } finally {
    brokerLoading.value = false
  }
}

const handleSaveBrokerSettings = async () => {
  brokerSaving.value = true
  try {
    await mqttServerApi.setBrokerSettings({
      MessageCapacity: brokerForm.MessageCapacity,
      MessageExpirySeconds: brokerForm.MessageExpirySeconds,
      SessionExpirySeconds: brokerForm.SessionExpirySeconds
    })
    ElMessage.success(t("保存成功"))
  } catch (e) {
    ElMessage.error(e.message || t("保存失败"))
  } finally {
    brokerSaving.value = false
  }
}

const loadDiscardedStats = async () => {
  discardLoading.value = true
  try {
    const res = await mqttServerApi.getDiscardedStats()
    Object.assign(discardStats, {
      Total: res.Total ?? 0,
      QueueFull: res.QueueFull ?? 0,
      Expired: res.Expired ?? 0,
      PublishFailed: res.PublishFailed ?? 0,
      LastResetAt: res.LastResetAt ?? ''
    })
  } catch (e) {
    ElMessage.error(e.message || t("加载丢弃统计失败"))
  } finally {
    discardLoading.value = false
  }
}

const handleResetDiscardedStats = async () => {
  try {
    await ElMessageBox.confirm(t("确定要重置消息丢弃统计吗？"), t("提示"), {confirmButtonText: t('确定'), cancelButtonText: t('取消'),  type: 'warning' })
    discardResetting.value = true
    await mqttServerApi.resetDiscardedStats()
    ElMessage.success(t("重置成功"))
    await loadDiscardedStats()
  } catch (e) {
    if (e !== 'cancel') ElMessage.error(e.message || t("重置失败"))
  } finally {
    discardResetting.value = false
  }
}

const formatDate = dateStr => {
  if (!dateStr) return '-'
  return new Date(dateStr).toLocaleString(locale.value)
}

const isTcpType = type => type === 0 || type === 'Tcp'

const loadSslCerts = async () => {
  sslLoading.value = true
  try {
    const res = await mqttServerApi.getSslCerts()
    sslCertList.value = res.Certs || []
  } catch (e) {
    ElMessage.error(e.message || t("加载证书失败"))
  } finally {
    sslLoading.value = false
  }
}

const handleAddCert = () => {
  Object.assign(certForm, { Name: '', Type: 0, CertData: '', KeyData: '', Password: '' })
  certDialogVisible.value = true
}

const readFileAsBase64 = file => new Promise((resolve, reject) => {
  const reader = new FileReader()
  reader.onload = e => resolve(e.target.result.split(',')[1])
  reader.onerror = reject
  reader.readAsDataURL(file)
})

const readFileAsText = file => new Promise((resolve, reject) => {
  const reader = new FileReader()
  reader.onload = e => resolve(e.target.result)
  reader.onerror = reject
  reader.readAsText(file)
})

const handlePfxFileChange = async ({ raw }) => {
  if (!raw) return
  certForm.CertData = await readFileAsBase64(raw)
}

const handlePemCertFileChange = async ({ raw }) => {
  if (!raw) return
  certForm.CertData = await readFileAsText(raw)
}

const handlePemKeyFileChange = async ({ raw }) => {
  if (!raw) return
  certForm.KeyData = await readFileAsText(raw)
}

const handleCertSubmit = async () => {
  if (!certFormRef.value) return
  await certFormRef.value.validate(async valid => {
    if (!valid) return
    certSubmitting.value = true
    try {
      await mqttServerApi.addSslCert({
        Name: certForm.Name,
        Type: certForm.Type,
        CertData: certForm.CertData,
        KeyData: certForm.Type === 1 ? certForm.KeyData : undefined,
        Password: certForm.Type === 0 && certForm.Password ? certForm.Password : undefined
      })
      ElMessage.success(t("证书上传成功"))
      certDialogVisible.value = false
      loadSslCerts()
    } catch (e) {
      ElMessage.error(e.message || t("上传失败"))
    } finally {
      certSubmitting.value = false
    }
  })
}

const handleDeleteCert = async row => {
  try {
    await ElMessageBox.confirm(t("确定要删除证书 \"{p0}\" 吗？", { p0: row.Name }), t("提示"), {confirmButtonText: t('确定'), cancelButtonText: t('取消'),  type: 'warning' })
    await mqttServerApi.deleteSslCert({ CertId: row.Id })
    ElMessage.success(t("删除成功"))
    loadSslCerts()
  } catch (e) { if (e !== 'cancel') ElMessage.error(e.message || t("删除失败")) }
}
</script>

<style scoped>
.mqtt-server-manager { width: 100%; }
.card-header { display: flex; justify-content: space-between; align-items: center; }
.path-text { margin-left: 4px; color: #909399; font-size: 12px; }
.form-hint { margin-left: 8px; color: #909399; font-size: 12px; }
</style>
