'use client'

import type { RoomStatus, BuildingDto } from '@/types/rooms'

const statusOptions: { value: RoomStatus; label: string }[] = [
  { value: 'Free', label: 'Slobodna' },
  { value: 'Occupied', label: 'Zauzeta' },
  { value: 'Reserved', label: 'Rezervirana' },
  { value: 'Dirty', label: 'Prljava' },
  { value: 'OutOfOrder', label: 'Van funkcije' },
  { value: 'OutOfService', label: 'Van servisa' },
]

interface FilterBarProps {
  statusFilter: RoomStatus[]
  buildingFilter: string
  floorFilter: string
  search: string
  buildings: BuildingDto[]
  onStatusChange: (statuses: RoomStatus[]) => void
  onBuildingChange: (buildingId: string) => void
  onFloorChange: (floor: string) => void
  onSearchChange: (search: string) => void
  onReset: () => void
  onRefresh: () => void
}

export function FilterBar({
  statusFilter,
  buildingFilter,
  floorFilter,
  search,
  buildings,
  onStatusChange,
  onBuildingChange,
  onFloorChange,
  onSearchChange,
  onReset,
  onRefresh,
}: FilterBarProps) {
  const toggleStatus = (status: RoomStatus) => {
    if (statusFilter.includes(status)) {
      onStatusChange(statusFilter.filter((s) => s !== status))
    } else {
      onStatusChange([...statusFilter, status])
    }
  }

  return (
    <div className="rounded-xl border border-border bg-surface p-4">
      <div className="flex flex-wrap items-center gap-3">
        <div className="flex flex-wrap gap-2">
          {statusOptions.map((opt) => (
            <button
              key={opt.value}
              onClick={() => toggleStatus(opt.value)}
              className={`rounded-lg px-3 py-1.5 text-xs font-medium transition ${
                statusFilter.includes(opt.value)
                  ? 'bg-primary-500 text-white'
                  : 'bg-surface-tertiary text-text-secondary hover:bg-surface-secondary'
              }`}
            >
              {opt.label}
            </button>
          ))}
        </div>

        <select
          value={buildingFilter}
          onChange={(e) => onBuildingChange(e.target.value)}
          className="rounded-lg border border-border bg-surface px-3 py-1.5 text-sm text-text"
        >
          <option value="">Sve zgrade</option>
          {buildings.map((b) => (
            <option key={b.id} value={b.id}>{b.name}</option>
          ))}
        </select>

        <select
          value={floorFilter}
          onChange={(e) => onFloorChange(e.target.value)}
          className="rounded-lg border border-border bg-surface px-3 py-1.5 text-sm text-text"
        >
          <option value="">Svi katovi</option>
          {[0, 1, 2, 3, 4, 5, 6, 7, 8, 9].map((f) => (
            <option key={f} value={String(f)}>{f}. kat</option>
          ))}
        </select>

        <input
          type="text"
          placeholder="Pretrazi po broju..."
          value={search}
          onChange={(e) => onSearchChange(e.target.value)}
          className="rounded-lg border border-border bg-surface px-3 py-1.5 text-sm text-text"
        />

        <button onClick={onRefresh} className="rounded-lg bg-primary-500 px-3 py-1.5 text-sm font-medium text-white hover:bg-primary-600">
          Refresh
        </button>
        <button onClick={onReset} className="rounded-lg border border-border px-3 py-1.5 text-sm text-text-secondary hover:text-text">
          Reset
        </button>
      </div>
    </div>
  )
}
