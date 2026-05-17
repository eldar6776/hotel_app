'use client'

import { useEffect, useState, useCallback } from 'react'
import type { GuestDto } from '@/types/guests'
import apiClient from '@/lib/api/client'
import { GuestCard } from '@/components/guests/GuestCard'
import { GuestDetail } from '@/components/guests/GuestDetail'

export default function GuestsPage() {
  const [guests, setGuests] = useState<GuestDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [search, setSearch] = useState('')
  const [totalCount, setTotalCount] = useState(0)
  const [page, setPage] = useState(1)
  const [selectedGuest, setSelectedGuest] = useState<GuestDto | null>(null)
  const pageSize = 20

  const loadGuests = useCallback(async (p?: number) => {
    setIsLoading(true)
    try {
      const res = await apiClient.get('/guests', {
        params: { search: search || undefined, page: p || page, pageSize }
      })
      setGuests(res.data.items)
      setTotalCount(res.data.totalCount)
    } catch { /* silent */ }
    finally { setIsLoading(false) }
  }, [search, page])

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    loadGuests()
  }, [loadGuests])

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault()
    setPage(1)
    loadGuests(1)
  }

  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-bold text-text">Gosti</h1>

      <form onSubmit={handleSearch} className="flex gap-2">
        <input
          value={search}
          onChange={e => setSearch(e.target.value)}
          placeholder="Pretraga po imenu, email-u, telefonu..."
          className="flex-1 rounded-lg border border-border bg-surface px-4 py-2 text-sm text-text"
        />
        <button type="submit" className="rounded-lg bg-primary-500 px-4 py-2 text-sm font-medium text-white">
          Trazi
        </button>
      </form>

      {isLoading ? (
        <div className="animate-pulse h-64 rounded-xl bg-surface-tertiary" />
      ) : guests.length === 0 ? (
        <p className="text-sm text-text-secondary text-center py-12">Nema rezultata</p>
      ) : (
        <div className="space-y-2">
          {guests.map(g => (
            <GuestCard key={g.id} guest={g} onClick={() => setSelectedGuest(g)} />
          ))}
        </div>
      )}

      {totalCount > pageSize && (
        <div className="flex justify-center gap-2">
          {Array.from({ length: Math.ceil(totalCount / pageSize) }, (_, i) => (
            <button
              key={i}
              onClick={() => { setPage(i + 1); loadGuests(i + 1) }}
              className={`rounded px-3 py-1 text-sm ${page === i + 1 ? 'bg-primary-500 text-white' : 'bg-surface-secondary text-text-secondary'}`}
            >
              {i + 1}
            </button>
          ))}
        </div>
      )}

      {selectedGuest && (
        <GuestDetail guest={selectedGuest} onClose={() => { setSelectedGuest(null); loadGuests() }} />
      )}
    </div>
  )
}
