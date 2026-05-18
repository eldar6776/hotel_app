export interface FolioChargeDto {
  id: string
  folioId: string
  chargeType: string
  description: string
  quantity: number
  unitPrice: number
  totalPrice: number
  chargeDate: string
  posReference: string | null
}

export interface FolioStayNightDto {
  id: string
  folioId: string
  stayId: string | null
  roomId: string
  date: string
  tariffAmount: number
  discountPercent: number
  status: string
  isComp: boolean
  description: string | null
  notes: string | null
  closedAt: string | null
}

export interface FolioDto {
  id: string
  folioNumber: string
  bookingId: string | null
  guestId: string | null
  guestName: string
  status: string
  balance: number
  createdAt: string
  closedAt: string | null
  notes: string | null
  charges: FolioChargeDto[]
  stayNights: FolioStayNightDto[]
}

export interface CreateFolioChargeDto {
  chargeType: string
  description: string
  quantity: number
  unitPrice: number
  chargeDate: string
  posReference: string | null
}

export const CHARGE_TYPES = [
  { value: 'Room', label: 'Soba' },
  { value: 'Food', label: 'Hrana' },
  { value: 'Beverage', label: 'Pice' },
  { value: 'Minibar', label: 'Minibar' },
  { value: 'Spa', label: 'Spa/Wellness' },
  { value: 'Parking', label: 'Parking' },
  { value: 'Laundry', label: 'Vesernica' },
  { value: 'Phone', label: 'Telefon' },
  { value: 'Other', label: 'Ostalo' },
]
