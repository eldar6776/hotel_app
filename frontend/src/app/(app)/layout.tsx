'use client'

import { useState, useCallback, useEffect } from 'react'
import { AuthGuard } from '@/components/auth/AuthGuard'
import AppLayout from '@/components/layout/AppLayout'
import { HelpProvider, useHelp } from '@/components/help/HelpProvider'
import { TourProvider } from '@/components/help/TourProvider'
import { CommandPalette } from '@/components/help/CommandPalette'

function HelpOverlay({
  children,
}: {
  children: React.ReactNode
}) {
  const { isHelpMode } = useHelp()

  return (
    <>
      {isHelpMode && (
        <div className="pointer-events-none fixed inset-0 z-20">
          <div className="pointer-events-auto absolute bottom-4 left-1/2 -translate-x-1/2 rounded-xl bg-primary-600 px-4 py-2 text-sm text-white shadow-lg">
            Kliknite na element za koji zelite pomoc. Pritisnite Esc za izlaz.
          </div>
        </div>
      )}
      {children}
    </>
  )
}

function AppShell({ children }: { children: React.ReactNode }) {
  const [commandOpen, setCommandOpen] = useState(false)
  const { toggleHelpMode } = useHelp()

  const handleSearchOpen = useCallback(() => {
    setCommandOpen(true)
  }, [])

  // Global keyboard shortcuts
  useEffect(() => {
    const handler = (e: KeyboardEvent) => {
      // ? toggles help mode (avoid when typing in inputs)
      if (
        e.key === '?' &&
        !e.ctrlKey &&
        !e.metaKey &&
        !['INPUT', 'TEXTAREA', 'SELECT'].includes(
          (e.target as HTMLElement)?.tagName || ''
        )
      ) {
        e.preventDefault()
        toggleHelpMode()
      }
    }
    window.addEventListener('keydown', handler)
    return () => window.removeEventListener('keydown', handler)
  }, [toggleHelpMode])

  return (
    <>
      <AppLayout onHelpToggle={toggleHelpMode} onSearchOpen={handleSearchOpen}>
        <HelpOverlay>{children}</HelpOverlay>
      </AppLayout>
      <CommandPalette open={commandOpen} onClose={() => setCommandOpen(false)} />
    </>
  )
}

export default function AppRouteLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <AuthGuard>
      <HelpProvider>
        <TourProvider>
          <AppShell>{children}</AppShell>
        </TourProvider>
      </HelpProvider>
    </AuthGuard>
  )
}
