<template>
  <div class="rpc-role-manager">
    <el-card>
      <template #header>
        <div class="card-header">
          <span>RPC 角色授权管理</span>
          <el-button type="primary" :loading="loading" @click="loadData">
            <el-icon><Refresh /></el-icon>
            刷新
          </el-button>
        </div>
      </template>

      <el-table
        :data="groupedData"
        border
        stripe
        row-key="ServiceName"
        v-loading="loading"
      >
        <el-table-column type="expand">
          <template #default="{ row }">
            <div class="expand-content">
              <el-table :data="row.methods" border size="small">
                <el-table-column prop="MethodName" label="方法名称" width="220" />
                <el-table-column prop="MethodTargetKey" label="目标键" width="260" />
                <el-table-column label="已授权角色">
                  <template #default="{ row: method }">
                    <template v-if="getRoleCodes(method.MethodTargetKey).length > 0">
                      <el-tag
                        v-for="code in getRoleCodes(method.MethodTargetKey)"
                        :key="code"
                        type="success"
                        size="small"
                        style="margin-right: 4px"
                      >{{ code }}</el-tag>
                    </template>
                    <el-text v-else type="info" size="small">未设置（继承服务级）</el-text>
                  </template>
                </el-table-column>
                <el-table-column label="操作" width="160" fixed="right">
                  <template #default="{ row: method }">
                    <el-button link type="primary" @click="handleSetRoles(method.MethodName, method.MethodTargetKey)">设置角色</el-button>
                    <el-button
                      link
                      type="danger"
                      :disabled="getRoleCodes(method.MethodTargetKey).length === 0"
                      @click="handleRemoveRoles(method.MethodName, method.MethodTargetKey)"
                    >清除</el-button>
                  </template>
                </el-table-column>
              </el-table>
            </div>
          </template>
        </el-table-column>
        <el-table-column prop="ServiceName" label="服务名称" />
        <el-table-column prop="ClassTargetKey" label="服务目标键" width="220" />
        <el-table-column label="服务级已授权角色">
          <template #default="{ row }">
            <template v-if="getRoleCodes(row.ClassTargetKey).length > 0">
              <el-tag
                v-for="code in getRoleCodes(row.ClassTargetKey)"
                :key="code"
                type="warning"
                size="small"
                style="margin-right: 4px"
              >{{ code }}</el-tag>
            </template>
            <el-text v-else type="info" size="small">未设置</el-text>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="160" fixed="right">
          <template #default="{ row }">
            <el-button link type="primary" @click="handleSetRoles(row.ServiceName, row.ClassTargetKey)">设置角色</el-button>
            <el-button
              link
              type="danger"
              :disabled="getRoleCodes(row.ClassTargetKey).length === 0"
              @click="handleRemoveRoles(row.ServiceName, row.ClassTargetKey)"
            >清除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <el-dialog v-model="dialogVisible" title="设置角色授权" width="520px">
      <el-descriptions :column="1" border size="small" style="margin-bottom: 16px">
        <el-descriptions-item label="目标">{{ currentLabel }}</el-descriptions-item>
        <el-descriptions-item label="目标键">{{ currentTargetKey }}</el-descriptions-item>
      </el-descriptions>

      <el-form label-width="80px">
        <el-form-item label="授权角色">
          <el-select
            v-model="selectedRoleCodes"
            multiple
            placeholder="请选择角色"
            style="width: 100%"
          >
            <el-option
              v-for="role in availableRoles"
              :key="role.RoleCode"
              :label="`${role.RoleName}（${role.RoleCode}）`"
              :value="role.RoleCode"
            />
          </el-select>
        </el-form-item>
      </el-form>

      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="submitting" @click="handleSubmitRoles">确定</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { rpcAuthorizationApi, roleApi } from '@/api'
import logger from '@/utils/logger'

logger.info('RpcMethodRoleManager', 'RPC 角色授权管理页面加载')

const loading = ref(false)
const submitting = ref(false)
const dialogVisible = ref(false)

const tableData = ref([])
const rpcRoleMap = ref({})
const availableRoles = ref([])
const currentLabel = ref('')
const currentTargetKey = ref('')
const selectedRoleCodes = ref([])

const groupedData = computed(() => {
  const map = {}
  for (const method of tableData.value) {
    if (!map[method.ServiceName]) {
      map[method.ServiceName] = {
        ServiceName: method.ServiceName,
        ClassTargetKey: method.ClassTargetKey,
        methods: []
      }
    }
    map[method.ServiceName].methods.push(method)
  }
  return Object.values(map)
})

onMounted(() => {
  logger.info('RpcMethodRoleManager', '组件挂载完成，开始加载数据')
  loadData()
})

const loadData = async () => {
  loading.value = true
  logger.debug('RpcMethodRoleManager', '开始加载数据')
  try {
    const [methodsRes, rolesRes, rpcRolesRes] = await Promise.allSettled([
      rpcAuthorizationApi.getAllRpcMethods(),
      roleApi.getRoleList(),
      rpcAuthorizationApi.getAllRpcRoles()
    ])

    if (methodsRes.status === 'fulfilled') {
      tableData.value = methodsRes.value.Methods || []
      logger.info('RpcMethodRoleManager', `加载 RPC 方法成功：${tableData.value.length} 个`)
    } else {
      logger.error('RpcMethodRoleManager', '加载 RPC 方法失败', methodsRes.reason)
      ElMessage.error('加载 RPC 方法失败：' + (methodsRes.reason?.message || '未知错误'))
    }

    if (rolesRes.status === 'fulfilled') {
      availableRoles.value = rolesRes.value.Roles || []
      logger.info('RpcMethodRoleManager', `加载角色列表成功：${availableRoles.value.length} 个`)
    } else {
      logger.error('RpcMethodRoleManager', '加载角色列表失败', rolesRes.reason)
    }

    if (rpcRolesRes.status === 'fulfilled') {
      const map = {}
      const items = rpcRolesRes.value.Items || []
      for (const item of items) {
        map[item.TargetKey] = item.RoleCodes || []
      }
      rpcRoleMap.value = map
      logger.info('RpcMethodRoleManager', `加载授权记录成功：${items.length} 条`)
    } else {
      logger.warn('RpcMethodRoleManager', '加载授权记录失败', rpcRolesRes.reason)
    }
  } finally {
    loading.value = false
  }
}

const getRoleCodes = targetKey => {
  return rpcRoleMap.value[targetKey] || []
}

const handleSetRoles = (label, targetKey) => {
  logger.info('RpcMethodRoleManager', '打开设置角色对话框', { label, targetKey })
  currentLabel.value = label
  currentTargetKey.value = targetKey
  selectedRoleCodes.value = [...(rpcRoleMap.value[targetKey] || [])]
  dialogVisible.value = true
}

const handleSubmitRoles = async () => {
  if (!currentTargetKey.value) return
  submitting.value = true
  logger.info('RpcMethodRoleManager', '提交角色设置', {
    targetKey: currentTargetKey.value,
    roleCodes: selectedRoleCodes.value
  })
  try {
    await rpcAuthorizationApi.setRpcRoles({
      TargetKey: currentTargetKey.value,
      RoleCodes: selectedRoleCodes.value
    })
    rpcRoleMap.value[currentTargetKey.value] = [...selectedRoleCodes.value]
    ElMessage.success('设置成功')
    logger.info('RpcMethodRoleManager', '角色设置成功', { targetKey: currentTargetKey.value })
    dialogVisible.value = false
  } catch (error) {
    logger.error('RpcMethodRoleManager', '设置角色失败', error)
  } finally {
    submitting.value = false
  }
}

const handleRemoveRoles = async (label, targetKey) => {
  logger.info('RpcMethodRoleManager', '尝试清除角色授权', { label, targetKey })
  try {
    await ElMessageBox.confirm(
      `确定要清除【${label}】的所有角色授权吗？`,
      '提示',
      { type: 'warning' }
    )
    loading.value = true
    await rpcAuthorizationApi.removeRpcRoles({ TargetKey: targetKey })
    delete rpcRoleMap.value[targetKey]
    rpcRoleMap.value = { ...rpcRoleMap.value }
    ElMessage.success('清除成功')
    logger.info('RpcMethodRoleManager', '角色授权清除成功', { targetKey })
  } catch (error) {
    if (error !== 'cancel') {
      logger.error('RpcMethodRoleManager', '清除角色授权失败', error)
    } else {
      logger.debug('RpcMethodRoleManager', '用户取消清除操作')
    }
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.rpc-role-manager {
  width: 100%;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.expand-content {
  padding: 12px 24px;
}
</style>
