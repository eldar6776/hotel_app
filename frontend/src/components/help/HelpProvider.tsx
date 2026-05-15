'use client'

import {
  createContext,
  useContext,
  useState,
  useCallback,
  useRef,
  useEffect,
} from 'react'
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

const HELP_MODE_TIMEOUT = 30000 // 30 seconds

export function HelpProvider({ children }: { children: React.ReactNode }) {
  const [isHelpMode, setIsHelpMode] = useState(false)
  const [activeTooltip, setActiveTooltip] = useState<string | null>(null)
  const timeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  const clearHelpTimeout = useCallback(() => {
    if (timeoutRef.current) {
      clearTimeout(timeoutRef.current)
      timeoutRef.current = null
    }
  }, [])

  const scheduleAutoDisable = useCallback(() => {
    clearHelpTimeout()
    timeoutRef.current = setTimeout(() => {
      setIsHelpMode(false)
      setActiveTooltip(null)
    }, HELP_MODE_TIMEOUT)
  }, [clearHelpTimeout])

  const toggleHelpMode = useCallback(() => {
    setIsHelpMode((prev) => {
      const next = !prev
      if (next) {
        scheduleAutoDisable()
      } else {
        clearHelpTimeout()
        setActiveTooltip(null)
      }
      return next
    })
  }, [scheduleAutoDisable, clearHelpTimeout])

  const showTooltip = useCallback(
    (id: string) => {
      setActiveTooltip(id)
      scheduleAutoDisable()
    },
    [scheduleAutoDisable]
  )

  const hideTooltip = useCallback(() => {
    setActiveTooltip(null)
    scheduleAutoDisable()
  }, [scheduleAutoDisable])

  const getContent = useCallback((id: string) => helpContent[id], [])

  // Reset timer on any user interaction while help mode is active
  useEffect(() => {
    if (!isHelpMode) return

    const events = ['click', 'keydown', 'mousemove', 'touchstart']
    const handler = () => scheduleAutoDisable()

    events.forEach((event) => window.addEventListener(event, handler))
    return () => {
      events.forEach((event) => window.removeEventListener(event, handler))
      clearHelpTimeout()
    }
  }, [isHelpMode, scheduleAutoDisable, clearHelpTimeout])

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
