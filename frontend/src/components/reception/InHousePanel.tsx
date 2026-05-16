'use client'

import type { BookingDto } from '@/types/bookings'

interface Props {
  bookings: BookingDto[]
  onCheckOut: (b: BookingDto) => void
}

export function InHousePanel({ bookings, onCheckOut }: Props) {
  if (bookings.length === 0) return (
    <div className="text-center py-12 text-text-secondary text-sm">Nema gostiju u hotelu.</div>
  )

  return (
    <div className="space-y-2">
      {bookings.map((b) => (
        <div key={b.id} className="flex items-center justify-between rounded-lg border border-border bg-surface p-3 hover:bg-surface-secondary">
          <div>
            <p className="font-medium text-text">{b.guestName}</p>
            <p className="text-xs text-text-secondary">
              Soba {b.rooms?.[0]?.roomNumber || 'N/A'} · Do: {new Date(b.departureDate).toLocaleDateString()}
            </p>
            <p className="text-xs text-text-muted">{b.nights}n boravka</p>
          </div>
          <button
            onClick={() => onCheckOut(b)}
            className="rounded-lg bg-amber-500 px-3 py-1.5 text-xs font-medium text-white hover:bg-amber-600"
          >
            Check-out
          </button>
        </div>
      ))}
    </div>
  )
}
