import { usePreferences } from '@/composables/preferences'
import en from './en.json'
const messages: Record<string, string> = en
const patterns = Object.entries(messages).filter(([key]) => key.includes('{p')).map(([key, translated]) => ({
  expression: new RegExp('^' + key.replace(/[.*+?^${}()|[\]\\]/g, '\\$&').replace(/\\\{p\d+\\\}/g, '(.+?)') + '$'),
  translated
}))
export function t(key: string, params: Record<string, string | number> = {}) {
  const { locale } = usePreferences()
  let message = locale.value === 'en-US' ? messages[key] || key : key
  if (locale.value === 'en-US' && !messages[key]) {
    for (const pattern of patterns) {
      const match = key.match(pattern.expression)
      if (match) { message = pattern.translated.replace(/\{p(\d+)\}/g, (_, index) => match[Number(index) + 1] || ''); break }
    }
  }
  return message.replace(/\{(\w+)\}/g, (match, name) => String(params[name] ?? match))
}
export function useLocale() { return { t, locale: usePreferences().locale } }
