import apiClient from '@/lib/api/client'
import type { RoomDto, RoomFilters, PagedResult, RoomStatus, RoomTypeDto, BuildingDto } from '@/types/rooms'

export const roomService = {
  async getRooms(filters?: RoomFilters): Promise<PagedResult<RoomDto>> {
    const params = new URLSearchParams()
    if (filters?.status?.length) params.append('status', filters.status.join(','))
    if (filters?.buildingId) params.append('buildingId', filters.buildingId)
    if (filters?.roomTypeId) params.append('roomTypeId', filters.roomTypeId)
    if (filters?.floor) params.append('floor', String(filters.floor))
    if (filters?.search) params.append('search', filters.search)
    const response = await apiClient.get(`/v2/rooms?${params}`)
    return response.data
  },

  async getRoom(id: string): Promise<RoomDto> {
    const response = await apiClient.get(`/v2/rooms/${id}`)
    return response.data
  },

  async updateStatus(id: string, status: RoomStatus): Promise<void> {
    await apiClient.patch(`/v2/rooms/${id}/status`, status)
  },

  async getRoomTypes(): Promise<RoomTypeDto[]> {
    const response = await apiClient.get('/v2/room-types')
    return response.data
  },

  async getBuildings(): Promise<BuildingDto[]> {
    const response = await apiClient.get('/v2/buildings')
    return response.data
  },
}
