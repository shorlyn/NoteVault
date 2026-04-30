import axios from 'axios'
import { useAuthStore } from '../stores/auth'

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5111',
})

api.interceptors.request.use((config) => {
  const auth = useAuthStore()
  if (auth.accessToken) config.headers.Authorization = `Bearer ${auth.accessToken}`
  return config
})

api.interceptors.response.use(undefined, async (error) => {
  const auth = useAuthStore()
  const original = error.config
  if (error.response?.status === 401 && auth.refreshToken && !original.__retry) {
    original.__retry = true
    await auth.refresh()
    original.headers.Authorization = `Bearer ${auth.accessToken}`
    return api(original)
  }
  throw error
})
