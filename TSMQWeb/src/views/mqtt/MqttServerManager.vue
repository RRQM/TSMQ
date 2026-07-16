<template>
  <div class="mqtt-server-manager">
    <el-tabs v-model="activeTab">
      <el-tab-pane label="服务器列表" name="servers">
        <el-card>
          <template #header>
            <div class="card-header">
              <span>MQTT 服务器管理</span>
              <el-button type="primary" @click="handleAdd">
                <el-icon><Plus /></el-icon>
                添加服务器
              </el-button>
            </div>
          </template>
          <el-alert type="info" :closable="false" style="margin-bottom: 16px"
            title="所有 MQTT 服务器监听共享同一个 Broker，消息在不同协议之间互通。" />
          <el-table :data="serverList" border stripe v-loading="loading">
            <el-table-column prop="Id" label="ID" width="300" show-overflow-tooltip />
            <el-table-column label="类型" width="120" align="center">
              <template #default="{ row }">
                <el-tag :type="isTcpType(row.Type) ? 'primary' : 'warning'">
                  {{ isTcpType(row.Type) ? 'TCP' : 'WebSocket' }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column label="监听地址" min-width="180">
              <template #default="{ row }">
                <span>{{ row.Host }}:{{ row.Port }}</span>
                <span v-if="!isTcpType(row.Type)" class="path-text">{{ row.Path }}</span>
              </template>
            </el-table-column>
            <el-table-column label="状态" width="100" align="center">
              <template #default="{ row }">
                <el-tag :type="row.IsRunning ? 'success' : 'info'">
                  {{ row.IsRunning ? '运行中' : '已停止' }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column label="SSL 证书" width="150" show-overflow-tooltip>
              <template #default="{ row }">
                <el-tag v-if="row.SslCertName" type="success" size="small">
                  <el-icon><Lock /></el-icon> {{ row.SslCertName }}
                </el-tag>
                <el-text v-else type="info" size="small">无</el-text>
              </template>
            </el-table-column>
            <el-table-column prop="CreatedAt" label="创建时间" width="180" show-overflow-tooltip>
              <template #default="{ row }">{{ formatDate(row.CreatedAt) }}</template>
            </el-table-column>
            <el-table-column label="操作" width="200" fixed="right" align="center">
              <template #default="{ row }">
                <el-button v-if="!row.IsRunning" link type="success" @click="handleStart(row)">启动</el-button>
                <el-button v-if="row.IsRunning" link type="warning" @click="handleStop(row)">停止</el-button>
                <el-button link type="danger" @click="handleRemove(row)">移除</el-button>
              </template>
            </el-table-column>
          </el-table>
        </el-card>
      </el-tab-pane>
      <el-tab-pane label="连接授权" name="auth">
        <el-card>
          <template #header>
            <div class="card-header">
              <span>连接授权用户管理</span>
              <el-button type="primary" @click="handleAddAuthUser">
                <el-icon><Plus /></el-icon>
                添加用户
              </el-button>
            </div>
          </template>
          <el-alert type="info" :closable="false" style="margin-bottom: 16px"
            title="配置允许连接的用户名和密码。启用授权后，未配置的客户端将被拒绝连接。" />
          <el-table :data="authUserList" border stripe v-loading="authLoading">
            <el-table-column prop="UserName" label="用户名" min-width="160" />
            <el-table-column prop="Remark" label="备注" min-width="200" show-overflow-tooltip>
              <template #default="{ row }">{{ row.Remark || '-' }}</template>
            </el-table-column>
            <el-table-column prop="CreatedAt" label="创建时间" width="180">
              <template #default="{ row }">{{ formatDate(row.CreatedAt) }}</template>
            </el-table-column>
            <el-table-column label="操作" width="160" fixed="right" align="center">
              <template #default="{ row }">
                <el-button link type="primary" @click="handleEditAuthUser(row)">编辑</el-button>
                <el-button link type="danger" @click="handleDeleteAuthUser(row)">删除</el-button>
              </template>
            </el-table-column>
          </el-table>
        </el-card>
      </el-tab-pane>
      <el-tab-pane label="Broker 设置" name="broker">
        <el-card>
          <template #header>
            <div class="card-header">
              <span>MQTT Broker 参数设置</span>
              <el-button type="primary" @click="handleSaveBrokerSettings" :loading="brokerSaving">保存设置</el-button>
            </div>
          </template>
          <el-alert type="info" :closable="false" style="margin-bottom: 16px"
            title="修改后立即对新会话和现有 Broker 生效，无需重启服务器。" />
          <el-form :model="brokerForm" label-width="180px" v-loading="brokerLoading" style="max-width: 600px">
            <el-form-item label="消息队列容量">
              <el-input-number v-model="brokerForm.MessageCapacity" :min="0" style="width: 200px" />
              <span class="form-hint">每个离线会话最多缓存的消息数，0 表示不限制</span>
            </el-form-item>
            <el-form-item label="消息过期时长（秒）">
              <el-input-number v-model="brokerForm.MessageExpirySeconds" :min="0" style="width: 200px" />
              <span class="form-hint">队列中消息的最大存活时间，0 表示不过期</span>
            </el-form-item>
            <el-form-item label="会话过期时长（秒）">
              <el-input-number v-model="brokerForm.SessionExpirySeconds" :min="0" style="width: 200px" />
              <span class="form-hint">离线会话的保留时长，0 表示不自动清除</span>
            </el-form-item>
          </el-form>
        </el-card>
      </el-tab-pane>
      <el-tab-pane label="丢弃统计" name="discard">
        <el-card>
          <template #header>
            <div class="card-header">
              <span>消息丢弃统计</span>
              <div>
                <el-button @click="loadDiscardedStats" :loading="discardLoading">刷新</el-button>
                <el-button type="danger" @click="handleResetDiscardedStats" :loading="discardResetting" style="margin-left: 8px">重置统计</el-button>
              </div>
            </div>
          </template>
          <el-alert type="info" :closable="false" style="margin-bottom: 16px"
            title="统计自上次重置以来被丢弃的消息数量及原因。" />
          <el-descriptions :column="2" border v-loading="discardLoading">
            <el-descriptions-item label="总丢弃数">
              <el-text type="danger" size="large">{{ discardStats.Total }}</el-text>
            </el-descriptions-item>
            <el-descriptions-item label="上次重置时间">
              {{ formatDate(discardStats.LastResetAt) }}
            </el-descriptions-item>
            <el-descriptions-item label="队列已满">{{ discardStats.QueueFull }}</el-descriptions-item>
            <el-descriptions-item label="消息过期">{{ discardStats.Expired }}</el-descriptions-item>
            <el-descriptions-item label="发送失败">{{ discardStats.PublishFailed }}</el-descriptions-item>
          </el-descriptions>
        </el-card>
      </el-tab-pane>
      <el-tab-pane label="SSL 证书" name="ssl">
        <el-card>
          <template #header>
            <div class="card-header">
              <span>SSL 证书管理</span>
              <el-button type="primary" @click="handleAddCert">
                <el-icon><Plus /></el-icon>
                上传证书
              </el-button>
            </div>
          </template>
          <el-alert type="info" :closable="false" style="margin-bottom: 16px"
            title="上传 SSL/TLS 证书后，可在添加服务器时选择使用，实现加密连接。" />
          <el-table :data="sslCertList" border stripe v-loading="sslLoading">
            <el-table-column prop="Name" label="名称" min-width="150" />
            <el-table-column label="类型" width="80" align="center">
              <template #default="{ row }">
                <el-tag :type="row.Type === 0 ? 'primary' : 'success'" size="small">
                  {{ row.Type === 0 ? 'PFX' : 'PEM' }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="Subject" label="主题 (Subject)" min-width="200" show-overflow-tooltip />
            <el-table-column prop="Thumbprint" label="指纹" min-width="180" show-overflow-tooltip />
            <el-table-column label="有效期" width="240">
              <template #default="{ row }">
                <span>{{ formatDate(row.NotBefore) }} ~ {{ formatDate(row.NotAfter) }}</span>
              </template>
            </el-table-column>
            <el-table-column prop="CreatedAt" label="上传时间" width="180">
              <template #default="{ row }">{{ formatDate(row.CreatedAt) }}</template>
            </el-table-column>
            <el-table-column label="操作" width="100" fixed="right" align="center">
              <template #default="{ row }">
                <el-button link type="danger" @click="handleDeleteCert(row)">删除</el-button>
              </template>
            </el-table-column>
          </el-table>
        </el-card>
      </el-tab-pane>
    </el-tabs>

    <el-dialog v-model="dialogVisible" title="添加 MQTT 服务器" width="500px">
      <el-form :model="form" :rules="formRules" ref="formRef" label-width="110px">
        <el-form-item label="服务器类型" prop="Type">
          <el-radio-group v-model="form.Type">
            <el-radio :value="0">TCP</el-radio>
            <el-radio :value="1">WebSocket</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="监听地址" prop="Host">
          <el-input v-model="form.Host" placeholder="0.0.0.0" />
        </el-form-item>
        <el-form-item label="监听端口" prop="Port">
          <el-input-number v-model="form.Port" :min="1" :max="65535" style="width: 100%" />
        </el-form-item>
        <el-form-item v-if="form.Type === 1" label="WebSocket路径" prop="Path">
          <el-input v-model="form.Path" placeholder="/mqtt" />
        </el-form-item>
        <el-form-item label="SSL 证书" prop="SslCertId">
          <el-select v-model="form.SslCertId" placeholder="不使用 SSL" clearable style="width: 100%">
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
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSubmit" :loading="submitting">确定</el-button>
      </template>
    </el-dialog>

    <el-dialog v-model="certDialogVisible" title="上传 SSL 证书" width="560px">
      <el-form :model="certForm" :rules="certFormRules" ref="certFormRef" label-width="110px">
        <el-form-item label="证书名称" prop="Name">
          <el-input v-model="certForm.Name" placeholder="请输入证书名称" />
        </el-form-item>
        <el-form-item label="证书类型" prop="Type">
          <el-radio-group v-model="certForm.Type">
            <el-radio :value="0">PFX / P12</el-radio>
            <el-radio :value="1">PEM</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item v-if="certForm.Type === 0" label="PFX 文件" prop="CertData">
          <el-upload :auto-upload="false" :show-file-list="false" accept=".pfx,.p12"
            :on-change="handlePfxFileChange">
            <el-button>选择 PFX 文件</el-button>
            <span v-if="certForm.CertData" style="margin-left: 8px; color: #67c23a">已选择文件</span>
          </el-upload>
        </el-form-item>
        <el-form-item v-if="certForm.Type === 0" label="证书密码" prop="Password">
          <el-input v-model="certForm.Password" placeholder="无密码可留空" type="password" show-password />
        </el-form-item>
        <el-form-item v-if="certForm.Type === 1" label="证书文件 (.crt)" prop="CertData">
          <el-upload :auto-upload="false" :show-file-list="false" accept=".crt,.pem,.cer"
            :on-change="handlePemCertFileChange">
            <el-button>选择证书文件</el-button>
            <span v-if="certForm.CertData" style="margin-left: 8px; color: #67c23a">已选择文件</span>
          </el-upload>
        </el-form-item>
        <el-form-item v-if="certForm.Type === 1" label="私钥文件 (.key)" prop="KeyData">
          <el-upload :auto-upload="false" :show-file-list="false" accept=".key,.pem"
            :on-change="handlePemKeyFileChange">
            <el-button>选择私钥文件</el-button>
            <span v-if="certForm.KeyData" style="margin-left: 8px; color: #67c23a">已选择文件</span>
          </el-upload>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="certDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleCertSubmit" :loading="certSubmitting">上传</el-button>
      </template>
    </el-dialog>

    <el-dialog v-model="authDialogVisible" :title="authDialogTitle" width="460px">
      <el-form :model="authForm" :rules="authFormRules" ref="authFormRef" label-width="80px">
        <el-form-item label="用户名" prop="UserName">
          <el-input v-model="authForm.UserName" placeholder="请输入用户名" :disabled="authEditMode" />
        </el-form-item>
        <el-form-item label="密码" prop="Password">
          <el-input v-model="authForm.Password" placeholder="请输入密码" type="password" show-password />
        </el-form-item>
        <el-form-item label="备注" prop="Remark">
          <el-input v-model="authForm.Remark" placeholder="可选备注信息" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="authDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleAuthSubmit" :loading="authSubmitting">确定</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { mqttServerApi } from '@/api/index'

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

const formRules = {
  Port: [{ required: true, message: '请输入端口号', trigger: 'blur' }],
  Path: [{
    validator: (_, value, callback) => {
      if (form.Type === 1 && !value) callback(new Error('请输入 WebSocket 路径'))
      else callback()
    },
    trigger: 'blur'
  }]
}

const authFormRules = {
  UserName: [{ required: true, message: '请输入用户名', trigger: 'blur' }],
  Password: [{ required: true, message: '请输入密码', trigger: 'blur' }]
}

const certFormRules = {
  Name: [{ required: true, message: '请输入证书名称', trigger: 'blur' }],
  CertData: [{ required: true, message: '请选择证书文件', trigger: 'change' }],
  KeyData: [{
    validator: (_, value, callback) => {
      if (certForm.Type === 1 && !value) callback(new Error('请选择私鑰文件'))
      else callback()
    },
    trigger: 'change'
  }]
}

onMounted(() => { loadData(); loadAuthUsers(); loadBrokerSettings(); loadDiscardedStats(); loadSslCerts() })

const loadData = async () => {
  loading.value = true
  try {
    const res = await mqttServerApi.getList()
    serverList.value = res.Servers || []
  } catch (e) {
    ElMessage.error(e.message || '加载失败')
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
    ElMessage.error(e.message || '加载授权用户失败')
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
      ElMessage.success('添加成功')
      dialogVisible.value = false
      loadData()
    } catch (e) {
      ElMessage.error(e.message || '添加失败')
    } finally {
      submitting.value = false
    }
  })
}

const handleStart = async row => {
  try {
    await mqttServerApi.startServer({ ServerId: row.Id })
    ElMessage.success('启动成功')
    loadData()
  } catch (e) { ElMessage.error(e.message || '启动失败') }
}

const handleStop = async row => {
  try {
    await ElMessageBox.confirm('确定要停止该 MQTT 服务器吗？', '提示', { type: 'warning' })
    await mqttServerApi.stopServer({ ServerId: row.Id })
    ElMessage.success('停止成功')
    loadData()
  } catch (e) { if (e !== 'cancel') ElMessage.error(e.message || '停止失败') }
}

const handleRemove = async row => {
  try {
    await ElMessageBox.confirm('确定要移除该 MQTT 服务器配置吗？', '提示', { type: 'warning' })
    await mqttServerApi.removeServer({ ServerId: row.Id })
    ElMessage.success('移除成功')
    loadData()
  } catch (e) { if (e !== 'cancel') ElMessage.error(e.message || '移除失败') }
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
        ElMessage.success('更新成功')
      } else {
        await mqttServerApi.addAuthUser({ UserName: authForm.UserName, Password: authForm.Password, Remark: authForm.Remark || undefined })
        ElMessage.success('添加成功')
      }
      authDialogVisible.value = false
      loadAuthUsers()
    } catch (e) {
      ElMessage.error(e.message || '操作失败')
    } finally {
      authSubmitting.value = false
    }
  })
}

const handleDeleteAuthUser = async row => {
  try {
    await ElMessageBox.confirm(`确定要删除授权用户 "${row.UserName}" 吗？`, '提示', { type: 'warning' })
    await mqttServerApi.deleteAuthUser({ UserName: row.UserName })
    ElMessage.success('删除成功')
    loadAuthUsers()
  } catch (e) { if (e !== 'cancel') ElMessage.error(e.message || '删除失败') }
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
    ElMessage.error(e.message || '加载 Broker 设置失败')
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
    ElMessage.success('保存成功')
  } catch (e) {
    ElMessage.error(e.message || '保存失败')
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
    ElMessage.error(e.message || '加载丢弃统计失败')
  } finally {
    discardLoading.value = false
  }
}

const handleResetDiscardedStats = async () => {
  try {
    await ElMessageBox.confirm('确定要重置消息丢弃统计吗？', '提示', { type: 'warning' })
    discardResetting.value = true
    await mqttServerApi.resetDiscardedStats()
    ElMessage.success('重置成功')
    await loadDiscardedStats()
  } catch (e) {
    if (e !== 'cancel') ElMessage.error(e.message || '重置失败')
  } finally {
    discardResetting.value = false
  }
}

const formatDate = dateStr => {
  if (!dateStr) return '-'
  return new Date(dateStr).toLocaleString('zh-CN')
}

const isTcpType = type => type === 0 || type === 'Tcp'

const loadSslCerts = async () => {
  sslLoading.value = true
  try {
    const res = await mqttServerApi.getSslCerts()
    sslCertList.value = res.Certs || []
  } catch (e) {
    ElMessage.error(e.message || '加载证书失败')
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
      ElMessage.success('证书上传成功')
      certDialogVisible.value = false
      loadSslCerts()
    } catch (e) {
      ElMessage.error(e.message || '上传失败')
    } finally {
      certSubmitting.value = false
    }
  })
}

const handleDeleteCert = async row => {
  try {
    await ElMessageBox.confirm(`确定要删除证书 "${row.Name}" 吗？`, '提示', { type: 'warning' })
    await mqttServerApi.deleteSslCert({ CertId: row.Id })
    ElMessage.success('删除成功')
    loadSslCerts()
  } catch (e) { if (e !== 'cancel') ElMessage.error(e.message || '删除失败') }
}
</script>

<style scoped>
.mqtt-server-manager { width: 100%; }
.card-header { display: flex; justify-content: space-between; align-items: center; }
.path-text { margin-left: 4px; color: #909399; font-size: 12px; }
.form-hint { margin-left: 8px; color: #909399; font-size: 12px; }
</style>