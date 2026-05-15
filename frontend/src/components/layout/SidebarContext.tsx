'use client'

import { createContext, useContext, useState } from 'react'

interface SidebarContextType {
  isExpanded: boolean
  isMobileOpen: boolean
  toggleExpanded: () => void
  toggleMobile: () => void
  closeMobile: () => void
}

const SidebarContext = createContext<SidebarContextType>({
  isExpanded: true,
  isMobileOpen: false,
  toggleExpanded: () => {},
  toggleMobile: () => {},
  closeMobile: () => {},
})

function getInitialExpanded(): boolean {
  if (typeof window === 'undefined') return true
  return localStorage.getItem('sidebar_expanded') !== 'false'
}

export function SidebarProvider({ children }: { children: React.ReactNode }) {
  const [isExpanded, setIsExpanded] = useState(getInitialExpanded)
  const [isMobileOpen, setIsMobileOpen] = useState(false)

  const toggleExpanded = () => {
    setIsExpanded((prev) => {
      const next = !prev
      localStorage.setItem('sidebar_expanded', String(next))
      return next
    })
  }

  const toggleMobile = () => setIsMobileOpen((prev) => !prev)
  const closeMobile = () => setIsMobileOpen(false)

  return (
    <SidebarContext.Provider
      value={{
        isExpanded,
        isMobileOpen,
        toggleExpanded,
        toggleMobile,
        closeMobile,
      }}
    >
      {children}
    </SidebarContext.Provider>
  )
}

export const useSidebar = () => useContext(SidebarContext)
