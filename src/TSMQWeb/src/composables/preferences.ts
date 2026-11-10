import { computed, ref, watch } from 'vue'

const savedTheme = localStorage.getItem('tsmq.theme')
const theme = ref<'light' | 'dark'>(savedTheme === 'dark' || (!savedTheme && matchMedia('(prefers-color-scheme: dark)').matches) ? 'dark' : 'light')
const locale = ref<'zh-CN' | 'en-US'>(localStorage.getItem('tsmq.locale') === 'en-US' ? 'en-US' : 'zh-CN')
const collapsed = ref(localStorage.getItem('tsmq.sidebar') === 'true')
watch(theme, value => {
  document.documentElement.classList.toggle('dark', value === 'dark')
  document.documentElement.dataset.theme = value
  localStorage.setItem('tsmq.theme', value)
}, { immediate: true })
watch(locale, value => {
  document.documentElement.lang = value
  document.title = value === 'zh-CN' ? 'TSMQ · 消息控制台' : 'TSMQ · Messaging console'
  localStorage.setItem('tsmq.locale', value)
}, { immediate: true })
watch(collapsed, value => localStorage.setItem('tsmq.sidebar', String(value)))
const isDark = computed(() => theme.value === 'dark')
const preferences = { theme, locale, collapsed, isDark,
  toggleTheme: () => { theme.value = theme.value === 'dark' ? 'light' : 'dark' },
  toggleLocale: () => { locale.value = locale.value === 'zh-CN' ? 'en-US' : 'zh-CN' } }
export function usePreferences() {
  return preferences
}
