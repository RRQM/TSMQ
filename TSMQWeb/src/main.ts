import { createApp } from 'vue'
import { createPinia } from 'pinia'
import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css'
import * as ElementPlusIconsVue from '@element-plus/icons-vue'
import App from './App.vue'
import router from './router'
import setupDirectives from './directives'
import { setBaseURL } from '@/utils/request'

/**
 * 从服务器读取运行时配置，并根据结果更新 axios baseURL。
 * 校验规则：若配置中的 apiBaseUrl 是绝对地址且与当前页面 origin 不同，
 * 则强制回退到 "/"，确保 API 调用始终命中当前托管服务器。
 */
async function initRuntimeConfig(): Promise<void> {
  try {
    const resp = await fetch('/tsmq-runtime-config.json', { cache: 'no-store' })
    if (!resp.ok) return
    const config: { apiBaseUrl?: string } = await resp.json()
    if (!config.apiBaseUrl) return

    let baseUrl = config.apiBaseUrl
    if (/^https?:\/\//i.test(baseUrl)) {
      try {
        const parsed = new URL(baseUrl)
        // 若绝对地址的 origin 与当前页面不一致，回退到相对路径
        if (parsed.origin !== window.location.origin) {
          baseUrl = '/'
        }
      } catch {
        baseUrl = '/'
      }
    }
    setBaseURL(baseUrl)
  } catch {
    // 运行时配置不可用（如开发模式未代理），保持默认值
  }
}

async function bootstrap(): Promise<void> {
  await initRuntimeConfig()

  const app = createApp(App)
  const pinia = createPinia()

  for (const [key, component] of Object.entries(ElementPlusIconsVue)) {
    app.component(key, component)
  }

  app.use(pinia)
  app.use(router)
  app.use(ElementPlus)
  setupDirectives(app)

  app.mount('#app')
}

bootstrap()
