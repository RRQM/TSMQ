<template>
  <div class="mqtt-client-list">
    <el-card>
      <template #header>
        <div class="card-header">
          <span>{{ t('MQTT 客户端连接') }}</span>
          <div class="header-actions">
            <el-button v-permission="'MqttClientManagerService.KickMqttClients'"
              type="danger"
              :disabled="selectedClients.length === 0"
              @click="handleKick"
            >
              <el-icon><Remove /></el-icon> {{ t('踢除所选 (') }}{{ selectedClients.length }})
            </el-button>
            <el-button @click="loadData" :loading="loading">
              <el-icon><Refresh /></el-icon> {{ t('刷新') }} </el-button>
          </div>
        </div>
      </template>

      <CollectionToolbar v-model="query" :title="t('在线连接')" :total="clientList.length" icon="Connection" />
      <el-table
        :data="filteredRows"
        border
        stripe
        v-loading="loading"
        @selection-change="handleSelectionChange"
      >
        <el-table-column type="selection" width="50" align="center" />
        <el-table-column prop="ClientId" :label="t('客户端ID')" min-width="160" show-overflow-tooltip />
        <el-table-column prop="UserName" :label="t('用户名')" width="130" show-overflow-tooltip>
          <template #default="{ row }">
            {{ row.UserName || '-' }}
          </template>
        </el-table-column>
        <el-table-column :label="t('状态')" width="90" align="center">
          <template #default="{ row }">
            <el-tag type="success">{{ row.Status }}</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="IpAddress" :label="t('IP地址')" width="170" show-overflow-tooltip />
        <el-table-column :label="t('心跳(秒)')" width="100" align="center">
          <template #default="{ row }">
            {{ row.KeepAlive }}
          </template>
        </el-table-column>
        <el-table-column label="Clean Start" width="110" align="center">
          <template #default="{ row }">
            <el-tag :type="row.CleanStart ? 'primary' : 'info'">
              {{ row.CleanStart ? t('是') : t('否') }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column :label="t('会话过期间隔(秒)')" width="150" align="center">
          <template #default="{ row }">
            {{ row.SessionExpiryInterval === 0 ? t('立即过期') : row.SessionExpiryInterval }}
          </template>
        </el-table-column>
        <el-table-column :label="t('连接时间')" width="180" show-overflow-tooltip>
          <template #default="{ row }">
            {{ formatDate(row.ConnectedAt) }}
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
import { computed, ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { mqttClientApi } from '@/api/index'

const loading = ref(false)
const clientList = ref([])
const selectedClients = ref([])

onMounted(() => {
  loadData()
})

const loadData = async () => {
  loading.value = true
  try {
    const res = await mqttClientApi.getClientList()
    clientList.value = res.Clients || []
  } catch (error) {
    ElMessage.error(error.message || t("加载失败"))
  } finally {
    loading.value = false
  }
}

const handleSelectionChange = rows => {
  selectedClients.value = rows
}

const handleKick = async () => {
  if (selectedClients.value.length === 0) return

  const clientIds = selectedClients.value.map(c => c.ClientId)
  try {
    await ElMessageBox.confirm(
      t("确定要踢除选中的 {p0} 个客户端吗？", { p0: clientIds.length }),
      t("踢除确认"),
      {confirmButtonText: t('确定'), cancelButtonText: t('取消'),  type: 'warning' }
    )
    await mqttClientApi.kickClients({ ClientIds: clientIds })
    ElMessage.success(t("踢除成功"))
    await loadData()
  } catch (error) {
    if (error !== 'cancel') {
      ElMessage.error(error.message || t("踢除失败"))
    }
  }
}

const formatDate = dateStr => {
  if (!dateStr) return '-'
  const d = new Date(dateStr)
  return d.toLocaleString(locale.value)
}
const query = ref('')
const filteredRows = computed(() => clientList.value.filter(row => [row.ClientId,row.UserName,row.IpAddress].some(value => String(value || '').toLowerCase().includes(query.value.toLowerCase()))))
</script>

<style scoped>
.mqtt-client-list {
  width: 100%;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.header-actions {
  display: flex;
  gap: 8px;
}
</style>
