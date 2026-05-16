'use client'

export function QuickActions() {
  return (
    <div className="flex flex-wrap gap-2">
      <div className="rounded-lg border border-border bg-surface px-4 py-3 flex items-center gap-3">
        <span className="text-lg">🏨</span>
        <div>
          <p className="text-sm font-medium text-text">Recepcija</p>
          <p className="text-[11px] text-text-secondary">Check-in / Check-out</p>
        </div>
      </div>
      <div className="rounded-lg border border-emerald-500/30 bg-emerald-50 dark:bg-emerald-900/10 px-4 py-3 flex items-center gap-3">
        <span className="text-lg">✅</span>
        <div>
          <p className="text-sm font-medium text-text">Auto-refresh</p>
          <p className="text-[11px] text-text-secondary">svakih 30s</p>
        </div>
      </div>
    </div>
  )
}
