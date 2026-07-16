<template>
  <div class="user-list">
    <el-card>
      <template #header>
        <div class="card-header">
          <span>用户管理</span>
          <el-button type="primary" :icon="Plus" @click="handleAdd">新增用户</el-button>
        </div>
      </template>

      <el-table :data="tableData" border stripe v-loading="loading">
        <el-table-column prop="Id" label="ID" width="80" />
        <el-table-column prop="Account" label="账号" width="160" />
        <el-table-column prop="UserName" label="用户名" width="140" />
        <el-table-column prop="Email" label="邮箱" />
        <el-table-column prop="Phone" label="手机号" width="140" />
        <el-table-column label="角色" width="200">
          <template #default="{ row }">
            <template v-if="getRoleNames(row.RoleIds).length > 0">
              <el-tag
                v-for="name in getRoleNames(row.RoleIds)"
                :key="name"
                size="small"
                style="margin-right: 4px; margin-bottom: 2px"
              >{{ name }}</el-tag>
            </template>
            <el-text v-else type="info" size="small">无角色</el-text>
          </template>
        </el-table-column>
        <el-table-column label="状态" width="80">
          <template #default="{ row }">
            <el-tag :type="row.IsEnabled ? 'success' : 'danger'" size="small">
              {{ row.IsEnabled ? '启用' : '禁用' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="CreateTime" label="创建时间" width="170">
          <template #default="{ row }">{{ formatTime(row.CreateTime) }}</template>
        </el-table-column>
        <el-table-column label="操作" width="240" fixed="right">
          <template #default="{ row }">
            <el-button link type="primary" :icon="View" @click="handleView(row)">查看</el-button>
            <el-button link type="primary" :icon="Key" @click="handleAssignRole(row)">分配角色</el-button>
            <el-button link type="warning" :icon="Lock" @click="handleChangePassword(row)">改密码</el-button>
            <el-button link type="danger" :icon="Delete" @click="handleDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- 新增用户对话框 -->
    <el-dialog v-model="addDialogVisible" title="新增用户" width="480px" @close="resetAddForm">
      <el-form :model="addForm" :rules="addRules" ref="addFormRef" label-width="90px">
        <el-form-item label="账号" prop="Account">
          <el-input v-model="addForm.Account" placeholder="请输入账号" />
        </el-form-item>
        <el-form-item label="密码" prop="Password">
          <el-input v-model="addForm.Password" type="password" show-password placeholder="请输入密码" />
        </el-form-item>
        <el-form-item label="确认密码" prop="ConfirmPassword">
          <el-input v-model="addForm.ConfirmPassword" type="password" show-password placeholder="请再次输入密码" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="addDialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="submitting" @click="handleSubmitAdd">确定</el-button>
      </template>
    </el-dialog>

    <!-- 查看用户详情对话框 -->
    <el-dialog v-model="viewDialogVisible" title="用户详情" width="480px">
      <el-descriptions :column="1" border>
        <el-descriptions-item label="ID">{{ currentUser?.Id }}</el-descriptions-item>
        <el-descriptions-item label="账号">{{ currentUser?.Account }}</el-descriptions-item>
        <el-descriptions-item label="用户名">{{ currentUser?.UserName || '-' }}</el-descriptions-item>
        <el-descriptions-item label="邮箱">{{ currentUser?.Email || '-' }}</el-descriptions-item>
        <el-descriptions-item label="手机号">{{ currentUser?.Phone || '-' }}</el-descriptions-item>
        <el-descriptions-item label="角色">
          <template v-if="getRoleNames(currentUser?.RoleIds).length > 0">
            <el-tag
              v-for="name in getRoleNames(currentUser?.RoleIds)"
              :key="name"
              size="small"
              style="margin-right: 4px"
            >{{ name }}</el-tag>
          </template>
          <el-text v-else type="info" size="small">无角色</el-text>
        </el-descriptions-item>
        <el-descriptions-item label="状态">
          <el-tag :type="currentUser?.IsEnabled ? 'success' : 'danger'" size="small">
            {{ currentUser?.IsEnabled ? '启用' : '禁用' }}
          </el-tag>
        </el-descriptions-item>
        <el-descriptions-item label="创建时间">{{ formatTime(currentUser?.CreateTime) }}</el-descriptions-item>
      </el-descriptions>
      <template #footer>
        <el-button @click="viewDialogVisible = false">关闭</el-button>
      </template>
    </el-dialog>

    <!-- 分配角色对话框 -->
    <el-dialog v-model="roleDialogVisible" title="分配角色" width="480px">
      <el-descriptions :column="1" border size="small" style="margin-bottom: 16px">
        <el-descriptions-item label="账号">{{ currentUser?.Account }}</el-descriptions-item>
      </el-descriptions>
      <el-checkbox-group v-model="selectedRoleIds">
        <div v-for="role in allRoles" :key="role.Id" style="margin-bottom: 8px">
          <el-checkbox :value="role.Id">
            {{ role.RoleName }}
            <el-text type="info" size="small">（{{ role.RoleCode }}）</el-text>
          </el-checkbox>
        </div>
      </el-checkbox-group>
      <template #footer>
        <el-button @click="roleDialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="submitting" @click="handleSaveRoles">确定</el-button>
      </template>
    </el-dialog>

    <!-- 修改密码对话框 -->
    <el-dialog v-model="passwordDialogVisible" title="修改密码" width="420px">
      <el-descriptions :column="1" border size="small" style="margin-bottom: 16px">
        <el-descriptions-item label="账号">{{ currentUser?.Account }}</el-descriptions-item>
      </el-descriptions>
      <el-form :model="passwordForm" :rules="passwordRules" ref="passwordFormRef" label-width="90px">
        <el-form-item label="旧密码" prop="OldPassword">
          <el-input v-model="passwordForm.OldPassword" type="password" show-password />
        </el-form-item>
        <el-form-item label="新密码" prop="NewPassword">
          <el-input v-model="passwordForm.NewPassword" type="password" show-password />
        </el-form-item>
        <el-form-item label="确认新密码" prop="ConfirmPassword">
          <el-input v-model="passwordForm.ConfirmPassword" type="password" show-password />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="passwordDialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="submitting" @click="handleSubmitPassword">确定</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Plus, View, Key, Lock, Delete } from '@element-plus/icons-vue'
import { userApi, roleApi } from '@/api'
import logger from '@/utils/logger'

logger.info('UserList', '用户管理页面加载')

const loading = ref(false)
const submitting = ref(false)
const tableData = ref([])
const allRoles = ref([])

const addDialogVisible = ref(false)
const viewDialogVisible = ref(false)
const roleDialogVisible = ref(false)
const passwordDialogVisible = ref(false)

const currentUser = ref(null)
const selectedRoleIds = ref([])
const addFormRef = ref(null)
const passwordFormRef = ref(null)

const addForm = reactive({
  Account: '',
  Password: '',
  ConfirmPassword: ''
})

const addRules = {
  Account: [{ required: true, message: '请输入账号', trigger: 'blur' }],
  Password: [{ required: true, message: '请输入密码', trigger: 'blur' }],
  ConfirmPassword: [
    { required: true, message: '请再次输入密码', trigger: 'blur' },
    {
      validator: (_, value, callback) => {
        if (value !== addForm.Password) {
          callback(new Error('两次输入的密码不一致'))
        } else {
          callback()
        }
      },
      trigger: 'blur'
    }
  ]
}

const passwordForm = reactive({
  OldPassword: '',
  NewPassword: '',
  ConfirmPassword: ''
})

const passwordRules = {
  OldPassword: [{ required: true, message: '请输入旧密码', trigger: 'blur' }],
  NewPassword: [{ required: true, message: '请输入新密码', trigger: 'blur' }],
  ConfirmPassword: [
    { required: true, message: '请再次输入新密码', trigger: 'blur' },
    {
      validator: (_, value, callback) => {
        if (value !== passwordForm.NewPassword) {
          callback(new Error('两次输入的密码不一致'))
        } else {
          callback()
        }
      },
      trigger: 'blur'
    }
  ]
}

onMounted(() => {
  logger.info('UserList', '组件挂载完成，开始加载数据')
  loadData()
})

const loadData = async () => {
  loading.value = true
  try {
    const [usersRes, rolesRes] = await Promise.allSettled([
      userApi.getUserList(),
      roleApi.getRoleList()
    ])

    if (usersRes.status === 'fulfilled') {
      tableData.value = usersRes.value.Users || []
      logger.info('UserList', `加载用户列表成功：${tableData.value.length} 个用户`)
    } else {
      logger.error('UserList', '加载用户列表失败', usersRes.reason)
      ElMessage.error('加载用户列表失败：' + (usersRes.reason?.message || '未知错误'))
    }

    if (rolesRes.status === 'fulfilled') {
      allRoles.value = rolesRes.value.Roles || []
    } else {
      logger.error('UserList', '加载角色列表失败', rolesRes.reason)
    }
  } finally {
    loading.value = false
  }
}

const getRoleNames = roleIdsStr => {
  if (!roleIdsStr) return []
  const ids = roleIdsStr.split(',').map(Number).filter(Boolean)
  return ids
    .map(id => allRoles.value.find(r => r.Id === id)?.RoleName)
    .filter(Boolean)
}

const formatTime = timeStr => {
  if (!timeStr) return '-'
  return new Date(timeStr).toLocaleString('zh-CN')
}

// 新增用户
const handleAdd = () => {
  addDialogVisible.value = true
}

const resetAddForm = () => {
  addForm.Account = ''
  addForm.Password = ''
  addForm.ConfirmPassword = ''
  addFormRef.value?.clearValidate()
}

const handleSubmitAdd = async () => {
  if (!addFormRef.value) return
  await addFormRef.value.validate(async valid => {
    if (!valid) return
    submitting.value = true
    logger.info('UserList', '提交新增用户', { account: addForm.Account })
    try {
      const res = await userApi.register({
        Account: addForm.Account,
        Password: addForm.Password
      })
      if (res.Code === 'Success') {
        ElMessage.success('新增用户成功')
        addDialogVisible.value = false
        await loadData()
      } else {
        ElMessage.error(res.Message || '新增失败')
      }
    } catch (error) {
      logger.error('UserList', '新增用户失败', error)
    } finally {
      submitting.value = false
    }
  })
}

// 查看详情
const handleView = row => {
  currentUser.value = row
  viewDialogVisible.value = true
}

// 删除用户
const handleDelete = async row => {
  try {
    await ElMessageBox.confirm(
      `确定要删除用户 "${row.Account}" 吗？此操作不可恢复。`,
      '删除确认',
      { type: 'warning', confirmButtonText: '确定删除', cancelButtonText: '取消', confirmButtonClass: 'el-button--danger' }
    )
  } catch {
    return
  }

  logger.info('UserList', '删除用户', { userId: row.Id, account: row.Account })
  try {
    const res = await userApi.deleteUser({ UserId: row.Id })
    if (res.Code === 'Success') {
      ElMessage.success('删除成功')
      tableData.value = tableData.value.filter(u => u.Id !== row.Id)
    } else {
      ElMessage.error(res.Message || '删除失败')
    }
  } catch (error) {
    logger.error('UserList', '删除用户失败', error)
  }
}

// 分配角色
const handleAssignRole = row => {
  logger.info('UserList', '打开分配角色对话框', { userId: row.Id, account: row.Account })
  currentUser.value = row
  const ids = row.RoleIds ? row.RoleIds.split(',').map(Number).filter(Boolean) : []
  selectedRoleIds.value = ids
  roleDialogVisible.value = true
}

const handleSaveRoles = async () => {
  if (!currentUser.value) return
  submitting.value = true
  logger.info('UserList', '提交角色分配', { userId: currentUser.value.Id, roleIds: selectedRoleIds.value })
  try {
    const res = await userApi.assignUserRoles({
      UserId: currentUser.value.Id,
      RoleIds: selectedRoleIds.value
    })
    if (res.Code === 'Success') {
      currentUser.value.RoleIds = selectedRoleIds.value.join(',')
      ElMessage.success('角色分配成功')
      roleDialogVisible.value = false
    } else {
      ElMessage.error(res.Message || '角色分配失败')
    }
  } catch (error) {
    logger.error('UserList', '角色分配失败', error)
  } finally {
    submitting.value = false
  }
}

// 修改密码
const handleChangePassword = row => {
  logger.info('UserList', '打开修改密码对话框', { userId: row.Id, account: row.Account })
  currentUser.value = row
  passwordForm.OldPassword = ''
  passwordForm.NewPassword = ''
  passwordForm.ConfirmPassword = ''
  passwordDialogVisible.value = true
}

const handleSubmitPassword = async () => {
  if (!passwordFormRef.value) return
  await passwordFormRef.value.validate(async valid => {
    if (!valid) return
    submitting.value = true
    logger.info('UserList', '提交修改密码', { userId: currentUser.value?.Id })
    try {
      const res = await userApi.changePassword({
        OldPassword: passwordForm.OldPassword,
        NewPassword: passwordForm.NewPassword
      })
      if (res.Code === 'Success') {
        ElMessage.success('密码修改成功')
        passwordDialogVisible.value = false
      } else {
        ElMessage.error(res.Message || '密码修改失败')
      }
    } catch (error) {
      logger.error('UserList', '密码修改失败', error)
    } finally {
      submitting.value = false
    }
  })
}
</script>

<style scoped>
.user-list {
  width: 100%;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
</style>
