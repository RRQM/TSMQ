<template><div class="preference-controls"><button class="icon-button language-button" @click="toggleLocale" :aria-label="t('切换语言')" :title="t('切换语言')"><span class="language-symbol">文</span><span>{{ locale === 'zh-CN' ? 'EN' : '中' }}</span></button><button class="icon-button" @click="toggleTheme" :aria-label="isDark ? t('切换浅色主题') : t('切换深色主题')" :title="isDark ? t('切换浅色主题') : t('切换深色主题')"><el-icon><Sunny v-if="isDark" /><Moon v-else /></el-icon></button></div></template>
<script setup lang="ts">
import { usePreferences } from '@/composables/preferences'
import { t } from '@/locales'
const { locale, isDark, toggleLocale, toggleTheme } = usePreferences()
</script>
