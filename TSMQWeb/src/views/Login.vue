<template>
  <div class="login-container">
    <el-card class="login-card">
      <template #header>
        <div class="card-header">
          <h2>TSMQ权限管理系统</h2>
        </div>
      </template>

      <el-form :model="form" :rules="rules" ref="formRef" label-width="0">
        <el-form-item prop="Account">
          <el-input v-model="form.Account" placeholder="账号" prefix-icon="User" size="large" />
        </el-form-item>

        <el-form-item prop="Password">
          <el-input
            v-model="form.Password"
            type="password"
            placeholder="密码"
            prefix-icon="Lock"
            size="large"
            @keyup.enter="handleLogin"
          />
        </el-form-item>

        <el-form-item>
          <el-button type="primary" size="large" style="width: 100%" :loading="loading" @click="handleLogin">
            登录
          </el-button>
        </el-form-item>
      </el-form>
    </el-card>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { useUserStore } from '@/stores/user'
import logger from '@/utils/logger'

logger.info('Login', '登录页面加载')

const router = useRouter()
const userStore = useUserStore()
const formRef = ref(null)
const loading = ref(false)

const form = ref({
  Account: '',
  Password: ''
})

const rules = {
  Account: [{ required: true, message: '请输入账号', trigger: 'blur' }],
  Password: [{ required: true, message: '请输入密码', trigger: 'blur' }]
}

const handleLogin = async () => {
  if (!formRef.value) return

  await formRef.value.validate(async valid => {
    if (valid) {
      loading.value = true
      logger.info('Login', '用户尝试登录', { account: form.value.Account })
      try {
        await userStore.login(form.value)
        ElMessage.success('登录成功')
        logger.info('Login', '登录成功，跳转到首页', { account: form.value.Account })
        router.push('/')
      } catch (error) {
        logger.error('Login', '登录失败', error)
        ElMessage.error(error.message || '登录失败')
      } finally {
        loading.value = false
      }
    }
  })
}
</script>

<style scoped>
.login-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.login-card {
  width: 400px;
}

.card-header {
  text-align: center;
}

.card-header h2 {
  margin: 0;
  color: #409eff;
}
</style>
