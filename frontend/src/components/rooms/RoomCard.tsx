import type { RoomDto } from '@/types/rooms'

const statusColors: Record<string, string> = {
  Free: 'bg-emerald-500 text-white',
  Occupied: 'bg-blue-500 text-white',
  Departing: 'bg-orange-500 text-white',
  Reserved: 'bg-amber-500 text-white',
  ReservedConfirmed: 'bg-amber-500 text-white',
  OccupiedReserved: 'bg-fuchsia-500 text-white',
  ReservedUnconfirmed: 'bg-yellow-400 text-gray-900',
  Dirty: 'bg-gray-500 text-white',
  OutOfOrder: 'bg-red-500 text-white',
  OutOfService: 'bg-slate-600 text-white',
}

const statusLabels: Record<string, string> = {
  Free: 'Slobodna',
  Occupied: 'Zauzeta',
  Departing: 'Odlazak danas',
  Reserved: 'Rezervirana',
  ReservedConfirmed: 'Rezervirana - potvrdeno',
  OccupiedReserved: 'Zauzeta + rezervirana',
  ReservedUnconfirmed: 'Rezervirana - nepotvrdeno',
  Dirty: 'Nije spremna',
  OutOfOrder: 'Van upotrebe',
  OutOfService: 'Van servisa',
}

interface RoomCardProps {
  room: RoomDto
  onClick: (room: RoomDto) => void
}

export function RoomCard({ room, onClick }: RoomCardProps) {
  return (
    <div
      onClick={() => onClick(room)}
      className="group cursor-pointer rounded-xl border border-border bg-surface p-4 shadow-sm transition-all hover:shadow-md hover:border-primary-300"
      data-help-id={`room-card-${room.roomNumber}`}
    >
      <div className="mb-3 flex items-center justify-between">
        <span className="text-lg font-bold text-text">{room.roomNumber}</span>
        <span className={`rounded-full px-2.5 py-0.5 text-xs font-medium ${statusColors[room.status] || 'bg-gray-400 text-white'}`}>
          {statusLabels[room.status] || room.status}
        </span>
      </div>
      <div className="space-y-1 text-sm text-text-secondary">
        <p>{room.roomTypeName}</p>
        <p>Kapacitet: {room.baseCapacity}-{room.maxCapacity} osobe</p>
        <p>Kat: {room.floor} | {room.buildingName}</p>
        {room.basePrice != null && <p className="font-medium text-text">{room.basePrice.toFixed(2)} EUR</p>}
      </div>
    </div>
  )
}
