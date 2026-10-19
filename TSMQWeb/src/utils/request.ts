import axios from 'axios'
import type { AxiosResponse, InternalAxiosRequestConfig } from 'axios'
import { ElMessage } from 'element-plus'
import { useUserStore } from '@/stores/user'
import logger from '@/utils/logger'

const request = axios.create({
  baseURL: import.meta.env.DEV ? '/api' : '/',
  timeout: 15000,
  headers: {
    'Content-Type': 'application/json'
  }
})

/**
 * 由 initRuntimeConfig() 调用，将 axios baseURL 更新为服务器托管地址。
 * 确保生产环境下前端始终连接到当前提供页面的同一台服务器。
 */
export function setBaseURL(url: string): void {
  request.defaults.baseURL = url
}

request.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    logger.debug('Request', `发起请求: ${config.method?.toUpperCase()} ${config.url}`)

    const userStore = useUserStore()
    if (userStore.token) {
      if (!config.data) {
        config.data = {}
      }
      config.data = {
        ...config.data,
        LoginedToken: userStore.token
      }
    }
    return config
  },
  error => {
    logger.error('Request', '请求拦截器错误', error)
    return Promise.reject(error)
  }
)

request.interceptors.response.use(
  (response: AxiosResponse) => {
    const res = response.data

    logger.debug('Response', `请求成功: ${response.config.method?.toUpperCase()} ${response.config.url}`, {
      code: res.Code,
      message: res.Message
    })

    if (res.Code !== undefined && res.Code !== 'Success') {
      logger.warn('Response', `请求返回错误: ${res.Message}`, {
        code: res.Code,
        url: response.config.url
      })

      const jwtExpiredMessages = ['JWT令牌无效或已过期', '未提供JWT令牌', 'JWT服务未配置']
      if (res.Code === 'Unauthorized' || jwtExpiredMessages.some(msg => res.Message?.includes(msg))) {
        logger.warn('Response', 'JWT授权失败，跳转到登录页')
        const userStore = useUserStore()
        userStore.logout()
        window.location.href = '/login'
        return Promise.reject(new Error(res.Message))
      }

      ElMessage.error(res.Message || '请求失败')
      return Promise.reject(new Error(res.Message || '请求失败'))
    }

    return res
  },
  error => {
    logger.error('Response', '请求失败', {
      url: error.config?.url,
      status: error.response?.status,
      message: error.message,
      data: error.response?.data
    })

    if (error.response?.status === 401) {
      logger.warn('Response', '认证失败，跳转到登录页')
      const userStore = useUserStore()
      userStore.logout()
      window.location.href = '/login'
    } else if (error.response?.data) {
      const errorData = error.response.data
      const errorMessage = errorData.Message || errorData.message || error.message || '请求失败'
      ElMessage.error(errorMessage)
      return Promise.reject(new Error(errorMessage))
    } else {
      ElMessage.error(error.message || '网络错误')
    }
    return Promise.reject(error)
  }
)

export default request
