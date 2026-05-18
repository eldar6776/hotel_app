import apiClient from '@/lib/api/client'
import type {
  StayCheckInRequest,
  StayCheckInResponse,
  StayDto,
} from '@/types/stays'
import type {
  CheckOutWorkflowResponse,
  PartialCheckOutResponse,
  FullCheckOutRequest,
  PartialCheckOutRequest,
} from '@/types/finance'

export const stayService = {
  async checkIn(request: StayCheckInRequest): Promise<StayCheckInResponse> {
    const response = await apiClient.post<StayCheckInResponse>('/stays/check-in', request)
    return response.data
  },

  async fullCheckOut(request: FullCheckOutRequest): Promise<CheckOutWorkflowResponse> {
    const response = await apiClient.post<CheckOutWorkflowResponse>('/stays/check-out/full', request)
    return response.data
  },

  async partialCheckOut(request: PartialCheckOutRequest): Promise<PartialCheckOutResponse> {
    const response = await apiClient.post<PartialCheckOutResponse>('/stays/check-out/partial', request)
    return response.data
  },
}
