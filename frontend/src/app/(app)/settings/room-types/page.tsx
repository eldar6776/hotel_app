'use client'

import { useEffect, useState, useCallback } from 'react'
import apiClient from '@/lib/api/client'
import { BedDouble, Plus, Edit2, Trash2 } from 'lucide-react'

interface RoomTypeDto {
  id: string
  name: string
  code: string
  baseCapacity: number
  maxCapacity: number
  defaultPrice: number
  description: string | null
  isActive: boolean
  sortOrder: number
}

export default function RoomTypesPage() {
  const [roomTypes, setRoomTypes] = useState<RoomTypeDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [showForm, setShowForm] = useState(false)
  const [form, setForm] = useState({ name: '', code: '', baseCapacity: 1, maxCapacity: 2, defaultPrice: 0, description: '' })
  const [editingId, setEditingId] = useState<string | null>(null)

  const loadData = useCallback(async () => {
    setIsLoading(true)
    try {
      const res = await apiClient.get<RoomTypeDto[]>('/room-types')
      setRoomTypes(res.data)
    } catch { /* silent */ }
    finally { setIsLoading(false) }
  }, [])

  useEffect(() => { loadData() }, [loadData])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      if (editingId) {
        await apiClient.put(`/room-types/${editingId}`, form)
      } else {
        await apiClient.post('/room-types', { ...form, isActive: true })
      }
      setShowForm(false)
      setEditingId(null)
      setForm({ name: '', code: '', baseCapacity: 1, maxCapacity: 2, defaultPrice: 0, description: '' })
      loadData()
    } catch { alert('Greska pri spremanju') }
  }

  const handleEdit = (rt: RoomTypeDto) => {
    setForm({ name: rt.name, code: rt.code, baseCapacity: rt.baseCapacity, maxCapacity: rt.maxCapacity, defaultPrice: rt.defaultPrice, description: rt.description || '' })
    setEditingId(rt.id)
    setShowForm(true)
  }

  const handleDelete = async (id: string) => {
    if (!confirm('Obrisati ovaj tip sobe?')) return
    try { await apiClient.delete(`/room-types/${id}`); loadData() }
    catch { alert('Greska pri brisanju') }
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-semibold text-text">Tipovi soba</h1>
          <p className="text-sm text-text-secondary mt-1">{roomTypes.length} tipova</p>
        </div>
        <button onClick={() => { setShowForm(!showForm); setEditingId(null); setForm({ name: '', code: '', baseCapacity: 1, maxCapacity: 2, defaultPrice: 0, description: '' }) }} className="flex items-center gap-1 rounded-lg bg-primary-500 px-4 py-2 text-sm font-medium text-white hover:bg-primary-600">
          <Plus className="h-4 w-4" /> Novi tip
        </button>
      </div>

      {showForm && (
        <form onSubmit={handleSubmit} className="rounded-xl border border-border bg-white dark:bg-gray-900 p-4 space-y-3">
          <div className="grid grid-cols-2 gap-3">
            <input required placeholder="Naziv" value={form.name} onChange={e => setForm({ ...form, name: e.target.value })} className="rounded-lg border border-border bg-gray-50 dark:bg-gray-800 px-3 py-2 text-sm text-text" />
            <input required placeholder="Kod" value={form.code} onChange={e => setForm({ ...form, code: e.target.value.toUpperCase() })} className="rounded-lg border border-border bg-gray-50 dark:bg-gray-800 px-3 py-2 text-sm text-text" />
          </div>
          <div className="grid grid-cols-3 gap-3">
            <input type="number" min="1" placeholder="Osnovni kap." value={form.baseCapacity} onChange={e => setForm({ ...form, baseCapacity: Number(e.target.value) })} className="rounded-lg border border-border bg-gray-50 dark:bg-gray-800 px-3 py-2 text-sm text-text" />
            <input type="number" min="1" placeholder="Max kap." value={form.maxCapacity} onChange={e => setForm({ ...form, maxCapacity: Number(e.target.value) })} className="rounded-lg border border-border bg-gray-50 dark:bg-gray-800 px-3 py-2 text-sm text-text" />
            <input type="number" step="0.01" min="0" placeholder="Cijena" value={form.defaultPrice} onChange={e => setForm({ ...form, defaultPrice: Number(e.target.value) })} className="rounded-lg border border-border bg-gray-50 dark:bg-gray-800 px-3 py-2 text-sm text-text" />
          </div>
          <input placeholder="Opis (opciono)" value={form.description} onChange={e => setForm({ ...form, description: e.target.value })} className="w-full rounded-lg border border-border bg-gray-50 dark:bg-gray-800 px-3 py-2 text-sm text-text" />
          <div className="flex justify-end gap-2">
            <button type="button" onClick={() => { setShowForm(false); setEditingId(null) }} className="rounded-lg px-4 py-2 text-sm text-text-secondary hover:text-text">Odustani</button>
            <button type="submit" className="rounded-lg bg-primary-500 px-4 py-2 text-sm font-medium text-white hover:bg-primary-600">{editingId ? 'Azuriraj' : 'Kreiraj'}</button>
          </div>
        </form>
      )}

      {isLoading ? (
        <div className="animate-pulse h-48 rounded-xl bg-gray-200 dark:bg-gray-700" />
      ) : roomTypes.length === 0 ? (
        <div className="rounded-xl border border-border bg-white dark:bg-gray-900 p-12 text-center">
          <BedDouble className="h-12 w-12 mx-auto text-text-secondary mb-3" />
          <p className="text-sm text-text-secondary">Nema tipova soba</p>
        </div>
      ) : (
        <div className="rounded-xl border border-border bg-white dark:bg-gray-900 overflow-hidden">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-border">
                <th className="text-left py-3 px-4 text-text-secondary font-medium">Kod</th>
                <th className="text-left py-3 px-4 text-text-secondary font-medium">Naziv</th>
                <th className="text-left py-3 px-4 text-text-secondary font-medium">Kapacitet</th>
                <th className="text-left py-3 px-4 text-text-secondary font-medium">Cijena</th>
                <th className="text-left py-3 px-4 text-text-secondary font-medium">Status</th>
                <th className="text-right py-3 px-4 text-text-secondary font-medium">Akcije</th>
              </tr>
            </thead>
            <tbody>
              {roomTypes.map(rt => (
                <tr key={rt.id} className="border-b border-border last:border-0">
                  <td className="py-3 px-4 font-mono text-text">{rt.code}</td>
                  <td className="py-3 px-4 text-text font-medium">{rt.name}</td>
                  <td className="py-3 px-4 text-text-secondary">{rt.baseCapacity} - {rt.maxCapacity}</td>
                  <td className="py-3 px-4 text-text font-medium">{rt.defaultPrice.toFixed(2)} €</td>
                  <td className="py-3 px-4"><span className={`rounded-full px-2 py-0.5 text-xs font-medium ${rt.isActive ? 'bg-emerald-100 text-emerald-700' : 'bg-gray-100 text-gray-600'}`}>{rt.isActive ? 'Aktivan' : 'Neaktivan'}</span></td>
                  <td className="py-3 px-4 text-right">
                    <div className="flex justify-end gap-1">
                      <button onClick={() => handleEdit(rt)} className="rounded p-1 text-text-secondary hover:text-primary-600"><Edit2 className="h-4 w-4" /></button>
                      <button onClick={() => handleDelete(rt.id)} className="rounded p-1 text-text-secondary hover:text-red-600"><Trash2 className="h-4 w-4" /></button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}
