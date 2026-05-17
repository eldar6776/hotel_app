import apiClient from '@/lib/api/client'
import type { WorkOrderDto, DirtyRoomDto, HousekeepingLogDto } from '@/types/housekeeping'

export const housekeepingService = {
  async getDirtyRooms(): Promise<DirtyRoomDto[]> {
    const response = await apiClient.get<DirtyRoomDto[]>('/housekeeping/rooms')
    return response.data
  },

  async markRoomClean(roomId: string, notes?: string): Promise<void> {
    await apiClient.post(`/housekeeping/rooms/${roomId}/clean`, { notes })
  },

  async inspectRoom(roomId: string, passed: boolean, notes?: string): Promise<void> {
    await apiClient.post(`/housekeeping/rooms/${roomId}/inspect`, { notes, passed })
  },

  async getRoomLogs(roomId: string, limit = 20): Promise<HousekeepingLogDto[]> {
    const response = await apiClient.get<HousekeepingLogDto[]>(`/housekeeping/rooms/${roomId}/logs`, { params: { limit } })
    return response.data
  },

  async getWorkOrders(status?: string): Promise<WorkOrderDto[]> {
    const response = await apiClient.get<WorkOrderDto[]>('/housekeeping/work-orders', { params: { status } })
    return response.data
  },

  async createWorkOrder(roomId: string, category: string, priority: string, description: string): Promise<void> {
    await apiClient.post('/housekeeping/work-orders', { roomId, category, priority, description })
  },
}
