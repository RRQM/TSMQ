<template><el-config-provider :locale="locale === 'zh-CN' ? zhCn : en" :button="{ autoInsertSpace: false }"><router-view /></el-config-provider></template>
<script setup lang="ts">
import zhCn from 'element-plus/es/locale/lang/zh-cn'
import en from 'element-plus/es/locale/lang/en'
import { usePreferences } from '@/composables/preferences'
const { locale } = usePreferences()
</script>
