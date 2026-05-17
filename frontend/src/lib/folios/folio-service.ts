import apiClient from '@/lib/api/client'
import type { FolioDto, CreateFolioChargeDto } from '@/types/folios'

export const folioService = {
  async getOpenFolios(): Promise<FolioDto[]> {
    const response = await apiClient.get<FolioDto[]>('/folios/open')
    return response.data
  },

  async getFoliosByBooking(bookingId: string): Promise<FolioDto[]> {
    const response = await apiClient.get<FolioDto[]>(`/folios/booking/${bookingId}`)
    return response.data
  },

  async addCharge(folioId: string, dto: CreateFolioChargeDto): Promise<void> {
    await apiClient.post(`/folios/${folioId}/charges`, dto)
  },

  async deleteCharge(chargeId: string): Promise<void> {
    await apiClient.delete(`/folios/charges/${chargeId}`)
  },

  async stornoCharge(chargeId: string, reason: string): Promise<void> {
    await apiClient.post(`/folios/charges/${chargeId}/storno`, { reason })
  },

  async closeFolio(folioId: string): Promise<void> {
    await apiClient.post(`/folios/${folioId}/close`)
  },
}
