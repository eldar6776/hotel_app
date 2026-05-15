'use client'

import { useState } from 'react'
import type { RoomDto, RoomStatus } from '@/types/rooms'
import { roomService } from '@/lib/rooms/room-service'

const statusColors: Record<string, string> = {
  Free: 'bg-emerald-500 text-white',
  Occupied: 'bg-blue-500 text-white',
  Reserved: 'bg-amber-500 text-white',
  Dirty: 'bg-orange-500 text-white',
  OutOfOrder: 'bg-red-500 text-white',
  OutOfService: 'bg-gray-500 text-white',
}

const statusLabels: Record<string, string> = {
  Free: 'Slobodna',
  Occupied: 'Zauzeta',
  Reserved: 'Rezervirana',
  Dirty: 'Prljava',
  OutOfOrder: 'Van funkcije',
  OutOfService: 'Van servisa',
}

interface RoomDetailProps {
  room: RoomDto
  isOpen: boolean
  onClose: () => void
  onStatusChange?: () => void
}

export function RoomDetail({ room, isOpen, onClose, onStatusChange }: RoomDetailProps) {
  const [activeTab, setActiveTab] = useState<'info' | 'status'>('info')
  const [isChanging, setIsChanging] = useState(false)

  if (!isOpen) return null

  const handleStatusChange = async (newStatus: RoomStatus) => {
    try {
      setIsChanging(true)
      await roomService.updateStatus(room.id, newStatus)
      onStatusChange?.()
    } catch {
      alert('Greska pri promjeni statusa')
    } finally {
      setIsChanging(false)
    }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50" onClick={onClose}>
      <div className="w-full max-w-2xl rounded-xl bg-surface p-6 shadow-xl" onClick={(e) => e.stopPropagation()}>
        <div className="mb-4 flex items-center justify-between">
          <h2 className="text-xl font-bold text-text">Soba {room.roomNumber}</h2>
          <button onClick={onClose} className="text-text-secondary hover:text-text" aria-label="Zatvori">&times;</button>
        </div>

        <div className="mb-4 flex gap-2 border-b border-border">
          <button
            className={`px-4 py-2 text-sm font-medium ${activeTab === 'info' ? 'border-b-2 border-primary-500 text-primary-500' : 'text-text-secondary'}`}
            onClick={() => setActiveTab('info')}
          >
            Informacije
          </button>
          <button
            className={`px-4 py-2 text-sm font-medium ${activeTab === 'status' ? 'border-b-2 border-primary-500 text-primary-500' : 'text-text-secondary'}`}
            onClick={() => setActiveTab('status')}
          >
            Status
          </button>
        </div>

        {activeTab === 'info' && (
          <div className="space-y-3">
            <div className="grid grid-cols-2 gap-3 text-sm">
              <div>
                <span className="text-text-secondary">Zgrada:</span>
                <p className="font-medium text-text">{room.buildingName}</p>
              </div>
              <div>
                <span className="text-text-secondary">Kat:</span>
                <p className="font-medium text-text">{room.floor}</p>
              </div>
              <div>
                <span className="text-text-secondary">Tip:</span>
                <p className="font-medium text-text">{room.roomTypeName}</p>
              </div>
              <div>
                <span className="text-text-secondary">Kapacitet:</span>
                <p className="font-medium text-text">{room.baseCapacity} - {room.maxCapacity}</p>
              </div>
              <div>
                <span className="text-text-secondary">Cijena:</span>
                <p className="font-medium text-text">{room.basePrice ? `${room.basePrice.toFixed(2)} EUR` : '-'}</p>
              </div>
              <div>
                <span className="text-text-secondary">Status:</span>
                <p><span className={`rounded-full px-2 py-0.5 text-xs font-medium ${statusColors[room.status]}`}>{statusLabels[room.status]}</span></p>
              </div>
            </div>
            {room.notes && (
              <div>
                <span className="text-text-secondary">Napomene:</span>
                <p className="text-sm text-text">{room.notes}</p>
              </div>
            )}
          </div>
        )}

        {activeTab === 'status' && (
          <div className="space-y-3">
            <p className="text-sm text-text-secondary">Promijeni status sobe:</p>
            <div className="flex flex-wrap gap-2">
              {(['Free', 'Reserved', 'Dirty', 'OutOfOrder', 'OutOfService'] as RoomStatus[]).map((s) => (
                <button
                  key={s}
                  disabled={isChanging || room.status === s}
                  onClick={() => handleStatusChange(s)}
                  className={`rounded-lg px-3 py-1.5 text-sm font-medium transition ${
                    room.status === s
                      ? 'bg-primary-500 text-white'
                      : `${statusColors[s]} opacity-70 hover:opacity-100 disabled:opacity-40`
                  }`}
                >
                  {statusLabels[s]}
                </button>
              ))}
            </div>
            {isChanging && <p className="text-sm text-text-secondary">Mijenjam status...</p>}
          </div>
        )}
      </div>
    </div>
  )
}
