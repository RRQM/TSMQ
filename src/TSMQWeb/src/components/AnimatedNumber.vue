<template><span>{{ formatted }}</span></template>
<script setup lang="ts">
import { computed, onUnmounted, ref, watch } from 'vue'
import { usePreferences } from '@/composables/preferences'
const props = defineProps<{ value: number }>(), displayed = ref(0), { locale } = usePreferences()
let frame = 0
const formatted = computed(() => new Intl.NumberFormat(locale.value, { notation: displayed.value >= 1000000 ? 'compact' : 'standard', maximumFractionDigits: displayed.value >= 1000000 ? 1 : 0 }).format(displayed.value))
watch(() => props.value, value => {
  cancelAnimationFrame(frame)
  if (matchMedia('(prefers-reduced-motion: reduce)').matches) { displayed.value = value; return }
  const from = displayed.value, started = performance.now()
  function update(time: number) { const progress = Math.min(1, (time - started) / 650); displayed.value = from + (value - from) * (1 - (1 - progress) ** 3); if (progress < 1) frame = requestAnimationFrame(update) }
  frame = requestAnimationFrame(update)
}, { immediate: true })
onUnmounted(() => cancelAnimationFrame(frame))
</script>
