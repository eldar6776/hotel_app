'use client'

import { useEffect, useState } from 'react'
import type { AmenityDto } from '@/types/rooms'
import apiClient from '@/lib/api/client'

export default function AmenitiesPage() {
  const [amenities, setAmenities] = useState<AmenityDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [showForm, setShowForm] = useState(false)
  const [form, setForm] = useState({ name: '', icon: '' })

  const loadData = async () => {
    setIsLoading(true)
    try {
      const res = await apiClient.get('/v2/amenities')
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
      await apiClient.post('/v2/amenities', form)
      setShowForm(false)
      setForm({ name: '', icon: '' })
      loadData()
    } catch {
      alert('Greska pri kreiranju')
    }
  }

  const toggleActive = async (id: string, current: boolean) => {
    try {
      await apiClient.put(`/v2/amenities/${id}`, { isActive: !current })
      loadData()
    } catch {
      alert('Greska')
    }
  }

  if (isLoading) return <div className="animate-pulse h-64 rounded-xl bg-gray-200 dark:bg-gray-700"></div>

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-text">Sadrzaji</h1>
        <button onClick={() => setShowForm(!showForm)} className="rounded-lg bg-primary-500 px-4 py-2 text-sm font-medium text-white hover:bg-primary-600">
          {showForm ? 'Odustani' : '+ Novi Sadrzaj'}
        </button>
      </div>

      {showForm && (
        <form onSubmit={handleSubmit} className="rounded-xl border border-border bg-surface p-4 space-y-3">
          <div className="flex gap-3">
            <input required placeholder="Naziv" value={form.name} onChange={e => setForm({...form, name: e.target.value})} className="rounded-lg border border-border bg-background px-3 py-2 text-sm text-text flex-1" />
            <input placeholder="Ikona (emoji)" value={form.icon} onChange={e => setForm({...form, icon: e.target.value})} className="rounded-lg border border-border bg-background px-3 py-2 text-sm text-text w-24 text-center" />
            <button type="submit" className="rounded-lg bg-primary-500 px-4 py-2 text-sm font-medium text-white">Kreiraj</button>
          </div>
        </form>
      )}

      <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-3">
        {amenities.map(a => (
          <div key={a.id} className="flex items-center justify-between rounded-xl border border-border bg-surface p-4">
            <div className="flex items-center gap-3">
              {a.icon && <span className="text-2xl">{a.icon}</span>}
              <span className="font-medium text-text">{a.name}</span>
            </div>
            <button onClick={() => toggleActive(a.id, a.isActive)} className={`rounded-full px-2 py-0.5 text-xs font-medium ${a.isActive ? 'bg-emerald-500 text-white' : 'bg-gray-400 text-white'}`}>
              {a.isActive ? 'Aktivan' : 'Neaktivan'}
            </button>
          </div>
        ))}
      </div>
    </div>
  )
}
