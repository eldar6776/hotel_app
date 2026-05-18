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

export interface FullCheckOutRequest {
  roomId: string
  checkedOutBy?: string | null
  createUnpaidRecords?: boolean
}

export interface PartialCheckOutRequest {
  stayId: string
  checkedOutBy?: string | null
}

export interface CheckOutWorkflowResponse {
  roomId: string
  roomNumber: string
  folioId: string
  folioNumber: string
  guestsCheckedOut: number
  nightsClosed: number
  expensesLocked: number
  folioClosed: boolean
  hasUnpaidBalance: boolean
  outstandingAmount: number
}

export interface PartialCheckOutResponse {
  stayId: string
  guestId: string
  guestName: string
  folioId: string
  nightsClosedForGuest: number
  nightsCreatedForRemaining: number
  folioStillOpen: boolean
  remainingGuests: number
}

export interface FolioLedgerEntryDto {
  type: string
  date: string
  description: string
  debit: number
  credit: number
  referenceId: string | null
}

export interface FolioLedgerDto {
  folioId: string
  folioNumber: string
  status: string
  nightCharges: number
  otherCharges: number
  totalCharges: number
  totalPayments: number
  balance: number
  entries: FolioLedgerEntryDto[]
}
