import apiClient from '@/lib/api/client'
import { tokenStorage } from '@/lib/auth/token-storage'

export interface AuthResponse {
  accessToken: string
  refreshToken: string
}

export interface CurrentUser {
  id: string
  email: string
  role: string
  name: string
}

export const authService = {
  async login(email: string, password: string): Promise<AuthResponse> {
    const response = await apiClient.post<AuthResponse>('/auth/login', {
      email,
      password,
    })
    return response.data
  },

  async refresh(refreshToken: string): Promise<AuthResponse> {
    const response = await apiClient.post<AuthResponse>('/auth/refresh', {
      refreshToken,
    })
    return response.data
  },

  async logout() {
    tokenStorage.clear()
    if (typeof window !== 'undefined') {
      window.location.href = '/login'
    }
  },

  getCurrentUser(): CurrentUser | null {
    const token = tokenStorage.getAccessToken()
    if (!token) return null
    try {
      const payload = JSON.parse(atob(token.split('.')[1]))
      return {
        id: payload.sub,
        email: payload.email,
        role: payload.role,
        name: payload.name,
      }
    } catch {
      return null
    }
  },
}
