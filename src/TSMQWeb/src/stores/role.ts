import { defineStore } from 'pinia'
import { roleApi, userApi } from '@/api'
import logger from '@/utils/logger'

interface RoleState {
  roles: any[]
  loading: boolean
}

export const useRoleStore = defineStore('role', {
  state: (): RoleState => ({
    roles: [],
    loading: false
  }),

  actions: {
    async getRoleList(params: any = {}) {
      logger.info('RoleStore', '开始获取角色列表', params)
      this.loading = true
      try {
        const res = await roleApi.getRoleList(params)
        this.roles = (res as any).Roles || []
        logger.info('RoleStore', `获取角色列表成功，共 ${this.roles.length} 条数据`)
        return res
      } catch (error) {
        logger.error('RoleStore', '获取角色列表失败', error)
        throw error
      } finally {
        this.loading = false
      }
    },

    async createRole(data: any) {
      logger.info('RoleStore', '开始创建角色', data)
      try {
        const res = await roleApi.createRole(data)
        logger.info('RoleStore', '创建角色成功', { name: data.name })
        return res
      } catch (error) {
        logger.error('RoleStore', '创建角色失败', error)
        throw error
      }
    },

    async updateRole(data: any) {
      logger.info('RoleStore', '开始更新角色', data)
      try {
        const res = await roleApi.updateRole(data)
        logger.info('RoleStore', '更新角色成功', { id: data.id, name: data.name })
        return res
      } catch (error) {
        logger.error('RoleStore', '更新角色失败', error)
        throw error
      }
    },

    async deleteRole(data: any) {
      logger.info('RoleStore', '开始删除角色', data)
      try {
        const res = await roleApi.deleteRole(data)
        logger.info('RoleStore', '删除角色成功', { id: data.id })
        return res
      } catch (error) {
        logger.error('RoleStore', '删除角色失败', error)
        throw error
      }
    },

    async assignUserRoles(data: any) {
      logger.info('RoleStore', '开始分配用户角色', data)
      try {
        const res = await userApi.assignUserRoles(data)
        logger.info('RoleStore', '分配用户角色成功', { userId: data.userId, roleCount: data.roleIds?.length })
        return res
      } catch (error) {
        logger.error('RoleStore', '分配用户角色失败', error)
        throw error
      }
    }
  }
})
