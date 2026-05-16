export type RoomStatus = 'Free' | 'Occupied' | 'Reserved' | 'Dirty' | 'OutOfOrder' | 'OutOfService'

export const ROOM_STATUS_COLORS: Record<RoomStatus, string> = {
  Free: 'bg-emerald-500 text-white',
  Occupied: 'bg-blue-500 text-white',
  Reserved: 'bg-amber-500 text-white',
  Dirty: 'bg-orange-500 text-white',
  OutOfOrder: 'bg-red-500 text-white',
  OutOfService: 'bg-gray-500 text-white',
}

export const ROOM_STATUS_LABELS: Record<RoomStatus, string> = {
  Free: 'Slobodna',
  Occupied: 'Zauzeta',
  Reserved: 'Rezervirana',
  Dirty: 'Prljava',
  OutOfOrder: 'Van funkcije',
  OutOfService: 'Van servisa',
}

export interface RoomDto {
  id: string
  roomNumber: string
  floor: number
  buildingId: string
  buildingName: string
  roomTypeId: string
  roomTypeName: string
  status: RoomStatus
  baseCapacity: number
  maxCapacity: number
  basePrice: number | null
  notes: string | null
}

export interface RoomTypeDto {
  id: string
  name: string
  code: string
  baseCapacity: number
  maxCapacity: number
  defaultPrice: number
  description: string | null
  isActive: boolean
}

export interface BuildingDto {
  id: string
  name: string
  code: string
  address: string | null
  city: string | null
  isActive: boolean
}

export interface TariffDto {
  id: string
  name: string
  roomTypeId: string | null
  roomTypeName: string | null
  validFrom: string | null
  validTo: string | null
  basePrice: number
  currency: string
  isActive: boolean
}

export interface AmenityDto {
  id: string
  name: string
  icon: string | null
  isActive: boolean
}

export interface RoomOutOfOrderDto {
  id: string
  roomId: string
  roomNumber: string
  reason: string
  description: string | null
  startDate: string
  endDate: string | null
  status: string
  createdAt: string
  resolutionNotes: string | null
  resolvedAt: string | null
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
}

export interface RoomFilters {
  status?: RoomStatus[]
  buildingId?: string
  roomTypeId?: string
  floor?: number
  search?: string
  page?: number
  pageSize?: number
}

export interface RoomStatusChange {
  roomId: string
  roomNumber: string
  oldStatus: RoomStatus
  newStatus: RoomStatus
  timestamp: string
}
