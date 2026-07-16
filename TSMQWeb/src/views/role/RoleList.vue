<template>
  <div class="role-list">
    <el-card>
      <template #header>
        <div class="card-header">
          <span>角色管理</span>
          <el-button type="primary" @click="handleAdd">
            <el-icon>
              <Plus />
            </el-icon>
            添加角色
          </el-button>
        </div>
      </template>

      <el-table :data="tableData" border stripe v-loading="roleStore.loading">
        <el-table-column prop="Id" label="ID" width="80" />
        <el-table-column prop="RoleName" label="角色名称" />
        <el-table-column prop="Description" label="描述" />
        <el-table-column prop="CreateTime" label="创建时间" />
        <el-table-column label="启用状态" width="100" align="center">
          <template #default="{ row }">
            <el-tag :type="row.IsEnabled ? 'success' : 'info'">
              {{ row.IsEnabled ? '已启用' : '已禁用' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="150" fixed="right">
          <template #default="{ row }">
            <el-button link type="primary" @click="handleEdit(row)">编辑</el-button>
            <el-button link type="danger" @click="handleDelete(row)">删除</el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <el-dialog v-model="dialogVisible" :title="dialogTitle" width="500px">
      <el-form :model="form" :rules="rules" ref="formRef" label-width="80px">
        <el-form-item label="角色名称" prop="RoleName">
          <el-input v-model="form.RoleName" />
        </el-form-item>
        <el-form-item label="角色编码" prop="RoleCode" v-if="!form.RoleId">
          <el-input v-model="form.RoleCode" />
        </el-form-item>
        <el-form-item label="描述" prop="Description">
          <el-input v-model="form.Description" type="textarea" :rows="3" />
        </el-form-item>
        <el-form-item label="是否启用" prop="IsEnabled">
          <el-switch v-model="form.IsEnabled" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSubmit" :loading="loading">确定</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { useRoleStore } from '@/stores/role'
import logger from '@/utils/logger'

logger.info('RoleList', '角色管理页面加载')

const roleStore = useRoleStore()

const tableData = ref([])
const dialogVisible = ref(false)
const dialogTitle = ref('')
const formRef = ref(null)
const loading = ref(false)

const form = reactive({
  RoleId: null,
  RoleName: '',
  RoleCode: '',
  Description: '',
  IsEnabled: true
})

const rules = {
  RoleName: [{ required: true, message: '请输入角色名称', trigger: 'blur' }],
  RoleCode: [{ required: true, message: '请输入角色编码', trigger: 'blur' }]
}

onMounted(() => {
  logger.info('RoleList', '组件挂载完成，开始加载数据')
  loadData()
})

const loadData = async () => {
  logger.debug('RoleList', '开始加载角色列表')
  try {
    const res = await roleStore.getRoleList()
    tableData.value = res.Roles || []
    logger.info('RoleList', `加载角色列表成功，显示 ${tableData.value.length} 条记录`)
  } catch (error) {
    logger.error('RoleList', '加载角色列表失败', error)
    console.error(error)
  }
}


const handleAdd = () => {
  logger.info('RoleList', '打开添加角色对话框')
  dialogTitle.value = '添加角色'
  Object.assign(form, {
    RoleId: null,
    RoleName: '',
    RoleCode: '',
    Description: '',
    IsEnabled: true
  })
  dialogVisible.value = true
}

const handleEdit = row => {
  logger.info('RoleList', '打开编辑角色对话框', { roleId: row.Id, roleName: row.RoleName })
  dialogTitle.value = '编辑角色'
  Object.assign(form, {
    RoleId: row.Id,
    RoleName: row.RoleName,
    RoleCode: row.RoleCode,
    Description: row.Description,
    IsEnabled: row.IsEnabled
  })
  dialogVisible.value = true
}

const handleDelete = async row => {
  logger.info('RoleList', '尝试删除角色', { roleId: row.Id, roleName: row.RoleName })
  try {
    await ElMessageBox.confirm('确定要删除该角色吗？', '提示', {
      type: 'warning'
    })
    loading.value = true
    await roleStore.deleteRole({ RoleId: row.Id })
    ElMessage.success('删除成功')
    logger.info('RoleList', '删除角色成功', { roleId: row.Id })
    loadData()
  } catch (error) {
    if (error !== 'cancel') {
      logger.error('RoleList', '删除角色失败', error)
      ElMessage.error(error.message || '删除失败')
    } else {
      logger.debug('RoleList', '用户取消删除角色')
    }
  } finally {
    loading.value = false
  }
}

const handleSubmit = async () => {
  if (!formRef.value) return

  await formRef.value.validate(async valid => {
    if (valid) {
      loading.value = true
      logger.info('RoleList', form.RoleId ? '开始更新角色' : '开始创建角色', form)
      try {
        if (form.RoleId) {
          await roleStore.updateRole({ RoleId: form.RoleId, RoleName: form.RoleName, Description: form.Description, IsEnabled: form.IsEnabled })
          ElMessage.success('更新成功')
          logger.info('RoleList', '更新角色成功', { roleId: form.RoleId })
        } else {
          await roleStore.createRole({ RoleName: form.RoleName, RoleCode: form.RoleCode, Description: form.Description, IsEnabled: form.IsEnabled })
          ElMessage.success('添加成功')
          logger.info('RoleList', '创建角色成功', { roleName: form.RoleName })
        }
        dialogVisible.value = false
        loadData()
      } catch (error) {
        logger.error('RoleList', form.RoleId ? '更新角色失败' : '创建角色失败', error)
        ElMessage.error(error.message || '操作失败')
      } finally {
        loading.value = false
      }
    }
  })
}

</script>

<style scoped>
.role-list {
  width: 100%;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
</style>
