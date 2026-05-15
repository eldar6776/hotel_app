'use client'

import { createContext, useContext, useState, useCallback } from 'react'
import { helpContent } from '@/lib/help/help-content'

interface HelpContextType {
  isHelpMode: boolean
  toggleHelpMode: () => void
  activeTooltip: string | null
  showTooltip: (id: string) => void
  hideTooltip: () => void
  getContent: (id: string) => string | undefined
}

const HelpContext = createContext<HelpContextType>({
  isHelpMode: false,
  toggleHelpMode: () => {},
  activeTooltip: null,
  showTooltip: () => {},
  hideTooltip: () => {},
  getContent: () => undefined,
})

export function HelpProvider({ children }: { children: React.ReactNode }) {
  const [isHelpMode, setIsHelpMode] = useState(false)
  const [activeTooltip, setActiveTooltip] = useState<string | null>(null)

  const toggleHelpMode = useCallback(() => {
    setIsHelpMode((prev) => {
      if (prev) {
        setActiveTooltip(null)
      }
      return !prev
    })
  }, [])

  const showTooltip = useCallback((id: string) => {
    setActiveTooltip(id)
  }, [])

  const hideTooltip = useCallback(() => {
    setActiveTooltip(null)
  }, [])

  const getContent = useCallback(
    (id: string) => helpContent[id],
    []
  )

  return (
    <HelpContext.Provider
      value={{
        isHelpMode,
        toggleHelpMode,
        activeTooltip,
        showTooltip,
        hideTooltip,
        getContent,
      }}
    >
      {children}
    </HelpContext.Provider>
  )
}

export const useHelp = () => useContext(HelpContext)
