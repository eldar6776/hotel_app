'use client'

import type { GanttBooking } from '@/types/bookings'
import { STATUS_COLORS, STATUS_LABELS } from '@/types/bookings'

interface GanttBarProps {
  booking: GanttBooking
  left: number
  width: number
  rowHeight: number
  isDragging: boolean
  dragOffsetX: number
  dragOffsetY: number
  onPointerDown: (e: React.PointerEvent) => void
}

export function GanttBar({
  booking,
  left,
  width,
  rowHeight,
  isDragging,
  dragOffsetX,
  dragOffsetY,
  onPointerDown,
}: GanttBarProps) {
  const isGroupPending = booking.type === 'Group' && booking.status === 'Pending'
  const displayStatus = isGroupPending ? 'Blocked' : booking.status
  const color = STATUS_COLORS[displayStatus] || '#9E9E9E'
  const isCancelled = booking.status === 'Cancelled'
  const barHeight = rowHeight - 8
  const top = 4

  return (
    <div
      className={`absolute rounded-md cursor-grab active:cursor-grabbing select-none transition-shadow ${
        isDragging ? 'z-50 shadow-xl opacity-90' : 'z-10 hover:shadow-md hover:z-20'
      }`}
      style={{
        left: left + dragOffsetX,
        top: top + dragOffsetY,
        width: Math.max(width, 4),
        height: barHeight,
        backgroundColor: color,
        borderLeft: isCancelled ? '3px dashed rgba(0,0,0,0.3)' : undefined,
        borderStyle: isGroupPending ? 'dashed' : undefined,
        opacity: isCancelled ? 0.5 : undefined,
      }}
      onPointerDown={onPointerDown}
      title={`${booking.guestName} - ${STATUS_LABELS[displayStatus]}\n${booking.arrivalDate} → ${booking.departureDate} (${booking.nights}n)\n${booking.totalPrice.toFixed(2)} EUR`}
    >
      <div className="flex items-center h-full px-2 overflow-hidden">
        <span className="text-[11px] font-medium text-white truncate drop-shadow-sm">
          {booking.guestName}
        </span>
        {width > 80 && (
          <span className="text-[10px] text-white/80 ml-2 truncate">
            {booking.nights}n · {booking.totalPrice.toFixed(0)}€
          </span>
        )}
      </div>
    </div>
  )
}
