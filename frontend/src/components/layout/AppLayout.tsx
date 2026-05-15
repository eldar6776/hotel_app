'use client'

import { SidebarProvider, useSidebar } from '@/components/layout/SidebarContext'
import Sidebar from '@/components/layout/Sidebar'
import MobileSidebar from '@/components/layout/MobileSidebar'
import Navbar from '@/components/layout/Navbar'

function AppLayoutInner({
  children,
  onHelpToggle,
  onSearchOpen,
}: {
  children: React.ReactNode
  onHelpToggle?: () => void
  onSearchOpen?: () => void
}) {
  const { isExpanded } = useSidebar()

  return (
    <div className="flex h-screen overflow-hidden bg-surface-secondary">
      <Sidebar />
      <MobileSidebar />
      <div
        className={`flex flex-1 flex-col overflow-hidden transition-[margin] duration-300 ease-in-out ${
          isExpanded ? 'ml-[var(--sidebar-width)]' : 'ml-[var(--sidebar-collapsed-width)]'
        } max-lg:!ml-0`}
      >
        <Navbar onHelpToggle={onHelpToggle} onSearchOpen={onSearchOpen} />
        <main className="flex-1 overflow-y-auto p-4 md:p-6">
          {children}
        </main>
      </div>
    </div>
  )
}

export default function AppLayout({
  children,
  onHelpToggle,
  onSearchOpen,
}: {
  children: React.ReactNode
  onHelpToggle?: () => void
  onSearchOpen?: () => void
}) {
  return (
    <SidebarProvider>
      <AppLayoutInner onHelpToggle={onHelpToggle} onSearchOpen={onSearchOpen}>
        {children}
      </AppLayoutInner>
    </SidebarProvider>
  )
}
