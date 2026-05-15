'use client'

import { useState } from 'react'
import type { RoomDto, RoomStatus, RoomOutOfOrderDto } from '@/types/rooms'
import { roomService } from '@/lib/rooms/room-service'
import apiClient from '@/lib/api/client'

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

const oooReasons = [
  'Kvar na vodovodu',
  'Kvar na struji',
  'Kvar na klimi',
  'Renoviranje',
  'Ciscenje dubinsko',
  'Ostalo',
]

interface RoomDetailProps {
  room: RoomDto
  isOpen: boolean
  onClose: () => void
  onStatusChange?: () => void
}

export function RoomDetail({ room, isOpen, onClose, onStatusChange }: RoomDetailProps) {
  const [activeTab, setActiveTab] = useState<'info' | 'status' | 'ooo'>('info')
  const [isChanging, setIsChanging] = useState(false)
  const [oooEntries, setOooEntries] = useState<RoomOutOfOrderDto[]>([])
  const [showOooForm, setShowOooForm] = useState(false)
  const [oooForm, setOooForm] = useState({ reason: oooReasons[0], description: '', startDate: '', endDate: '' })
  const [isLoadingOoo, setIsLoadingOoo] = useState(false)

  const loadOooEntries = async () => {
    try {
      setIsLoadingOoo(true)
      const res = await apiClient.get(`/v2/rooms/${room.id}/ooo`)
      setOooEntries(res.data)
    } catch {
      // silent
    } finally {
      setIsLoadingOoo(false)
    }
  }

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

  const handleCreateOoo = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await apiClient.post(`/v2/rooms/${room.id}/ooo`, {
        reason: oooForm.reason,
        description: oooForm.description || null,
        startDate: oooForm.startDate || new Date().toISOString(),
        endDate: oooForm.endDate || null,
      })
      setShowOooForm(false)
      setOooForm({ reason: oooReasons[0], description: '', startDate: '', endDate: '' })
      onStatusChange?.()
      loadOooEntries()
    } catch {
      alert('Greska pri kreiranju OOO')
    }
  }

  const handleResolveOoo = async (id: string) => {
    try {
      await apiClient.post(`/v2/rooms/${room.id}/ooo/${id}/resolve`, {
        resolutionNotes: 'Reseno',
      })
      onStatusChange?.()
      loadOooEntries()
    } catch {
      alert('Greska pri rjesavanju OOO')
    }
  }

  if (!isOpen) return null

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50" onClick={onClose}>
      <div className="w-full max-w-2xl rounded-xl bg-surface p-6 shadow-xl max-h-[90vh] overflow-y-auto" onClick={(e) => e.stopPropagation()}>
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
          <button
            className={`px-4 py-2 text-sm font-medium ${activeTab === 'ooo' ? 'border-b-2 border-primary-500 text-primary-500' : 'text-text-secondary'}`}
            onClick={() => { setActiveTab('ooo'); loadOooEntries() }}
          >
            Van funkcije
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

        {activeTab === 'ooo' && (
          <div className="space-y-4">
            <div className="flex items-center justify-between">
              <p className="text-sm text-text-secondary">Upravljanje van funkcije statusom</p>
              <button
                onClick={() => setShowOooForm(!showOooForm)}
                className="rounded-lg bg-primary-500 px-3 py-1.5 text-sm font-medium text-white hover:bg-primary-600"
              >
                {showOooForm ? 'Odustani' : '+ Dodaj OOO'}
              </button>
            </div>

            {showOooForm && (
              <form onSubmit={handleCreateOoo} className="rounded-lg border border-border bg-surface-secondary p-4 space-y-3">
                <select
                  value={oooForm.reason}
                  onChange={(e) => setOooForm({ ...oooForm, reason: e.target.value })}
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2 text-sm text-text"
                >
                  {oooReasons.map((r) => <option key={r} value={r}>{r}</option>)}
                </select>
                <textarea
                  placeholder="Opis (opciono)"
                  value={oooForm.description}
                  onChange={(e) => setOooForm({ ...oooForm, description: e.target.value })}
                  className="w-full rounded-lg border border-border bg-surface px-3 py-2 text-sm text-text"
                  rows={2}
                />
                <div className="grid grid-cols-2 gap-3">
                  <input
                    type="datetime-local"
                    value={oooForm.startDate}
                    onChange={(e) => setOooForm({ ...oooForm, startDate: e.target.value })}
                    className="rounded-lg border border-border bg-surface px-3 py-2 text-sm text-text"
                  />
                  <input
                    type="datetime-local"
                    value={oooForm.endDate}
                    onChange={(e) => setOooForm({ ...oooForm, endDate: e.target.value })}
                    className="rounded-lg border border-border bg-surface px-3 py-2 text-sm text-text"
                  />
                </div>
                <button type="submit" className="rounded-lg bg-primary-500 px-4 py-2 text-sm font-medium text-white">
                  Kreiraj
                </button>
              </form>
            )}

            {isLoadingOoo ? (
              <div className="animate-pulse h-20 rounded-lg bg-surface-tertiary" />
            ) : oooEntries.length === 0 ? (
              <p className="text-sm text-text-secondary text-center py-4">Nema OOO zapisa</p>
            ) : (
              <div className="space-y-2">
                {oooEntries.map((entry) => (
                  <div key={entry.id} className="rounded-lg border border-border bg-surface-secondary p-3">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-sm font-medium text-text">{entry.reason}</p>
                        <p className="text-xs text-text-secondary">
                          {new Date(entry.startDate).toLocaleDateString()}
                          {entry.endDate ? ` - ${new Date(entry.endDate).toLocaleDateString()}` : ' (bez kraja)'}
                        </p>
                        {entry.description && <p className="text-xs text-text-secondary mt-1">{entry.description}</p>}
                      </div>
                      <div className="flex items-center gap-2">
                        <span className={`rounded-full px-2 py-0.5 text-xs font-medium ${
                          entry.status === 'Active' ? 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-300' : 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-300'
                        }`}>
                          {entry.status === 'Active' ? 'Aktivan' : 'Resen'}
                        </span>
                        {entry.status === 'Active' && (
                          <button
                            onClick={() => handleResolveOoo(entry.id)}
                            className="rounded-lg bg-emerald-500 px-2 py-1 text-xs font-medium text-white hover:bg-emerald-600"
                          >
                            Resi
                          </button>
                        )}
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  )
}
