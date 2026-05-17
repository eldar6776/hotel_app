'use client'

import type { BookingDto } from '@/types/bookings'
import { STATUS_LABELS } from '@/types/bookings'

interface Props {
  bookings: BookingDto[]
  onCheckIn: (b: BookingDto) => void
}

export function ArrivalsPanel({ bookings, onCheckIn }: Props) {
  if (bookings.length === 0) return (
    <div className="text-center py-12 text-text-secondary text-sm">Nema dolazaka za danas.</div>
  )

  return (
    <div className="space-y-2">
      {bookings.map((b) => (
        <div key={b.id} className="flex items-center justify-between rounded-lg border border-border bg-surface p-3 hover:bg-surface-secondary">
          <div>
            <p className="font-medium text-text">{b.guestName}</p>
            <p className="text-xs text-text-secondary">
              {b.rooms?.[0]?.roomTypeName || 'N/A'} · {b.nights}n · {b.totalPrice.toFixed(0)} {b.currency}
            </p>
            <p className="text-xs text-text-secondary">
              {b.adultCount} odraslih{b.childCount > 0 ? ` · ${b.childCount} djece` : ''}
            </p>
          </div>
          <div className="flex items-center gap-2">
            <span className="text-xs text-text-secondary">{STATUS_LABELS[b.status as keyof typeof STATUS_LABELS] || b.status}</span>
            <button
              onClick={() => onCheckIn(b)}
              className="rounded-lg bg-primary-500 px-3 py-1.5 text-xs font-medium text-white hover:bg-primary-600"
            >
              Check-in
            </button>
          </div>
        </div>
      ))}
    </div>
  )
}
