'use client'

import { useEffect, useState } from 'react'
import type { AmenityDto } from '@/types/rooms'
import apiClient from '@/lib/api/client'
import Button from '@/components/ui/Button'

export default function AmenitiesPage() {
  const [amenities, setAmenities] = useState<AmenityDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [showForm, setShowForm] = useState(false)
  const [draggedIdx, setDraggedIdx] = useState<number | null>(null)
  const [form, setForm] = useState({ name: '', icon: '' })

  const loadData = async () => {
    setIsLoading(true)
    try {
      const res = await apiClient.get('/amenities')
      setAmenities(res.data)
    } catch {
      alert('Greska pri ucitavanju')
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    loadData()
  }, [])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await apiClient.post('/amenities', { ...form, sortOrder: amenities.length })
      setShowForm(false)
      setForm({ name: '', icon: '' })
      loadData()
    } catch {
      alert('Greska pri kreiranju')
    }
  }

  const toggleActive = async (id: string, current: boolean) => {
    try {
      await apiClient.put(`/amenities/${id}`, { isActive: !current })
      loadData()
    } catch {
      alert('Greska')
    }
  }

  const moveAmenity = async (idx: number, direction: 'up' | 'down') => {
    const newIdx = direction === 'up' ? idx - 1 : idx + 1
    if (newIdx < 0 || newIdx >= amenities.length) return
    const reordered = [...amenities]
    const [moved] = reordered.splice(idx, 1)
    reordered.splice(newIdx, 0, moved)
    setAmenities(reordered)

    try {
      for (let i = 0; i < reordered.length; i++) {
        await apiClient.put(`/amenities/${reordered[i].id}`, { sortOrder: i })
      }
    } catch {
      loadData()
    }
  }

  const handleDragStart = (idx: number) => setDraggedIdx(idx)

  const handleDragOver = (e: React.DragEvent, idx: number) => {
    e.preventDefault()
    if (draggedIdx === null || draggedIdx === idx) return
    const reordered = [...amenities]
    const [moved] = reordered.splice(draggedIdx, 1)
    reordered.splice(idx, 0, moved)
    setAmenities(reordered)
    setDraggedIdx(idx)
  }

  const handleDragEnd = async () => {
    setDraggedIdx(null)
    try {
      for (let i = 0; i < amenities.length; i++) {
        await apiClient.put(`/amenities/${amenities[i].id}`, { sortOrder: i })
      }
    } catch {
      loadData()
    }
  }

  if (isLoading) return <div className="animate-pulse h-64 rounded-xl bg-surface-tertiary"></div>

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-text">Sadrzaji</h1>
        {showForm ? (
          <Button variant="secondary" onClick={() => setShowForm(false)}>Odustani</Button>
        ) : (
          <button onClick={() => setShowForm(true)} className="rounded-lg bg-primary-500 px-4 py-2 text-sm font-medium text-white hover:bg-primary-600">+ Novi Sadrzaj</button>
        )}
      </div>

      {showForm && (
        <form onSubmit={handleSubmit} className="rounded-xl border border-border bg-surface p-4 space-y-3">
          <div className="flex gap-3">
            <input required placeholder="Naziv" value={form.name} onChange={e => setForm({...form, name: e.target.value})} className="rounded-lg border border-border bg-surface px-3 py-2 text-sm text-text flex-1" />
            <input placeholder="Ikona (emoji)" value={form.icon} onChange={e => setForm({...form, icon: e.target.value})} className="rounded-lg border border-border bg-surface px-3 py-2 text-sm text-text w-24 text-center" />
            <button type="submit" className="rounded-lg bg-primary-500 px-4 py-2 text-sm font-medium text-white">Kreiraj</button>
          </div>
        </form>
      )}

      <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-3">
        {amenities.map((a, idx) => (
          <div
            key={a.id}
            draggable
            onDragStart={() => handleDragStart(idx)}
            onDragOver={(e) => handleDragOver(e, idx)}
            onDragEnd={handleDragEnd}
            className={`flex items-center justify-between rounded-xl border border-border bg-surface p-4 cursor-grab active:cursor-grabbing transition-shadow ${
              draggedIdx === idx ? 'opacity-50 shadow-xl' : 'hover:shadow-md'
            }`}
          >
            <div className="flex items-center gap-3">
              <div className="flex items-center gap-1">
                {idx > 0 && (
                  <button onClick={() => moveAmenity(idx, 'up')} className="text-text-secondary hover:text-text text-xs px-1">▲</button>
                )}
                {idx < amenities.length - 1 && (
                  <button onClick={() => moveAmenity(idx, 'down')} className="text-text-secondary hover:text-text text-xs px-1">▼</button>
                )}
              </div>
              {a.icon && <span className="text-2xl">{a.icon}</span>}
              <span className="font-medium text-text">{a.name}</span>
              <span className="text-[10px] text-text-secondary">#{idx}</span>
            </div>
            <button onClick={(e) => { e.stopPropagation(); toggleActive(a.id, a.isActive) }} className={`rounded-full px-2 py-0.5 text-xs font-medium ${a.isActive ? 'bg-emerald-500 text-white' : 'bg-surface-tertiary text-text-secondary'}`}>
              {a.isActive ? 'Aktivan' : 'Neaktivan'}
            </button>
          </div>
        ))}
      </div>
    </div>
  )
}
