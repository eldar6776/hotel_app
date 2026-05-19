'use client'

import { useEffect, useState } from 'react'
import type { TariffDto, RoomTypeDto } from '@/types/rooms'
import { roomService } from '@/lib/rooms/room-service'
import apiClient from '@/lib/api/client'
import Button from '@/components/ui/Button'

type SortField = 'name' | 'price' | 'validFrom'

export default function TariffsPage() {
  const [tariffs, setTariffs] = useState<TariffDto[]>([])
  const [roomTypes, setRoomTypes] = useState<RoomTypeDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [showForm, setShowForm] = useState(false)
  const [editTarget, setEditTarget] = useState<TariffDto | null>(null)
  const [sortField, setSortField] = useState<SortField>('name')
  const [sortAsc, setSortAsc] = useState(true)
  const [form, setForm] = useState({ name: '', roomTypeId: '', validFrom: '', validTo: '', basePrice: '', currency: 'EUR' })

  const loadData = async () => {
    setIsLoading(true)
    try {
      const [t, rt] = await Promise.all([
        apiClient.get('/tariffs'),
        roomService.getRoomTypes(),
      ])
      setTariffs(t.data)
      setRoomTypes(rt)
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

  const handleSort = (field: SortField) => {
    if (sortField === field) {
      setSortAsc(!sortAsc)
    } else {
      setSortField(field)
      setSortAsc(true)
    }
  }

  const sortedTariffs = [...tariffs].sort((a, b) => {
    let cmp = 0
    if (sortField === 'name') cmp = a.name.localeCompare(b.name)
    else if (sortField === 'price') cmp = a.basePrice - b.basePrice
    else if (sortField === 'validFrom') cmp = (a.validFrom || '').localeCompare(b.validFrom || '')
    return sortAsc ? cmp : -cmp
  })

  const startEdit = (t: TariffDto) => {
    setEditTarget(t)
    setForm({
      name: t.name,
      roomTypeId: t.roomTypeId || '',
      validFrom: t.validFrom ? new Date(t.validFrom).toISOString().split('T')[0] : '',
      validTo: t.validTo ? new Date(t.validTo).toISOString().split('T')[0] : '',
      basePrice: t.basePrice.toString(),
      currency: t.currency,
    })
    setShowForm(true)
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      const payload = {
        name: form.name,
        roomTypeId: form.roomTypeId || null,
        validFrom: form.validFrom || null,
        validTo: form.validTo || null,
        basePrice: parseFloat(form.basePrice),
        currency: form.currency,
      }

      if (editTarget) {
        await apiClient.put(`/tariffs/${editTarget.id}`, payload)
      } else {
        await apiClient.post('/tariffs', payload)
      }
      setShowForm(false)
      setEditTarget(null)
      setForm({ name: '', roomTypeId: '', validFrom: '', validTo: '', basePrice: '', currency: 'EUR' })
      loadData()
    } catch {
      alert('Greska pri snimanju tarife')
    }
  }

  const handleDelete = async (id: string, name: string) => {
    if (!confirm(`Obrisati tarifu "${name}"?`)) return
    try {
      await apiClient.delete(`/tariffs/${id}`)
      loadData()
    } catch {
      alert('Greska pri brisanju')
    }
  }

  const toggleActive = async (id: string, current: boolean) => {
    try {
      await apiClient.put(`/tariffs/${id}`, { isActive: !current })
      loadData()
    } catch {
      alert('Greska')
    }
  }

  const sortIndicator = (field: SortField) => {
    if (sortField !== field) return ''
    return sortAsc ? ' ↑' : ' ↓'
  }

  if (isLoading) return <div className="animate-pulse h-64 rounded-xl bg-surface-tertiary"></div>

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-text">Tarife</h1>
        {showForm ? (
          <Button variant="secondary" onClick={() => { setShowForm(false); setEditTarget(null) }}>Odustani</Button>
        ) : (
          <button onClick={() => setShowForm(true)} className="rounded-lg bg-primary-500 px-4 py-2 text-sm font-medium text-white hover:bg-primary-600">+ Nova Tarifa</button>
        )}
      </div>

      {showForm && (
        <form onSubmit={handleSubmit} className="rounded-xl border border-border bg-surface p-4 space-y-3">
          <h2 className="font-medium text-text">{editTarget ? `Izmjena: ${editTarget.name}` : 'Nova tarifa'}</h2>
          <div className="grid grid-cols-2 gap-3">
            <input required placeholder="Naziv" value={form.name} onChange={e => setForm({...form, name: e.target.value})} className="rounded-lg border border-border bg-surface px-3 py-2 text-sm text-text" />
            <select value={form.roomTypeId} onChange={e => setForm({...form, roomTypeId: e.target.value})} className="rounded-lg border border-border bg-surface px-3 py-2 text-sm text-text">
              <option value="">Globalna</option>
              {roomTypes.map(rt => <option key={rt.id} value={rt.id}>{rt.name}</option>)}
            </select>
            <input type="date" value={form.validFrom} onChange={e => setForm({...form, validFrom: e.target.value})} className="rounded-lg border border-border bg-surface px-3 py-2 text-sm text-text" />
            <input type="date" value={form.validTo} onChange={e => setForm({...form, validTo: e.target.value})} className="rounded-lg border border-border bg-surface px-3 py-2 text-sm text-text" />
            <input required type="number" step="0.01" placeholder="Cijena" value={form.basePrice} onChange={e => setForm({...form, basePrice: e.target.value})} className="rounded-lg border border-border bg-surface px-3 py-2 text-sm text-text" />
            <select value={form.currency} onChange={e => setForm({...form, currency: e.target.value})} className="rounded-lg border border-border bg-surface px-3 py-2 text-sm text-text">
              <option value="EUR">EUR</option>
              <option value="USD">USD</option>
              <option value="GBP">GBP</option>
            </select>
          </div>
          <button type="submit" className="rounded-lg bg-primary-500 px-4 py-2 text-sm font-medium text-white">
            {editTarget ? 'Sačuvaj' : 'Kreiraj'}
          </button>
        </form>
      )}

      <div className="rounded-xl border border-border bg-surface overflow-hidden">
        <table className="w-full text-sm">
          <thead className="border-b border-border bg-surface-secondary">
            <tr>
              <th className="px-4 py-3 text-left text-text-secondary font-medium cursor-pointer hover:text-text select-none" onClick={() => handleSort('name')}>
                Naziv{sortIndicator('name')}
              </th>
              <th className="px-4 py-3 text-left text-text-secondary font-medium">Tip Sobe</th>
              <th className="px-4 py-3 text-left text-text-secondary font-medium cursor-pointer hover:text-text select-none" onClick={() => handleSort('price')}>
                Cijena{sortIndicator('price')}
              </th>
              <th className="px-4 py-3 text-left text-text-secondary font-medium cursor-pointer hover:text-text select-none" onClick={() => handleSort('validFrom')}>
                Valjanost{sortIndicator('validFrom')}
              </th>
              <th className="px-4 py-3 text-left text-text-secondary font-medium">Aktivna</th>
              <th className="px-4 py-3"></th>
            </tr>
          </thead>
          <tbody>
            {sortedTariffs.map(t => (
              <tr key={t.id} className="border-b border-border hover:bg-surface-secondary">
                <td className="px-4 py-3 text-text">{t.name}</td>
                <td className="px-4 py-3 text-text-secondary">{t.roomTypeName || 'Globalna'}</td>
                <td className="px-4 py-3 text-text">{t.basePrice.toFixed(2)} {t.currency}</td>
                <td className="px-4 py-3 text-text-secondary">{t.validFrom ? new Date(t.validFrom).toLocaleDateString() : '-'} do {t.validTo ? new Date(t.validTo).toLocaleDateString() : '-'}</td>
                <td className="px-4 py-3">
                  <button onClick={() => toggleActive(t.id, t.isActive)} className={`rounded-full px-2 py-0.5 text-xs font-medium ${t.isActive ? 'bg-emerald-500 text-white' : 'bg-surface-tertiary text-text-secondary'}`}>
                    {t.isActive ? 'Aktivna' : 'Neaktivna'}
                  </button>
                </td>
                <td className="px-4 py-3">
                  <div className="flex gap-2">
                    <button onClick={() => startEdit(t)} className="text-primary-500 text-xs hover:underline">Izmijeni</button>
                    <button onClick={() => handleDelete(t.id, t.name)} className="text-red-500 text-xs hover:underline">Obriši</button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}
