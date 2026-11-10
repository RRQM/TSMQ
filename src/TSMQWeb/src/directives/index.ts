import { watchEffect } from 'vue'
import type { App, DirectiveBinding } from 'vue'
import { useUserStore } from '@/stores/user'

const stops = new WeakMap<HTMLElement, () => void>()

export default function setupDirectives(app: App) {
  const apply = (el: HTMLElement, binding: DirectiveBinding<string>) => {
    stops.get(el)?.()
    const store = useUserStore()
    stops.set(el, watchEffect(() => {
      el.style.display = store.hasPermission(binding.value) ? '' : 'none'
    }))
  }
  app.directive('permission', {
    mounted: apply,
    updated: apply,
    unmounted(el: HTMLElement) { stops.get(el)?.(); stops.delete(el) }
  })
}
