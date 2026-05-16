'use client'

import { useEffect, useState, useCallback } from 'react'
import type { RoomDto, RoomStatus, BuildingDto } from '@/types/rooms'
import { ROOM_STATUS_LABELS } from '@/types/rooms'
import { roomService } from '@/lib/rooms/room-service'
import { roomHubService } from '@/lib/signalr/room-hub'
import { RoomCard } from '@/components/rooms/RoomCard'
import { RoomDetail } from '@/components/rooms/RoomDetail'
import { FilterBar } from '@/components/rooms/FilterBar'

export default function RoomsPage() {
  const [rooms, setRooms] = useState<RoomDto[]>([])
  const [buildings, setBuildings] = useState<BuildingDto[]>([])
  const [selectedRoom, setSelectedRoom] = useState<RoomDto | null>(null)
  const [isDetailOpen, setIsDetailOpen] = useState(false)
  const [viewMode, setViewMode] = useState<'grid' | 'floor'>('grid')
  const [statusFilter, setStatusFilter] = useState<RoomStatus[]>([])
  const [buildingFilter, setBuildingFilter] = useState('')
  const [floorFilter, setFloorFilter] = useState('')
  const [search, setSearch] = useState('')
  const [isLoading, setIsLoading] = useState(true)
  const [toast, setToast] = useState<{ message: string; roomId: string } | null>(null)

  const loadRooms = useCallback(async () => {
    setIsLoading(true)
    try {
      const filters = {
        status: statusFilter.length > 0 ? statusFilter : undefined,
        buildingId: buildingFilter || undefined,
        floor: floorFilter ? parseInt(floorFilter) : undefined,
        search: search || undefined,
      }
      const result = await roomService.getRooms(filters)
      setRooms(result.items)
    } catch {
      alert('Greska pri ucitavanju soba')
    } finally {
      setIsLoading(false)
    }
  }, [statusFilter, buildingFilter, floorFilter, search])

  const loadBuildings = useCallback(async () => {
    try {
      const result = await roomService.getBuildings()
      setBuildings(result)
    } catch {
      // silent
    }
  }, [])

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    loadRooms()
    loadBuildings()
  }, [loadRooms, loadBuildings])

  useEffect(() => {
    const token = localStorage.getItem('hotelpro_access_token')
    if (!token || typeof window === 'undefined') return

    const apiUrl = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000'

    roomHubService.connect(apiUrl, token).catch(() => {})

    const unsub = roomHubService.onStatusChange((data) => {
      setRooms((prev) =>
        prev.map((room) =>
          room.id === data.roomId ? { ...room, status: data.newStatus as RoomStatus } : room
        )
      )
      setToast({
        message: `Soba ${data.roomNumber}: ${ROOM_STATUS_LABELS[data.oldStatus as keyof typeof ROOM_STATUS_LABELS] || data.oldStatus} → ${ROOM_STATUS_LABELS[data.newStatus as keyof typeof ROOM_STATUS_LABELS] || data.newStatus}`,
        roomId: data.roomId,
      })
      setTimeout(() => setToast(null), 5000)
    })

    return () => {
      unsub()
      roomHubService.disconnect()
    }
  }, [])

  const handleRoomClick = (room: RoomDto) => {
    setSelectedRoom(room)
    setIsDetailOpen(true)
  }

  const handleStatusChange = () => {
    loadRooms()
    if (selectedRoom) {
      roomService.getRoom(selectedRoom.id).then(setSelectedRoom)
    }
  }

  const handleToastClick = () => {
    if (toast) {
      roomService.getRoom(toast.roomId).then((room) => {
        setSelectedRoom(room)
        setIsDetailOpen(true)
      }).catch(() => {})
    }
    setToast(null)
  }

  const handleReset = () => {
    setStatusFilter([])
    setBuildingFilter('')
    setFloorFilter('')
    setSearch('')
  }

  const floors = [...new Set(rooms.map((r) => r.floor))].sort((a, b) => a - b)
  const groupedByFloor = floors.map((floor) => ({
    floor,
    rooms: rooms.filter((r) => r.floor === floor),
  }))

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-text">Upravljanje Sobama</h1>
        <div className="flex gap-2">
          <button
            onClick={() => setViewMode('grid')}
            className={`rounded-lg px-3 py-1.5 text-sm font-medium ${viewMode === 'grid' ? 'bg-primary-500 text-white' : 'bg-surface-tertiary text-text-secondary hover:bg-surface-secondary'}`}
          >
            Grid
          </button>
          <button
            onClick={() => setViewMode('floor')}
            className={`rounded-lg px-3 py-1.5 text-sm font-medium ${viewMode === 'floor' ? 'bg-primary-500 text-white' : 'bg-surface-tertiary text-text-secondary hover:bg-surface-secondary'}`}
          >
            Floor Plan
          </button>
        </div>
      </div>

      <FilterBar
        statusFilter={statusFilter}
        buildingFilter={buildingFilter}
        floorFilter={floorFilter}
        search={search}
        buildings={buildings}
        onStatusChange={setStatusFilter}
        onBuildingChange={setBuildingFilter}
        onFloorChange={setFloorFilter}
        onSearchChange={setSearch}
        onReset={handleReset}
        onRefresh={loadRooms}
      />

      <div className="flex flex-wrap gap-3 text-xs text-text-secondary">
        <span className="flex items-center gap-1"><span className="h-3 w-3 rounded-full bg-emerald-500"></span>Slobodna</span>
        <span className="flex items-center gap-1"><span className="h-3 w-3 rounded-full bg-blue-500"></span>Zauzeta</span>
        <span className="flex items-center gap-1"><span className="h-3 w-3 rounded-full bg-amber-500"></span>Rezervirana</span>
        <span className="flex items-center gap-1"><span className="h-3 w-3 rounded-full bg-orange-500"></span>Prljava</span>
        <span className="flex items-center gap-1"><span className="h-3 w-3 rounded-full bg-red-500"></span>Van funkcije</span>
        <span className="flex items-center gap-1"><span className="h-3 w-3 rounded-full bg-gray-500"></span>Van servisa</span>
      </div>

      {isLoading ? (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 2xl:grid-cols-6">
          {Array.from({ length: 8 }).map((_, i) => (
            <div key={i} className="h-36 animate-pulse rounded-xl bg-surface-tertiary"></div>
          ))}
        </div>
      ) : viewMode === 'grid' ? (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 2xl:grid-cols-6">
          {rooms.map((room) => (
            <RoomCard key={room.id} room={room} onClick={handleRoomClick} />
          ))}
        </div>
      ) : (
        <div className="space-y-6">
          {groupedByFloor.map(({ floor, rooms: floorRooms }) => (
            <div key={floor}>
              <h3 className="mb-3 text-lg font-semibold text-text">{floor}. Kat ({floorRooms.length} soba)</h3>
              <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
                {floorRooms.map((room) => (
                  <RoomCard key={room.id} room={room} onClick={handleRoomClick} />
                ))}
              </div>
            </div>
          ))}
        </div>
      )}

      {selectedRoom && (
        <RoomDetail
          room={selectedRoom}
          isOpen={isDetailOpen}
          onClose={() => setIsDetailOpen(false)}
          onStatusChange={handleStatusChange}
        />
      )}

      {toast && (
        <button
          onClick={handleToastClick}
          className="fixed bottom-4 right-4 z-50 cursor-pointer rounded-lg bg-surface p-3 shadow-lg border border-border hover:bg-surface-secondary transition-colors"
        >
          <p className="text-sm text-text">{toast.message}</p>
          <p className="text-xs text-text-secondary mt-1">Klikni za detalje</p>
        </button>
      )}
    </div>
  )
}
