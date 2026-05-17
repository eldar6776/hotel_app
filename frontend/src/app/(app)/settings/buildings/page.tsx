'use client'

import { useEffect, useState, useCallback } from 'react'
import apiClient from '@/lib/api/client'
import { Building2, Plus, Edit2, Trash2 } from 'lucide-react'

interface BuildingDto {
  id: string
  name: string
  code: string
  address: string | null
  city: string | null
  isActive: boolean
  createdAt: string
}

export default function BuildingsPage() {
  const [buildings, setBuildings] = useState<BuildingDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [showForm, setShowForm] = useState(false)
  const [form, setForm] = useState({ name: '', code: '', address: '', city: '' })
  const [editingId, setEditingId] = useState<string | null>(null)

  const loadData = useCallback(async () => {
    setIsLoading(true)
    try {
      const res = await apiClient.get<BuildingDto[]>('/buildings')
      setBuildings(res.data)
    } catch { /* silent */ }
    finally { setIsLoading(false) }
  }, [])

  useEffect(() => { loadData() }, [loadData])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      if (editingId) {
        await apiClient.put(`/buildings/${editingId}`, { name: form.name, code: form.code, address: form.address || null, city: form.city || null })
      } else {
        await apiClient.post('/buildings', { name: form.name, code: form.code, address: form.address || null, city: form.city || null, isActive: true })
      }
      setShowForm(false)
      setEditingId(null)
      setForm({ name: '', code: '', address: '', city: '' })
      loadData()
    } catch { alert('Greska pri spremanju') }
  }

  const handleEdit = (b: BuildingDto) => {
    setForm({ name: b.name, code: b.code, address: b.address || '', city: b.city || '' })
    setEditingId(b.id)
    setShowForm(true)
  }

  const handleDelete = async (id: string) => {
    if (!confirm('Obrisati ovu zgradu?')) return
    try { await apiClient.delete(`/buildings/${id}`); loadData() }
    catch { alert('Greska pri brisanju') }
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-semibold text-text">Zgrade</h1>
          <p className="text-sm text-text-secondary mt-1">{buildings.length} zgrada</p>
        </div>
        <button onClick={() => { setShowForm(!showForm); setEditingId(null); setForm({ name: '', code: '', address: '', city: '' }) }} className="flex items-center gap-1 rounded-lg bg-primary-500 px-4 py-2 text-sm font-medium text-white hover:bg-primary-600">
          <Plus className="h-4 w-4" /> Nova zgrada
        </button>
      </div>

      {showForm && (
        <form onSubmit={handleSubmit} className="rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 p-4 space-y-3">
          <div className="grid grid-cols-2 gap-3">
            <input required placeholder="Naziv" value={form.name} onChange={e => setForm({ ...form, name: e.target.value })} className="rounded-lg border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 px-3 py-2 text-sm text-text" />
            <input required placeholder="Kod" value={form.code} onChange={e => setForm({ ...form, code: e.target.value })} className="rounded-lg border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 px-3 py-2 text-sm text-text" />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <input placeholder="Adresa" value={form.address} onChange={e => setForm({ ...form, address: e.target.value })} className="rounded-lg border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 px-3 py-2 text-sm text-text" />
            <input placeholder="Grad" value={form.city} onChange={e => setForm({ ...form, city: e.target.value })} className="rounded-lg border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 px-3 py-2 text-sm text-text" />
          </div>
          <div className="flex justify-end gap-2">
            <button type="button" onClick={() => { setShowForm(false); setEditingId(null) }} className="rounded-lg px-4 py-2 text-sm text-text-secondary hover:text-text">Odustani</button>
            <button type="submit" className="rounded-lg bg-primary-500 px-4 py-2 text-sm font-medium text-white hover:bg-primary-600">{editingId ? 'Azuriraj' : 'Kreiraj'}</button>
          </div>
        </form>
      )}

      {isLoading ? (
        <div className="animate-pulse h-48 rounded-xl bg-gray-200 dark:bg-gray-700" />
      ) : buildings.length === 0 ? (
        <div className="rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 p-12 text-center">
          <Building2 className="h-12 w-12 mx-auto text-text-secondary mb-3" />
          <p className="text-sm text-text-secondary">Nema zgrada</p>
        </div>
      ) : (
        <div className="rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 overflow-hidden">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-border">
                <th className="text-left py-3 px-4 text-text-secondary font-medium">Kod</th>
                <th className="text-left py-3 px-4 text-text-secondary font-medium">Naziv</th>
                <th className="text-left py-3 px-4 text-text-secondary font-medium">Adresa</th>
                <th className="text-left py-3 px-4 text-text-secondary font-medium">Status</th>
                <th className="text-right py-3 px-4 text-text-secondary font-medium">Akcije</th>
              </tr>
            </thead>
            <tbody>
              {buildings.map(b => (
                <tr key={b.id} className="border-b border-border last:border-0">
                  <td className="py-3 px-4 font-mono text-text">{b.code}</td>
                  <td className="py-3 px-4 text-text font-medium">{b.name}</td>
                  <td className="py-3 px-4 text-text-secondary">{b.address ? `${b.address}, ${b.city}` : '-'}</td>
                  <td className="py-3 px-4"><span className={`rounded-full px-2 py-0.5 text-xs font-medium ${b.isActive ? 'bg-emerald-100 text-emerald-700' : 'bg-gray-100 text-gray-600'}`}>{b.isActive ? 'Aktivna' : 'Neaktivna'}</span></td>
                  <td className="py-3 px-4 text-right">
                    <div className="flex justify-end gap-1">
                      <button onClick={() => handleEdit(b)} className="rounded p-1 text-text-secondary hover:text-primary-600"><Edit2 className="h-4 w-4" /></button>
                      <button onClick={() => handleDelete(b.id)} className="rounded p-1 text-text-secondary hover:text-red-600"><Trash2 className="h-4 w-4" /></button>
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
