import { createRouter, createWebHistory } from 'vue-router'
import type { RouteRecordRaw } from 'vue-router'
import { useUserStore } from '@/stores/user'
import logger from '@/utils/logger'

const routes: RouteRecordRaw[] = [
  {
    path: '/login',
    name: 'Login',
    component: () => import('@/views/Login.vue'),
    meta: { requiresAuth: false }
  },
  {
    path: '/',
    component: () => import('@/layout/MainLayout.vue'),
    redirect: '/dashboard',
    meta: { requiresAuth: true },
    children: [
      {
        path: 'dashboard',
        name: 'Dashboard',
        component: () => import('@/views/Dashboard.vue'),
        meta: { title: '首页', icon: 'House' }
      },
      {
        path: 'users',
        name: 'Users',
        component: () => import('@/views/user/UserList.vue'),
        meta: { title: '用户管理', icon: 'User' }
      },
      {
        path: 'roles',
        name: 'Roles',
        component: () => import('@/views/role/RoleList.vue'),
        meta: { title: '角色管理', icon: 'UserFilled' }
      },
      {
        path: 'rpc-roles',
        name: 'RpcRoles',
        component: () => import('@/views/rpc-authorization/RpcMethodRoleManager.vue'),
        meta: { title: 'RPC角色授权', icon: 'Setting' }
      },
      {
        path: 'logs',
        name: 'Logs',
        component: () => import('@/views/LogViewer.vue'),
        meta: { title: '系统日志', icon: 'Document' }
      },
      {
        path: 'mqtt-servers',
        name: 'MqttServers',
        component: () => import('@/views/mqtt/MqttServerManager.vue'),
        meta: { title: 'MQTT服务器', icon: 'Connection' }
      },
      {
        path: 'mqtt-clients',
        name: 'MqttClients',
        component: () => import('@/views/mqtt/MqttClientList.vue'),
        meta: { title: 'MQTT客户端', icon: 'Monitor' }
      },
      {
        path: 'mqtt-topics',
        name: 'MqttTopics',
        component: () => import('@/views/mqtt/MqttTopicMonitor.vue'),
        meta: { title: 'MQTT主题监控', icon: 'DataAnalysis' }
      }
    ]
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach(async (to, _from, next) => {
  logger.debug('Router', `路由导航: ${_from.path} -> ${to.path}`, {
    requiresAuth: to.meta.requiresAuth
  })

  const userStore = useUserStore()

  if (to.meta.requiresAuth !== false) {
    if (!userStore.token) {
      logger.warn('Router', '用户未登录，重定向到登录页', { targetPath: to.path })
      next('/login')
      return
    }

    if (!userStore.userInfo) {
      logger.debug('Router', '尝试获取用户信息')
      try {
        await userStore.getUserInfo()
      } catch (error) {
        logger.error('Router', '获取用户信息失败，退出登录', error)
        userStore.logout()
        next('/login')
        return
      }
    }

  }

  logger.info('Router', `导航成功: ${to.path}`)
  next()
})

export default router
