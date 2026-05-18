export type GuestCategory = 'Adult' | 'Child' | 'Infant' | 'Senior'
export type NightStatus = 'Active' | 'Closed' | 'Modified' | 'Cancelled'

export interface StayDto {
  id: string
  hotelId: string
  guestId: string
  roomId: string
  folioId: string | null
  bookingId: string | null
  checkInDate: string
  checkOutDate: string
  checkedOutAt: string | null
  isCheckedOut: boolean
  guestCategory: GuestCategory
  discountPercent: number
  discountReason: string | null
  stayNote: string | null
  createdAt: string
}

export interface StayCheckInRequest {
  bookingId: string
  guests: StayGuestEntry[]
}

export interface StayGuestEntry {
  guestId: string
  roomId: string
  folioId?: string
  guestCategory?: GuestCategory
  discountPercent?: number
  discountReason?: string | null
}

export interface StayCheckInResponse {
  stays: StayGuestResult[]
  warnings: string[]
}

export interface StayGuestResult {
  stayId: string
  folioId: string
  guestName: string
  roomNumber: string
  guestCategory: string
}

export const GUEST_CATEGORY_LABELS: Record<GuestCategory, string> = {
  Adult: 'Odrasli',
  Child: 'Dijete',
  Infant: 'Dojenče',
  Senior: 'Senior',
}
