<template>
  <div class="console-shell" :class="{ 'is-collapsed': collapsed, 'mobile-open': mobileOpen, 'focus-mode': focusMode }">
    <button v-if="mobileOpen" class="sidebar-backdrop" :aria-label="t('关闭菜单')" @click="mobileOpen = false" />
    <aside class="console-sidebar">
      <router-link to="/" class="brand"><ConsoleLogo /><span class="brand-word">TSMQ<span>CONSOLE</span></span></router-link>
      <div class="workspace-badge"><span class="workspace-icon"><el-icon><Connection /></el-icon></span><div><strong>{{ t('消息工作空间') }}</strong><small>MQTT / TCP / WebSocket</small></div><span class="status-dot" /></div>
      <nav class="sidebar-nav" :aria-label="t('主导航')">
        <div v-for="group in menuGroups" :key="group.label" class="nav-group">
          <div class="nav-group-label">{{ t(group.label) }}</div>
          <router-link v-for="item in group.items" :key="item.path" :to="'/' + item.path" class="nav-link" :title="t(String(item.meta?.title || ''))" @click="mobileOpen = false">
            <el-icon><component :is="item.meta?.icon" /></el-icon><span>{{ t(String(item.meta?.title || '')) }}</span><span v-if="item.path === 'dashboard'" class="nav-live" />
          </router-link>
        </div>
      </nav>
      <div class="sidebar-bottom"><div class="sidebar-note"><span class="mini-orbit"><el-icon><Cpu /></el-icon></span><strong>{{ t('连接每一条消息') }}</strong><p>{{ t('让消息流动，让系统可见。') }}</p></div><button class="collapse-button" @click="collapsed = !collapsed" :aria-label="t('折叠侧栏')"><el-icon><Expand v-if="collapsed" /><Fold v-else /></el-icon><span>{{ t('折叠侧栏') }}</span><kbd>⌘ B</kbd></button></div>
    </aside>
    <div class="console-workspace">
      <header class="console-topbar">
        <div class="breadcrumb"><button class="icon-button mobile-menu" :aria-label="t('打开菜单')" @click="mobileOpen = !mobileOpen"><el-icon><Menu /></el-icon></button><span class="breadcrumb-root">{{ t('工作空间') }}</span><el-icon class="breadcrumb-chevron"><ArrowRight /></el-icon><strong>{{ t(String(route.meta.title || '统计概览')) }}</strong></div>
        <div class="topbar-actions"><button class="search-trigger" @click="openSearch"><el-icon><Search /></el-icon><span>{{ t('快速导航') }}</span><kbd>⌘ K</kbd></button><span class="topbar-divider" /><PreferenceControls /><button class="icon-button" @click="focusMode = !focusMode" :aria-label="t('专注模式')" :title="t('专注模式')"><el-icon><FullScreen /></el-icon></button><el-dropdown trigger="click" @command="handleCommand"><button class="profile-button"><span class="avatar">{{ initial }}</span><span class="profile-name">{{ userStore.userInfo?.username }}</span><el-icon><ArrowDown /></el-icon></button><template #dropdown><el-dropdown-menu><el-dropdown-item command="account">{{ t('当前账号') }}</el-dropdown-item><el-dropdown-item divided command="logout">{{ t('退出登录') }}</el-dropdown-item></el-dropdown-menu></template></el-dropdown></div>
      </header>
      <main class="console-main" id="main-content"><router-view v-slot="{ Component, route: viewRoute }"><Transition name="page" mode="out-in"><div :key="viewRoute.path" class="route-page"><PageIntro v-if="route.name !== 'Dashboard'" :eyebrow="pageInfo.eyebrow" :title="t(String(route.meta.title || ''))" :description="t(pageInfo.description)" /><component :is="Component" /></div></Transition></router-view></main>
      <footer class="console-footer"><span>TSMQ <span class="footer-separator">/</span> {{ t('消息基础设施控制台') }}</span><span class="footer-right"><el-icon><Link /></el-icon>{{ t('管理工作空间') }}<span class="footer-separator">·</span>v1.0</span></footer>
    </div>
    <el-dialog v-model="searchOpen" :title="t('快速导航')" width="520px" class="command-dialog"><el-input ref="searchRef" v-model="search" :placeholder="t('搜索页面…')" prefix-icon="Search" clearable /><div class="command-results"><button v-for="item in searchResults" :key="item.path" @click="navigate(item.path)"><el-icon><component :is="item.meta?.icon" /></el-icon><span>{{ t(String(item.meta?.title || '')) }}</span><el-icon><Right /></el-icon></button><el-empty v-if="!searchResults.length" :description="t('没有匹配的页面')" :image-size="64" /></div></el-dialog>
  </div>
</template>
<script setup lang="ts">
import { computed, nextTick, onMounted, onUnmounted, ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { ElMessageBox } from 'element-plus'
import { useUserStore } from '@/stores/user'
import { userApi } from '@/api'
import { usePreferences } from '@/composables/preferences'
import { t } from '@/locales'
import ConsoleLogo from '@/components/ConsoleLogo.vue'
import PreferenceControls from '@/components/PreferenceControls.vue'
import PageIntro from '@/components/PageIntro.vue'
const router = useRouter(), route = useRoute(), userStore = useUserStore()
const { collapsed } = usePreferences()
const mobileOpen = ref(false), focusMode = ref(false), searchOpen = ref(false), search = ref(''), searchRef = ref()
const initial = computed(() => (userStore.userInfo?.account || 'U').slice(0, 1).toUpperCase())
const allowedRoutes = computed(() => (router.options.routes.find(r => r.path === '/')?.children || []).filter(r => !r.meta?.permission || userStore.hasPermission(String(r.meta.permission))))
const menuGroups = computed(() => [
  { label: '总览', paths: ['dashboard'] },
  { label: '消息服务', paths: ['mqtt-servers', 'mqtt-clients', 'mqtt-topics', 'mqtt-access', 'mqtt-diagnostics'] },
  { label: '工作空间管理', paths: ['users', 'roles', 'rpc-roles', 'logs', 'account'] }
].map(group => ({ ...group, items: group.paths.flatMap(path => allowedRoutes.value.filter(r => r.path === path)) })).filter(group => group.items.length))
const searchResults = computed(() => allowedRoutes.value.filter(r => t(String(r.meta?.title)).toLowerCase().includes(search.value.toLowerCase())))
const descriptions: Record<string, string> = {
  Users: '管理工作空间成员，为每个账号分配恰当的访问权限。', Roles: '通过角色组织权限，保持团队访问边界清晰。', RpcRoles: '精确配置服务与方法的访问权限，变更即时生效。', Logs: '检索运行日志，快速定位系统异常与关键事件。',
  MqttServers: '统一管理监听器、设备凭据、证书与 Broker 参数。', MqttClients: '查看实时连接状态，管理每一个接入的设备。', MqttTopics: '洞察主题订阅与消息流量，追踪每一条数据通路。', MqttAccess: '从连接认证到主题授权，建立清晰的设备访问边界。', MqttDiagnostics: '追踪连接与消息事件，让排查问题更直接。', Account: '管理个人账号，查看当前工作空间的访问权限。'
}
const pageInfo = computed(() => ({ eyebrow: String(route.name).startsWith('Mqtt') ? 'MESSAGING' : 'WORKSPACE', description: descriptions[String(route.name)] || '' }))
function navigate(path: string) { searchOpen.value = false; search.value = ''; router.push('/' + path) }
function openSearch() { searchOpen.value = true; nextTick(() => searchRef.value?.focus()) }
function onKey(event: KeyboardEvent) { if ((event.metaKey || event.ctrlKey) && ['k', 'b'].includes(event.key.toLowerCase())) { event.preventDefault(); if (event.key.toLowerCase() === 'b') collapsed.value = !collapsed.value; else openSearch() } if (event.key === 'Escape') { mobileOpen.value = false; focusMode.value = false } }
async function handleCommand(command: string) {
  if (command === 'account') return router.push('/account')
  try { await ElMessageBox.confirm(t('确定要退出登录吗？'), t('退出登录'), { type: 'warning', confirmButtonText: t('退出登录'), cancelButtonText: t('取消') }) } catch { return }
  await userApi.logout().catch(() => {})
  userStore.logout(); router.push('/login')
}
onMounted(() => window.addEventListener('keydown', onKey))
onUnmounted(() => window.removeEventListener('keydown', onKey))
</script>
