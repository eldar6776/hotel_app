'use client'

import { useHelp } from '@/components/help/HelpProvider'

export function HelpTooltip({
  id,
  children,
}: {
  id: string
  children: React.ReactNode
}) {
  const { isHelpMode, activeTooltip, showTooltip, hideTooltip, getContent } =
    useHelp()
  const content = getContent(id)
  const isActive = activeTooltip === id

  if (!isHelpMode) return <>{children}</>

  return (
    <div className="relative inline-block">
      <div
        className={`cursor-help rounded transition-all ${
          isActive
            ? 'ring-2 ring-primary-400'
            : 'ring-1 ring-primary-400/50 hover:ring-2 hover:ring-primary-400'
        }`}
        onClick={(e) => {
          e.stopPropagation()
          if (isActive) {
            hideTooltip()
          } else {
            showTooltip(id)
          }
        }}
        data-help-id={id}
      >
        {children}
      </div>
      {isActive && content && (
        <div className="absolute left-0 top-full z-50 mt-2 w-64 rounded-lg bg-surface p-3 shadow-xl border border-border text-sm text-text">
          <p>{content}</p>
          <button
            onClick={(e) => {
              e.stopPropagation()
              hideTooltip()
            }}
            className="mt-2 text-xs text-text-secondary hover:text-text"
          >
            Zatvori
          </button>
        </div>
      )}
    </div>
  )
}
