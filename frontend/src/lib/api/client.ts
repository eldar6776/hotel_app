import axios from 'axios'
import { tokenStorage } from '@/lib/auth/token-storage'

const apiClient = axios.create({
  baseURL: (process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5149') + '/api/v2',
  headers: { 'Content-Type': 'application/json' },
})

apiClient.interceptors.request.use((config) => {
  const token = tokenStorage.getAccessToken()
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  if (typeof window !== 'undefined') {
    const hotelCode = localStorage.getItem('hotelpro_hotel_code') || 'HVA'
    config.headers['X-Hotel-Code'] = hotelCode
  }
  return config
})

apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config
    if (
      error.response?.status === 401 &&
      !originalRequest._retry
    ) {
      originalRequest._retry = true
      try {
        const refreshToken = tokenStorage.getRefreshToken()
        if (!refreshToken) throw new Error('No refresh token')
        const response = await axios.post(
          `${apiClient.defaults.baseURL}/auth/refresh`,
          { refreshToken }
        )
        const { accessToken, refreshToken: newRefresh } = response.data
        tokenStorage.set(accessToken, newRefresh, true)
        originalRequest.headers.Authorization = `Bearer ${accessToken}`
        return apiClient(originalRequest)
      } catch {
        tokenStorage.clear()
        if (typeof window !== 'undefined') {
          window.location.href = '/login'
        }
        return Promise.reject(error)
      }
    }
    return Promise.reject(error)
  }
)

export default apiClient
