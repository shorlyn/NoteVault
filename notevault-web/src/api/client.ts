import axios from 'axios'
import type { InternalAxiosRequestConfig } from 'axios'
import { useAuthStore } from '../stores/auth'

type RetryableRequest = InternalAxiosRequestConfig & {
  __retry?: boolean
}

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || '/',
})

let refreshPromise: Promise<void> | null = null

function goLogin() {
  const auth = useAuthStore()
  auth.logout()
  if (window.location.pathname !== '/login') {
    window.location.replace('/login')
  }
}

api.interceptors.request.use((config) => {
  const auth = useAuthStore()
  if (auth.accessToken) config.headers.Authorization = `Bearer ${auth.accessToken}`
  return config
})

api.interceptors.response.use(undefined, async (error) => {
  const auth = useAuthStore()
  const original = error.config as RetryableRequest | undefined
  const status = error.response?.status
  const url = original?.url || ''
  const isAuthEndpoint = url.includes('/api/auth/')

  if (status !== 401 || !original) {
    throw error
  }

  if (isAuthEndpoint) {
    goLogin()
    throw error
  }

  if (auth.refreshToken && !original.__retry) {
    original.__retry = true
    try {
      refreshPromise ??= auth.refresh().finally(() => {
        refreshPromise = null
      })
      await refreshPromise
      if (!auth.accessToken) throw error
      original.headers = original.headers || {}
      original.headers.Authorization = `Bearer ${auth.accessToken}`
      return api(original)
    } catch {
      goLogin()
      throw error
    }
  }

  goLogin()
  throw error
})
