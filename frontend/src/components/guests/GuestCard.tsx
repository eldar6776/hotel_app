'use client'

import type { GuestDto } from '@/types/guests'

export function GuestCard({ guest, onClick }: { guest: GuestDto; onClick: () => void }) {
  return (
    <div
      className="flex items-center justify-between rounded-lg border border-border bg-surface p-3 cursor-pointer hover:bg-surface-secondary transition"
      onClick={onClick}
    >
      <div>
        <p className="font-medium text-text">{guest.firstName} {guest.lastName}</p>
        <p className="text-xs text-text-secondary">
          {guest.email || guest.phone || 'Nema kontakt'}
          {guest.city && ` · ${guest.city}`}
        </p>
      </div>
      <span className="text-xs text-text-secondary">{guest.isCompany ? guest.companyName || 'Pravno lice' : 'Fizicko lice'}</span>
    </div>
  )
}
