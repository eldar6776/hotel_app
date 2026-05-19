'use client'

import { useState, useEffect, useMemo, useCallback, useRef } from 'react'
import { ChevronLeft, ChevronRight, Calendar } from 'lucide-react'
import { GanttBar } from './GanttBar'
import { useDragAndDrop } from './useDragAndDrop'
import { bookingService } from '@/lib/bookings/booking-service'
import { roomService } from '@/lib/rooms/room-service'
import type { RoomDto } from '@/types/rooms'
import type { BookingDto, GanttRoom, GanttBooking } from '@/types/bookings'
import { STATUS_COLORS, STATUS_LABELS } from '@/types/bookings'

interface Toast {
  type: 'success' | 'error' | 'saving'
  message: string
}

const COLUMN_WIDTH = 80
const ROW_HEIGHT = 48
const ROOM_LABEL_WIDTH = 150
const DAYS_TO_SHOW = 31

function dateToStr(d: Date): string {
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
}

function generateDateRange(start: Date, days: number): string[] {
  return Array.from({ length: days }, (_, i) => {
    const d = new Date(start)
    d.setDate(d.getDate() + i)
    return dateToStr(d)
  })
}

function isWeekend(dateStr: string): boolean {
  const d = new Date(dateStr)
  return d.getDay() === 0 || d.getDay() === 6
}

export function GanttCalendar() {
  const [rooms, setRooms] = useState<RoomDto[]>([])
  const [bookings, setBookings] = useState<BookingDto[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [currentMonth, setCurrentMonth] = useState(() => {
    const now = new Date()
    return new Date(now.getFullYear(), now.getMonth(), 1)
  })
  const [scrollLeft, setScrollLeft] = useState(0)
  const [scrollTop, setScrollTop] = useState(0)
  const [toast, setToast] = useState<Toast | null>(null)
  const gridRef = useRef<HTMLDivElement>(null)
  const fetchDataRef = useRef<() => void>(() => {})
  const silentRefetchRef = useRef<() => void>(() => {})
  const hasScrolledToTodayRef = useRef(false)

  const dateRange = useMemo(() => generateDateRange(currentMonth, DAYS_TO_SHOW), [currentMonth])

  // Toast auto-dismiss
  useEffect(() => {
    if (toast && toast.type !== 'saving') {
      const t = setTimeout(() => setToast(null), 3000)
      return () => clearTimeout(t)
    }
  }, [toast])

  // Optimistic UI: odmah ažurira lokalni state, bez čekanja API-ja
  const handleOptimisticAssign = useCallback(
    (bookingId: string, newRoomId: string | null, _oldRoomId: string | null) => {
      setBookings((prev) =>
        prev.map((b) => {
          if (b.id !== bookingId) return b
          return {
            ...b,
            rooms: b.rooms.map((r, i) =>
              i === 0 ? { ...r, roomId: newRoomId, roomNumber: null } : r,
            ),
          }
        }),
      )
      setToast({ type: 'saving', message: 'Spašavanje...' })
    },
    [],
  )

  // Revert na originalno stanje ako API greška
  const handleAssignError = useCallback(
    (bookingId: string, originalRoomId: string | null) => {
      setBookings((prev) =>
        prev.map((b) => {
          if (b.id !== bookingId) return b
          return {
            ...b,
            rooms: b.rooms.map((r, i) =>
              i === 0 ? { ...r, roomId: originalRoomId } : r,
            ),
          }
        }),
      )
      setToast({ type: 'error', message: '✗ Greška — promjena vraćena' })
    },
    [],
  )

  const handleBookingMoved = useCallback(() => {
    setToast({ type: 'success', message: '✓ Promjena spašena' })
    silentRefetchRef.current()
  }, [])

  const fetchData = useCallback(async () => {
    try {
      setLoading(true)
      setError(null)

      const [roomsResult, bookingsResult] = await Promise.all([
        roomService.getRooms({ pageSize: 500 }),
        bookingService.getBookings({
          fromDate: dateRange[0],
          toDate: dateRange[dateRange.length - 1],
          pageSize: 500,
        }),
      ])

      setRooms(roomsResult.items)
      setBookings(bookingsResult.items)
    } catch {
      setError('Greska pri ucitavanju podataka.')
    } finally {
      setLoading(false)
    }
  }, [dateRange])

  // Tihi refresh u pozadini — ne prikazuje loading spinner
  const silentFetch = useCallback(async () => {
    try {
      const [roomsResult, bookingsResult] = await Promise.all([
        roomService.getRooms({ pageSize: 500 }),
        bookingService.getBookings({
          fromDate: dateRange[0],
          toDate: dateRange[dateRange.length - 1],
          pageSize: 500,
        }),
      ])
      setRooms(roomsResult.items)
      setBookings(bookingsResult.items)
    } catch {
      // Silent fail — optimistic state se čuva
    }
  }, [dateRange])

  useEffect(() => {
    fetchDataRef.current = fetchData
    silentRefetchRef.current = silentFetch
    fetchData()
  }, [fetchData, silentFetch])

  // Auto-scroll na danas pri prvom učitavanju
  useEffect(() => {
    if (loading || hasScrolledToTodayRef.current) return
    hasScrolledToTodayRef.current = true
    requestAnimationFrame(() => {
      const now = new Date()
      const todayCol = now.getDate() - 1
      const target = Math.max(0, todayCol * COLUMN_WIDTH - COLUMN_WIDTH * 2)
      setScrollLeft(target)
      if (gridRef.current) gridRef.current.scrollLeft = target
    })
  }, [loading])

  const ganttData = useMemo((): GanttRoom[] => {
    const roomMap = new Map<string, GanttRoom>()
    const unassigned: GanttRoom = {
      id: '__unassigned__',
      roomNumber: 'Nedodijeljene',
      roomTypeId: '',
      roomTypeName: '',
      bookings: [],
    }

    rooms.forEach((r) => {
      roomMap.set(r.id, {
        id: r.id,
        roomNumber: r.roomNumber,
        roomTypeId: r.roomTypeId,
        roomTypeName: r.roomTypeName,
        bookings: [],
      })
    })

    bookings.forEach((b) => {
      const gb: GanttBooking = {
        id: b.id,
        guestName: b.guestName,
        status: b.status,
        arrivalDate: b.arrivalDate.split('T')[0],
        departureDate: b.departureDate.split('T')[0],
        roomId: b.rooms[0]?.roomId || null,
        roomTypeId: b.rooms[0]?.roomTypeId || '',
        roomTypeName: b.rooms[0]?.roomTypeName || '',
        pricePerNight: b.rooms[0]?.pricePerNight || 0,
        nights: b.nights,
        totalPrice: b.totalPrice,
        source: b.source,
        type: b.type,
      }

      if (gb.roomId && roomMap.has(gb.roomId)) {
        roomMap.get(gb.roomId)!.bookings.push(gb)
      } else {
        unassigned.bookings.push(gb)
      }
    })

    const result = Array.from(roomMap.values())
    if (unassigned.bookings.length > 0) result.unshift(unassigned)
    return result
  }, [rooms, bookings])

  const {
    dragState,
    handleDragStart,
    handlePointerMove,
    handlePointerUp,
    getDragOffset,
    getDropTarget,
  } = useDragAndDrop(
    COLUMN_WIDTH,
    ROW_HEIGHT,
    dateRange,
    ganttData,
    gridRef,
    handleBookingMoved,
    handleOptimisticAssign,
    handleAssignError,
  )

  const startDate = useMemo(() => new Date(dateRange[0]), [dateRange])
  const totalWidth = dateRange.length * COLUMN_WIDTH
  const totalHeight = ganttData.length * ROW_HEIGHT

  const getLeft = useCallback(
    (dateStr: string) => {
      const d = new Date(dateStr)
      const diff = Math.floor((d.getTime() - startDate.getTime()) / (1000 * 60 * 60 * 24))
      return diff * COLUMN_WIDTH
    },
    [startDate]
  )

  const getWidth = useCallback(
    (arrival: string, departure: string) => {
      const a = new Date(arrival)
      const d = new Date(departure)
      const diff = Math.ceil((d.getTime() - a.getTime()) / (1000 * 60 * 60 * 24))
      return Math.max(diff * COLUMN_WIDTH, COLUMN_WIDTH)
    },
    []
  )

  const handleScroll = useCallback((e: React.UIEvent<HTMLDivElement>) => {
    setScrollLeft(e.currentTarget.scrollLeft)
    setScrollTop(e.currentTarget.scrollTop)
  }, [])

  const goToPrevMonth = () => {
    const prev = new Date(currentMonth)
    prev.setMonth(prev.getMonth() - 1)
    setCurrentMonth(prev)
    setScrollLeft(0)
  }

  const goToNextMonth = () => {
    const next = new Date(currentMonth)
    next.setMonth(next.getMonth() + 1)
    setCurrentMonth(next)
    setScrollLeft(0)
  }

  const goToToday = () => {
    const now = new Date()
    setCurrentMonth(new Date(now.getFullYear(), now.getMonth(), 1))
    requestAnimationFrame(() => {
      const todayCol = now.getDate() - 1
      const targetScroll = todayCol * COLUMN_WIDTH - 200
      setScrollLeft(Math.max(0, targetScroll))
      if (gridRef.current) {
        gridRef.current.scrollLeft = Math.max(0, targetScroll)
      }
    })
  }

  const offset = getDragOffset()
  const todayStr = dateToStr(new Date())

  const months = useMemo(() => {
    const result: { month: string; startIndex: number; span: number }[] = []
    let cur = ''
    let start = 0
    let span = 0
    dateRange.forEach((ds, i) => {
      const label = new Date(ds).toLocaleDateString('hr-HR', { month: 'short' })
      if (label !== cur) {
        if (span > 0) result.push({ month: cur, startIndex: start, span })
        cur = label
        start = i
        span = 1
      } else span++
    })
    if (span > 0) result.push({ month: cur, startIndex: start, span })
    return result
  }, [dateRange])

  if (loading) {
    return (
      <div className="flex items-center justify-center h-96">
        <div className="flex flex-col items-center gap-3">
          <div className="w-10 h-10 border-4 border-primary-200 border-t-primary-600 rounded-full animate-spin" />
          <span className="text-sm text-text-secondary">Ucitavanje rezervacija...</span>
        </div>
      </div>
    )
  }

  if (error) {
    return (
      <div className="flex items-center justify-center h-96">
        <div className="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-xl p-6 text-center">
          <p className="text-red-700 dark:text-red-300 font-medium">{error}</p>
          <button onClick={fetchData} className="mt-3 text-sm text-primary-600 hover:underline">
            Pokusaj ponovo
          </button>
        </div>
      </div>
    )
  }

  return (
    <div className="flex flex-col h-full">
      <div className="flex items-center justify-between mb-4 px-1">
        <div className="flex items-center gap-2">
          <h2 className="text-lg font-bold text-text">Kalendar rezervacija</h2>
          <div className="flex items-center gap-1 ml-4">
            <button onClick={goToPrevMonth} className="p-1.5 rounded-lg hover:bg-surface-secondary transition-colors" title="Prethodni mjesec">
              <ChevronLeft className="h-4 w-4 text-text-secondary" />
            </button>
            <button onClick={goToToday} className="px-3 py-1.5 text-xs font-medium rounded-lg border border-border hover:bg-surface-secondary transition-colors flex items-center gap-1.5">
              <Calendar className="h-3.5 w-3.5 text-text-secondary" />
              Danas
            </button>
            <button onClick={goToNextMonth} className="p-1.5 rounded-lg hover:bg-surface-secondary transition-colors" title="Sljedeci mjesec">
              <ChevronRight className="h-4 w-4 text-text-secondary" />
            </button>
          </div>
        </div>
        <div className="flex items-center gap-2 text-xs text-text-secondary">
          <span>{ganttData.length} soba</span>
          <span>·</span>
          <span>{bookings.length} rezervacija</span>
          <span>·</span>
          <span>{currentMonth.toLocaleDateString('hr-HR', { month: 'long', year: 'numeric' })}</span>
        </div>
      </div>

      <div className="rounded-xl border border-border overflow-hidden bg-surface">
        {/* Header */}
        <div className="flex border-b border-border bg-surface-secondary">
          <div className="shrink-0 flex items-end px-3 py-2" style={{ width: ROOM_LABEL_WIDTH }}>
            <span className="text-[11px] font-semibold text-text-secondary uppercase tracking-wider">Soba</span>
          </div>
          <div className="flex-1 overflow-hidden">
            <div style={{ width: totalWidth, transform: `translateX(${-scrollLeft}px)` }}>
              <div className="flex h-[24px] border-b border-border">
                {months.map((m, i) => (
                  <div
                    key={i}
                    className="shrink-0 flex items-center justify-center text-[11px] font-semibold text-text-secondary border-r border-border"
                    style={{ width: m.span * COLUMN_WIDTH }}
                  >
                    {m.month}
                  </div>
                ))}
              </div>
              <div className="flex h-[24px]">
                {dateRange.map((ds, i) => {
                  const d = new Date(ds)
                  const we = isWeekend(ds)
                  const isToday = ds === todayStr
                  return (
                    <div
                      key={i}
                      className={`shrink-0 flex items-center justify-center text-[10px] font-medium border-r border-border ${
                        we ? 'bg-surface-secondary text-text-tertiary' : 'text-text-secondary'
                      } ${isToday ? 'bg-primary-100 dark:bg-primary-900/30 text-primary-700 dark:text-primary-300 font-bold' : ''}`}
                      style={{ width: COLUMN_WIDTH }}
                    >
                      {d.getDate()}
                    </div>
                  )
                })}
              </div>
            </div>
          </div>
        </div>

        {/* Body */}
        <div className="flex" style={{ maxHeight: 'calc(100vh - 260px)', overflow: 'hidden' }}>
          {/* Room labels */}
          <div className="shrink-0 overflow-hidden" style={{ width: ROOM_LABEL_WIDTH }}>
            <div style={{ transform: `translateY(${-scrollTop}px)` }}>
              {ganttData.map((room) => (
                <div
                  key={room.id}
                  className={`flex items-center px-3 border-b border-border ${
                    room.id === '__unassigned__' ? 'bg-amber-50 dark:bg-amber-900/10' : 'bg-surface'
                  }`}
                  style={{ height: ROW_HEIGHT }}
                >
                  <div className="truncate">
                    <span className={`text-sm ${room.id === '__unassigned__' ? 'font-semibold text-amber-700 dark:text-amber-300' : 'font-medium text-text'}`}>
                      {room.roomNumber}
                    </span>
                    {room.roomTypeName && (
                      <span className="text-[10px] text-text-secondary ml-1 truncate">{room.roomTypeName}</span>
                    )}
                  </div>
                </div>
              ))}
            </div>
          </div>

          {/* Grid */}
          <div
            ref={gridRef}
            className="flex-1 overflow-auto"
            onScroll={handleScroll}
            onPointerMove={handlePointerMove}
            onPointerUp={handlePointerUp}
            onPointerCancel={handlePointerUp}
            style={{ position: 'relative' }}
          >
            <div style={{ width: totalWidth, height: totalHeight, position: 'relative' }}>
              {/* Timeline grid lines */}
              {dateRange.map((ds, i) => {
                const we = isWeekend(ds)
                const isToday = ds === todayStr
                return (
                  <div
                    key={`col-${i}`}
                    className={`absolute top-0 border-r ${
                      we ? 'border-border/60 bg-surface-secondary/40' : 'border-border/20'
                    } ${isToday ? 'border-primary-300 dark:border-primary-700 bg-primary-50/20 dark:bg-primary-900/10' : ''}`}
                    style={{ left: i * COLUMN_WIDTH, width: COLUMN_WIDTH, height: totalHeight }}
                  />
                )
              })}
              {ganttData.map((_, i) => (
                <div
                  key={`row-${i}`}
                  className="absolute left-0 w-full border-b border-border/30"
                  style={{ top: i * ROW_HEIGHT, height: ROW_HEIGHT }}
                />
              ))}

              {/* Booking bars */}
              {ganttData.map((room) =>
                room.bookings.map((booking) => {
                  const isDraggingMe = dragState?.booking?.id === booking.id
                  return (
                    <GanttBar
                      key={booking.id}
                      booking={booking}
                      left={getLeft(booking.arrivalDate)}
                      width={getWidth(booking.arrivalDate, booking.departureDate)}
                      rowHeight={ROW_HEIGHT}
                      isDragging={isDraggingMe}
                      dragOffsetX={isDraggingMe ? offset?.x || 0 : 0}
                      dragOffsetY={isDraggingMe ? offset?.y || 0 : 0}
                      onPointerDown={(e) => handleDragStart(booking, e)}
                    />
                  )
                })
              )}

              {/* Drop Zone Shadow — pojavljuje se tokom vertikalnog draga */}
              {(() => {
                const dropTarget = getDropTarget()
                if (!dropTarget || !dragState) return null
                const bk = dragState.booking
                const dropLeft = getLeft(bk.arrivalDate)
                const dropWidth = getWidth(bk.arrivalDate, bk.departureDate)
                const dropTop = dropTarget.rowIndex * ROW_HEIGHT
                return (
                  <div
                    className={`absolute pointer-events-none border-2 border-dashed rounded-md z-40 transition-colors ${
                      dropTarget.available
                        ? 'border-green-500 bg-green-100/40 dark:bg-green-900/20'
                        : 'border-red-500 bg-red-100/30 dark:bg-red-900/20'
                    }`}
                    style={{
                      left: dropLeft,
                      top: dropTop + 4,
                      width: dropWidth,
                      height: ROW_HEIGHT - 8,
                    }}
                  >
                    <div className="flex flex-col items-center justify-center h-full px-2 gap-0">
                      <span className="text-[10px] font-semibold text-text truncate w-full text-center leading-tight">
                        {bk.guestName}
                      </span>
                      <span
                        className={`text-[10px] font-medium leading-tight ${
                          dropTarget.available
                            ? 'text-green-700 dark:text-green-300'
                            : 'text-red-700 dark:text-red-300'
                        }`}
                      >
                        {dropTarget.available ? '✓ Slobodno' : '✗ Zauzeto'}
                      </span>
                    </div>
                  </div>
                )
              })()}

              {/* Drag preview tooltip */}
              {dragState?.booking && offset && (() => {
                const daysDiff = Math.round(offset.x / COLUMN_WIDTH)
                const oldArrival = new Date(dragState.booking.arrivalDate)
                const oldDeparture = new Date(dragState.booking.departureDate)
                const newArrival = new Date(oldArrival)
                newArrival.setDate(newArrival.getDate() + daysDiff)
                const newDeparture = new Date(oldDeparture)
                newDeparture.setDate(newDeparture.getDate() + daysDiff)
                const newNights = Math.round((newDeparture.getTime() - newArrival.getTime()) / 86400000)
                return (
                <div
                  className="absolute z-50 bg-surface border border-border rounded-lg shadow-lg px-3 py-2 text-xs text-text pointer-events-none"
                  style={{
                    left: getLeft(dragState.booking.arrivalDate) + offset.x + 10,
                    top: 8 + offset.y,
                  }}
                >
                  <div className="font-medium">{dragState.booking.guestName}</div>
                  <div className="text-text-secondary">
                    {newArrival.toISOString().split('T')[0]} → {newDeparture.toISOString().split('T')[0]}
                  </div>
                  <div className="text-text-secondary">{newNights} noci</div>
                </div>
                )
              })()}
            </div>
          </div>
        </div>
      </div>

      {/* Status legend */}
      <div className="mt-3 flex items-center gap-3 flex-wrap px-1">
        <span className="text-[11px] text-text-secondary mr-1">Status:</span>
        {(['Confirmed', 'Pending', 'CheckedIn', 'Blocked', 'Cancelled'] as string[]).map((status) => (
          <span key={status} className="inline-flex items-center gap-1.5 text-[11px] text-text-secondary">
            <span className="inline-block w-3 h-3 rounded-sm" style={{ backgroundColor: STATUS_COLORS[status] }} />
            {STATUS_LABELS[status]}
          </span>
        ))}
        {dragState && (
          <div className="ml-auto text-[11px] bg-primary-50 dark:bg-primary-900/20 text-primary-700 dark:text-primary-300 px-2 py-1 rounded-full animate-pulse">
            {dragState.booking.roomId ? 'Premještanje sobe...' : 'Dodjela sobe...'}
          </div>
        )}
      </div>

      {/* Toast notifikacija — Optimistic UI povratna informacija */}
      {toast && (
        <div
          className={`fixed bottom-6 right-6 z-[9999] flex items-center gap-2 px-4 py-3 rounded-xl shadow-xl text-sm font-medium text-white transition-all ${
            toast.type === 'success'
              ? 'bg-green-600'
              : toast.type === 'error'
                ? 'bg-red-600'
                : 'bg-gray-700'
          }`}
        >
          {toast.type === 'saving' && (
            <span className="inline-block w-3 h-3 border-2 border-white/30 border-t-white rounded-full animate-spin" />
          )}
          {toast.message}
        </div>
      )}
    </div>
  )
}
