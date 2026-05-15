'use client'

import { useState, useRef, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { ChevronDown, LogOut, User } from 'lucide-react'
import { authService } from '@/lib/auth/auth-service'

export default function NavbarUserMenu() {
  const [isOpen, setIsOpen] = useState(false)
  const ref = useRef<HTMLDivElement>(null)
  const router = useRouter()
  const user = authService.getCurrentUser()

  useEffect(() => {
    function handleClickOutside(e: MouseEvent) {
      if (ref.current && !ref.current.contains(e.target as Node)) {
        setIsOpen(false)
      }
    }
    document.addEventListener('mousedown', handleClickOutside)
    return () => document.removeEventListener('mousedown', handleClickOutside)
  }, [])

  if (!user) return null

  const initials = user.name
    .split(' ')
    .map((n) => n[0])
    .join('')
    .toUpperCase()
    .slice(0, 2)

  return (
    <div ref={ref} className="relative">
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="flex items-center gap-2 rounded-lg px-2 py-1.5 text-sm text-text hover:bg-surface-secondary dark:hover:bg-surface-tertiary transition-colors"
      >
        <div className="flex h-7 w-7 items-center justify-center rounded-full bg-primary-100 text-xs font-semibold text-primary-700 dark:bg-primary-900/30 dark:text-primary-300">
          {initials}
        </div>
        <span className="hidden md:inline">{user.name}</span>
        <ChevronDown className="h-3.5 w-3.5 text-text-secondary" />
      </button>

      {isOpen && (
        <div className="absolute right-0 top-full mt-1 w-48 rounded-lg border border-border bg-surface py-1 shadow-lg">
          <button
            onClick={() => {
              setIsOpen(false)
              router.push('/settings/profile')
            }}
            className="flex w-full items-center gap-2 px-3 py-2 text-sm text-text hover:bg-surface-secondary dark:hover:bg-surface-tertiary"
          >
            <User className="h-4 w-4" />
            Profil
          </button>
          <button
            onClick={() => authService.logout()}
            className="flex w-full items-center gap-2 px-3 py-2 text-sm text-error hover:bg-surface-secondary dark:hover:bg-surface-tertiary"
          >
            <LogOut className="h-4 w-4" />
            Odjavi se
          </button>
        </div>
      )}
    </div>
  )
}
