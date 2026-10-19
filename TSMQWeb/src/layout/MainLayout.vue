<template>
  <el-container class="layout-container">
    <el-aside width="200px">
      <div class="logo">
        <h3>TSMQ</h3>
      </div>

      <el-menu
        :default-active="activeMenu"
        :router="true"
        background-color="#304156"
        text-color="#bfcbd9"
        active-text-color="#409eff"
      >
        <template v-for="route in menuRoutes" :key="route.path">
          <el-menu-item :index="route.path">
            <el-icon v-if="route.meta?.icon">
              <component :is="route.meta.icon" />
            </el-icon>
            <span>{{ route.meta?.title }}</span>
          </el-menu-item>
        </template>
      </el-menu>
    </el-aside>

    <el-container>
      <el-header>
        <div class="header-content">
          <div class="breadcrumb">
            <el-breadcrumb separator="/">
              <el-breadcrumb-item :to="{ path: '/' }">首页</el-breadcrumb-item>
              <el-breadcrumb-item v-if="currentRoute.meta?.title">
                {{ currentRoute.meta.title }}
              </el-breadcrumb-item>
            </el-breadcrumb>
          </div>

          <div class="user-info">
            <el-dropdown @command="handleCommand">
              <span class="user-name">
                <el-icon><User /></el-icon>
                {{ userStore.userInfo?.username || '用户' }}
              </span>
              <template #dropdown>
                <el-dropdown-menu>
                  <el-dropdown-item command="logout">退出登录</el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>
          </div>
        </div>
      </el-header>

      <el-main>
        <router-view />
      </el-main>
    </el-container>
  </el-container>
</template>

<script setup>
import { computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { ElMessageBox } from 'element-plus'
import { useUserStore } from '@/stores/user'
import logger from '@/utils/logger'
import { userApi } from '@/api'

logger.info('MainLayout', '主布局组件加载')

const router = useRouter()
const route = useRoute()
const userStore = useUserStore()

onMounted(() => {
  logger.info('MainLayout', '主布局组件挂载完成', {
    currentRoute: route.path,
    user: userStore.userInfo?.account
  })
})

const menuRoutes = computed(() => {
  const mainRoute = router.options.routes.find(r => r.path === '/')
  return (mainRoute?.children || []).filter(r => !r.meta?.permission || userStore.hasPermission(String(r.meta.permission)))
})

const activeMenu = computed(() => route.path)
const currentRoute = computed(() => route)

const handleCommand = async command => {
  if (command === 'logout') {
    logger.info('MainLayout', '用户尝试退出登录')
    try {
      await ElMessageBox.confirm('确定要退出登录吗？', '提示', {
        type: 'warning'
      })
      logger.info('MainLayout', '用户确认退出登录')
      await userApi.logout().catch(() => {})
      userStore.logout()
      router.push('/login')
    } catch {
      logger.debug('MainLayout', '用户取消退出登录')
    }
  }
}
</script>

<style scoped>
.layout-container {
  height: 100vh;
}

.el-aside {
  background-color: #304156;
}

.logo {
  height: 60px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #fff;
  font-size: 20px;
  background-color: #2b3a4a;
}

.logo h3 {
  margin: 0;
}

.el-header {
  background-color: #fff;
  border-bottom: 1px solid #e6e6e6;
  display: flex;
  align-items: center;
}

.header-content {
  width: 100%;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.user-name {
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 5px;
}

.el-main {
  background-color: #f0f2f5;
  padding: 20px;
}
</style>
