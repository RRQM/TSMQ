import { defineStore } from 'pinia'
import { userApi } from '@/api'
import type { LoginRequest } from '@/api'
import logger from '@/utils/logger'

interface UserInfo {
  account: string
}

interface UserState {
  token: string
  userInfo: UserInfo | null
  permissions: string[]
}

export const useUserStore = defineStore('user', {
  state: (): UserState => ({
    token: localStorage.getItem('token') || '',
    userInfo: null,
    permissions: []
  }),

  getters: {
    isLogin: (state): boolean => !!state.token,

    hasPermission: state => {
      return (permission: string): boolean => {
        if (!permission) return true
        return state.permissions.includes(permission)
      }
    }
  },

  actions: {
    async login(loginData: LoginRequest) {
      logger.info('UserStore', '用户开始登录', { account: loginData.Account })
      try {
        const res = await userApi.login(loginData) as any
        if (res.LoginedToken) {
          this.token = res.LoginedToken
          localStorage.setItem('token', res.LoginedToken)
          this.userInfo = { account: loginData.Account }
          this.permissions = []
          logger.info('UserStore', '用户登录成功', { account: loginData.Account })
        }
        return res
      } catch (error) {
        logger.error('UserStore', '用户登录失败', error)
        throw error
      }
    },

    async register(registerData: any) {
      logger.info('UserStore', '用户开始注册', { account: registerData.Account })
      try {
        const res = await userApi.register(registerData)
        logger.info('UserStore', '用户注册成功', { account: registerData.Account })
        return res
      } catch (error) {
        logger.error('UserStore', '用户注册失败', error)
        throw error
      }
    },

    async getUserInfo(): Promise<UserInfo | null> {
      return this.userInfo
    },

    logout() {
      logger.info('UserStore', '用户登出', { account: this.userInfo?.account })
      this.token = ''
      this.userInfo = null
      this.permissions = []
      localStorage.removeItem('token')
    }
  }
})
