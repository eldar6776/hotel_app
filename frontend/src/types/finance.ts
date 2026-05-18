export interface InvoiceLineItemDto {
  description: string
  quantity: number
  unitPrice: number
  totalPrice: number
  vatRate: number
  vatAmount: number
}

export interface InvoiceResultDto {
  id: string
  invoiceNumber: string
  folioId: string
  guestName: string
  roomNumber: string
  periodFrom: string
  periodTo: string
  subTotal: number
  vatAmount: number
  totalAmount: number
  isStorno: boolean
  stornoReason: string | null
  createdAt: string
  lineItems: InvoiceLineItemDto[]
}

export interface CreateFolioInvoiceRequest {
  folioId: string
  issuedBy?: string | null
  notes?: string | null
}

export interface StornoInvoiceRequest {
  invoiceId: string
  reason: string
  issuedBy?: string | null
}

export interface FolioAllocationEntry {
  folioId: string
  amount: number
}

export interface PaymentAllocationRequest {
  totalAmount: number
  paymentMethod: string
  folioAllocations: FolioAllocationEntry[]
  reference?: string | null
  processedById?: string | null
  notes?: string | null
}

export interface FolioPaymentResultDto {
  paymentId: string
  folioId: string
  amount: number
  status: string
}

export interface PaymentAllocationResultDto {
  allocationId: string
  reference: string
  totalAmount: number
  folioPayments: FolioPaymentResultDto[]
}

export interface ReservationAuditEntryDto {
  action: string
  previousValue: string | null
  newValue: string | null
  changedAt: string
}

export interface ReservationResultDto {
  bookingId: string
  status: string
  cancellationReason: string | null
  cancelledAt: string | null
  auditTrail: ReservationAuditEntryDto[]
}

export interface ConfirmReservationRequest {
  bookingId: string
  confirmedById?: string | null
  notes?: string | null
}

export interface CancelReservationRequest {
  bookingId: string
  reason: string
  cancelledById?: string | null
}

export interface MarkNoShowRequest {
  bookingId: string
  markedById?: string | null
  notes?: string | null
}
