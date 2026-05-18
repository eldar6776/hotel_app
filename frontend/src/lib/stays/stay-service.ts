import apiClient from '@/lib/api/client'
import type {
  StayCheckInRequest,
  StayCheckInResponse,
  StayDto,
} from '@/types/stays'

export const stayService = {
  async checkIn(request: StayCheckInRequest): Promise<StayCheckInResponse> {
    const response = await apiClient.post<StayCheckInResponse>('/stays/check-in', request)
    return response.data
  },

  async getStay(stayId: string): Promise<StayDto> {
    const response = await apiClient.get<StayDto>(`/stays/${stayId}`)
    return response.data
  },

  async getStaysByFolio(folioId: string): Promise<StayDto[]> {
    const response = await apiClient.get<StayDto[]>(`/stays/folio/${folioId}`)
    return response.data
  },

  async checkOut(stayId: string): Promise<void> {
    await apiClient.post(`/stays/${stayId}/check-out`)
  },
}
