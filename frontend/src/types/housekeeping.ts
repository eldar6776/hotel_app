export interface WorkOrderDto {
  id: string
  roomId: string | null
  reportedById: string
  assignedToId: string | null
  priority: string
  category: string
  description: string
  status: string
  createdAt: string
  resolvedAt: string | null
  resolutionNotes: string | null
  roomNumber: string | null
}

export interface DirtyRoomDto {
  id: string
  roomNumber: string
  floor: number
  building: string
}

export interface HousekeepingLogDto {
  id: string
  roomId: string
  action: string
  status: string
  performedAt: string
  notes: string | null
}

export const PRIORITY_COLORS: Record<string, string> = {
  Low: 'bg-gray-100 text-gray-700 dark:bg-gray-700 dark:text-gray-300',
  Medium: 'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-300',
  High: 'bg-orange-100 text-orange-700 dark:bg-orange-900/30 dark:text-orange-300',
  Critical: 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-300',
}

export const PRIORITY_LABELS: Record<string, string> = {
  Low: 'Nizak',
  Medium: 'Srednji',
  High: 'Visok',
  Critical: 'Kritican',
}

export const CATEGORY_LABELS: Record<string, string> = {
  Plumbing: 'Vodovod',
  Electrical: 'Struja',
  HVAC: 'Klima',
  Furniture: 'Namjestaj',
  Other: 'Ostalo',
}

export const STATUS_LABELS: Record<string, string> = {
  Open: 'Otvoren',
  InProgress: 'U toku',
  Resolved: 'Rijesen',
  Closed: 'Zatvoren',
}

export const STATUS_COLORS: Record<string, string> = {
  Open: 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-300',
  InProgress: 'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-300',
  Resolved: 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-300',
  Closed: 'bg-gray-100 text-gray-700 dark:bg-gray-700 dark:text-gray-300',
}
