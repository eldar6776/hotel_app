'use client'

import { useState } from 'react'
import type { BookingDto } from '@/types/bookings'
import { roomService } from '@/lib/rooms/room-service'
import apiClient from '@/lib/api/client'

interface Props {
  booking: BookingDto
  onClose: () => void
  onSuccess: () => void
}

export function CheckInModal({ booking, onClose, onSuccess }: Props) {
  const [roomId, setRoomId] = useState('')
  const [rooms, setRooms] = useState<{ id: string; roomNumber: string }[]>([])
  const [isLoading, setIsLoading] = useState(false)
  const [warnings, setWarnings] = useState<string[]>([])
  const [loaded, setLoaded] = useState(false)

  const loadRooms = async () => {
    try {
      const res = await roomService.getRooms({ status: ['Free'] })
      setRooms(res.items.map((r) => ({ id: r.id, roomNumber: r.roomNumber })))
    } catch {
      // silent
    }
    setLoaded(true)
  }

  if (!loaded) { loadRooms(); return null }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!roomId) return
    setIsLoading(true)
    try {
      const res = await apiClient.post('/reception/check-in', {
        bookingId: booking.id,
        roomId,
        guestDocuments: null,
        rfidCardCode: null,
      })
      if (res.data?.warnings?.length) setWarnings(res.data.warnings)
      else onSuccess()
    } catch {
      alert('Greška pri check-inu')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm" onClick={onClose}>
      <div className="w-full max-w-md rounded-xl bg-surface p-6 shadow-2xl border border-border" onClick={(e) => e.stopPropagation()}>
        <h2 className="text-lg font-bold text-text mb-2">Check-in: {booking.guestName}</h2>
        <p className="text-sm text-text-secondary mb-4">{booking.rooms?.[0]?.roomTypeName} · {booking.nights}n</p>

        {warnings.length > 0 && (
          <div className="mb-4 rounded-lg bg-amber-50 dark:bg-amber-900/20 border border-amber-200 dark:border-amber-800 p-3">
            {warnings.map((w, i) => <p key={i} className="text-xs text-amber-700 dark:text-amber-300">{w}</p>)}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-3">
          <select
            value={roomId}
            onChange={(e) => setRoomId(e.target.value)}
            className="w-full rounded-lg border border-border bg-surface px-3 py-2 text-sm text-text"
            required
          >
            <option value="">Odaberi sobu...</option>
            {rooms.map((r) => (
              <option key={r.id} value={r.id}>{r.roomNumber}</option>
            ))}
          </select>
          <div className="flex gap-2 justify-end">
            <button type="button" onClick={onClose} className="rounded-lg px-4 py-2 text-sm text-text-secondary hover:text-text">Odustani</button>
            <button type="submit" disabled={isLoading || !roomId} className="rounded-lg bg-primary-500 px-4 py-2 text-sm font-medium text-white hover:bg-primary-600 disabled:opacity-50">
              {isLoading ? '...' : 'Check-in'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}
