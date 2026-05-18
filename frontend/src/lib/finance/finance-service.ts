import apiClient from '@/lib/api/client'
import type {
  CreateFolioInvoiceRequest,
  FolioLedgerDto,
  InvoiceResultDto,
  PaymentAllocationRequest,
  PaymentAllocationResultDto,
  ReservationResultDto,
  ConfirmReservationRequest,
  CancelReservationRequest,
  MarkNoShowRequest,
  StornoInvoiceRequest,
} from '@/types/finance'

export const invoiceService = {
  async createInvoice(request: CreateFolioInvoiceRequest): Promise<InvoiceResultDto> {
    const response = await apiClient.post<InvoiceResultDto>('/invoices/folio', request)
    return response.data
  },

  async getInvoice(invoiceId: string): Promise<InvoiceResultDto> {
    const response = await apiClient.get<InvoiceResultDto>(`/invoices/${invoiceId}`)
    return response.data
  },

  async getInvoicesForFolio(folioId: string): Promise<InvoiceResultDto[]> {
    const response = await apiClient.get<InvoiceResultDto[]>(`/invoices/folio/${folioId}`)
    return response.data
  },

  async stornoInvoice(request: StornoInvoiceRequest): Promise<InvoiceResultDto> {
    const response = await apiClient.post<InvoiceResultDto>(
      `/invoices/${request.invoiceId}/storno-workflow`,
      { reason: request.reason }
    )
    return response.data
  },
}

export const folioLedgerService = {
  async getLedger(folioId: string): Promise<FolioLedgerDto> {
    const response = await apiClient.get<FolioLedgerDto>(`/folios/${folioId}/ledger`)
    return response.data
  },

  async reconcileBalance(folioId: string): Promise<void> {
    await apiClient.post(`/folios/${folioId}/reconcile`)
  },
}

export const paymentService = {
  async allocatePayment(request: PaymentAllocationRequest): Promise<PaymentAllocationResultDto> {
    const response = await apiClient.post<PaymentAllocationResultDto>('/payments/allocate', request)
    return response.data
  },

  async getAllocationsByReference(reference: string): Promise<PaymentAllocationResultDto> {
    const response = await apiClient.get<PaymentAllocationResultDto>(`/payments/reference/${reference}`)
    return response.data
  },
}

export const reservationService = {
  async confirmReservation(request: ConfirmReservationRequest): Promise<ReservationResultDto> {
    const response = await apiClient.post<ReservationResultDto>('/reservations/confirm', request)
    return response.data
  },

  async cancelReservation(request: CancelReservationRequest): Promise<ReservationResultDto> {
    const response = await apiClient.post<ReservationResultDto>('/reservations/cancel', request)
    return response.data
  },

  async markNoShow(request: MarkNoShowRequest): Promise<ReservationResultDto> {
    const response = await apiClient.post<ReservationResultDto>('/reservations/noshow', request)
    return response.data
  },

  async getReservationStatus(bookingId: string): Promise<ReservationResultDto> {
    const response = await apiClient.get<ReservationResultDto>(`/reservations/${bookingId}/status`)
    return response.data
  },
}
