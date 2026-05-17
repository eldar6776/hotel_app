'use client'

import { useEffect, useState, useCallback } from 'react'
import { housekeepingService } from '@/lib/housekeeping/housekeeping-service'
import type { DirtyRoomDto, WorkOrderDto } from '@/types/housekeeping'
import {
  PRIORITY_COLORS,
  PRIORITY_LABELS,
  CATEGORY_LABELS,
  STATUS_LABELS,
  STATUS_COLORS,
} from '@/types/housekeeping'
import { Sparkles, Wrench, Plus, Check, Eye, X } from 'lucide-react'

export default function HousekeepingPage() {
  const [activeTab, setActiveTab] = useState<'dirty' | 'workorders'>('dirty')
  const [dirtyRooms, setDirtyRooms] = useState<DirtyRoomDto[]>([])
  const [workOrders, setWorkOrders] = useState<WorkOrderDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [showWoForm, setShowWoForm] = useState(false)
  const [woForm, setWoForm] = useState({ roomId: '', category: 'Plumbing', priority: 'Medium', description: '' })
  const [inspectingRoom, setInspectingRoom] = useState<string | null>(null)
  const [inspectNote, setInspectNote] = useState('')

  const loadData = useCallback(async () => {
    setIsLoading(true)
    try {
      const [rooms, orders] = await Promise.all([
        housekeepingService.getDirtyRooms(),
        housekeepingService.getWorkOrders(),
      ])
      setDirtyRooms(rooms)
      setWorkOrders(orders)
    } catch {
      // silent
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => {
    loadData()
  }, [loadData])

  const handleMarkClean = async (roomId: string) => {
    try {
      await housekeepingService.markRoomClean(roomId)
      loadData()
    } catch {
      alert('Greska pri oznacavanju ciste sobe')
    }
  }

  const handleInspect = async (roomId: string, passed: boolean) => {
    try {
      await housekeepingService.inspectRoom(roomId, passed, inspectNote || undefined)
      setInspectingRoom(null)
      setInspectNote('')
      loadData()
    } catch {
      alert('Greska pri inspekciji')
    }
  }

  const handleCreateWorkOrder = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!woForm.roomId || !woForm.description.trim()) return
    try {
      await housekeepingService.createWorkOrder(woForm.roomId, woForm.category, woForm.priority, woForm.description)
      setShowWoForm(false)
      setWoForm({ roomId: '', category: 'Plumbing', priority: 'Medium', description: '' })
      loadData()
    } catch {
      alert('Greska pri kreiranju work ordera')
    }
  }

  const dirtyCount = dirtyRooms.length
  const openWoCount = workOrders.filter((w) => w.status === 'Open' || w.status === 'InProgress').length

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-semibold text-text">Housekeeping</h1>
          <p className="text-sm text-text-secondary mt-1">
            {dirtyCount} prljavih soba · {openWoCount} otvorenih naloga
          </p>
        </div>
        <button onClick={loadData} className="rounded-lg bg-primary-500 px-4 py-2 text-sm font-medium text-white hover:bg-primary-600">
          Osvjezi
        </button>
      </div>

      <div className="flex gap-2 border-b border-border">
        <button
          className={`px-4 py-2 text-sm font-medium transition ${activeTab === 'dirty' ? 'border-b-2 border-primary-500 text-primary-500' : 'text-text-secondary hover:text-text'}`}
          onClick={() => setActiveTab('dirty')}
        >
          <Sparkles className="h-4 w-4 inline mr-1" /> Prljave sobe ({dirtyCount})
        </button>
        <button
          className={`px-4 py-2 text-sm font-medium transition ${activeTab === 'workorders' ? 'border-b-2 border-primary-500 text-primary-500' : 'text-text-secondary hover:text-text'}`}
          onClick={() => setActiveTab('workorders')}
        >
          <Wrench className="h-4 w-4 inline mr-1" /> Nalozi za popravku ({workOrders.length})
        </button>
      </div>

      {isLoading ? (
        <div className="animate-pulse h-64 rounded-xl bg-gray-200 dark:bg-gray-700" />
      ) : activeTab === 'dirty' ? (
        dirtyRooms.length === 0 ? (
          <div className="rounded-xl border border-border bg-white dark:bg-gray-900 p-12 text-center">
            <Sparkles className="h-12 w-12 mx-auto text-emerald-500 mb-3" />
            <p className="text-sm font-medium text-text">Sve sobe su ciste</p>
            <p className="text-xs text-text-secondary mt-1">Nema prljavih soba za ciscenje</p>
          </div>
        ) : (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
            {dirtyRooms.map((room) => (
              <div key={room.id} className="rounded-xl border border-orange-200 dark:border-orange-800 bg-white dark:bg-gray-900 p-4 shadow-sm">
                <div className="flex items-start justify-between mb-3">
                  <div>
                    <p className="text-lg font-bold text-text">Soba {room.roomNumber}</p>
                    <p className="text-xs text-text-secondary">{room.building} · Kat {room.floor}</p>
                  </div>
                  <span className="rounded-full bg-orange-100 dark:bg-orange-900/30 px-2 py-0.5 text-xs font-medium text-orange-700 dark:text-orange-300">
                    Prljava
                  </span>
                </div>

                {inspectingRoom === room.id ? (
                  <div className="space-y-2 mt-2">
                    <input
                      placeholder="Napomena za inspekciju (opciono)"
                      value={inspectNote}
                      onChange={(e) => setInspectNote(e.target.value)}
                      className="w-full rounded-lg border border-border bg-gray-50 dark:bg-gray-800 px-3 py-2 text-xs text-text"
                    />
                    <div className="flex gap-2">
                      <button onClick={() => handleInspect(room.id, true)} className="flex-1 rounded-lg bg-emerald-500 px-3 py-1.5 text-xs font-medium text-white hover:bg-emerald-600">
                        <Check className="h-3 w-3 inline mr-1" /> Prosao
                      </button>
                      <button onClick={() => handleInspect(room.id, false)} className="flex-1 rounded-lg bg-red-500 px-3 py-1.5 text-xs font-medium text-white hover:bg-red-600">
                        <X className="h-3 w-3 inline mr-1" /> Nije prosao
                      </button>
                      <button onClick={() => { setInspectingRoom(null); setInspectNote('') }} className="rounded-lg px-2 py-1.5 text-xs text-text-secondary">
                        Odustani
                      </button>
                    </div>
                  </div>
                ) : (
                  <div className="flex gap-2 mt-2">
                    <button onClick={() => handleMarkClean(room.id)} className="flex-1 rounded-lg bg-emerald-500 px-3 py-2 text-xs font-medium text-white hover:bg-emerald-600">
                      <Check className="h-3.5 w-3.5 inline mr-1" /> Označi čistom
                    </button>
                    <button onClick={() => setInspectingRoom(room.id)} className="rounded-lg border border-border px-3 py-2 text-xs text-text-secondary hover:text-text">
                      <Eye className="h-3.5 w-3.5" />
                    </button>
                  </div>
                )}
              </div>
            ))}
          </div>
        )
      ) : (
        <div className="space-y-3">
          <div className="flex justify-end">
            <button
              onClick={() => setShowWoForm(!showWoForm)}
              className="flex items-center gap-1 rounded-lg bg-primary-500 px-4 py-2 text-sm font-medium text-white hover:bg-primary-600"
            >
              <Plus className="h-4 w-4" /> Novi nalog
            </button>
          </div>

          {showWoForm && (
            <form onSubmit={handleCreateWorkOrder} className="rounded-xl border border-border bg-white dark:bg-gray-900 p-4 space-y-3">
              <div className="grid grid-cols-2 gap-3">
                <select
                  value={woForm.roomId}
                  onChange={(e) => setWoForm({ ...woForm, roomId: e.target.value })}
                  className="rounded-lg border border-border bg-gray-50 dark:bg-gray-800 px-3 py-2 text-sm text-text"
                  required
                >
                  <option value="">Odaberi sobu...</option>
                  {dirtyRooms.map((r) => (
                    <option key={r.id} value={r.id}>{r.roomNumber} ({r.building})</option>
                  ))}
                </select>
                <select
                  value={woForm.category}
                  onChange={(e) => setWoForm({ ...woForm, category: e.target.value })}
                  className="rounded-lg border border-border bg-gray-50 dark:bg-gray-800 px-3 py-2 text-sm text-text"
                >
                  {Object.entries(CATEGORY_LABELS).map(([k, v]) => (
                    <option key={k} value={k}>{v}</option>
                  ))}
                </select>
              </div>
              <div className="grid grid-cols-2 gap-3">
                <select
                  value={woForm.priority}
                  onChange={(e) => setWoForm({ ...woForm, priority: e.target.value })}
                  className="rounded-lg border border-border bg-gray-50 dark:bg-gray-800 px-3 py-2 text-sm text-text"
                >
                  {Object.entries(PRIORITY_LABELS).map(([k, v]) => (
                    <option key={k} value={k}>{v}</option>
                  ))}
                </select>
                <input
                  placeholder="Opis kvara / problema"
                  value={woForm.description}
                  onChange={(e) => setWoForm({ ...woForm, description: e.target.value })}
                  className="rounded-lg border border-border bg-gray-50 dark:bg-gray-800 px-3 py-2 text-sm text-text"
                  required
                />
              </div>
              <div className="flex justify-end gap-2">
                <button type="button" onClick={() => setShowWoForm(false)} className="rounded-lg px-4 py-2 text-sm text-text-secondary hover:text-text">
                  Odustani
                </button>
                <button type="submit" className="rounded-lg bg-primary-500 px-4 py-2 text-sm font-medium text-white hover:bg-primary-600">
                  Kreiraj nalog
                </button>
              </div>
            </form>
          )}

          {workOrders.length === 0 ? (
            <div className="rounded-xl border border-border bg-white dark:bg-gray-900 p-12 text-center">
              <Wrench className="h-12 w-12 mx-auto text-text-secondary mb-3" />
              <p className="text-sm text-text-secondary">Nema naloga za popravku</p>
            </div>
          ) : (
            <div className="space-y-2">
              {workOrders.map((wo) => (
                <div key={wo.id} className="rounded-xl border border-border bg-white dark:bg-gray-900 p-4 shadow-sm">
                  <div className="flex items-start justify-between">
                    <div className="flex-1">
                      <div className="flex items-center gap-2 mb-1">
                        <span className={`rounded-full px-2 py-0.5 text-xs font-medium ${PRIORITY_COLORS[wo.priority]}`}>
                          {PRIORITY_LABELS[wo.priority]}
                        </span>
                        <span className={`rounded-full px-2 py-0.5 text-xs font-medium ${STATUS_COLORS[wo.status]}`}>
                          {STATUS_LABELS[wo.status]}
                        </span>
                        <span className="text-xs text-text-secondary">{CATEGORY_LABELS[wo.category]}</span>
                      </div>
                      <p className="text-sm font-medium text-text">{wo.description}</p>
                      <p className="text-xs text-text-secondary mt-1">
                        Soba: {wo.roomNumber || 'N/A'} · Kreiran: {new Date(wo.createdAt).toLocaleDateString()}
                        {wo.resolvedAt && ` · Rijesen: ${new Date(wo.resolvedAt).toLocaleDateString()}`}
                      </p>
                      {wo.resolutionNotes && (
                        <p className="text-xs text-text-secondary mt-1 italic">Napomena: {wo.resolutionNotes}</p>
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
  )
}
