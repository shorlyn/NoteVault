import { defineStore } from 'pinia'
import { authApi } from '../api'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    accessToken: localStorage.getItem('nv_access') || '',
    refreshToken: localStorage.getItem('nv_refresh') || '',
    mustChangePassword: localStorage.getItem('nv_must_change') === 'true',
    user: JSON.parse(localStorage.getItem('nv_user') || 'null') as null | { id: string; username: string },
  }),
  actions: {
    setSession(data: any) {
      this.accessToken = data.accessToken
      this.refreshToken = data.refreshToken
      this.mustChangePassword = data.mustChangePassword
      this.user = data.user
      localStorage.setItem('nv_access', this.accessToken)
      localStorage.setItem('nv_refresh', this.refreshToken)
      localStorage.setItem('nv_must_change', String(this.mustChangePassword))
      localStorage.setItem('nv_user', JSON.stringify(this.user))
    },
    async login(username: string, password: string) {
      this.setSession(await authApi.login(username, password))
    },
    async refresh() {
      if (!this.refreshToken) return this.logout()
      this.setSession(await authApi.refresh(this.refreshToken))
    },
    logout() {
      this.accessToken = ''
      this.refreshToken = ''
      this.user = null
      localStorage.removeItem('nv_access')
      localStorage.removeItem('nv_refresh')
      localStorage.removeItem('nv_user')
    },
  },
})
