import { CalendarCheck, CalendarDays } from 'lucide-react'
import Badge from '@/components/ui/Badge'
import type { UpcomingCheckin } from '@/lib/dashboard/dashboard-service'

export default function UpcomingCheckins({
  checkins,
}: {
  checkins: UpcomingCheckin[]
}) {
  const today = new Date().toISOString().split('T')[0]

  return (
    <div className="rounded-xl bg-surface shadow-sm border border-border overflow-hidden">
      <div className="border-b border-border px-4 py-3">
        <h3 className="text-sm font-semibold text-text">Nadolazeci check-in</h3>
      </div>
      <div className="divide-y divide-border">
        {checkins.length === 0 ? (
          <div className="px-4 py-8 text-center text-sm text-text-secondary">
            Nema nadolazecih check-inova
          </div>
        ) : (
          checkins.map((c) => (
            <div
              key={c.id}
              className="flex items-center gap-3 px-4 py-3 hover:bg-surface-secondary dark:hover:bg-surface-tertiary"
            >
              <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-primary-100 text-primary-600 dark:bg-primary-900/30 dark:text-primary-400">
                {c.arrivalDate === today ? (
                  <CalendarCheck className="h-4 w-4" />
                ) : (
                  <CalendarDays className="h-4 w-4" />
                )}
              </div>
              <div className="flex-1 min-w-0">
                <p className="text-sm font-medium text-text truncate">
                  {c.guestName}
                </p>
                <p className="text-xs text-text-secondary">
                  Soba {c.roomNumber} &middot; {c.nights} nocenja
                </p>
              </div>
              <div className="text-right">
                <p className="text-sm text-text">{c.arrivalDate}</p>
                {c.arrivalDate === today && (
                  <Badge variant="today">Danas</Badge>
                )}
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  )
}
