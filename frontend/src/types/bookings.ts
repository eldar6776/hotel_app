export type BookingStatus = 'Pending' | 'Confirmed' | 'CheckedIn' | 'CheckedOut' | 'Cancelled' | 'NoShow'

export type BookingSource =
  | 'Direct'
  | 'BookingCom'
  | 'Expedia'
  | 'HotelWebsite'
  | 'Phone'
  | 'WalkIn'
  | 'Corporate'
  | 'TravelAgency'

export type BookingType =
  | 'Normal'
  | 'Group'
  | 'Corporate'
  | 'TravelAgency'
  | 'Complementary'

export type BookingRoomStatus = 'Blocked' | 'Assigned' | 'Released' | 'Occupied'

export interface BookingRoomDto {
  id: string
  bookingId: string
  roomId: string | null
  roomNumber: string | null
  roomTypeId: string
  roomTypeName: string
  ratePlanId: string
  pricePerNight: number
  status: BookingRoomStatus
}

export interface BookingDto {
  id: string
  hotelId: string
  guestId: string
  guestName: string
  groupId: string | null
  source: BookingSource
  type: BookingType
  status: BookingStatus
  arrivalDate: string
  departureDate: string
  adultCount: number
  childCount: number
  nights: number
  totalPrice: number
  exchangeRateTotal: number
  currency: string
  notes: string | null
  internalNotes: string | null
  cancellationReason: string | null
  cancelledAt: string | null
  createdAt: string
  updatedAt: string
  rooms: BookingRoomDto[]
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
}

export interface BookingFilters {
  status?: BookingStatus[]
  fromDate?: string
  toDate?: string
  guestId?: string
  roomId?: string
  page?: number
  pageSize?: number
}

export interface GanttBooking {
  id: string
  guestName: string
  status: BookingStatus
  arrivalDate: string
  departureDate: string
  roomId: string | null
  roomTypeId: string
  roomTypeName: string
  pricePerNight: number
  nights: number
  totalPrice: number
  source: BookingSource
  type: BookingType
}

export interface GanttRoom {
  id: string
  roomNumber: string
  roomTypeId: string
  roomTypeName: string
  bookings: GanttBooking[]
}

export const STATUS_COLORS: Record<BookingStatus, string> = {
  Pending: '#FFC107',
  Confirmed: '#4CAF50',
  CheckedIn: '#2196F3',
  CheckedOut: '#607D8B',
  Cancelled: '#F44336',
  NoShow: '#9E9E9E',
}

export const STATUS_LABELS: Record<BookingStatus, string> = {
  Pending: 'Na cekanju',
  Confirmed: 'Potvrdjena',
  CheckedIn: 'Prijavljen',
  CheckedOut: 'Odjavljen',
  Cancelled: 'Otkazana',
  NoShow: 'Nije se pojavio',
}

export const SOURCE_LABELS: Record<BookingSource, string> = {
  Direct: 'Direktno',
  BookingCom: 'Booking.com',
  Expedia: 'Expedia',
  HotelWebsite: 'Web stranica',
  Phone: 'Telefon',
  WalkIn: 'Walk-in',
  Corporate: 'Korporativno',
  TravelAgency: 'Agencija',
}

export const TYPE_LABELS: Record<BookingType, string> = {
  Normal: 'Normalna',
  Group: 'Grupna',
  Corporate: 'Korporativna',
  TravelAgency: 'Agencijska',
  Complementary: 'Komplementarna',
}
