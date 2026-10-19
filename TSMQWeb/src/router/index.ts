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
      { path: 'account', name: 'Account', component: () => import('@/views/Account.vue'), meta: { title: '当前账号', icon: 'User' } },
      { path: 'mqtt-access', name: 'MqttAccess', component: () => import('@/views/mqtt/MqttAccessControl.vue'), meta: { title: 'MQTT访问控制', icon: 'Lock', permission: 'MqttAccessService.GetMqttAccessPolicy' } },
      { path: 'mqtt-diagnostics', name: 'MqttDiagnostics', component: () => import('@/views/mqtt/MqttDiagnostics.vue'), meta: { title: 'MQTT消息排障', icon: 'Search', permission: 'MqttAccessService.GetMqttDiagnostics' } },
      {
        path: 'dashboard',
        name: 'Dashboard',
        component: () => import('@/views/Dashboard.vue'),
        meta: { title: '统计概览', permission: 'MqttServerManagerService.GetMqttDashboardStats', icon: 'DataAnalysis' }
      },
      {
        path: 'users',
        name: 'Users',
        component: () => import('@/views/user/UserList.vue'),
        meta: { title: '用户管理', permission: 'UserManagerService.GetUserList', icon: 'User' }
      },
      {
        path: 'roles',
        name: 'Roles',
        component: () => import('@/views/role/RoleList.vue'),
        meta: { title: '角色管理', permission: 'RoleManagerService.GetRoleList', icon: 'UserFilled' }
      },
      {
        path: 'rpc-roles',
        name: 'RpcRoles',
        component: () => import('@/views/rpc-authorization/RpcMethodRoleManager.vue'),
        meta: { title: 'RPC角色授权', permission: 'RpcAuthorizationService.GetAllRpcMethods', icon: 'Setting' }
      },
      {
        path: 'logs',
        name: 'Logs',
        component: () => import('@/views/LogViewer.vue'),
        meta: { title: '系统日志', permission: 'LogManagerService.GetLogList', icon: 'Document' }
      },
      {
        path: 'mqtt-servers',
        name: 'MqttServers',
        component: () => import('@/views/mqtt/MqttServerManager.vue'),
        meta: { title: 'MQTT服务器', permission: 'MqttServerManagerService.GetMqttServerList', icon: 'Connection' }
      },
      {
        path: 'mqtt-clients',
        name: 'MqttClients',
        component: () => import('@/views/mqtt/MqttClientList.vue'),
        meta: { title: 'MQTT客户端', permission: 'MqttClientManagerService.GetMqttClientList', icon: 'Monitor' }
      },
      {
        path: 'mqtt-topics',
        name: 'MqttTopics',
        component: () => import('@/views/mqtt/MqttTopicMonitor.vue'),
        meta: { title: 'MQTT主题监控', permission: 'MqttTopicManagerService.GetMqttTopicList', icon: 'DataAnalysis' }
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

    {
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

  if (to.meta.permission && !userStore.hasPermission(String(to.meta.permission))) {
    next('/account')
    return
  }
  logger.info('Router', `导航成功: ${to.path}`)
  next()
})

export default router
