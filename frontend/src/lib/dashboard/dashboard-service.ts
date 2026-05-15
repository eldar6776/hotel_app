import apiClient from '@/lib/api/client'

export interface KpiData {
  occupancyPercent: number
  adr: number
  revpar: number
  todayCheckIns: number
  todayCheckOuts: number
  openFolios: number
  freeRooms: number
  totalRooms: number
  occupiedRooms: number
  dirtyRooms: number
  oooRooms: number
}

export interface OccupancyTrendPoint {
  date: string
  occupancy: number
  revenue: number
}

export interface RecentBooking {
  id: string
  bookingNumber: string
  guestName: string
  roomNumber: string
  arrivalDate: string
  departureDate: string
  status: string
  amount: number
}

export interface UpcomingCheckin {
  id: string
  guestName: string
  roomNumber: string
  arrivalDate: string
  nights: number
}

export const dashboardService = {
  async getKpi(): Promise<KpiData> {
    const response = await apiClient.get<KpiData>('/dashboard/kpi')
    return response.data
  },

  async getOccupancyTrend(days = 30): Promise<OccupancyTrendPoint[]> {
    const response = await apiClient.get<OccupancyTrendPoint[]>(
      `/dashboard/occupancy-trend?days=${days}`
    )
    return response.data
  },

  async getRecentBookings(limit = 10): Promise<RecentBooking[]> {
    const response = await apiClient.get<RecentBooking[]>(
      `/dashboard/recent-bookings?limit=${limit}`
    )
    return response.data
  },

  async getUpcomingCheckIns(days = 7): Promise<UpcomingCheckin[]> {
    const response = await apiClient.get<UpcomingCheckin[]>(
      `/dashboard/upcoming-checkins?days=${days}`
    )
    return response.data
  },
}
