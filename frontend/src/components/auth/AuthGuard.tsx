'use client'

import { useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { tokenStorage } from '@/lib/auth/token-storage'

export function AuthGuard({ children }: { children: React.ReactNode }) {
  const token = tokenStorage.getAccessToken()
  const router = useRouter()

  useEffect(() => {
    if (!token) {
      router.push('/login')
    }
  }, [token, router])

  if (!token) return null
  return <>{children}</>
}
