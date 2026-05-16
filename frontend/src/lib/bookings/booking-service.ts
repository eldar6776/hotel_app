import apiClient from '@/lib/api/client'
import type { BookingDto, BookingFilters, PagedResult, BookingStatus } from '@/types/bookings'

export const bookingService = {
  async getBookings(filters?: BookingFilters): Promise<PagedResult<BookingDto>> {
    const params = new URLSearchParams()
    if (filters?.status?.length) params.append('status', filters.status.join(','))
    if (filters?.fromDate) params.append('fromDate', filters.fromDate)
    if (filters?.toDate) params.append('toDate', filters.toDate)
    if (filters?.guestId) params.append('guestId', filters.guestId)
    if (filters?.roomId) params.append('roomId', filters.roomId)
    if (filters?.page) params.append('page', String(filters.page))
    if (filters?.pageSize) params.append('pageSize', String(filters.pageSize))
    const response = await apiClient.get(`/bookings?${params}`)
    return response.data
  },

  async getBooking(id: string): Promise<BookingDto> {
    const response = await apiClient.get(`/bookings/${id}`)
    return response.data
  },

  async updateStatus(id: string, status: BookingStatus): Promise<void> {
    await apiClient.patch(`/bookings/${id}/status`, status)
  },

  async updateBooking(id: string, data: {
    arrivalDate?: string
    departureDate?: string
    roomId?: string | null
  }): Promise<BookingDto> {
    const response = await apiClient.put(`/bookings/${id}`, data)
    return response.data
  },
}
