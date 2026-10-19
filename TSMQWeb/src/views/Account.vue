<template>
  <el-card><template #header>当前账号</template>
    <p>账号：{{ store.userInfo?.account }}</p>
    <p>当前已授权 {{ store.permissions.length }} 项操作。菜单仅显示可访问的功能。</p>
    <el-button @click="store.getUserInfo()">刷新权限</el-button>
  </el-card>
</template>
<script setup lang="ts">
import { useUserStore } from '@/stores/user'
const store = useUserStore()
</script>
