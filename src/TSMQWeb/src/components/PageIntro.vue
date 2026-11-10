<template><div class="page-intro"><div><div class="eyebrow">{{ eyebrow }}</div><h1>{{ title }}</h1><p>{{ description }}</p></div><div class="page-intro-extra"><slot /></div></div></template>
<script setup lang="ts">
defineProps<{ title: string; description: string; eyebrow: string }>()
</script>
