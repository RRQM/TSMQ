<template><div class="collection-toolbar"><div class="collection-summary"><span class="collection-icon"><el-icon><component :is="icon || 'Grid'" /></el-icon></span><strong>{{ title }}</strong><span class="count-badge">{{ total }}</span></div><div class="collection-tools"><slot /><el-input :model-value="modelValue" @update:model-value="$emit('update:modelValue', $event)" :placeholder="t('搜索…')" prefix-icon="Search" clearable class="collection-search" /></div></div></template>
<script setup lang="ts">
import { t } from '@/locales'
defineProps<{ modelValue: string; title: string; total: number; icon?: string }>()
defineEmits<{ 'update:modelValue': [value: string] }>()
</script>
