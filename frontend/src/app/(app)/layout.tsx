'use client'

import { useState, useCallback, useEffect } from 'react'
import dynamic from 'next/dynamic'

const AppLayout = dynamic(() => import('@/components/layout/AppLayout'), { ssr: false })
const TourProvider = dynamic(() => import('@/components/help/TourProvider').then(m => ({ default: m.TourProvider })), { ssr: false })
const HelpProvider = dynamic(() => import('@/components/help/HelpProvider').then(m => ({ default: m.HelpProvider })), { ssr: false })
const AuthGuard = dynamic(() => import('@/components/auth/AuthGuard').then(m => ({ default: m.AuthGuard })), { ssr: false })
const CommandPalette = dynamic(() => import('@/components/help/CommandPalette').then(m => ({ default: m.CommandPalette })), { ssr: false })

function AppShell({ children }: { children: React.ReactNode }) {
  const [commandOpen, setCommandOpen] = useState(false)
  return (
    <>
      <AppLayout onHelpToggle={() => {}} onSearchOpen={() => setCommandOpen(true)}>
        {children}
      </AppLayout>
      <CommandPalette open={commandOpen} onClose={() => setCommandOpen(false)} />
    </>
  )
}

export default function AppRouteLayout({ children }: { children: React.ReactNode }) {
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
