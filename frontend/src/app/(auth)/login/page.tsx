'use client'

import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { Hotel } from 'lucide-react'
import Input from '@/components/ui/Input'
import Button from '@/components/ui/Button'
import Checkbox from '@/components/ui/Checkbox'
import Alert from '@/components/ui/Alert'
import { HelpTooltip } from '@/components/help/HelpTooltip'
import { authService } from '@/lib/auth/auth-service'
import { tokenStorage } from '@/lib/auth/token-storage'

export default function LoginPage() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [remember, setRemember] = useState(false)
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)
  const router = useRouter()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!email || !password) {
      setError('Molimo unesite email i lozinku.')
      return
    }
    setLoading(true)
    setError('')
    try {
      const { accessToken, refreshToken } = await authService.login(
        email,
        password
      )
      tokenStorage.set(accessToken, refreshToken, remember)
      router.push('/dashboard')
    } catch (err: unknown) {
      if (err && typeof err === 'object' && 'response' in err) {
        const axiosErr = err as { response?: { status?: number; data?: { message?: string } } }
        if (axiosErr.response?.status === 401) {
          setError('Pogresan email ili lozinka.')
        } else {
          setError(axiosErr.response?.data?.message || 'Greska na serveru. Pokusajte ponovo.')
        }
      } else if (err instanceof Error) {
        setError('Nije moguce povezati se sa serverom.')
      } else {
        setError('Doslo je do neocekivane greske.')
      }
    } finally {
      setLoading(false)
    }
  }

  // Ctrl+Enter shortcut to submit form
  useEffect(() => {
    const handler = (e: KeyboardEvent) => {
      if (e.ctrlKey && e.key === 'Enter') {
        e.preventDefault()
        const form = document.getElementById('login-form') as HTMLFormElement | null
        if (form) form.requestSubmit()
      }
    }
    window.addEventListener('keydown', handler)
    return () => window.removeEventListener('keydown', handler)
  }, [])

  return (
    <div className="w-full max-w-md rounded-xl bg-surface p-8 shadow-lg border border-border">
      <div className="mb-8 text-center">
        <div className="mx-auto mb-4 flex h-12 w-12 items-center justify-center rounded-xl bg-primary-100 dark:bg-primary-900/30">
          <Hotel className="h-6 w-6 text-primary-600 dark:text-primary-400" />
        </div>
        <h1 className="text-2xl font-semibold text-text">HotelPRO</h1>
        <p className="mt-2 text-sm text-text-secondary">Prijava u sistem</p>
      </div>

      <form id="login-form" onSubmit={handleSubmit} className="space-y-4">
        <HelpTooltip id="login-email">
          <Input
            label="Email"
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="ime@hotel.com"
            autoComplete="email"
            required
          />
        </HelpTooltip>

        <HelpTooltip id="login-password">
          <Input
            label="Lozinka"
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="••••••••"
            autoComplete="current-password"
            required
          />
        </HelpTooltip>

        <HelpTooltip id="login-remember">
          <Checkbox
            label="Zapamti me"
            checked={remember}
            onChange={setRemember}
          />
        </HelpTooltip>

        {error && <Alert type="error">{error}</Alert>}

        <Button type="submit" loading={loading} className="w-full">
          Prijavi se
        </Button>
      </form>

      <p className="mt-6 text-center text-xs text-text-secondary">
        Zaboravili ste lozinku? Kontaktirajte administratora.
      </p>
    </div>
  )
}
