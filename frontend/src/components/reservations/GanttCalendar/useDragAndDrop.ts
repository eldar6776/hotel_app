'use client'

import { useState, useCallback, useRef, useEffect } from 'react'
import type { GanttBooking, GanttRoom } from '@/types/bookings'
import { bookingService } from '@/lib/bookings/booking-service'

interface DragState {
  booking: GanttBooking
  sourceRoomId: string | null
  startX: number
  startY: number
  currentX: number
  currentY: number
  targetRowIndex: number | null
  targetRoomId: string | null
  dropAvailable: boolean | null
  daysDiff: number
  newArrival: string
  newDeparture: string
}

export interface DropTarget {
  rowIndex: number
  roomId: string
  available: boolean
}

function isRoomAvailableForDates(
  room: GanttRoom,
  bookingId: string,
  arrivalDate: string,
  departureDate: string
): boolean {
  if (room.id === '__unassigned__') return true
  return !room.bookings.some(
    (b) =>
      b.id !== bookingId &&
      b.arrivalDate < departureDate &&
      b.departureDate > arrivalDate,
  )
}

export function useDragAndDrop(
  columnWidth: number,
  rowHeight: number,
  dateRange: string[],
  ganttData: GanttRoom[],
  gridRef: React.RefObject<HTMLDivElement | null>,
  onBookingMoved?: () => void,
  onOptimisticAssign?: (bookingId: string, newRoomId: string | null, oldRoomId: string | null) => void,
  onAssignError?: (bookingId: string, originalRoomId: string | null) => void,
) {
  const [dragState, setDragState] = useState<DragState | null>(null)
  const dragRef = useRef<DragState | null>(null)
  const ganttDataRef = useRef<GanttRoom[]>(ganttData)

  // Svi callbackovi u refovima — handleri nikad ne trebaju biti re-kreirani
  const onBookingMovedRef = useRef(onBookingMoved)
  const onOptimisticAssignRef = useRef(onOptimisticAssign)
  const onAssignErrorRef = useRef(onAssignError)
  const columnWidthRef = useRef(columnWidth)
  const rowHeightRef = useRef(rowHeight)

  useEffect(() => { ganttDataRef.current = ganttData }, [ganttData])
  useEffect(() => { onBookingMovedRef.current = onBookingMoved }, [onBookingMoved])
  useEffect(() => { onOptimisticAssignRef.current = onOptimisticAssign }, [onOptimisticAssign])
  useEffect(() => { onAssignErrorRef.current = onAssignError }, [onAssignError])
  useEffect(() => { columnWidthRef.current = columnWidth }, [columnWidth])
  useEffect(() => { rowHeightRef.current = rowHeight }, [rowHeight])

  // ── DRAG START ──────────────────────────────────────────────────────────────
  const handleDragStart = useCallback(
    (booking: GanttBooking, e: React.PointerEvent) => {
      e.preventDefault()
      e.stopPropagation()
      // setPointerCapture: svi pointer eventi idu na ovaj element i bubble do grida
      ;(e.currentTarget as HTMLElement).setPointerCapture(e.pointerId)

      const state: DragState = {
        booking,
        sourceRoomId: booking.roomId,
        startX: e.clientX,
        startY: e.clientY,
        currentX: e.clientX,
        currentY: e.clientY,
        targetRowIndex: null,
        targetRoomId: null,
        dropAvailable: null,
        daysDiff: 0,
        newArrival: booking.arrivalDate,
        newDeparture: booking.departureDate,
      }
      dragRef.current = state
      setDragState(state)
    },
    [],
  )

  // ── POINTER MOVE — React handler, ide na grid div ───────────────────────────
  const handlePointerMove = useCallback((e: React.PointerEvent) => {
    if (!dragRef.current) return
    e.preventDefault()

    const current = dragRef.current
    const dx = e.clientX - current.startX
    const dy = e.clientY - current.startY

    const cw = columnWidthRef.current
    const rh = rowHeightRef.current

    // 1. Proračun horizontalne promjene datuma (dvodimenzionalno)
    let daysDiff = Math.round(dx / cw)

    // Ograničenje: ne smije u prošlost (dolazak na kalendaru)
    const today = new Date()
    today.setHours(0, 0, 0, 0)
    const originalArrival = new Date(current.booking.arrivalDate)
    originalArrival.setHours(0, 0, 0, 0)

    // minDaysDiff je razlika u danima do danas
    const minDaysDiff = Math.ceil((today.getTime() - originalArrival.getTime()) / (1000 * 60 * 60 * 24))
    if (daysDiff < minDaysDiff) {
      daysDiff = minDaysDiff
    }

    const newArrival = new Date(originalArrival)
    newArrival.setDate(newArrival.getDate() + daysDiff)
    const newDeparture = new Date(current.booking.departureDate)
    newDeparture.setDate(newDeparture.getDate() + daysDiff)

    const newArrivalStr = newArrival.toISOString().split('T')[0]
    const newDepartureStr = newDeparture.toISOString().split('T')[0]

    // 2. Proračun vertikalne promjene sobe
    let targetRowIndex: number | null = null
    let targetRoomId: string | null = null
    let dropAvailable: boolean | null = null

    const grid = gridRef.current
    if (grid) {
      const rect = grid.getBoundingClientRect()
      const relY = e.clientY - rect.top + grid.scrollTop
      const rowIndex = Math.max(
        0,
        Math.min(Math.floor(relY / rh), ganttDataRef.current.length - 1),
      )
      const targetRoom = ganttDataRef.current[rowIndex]
      if (targetRoom) {
        targetRowIndex = rowIndex
        targetRoomId = targetRoom.id
        dropAvailable = isRoomAvailableForDates(targetRoom, current.booking.id, newArrivalStr, newDepartureStr)
      }
    }

    const updated: DragState = {
      ...current,
      currentX: e.clientX,
      currentY: e.clientY,
      targetRowIndex,
      targetRoomId,
      dropAvailable,
      daysDiff,
      newArrival: newArrivalStr,
      newDeparture: newDepartureStr,
    }

    dragRef.current = updated
    setDragState({ ...updated })
  }, [gridRef])

  // ── POINTER UP — React handler, ide na grid div ─────────────────────────────
  const handlePointerUp = useCallback(async (e: React.PointerEvent) => {
    const current = dragRef.current
    if (!current) return

    dragRef.current = null
    setDragState(null)

    const dx = e.clientX - current.startX
    const dy = e.clientY - current.startY

    if (Math.abs(dx) < 5 && Math.abs(dy) < 5) return // klik bez pomaka

    const cw = columnWidthRef.current

    // Proračun konačnog daysDiff i datuma
    let daysDiff = Math.round(dx / cw)
    const today = new Date()
    today.setHours(0, 0, 0, 0)
    const originalArrival = new Date(current.booking.arrivalDate)
    originalArrival.setHours(0, 0, 0, 0)
    const minDaysDiff = Math.ceil((today.getTime() - originalArrival.getTime()) / (1000 * 60 * 60 * 24))
    if (daysDiff < minDaysDiff) {
      daysDiff = minDaysDiff
    }

    const newArrival = new Date(originalArrival)
    newArrival.setDate(newArrival.getDate() + daysDiff)
    const newDeparture = new Date(current.booking.departureDate)
    newDeparture.setDate(newDeparture.getDate() + daysDiff)

    const newArrivalStr = newArrival.toISOString().split('T')[0]
    const newDepartureStr = newDeparture.toISOString().split('T')[0]

    const targetRoomId = current.targetRoomId
    const newRoomId = targetRoomId === '__unassigned__' ? null : targetRoomId
    const currentRoomId = current.booking.roomId ?? null

    const datesChanged = daysDiff !== 0
    const roomChanged = newRoomId !== currentRoomId

    if (!datesChanged && !roomChanged) return // Nema promjena

    // ── PROVJERA DOSTUPNOSTI ──────────────────────────────────────────
    if (newRoomId) {
      const targetRoom = ganttDataRef.current.find((r) => r.id === newRoomId)
      const isAvailable = targetRoom ? isRoomAvailableForDates(targetRoom, current.booking.id, newArrivalStr, newDepartureStr) : false
      if (!isAvailable) return // zauzeto -> snap-back
    }

    if (!datesChanged && roomChanged) {
      // ── SAMO PROMJENA SOBE ──────────────────────────────────────────
      const canAssign =
        current.booking.status === 'Confirmed' ||
        current.booking.status === 'Pending' ||
        current.booking.status === 'CheckedIn'
      if (!canAssign) return

      onOptimisticAssignRef.current?.(current.booking.id, newRoomId, currentRoomId)

      try {
        await bookingService.assignRoom(current.booking.id, newRoomId)
        onBookingMovedRef.current?.()
      } catch (err) {
        console.error('[Drag] assignRoom greška:', err)
        onAssignErrorRef.current?.(current.booking.id, currentRoomId)
      }
    }
    else if (datesChanged && !roomChanged) {
      // ── SAMO PROMJENA DATUMA ────────────────────────────────────────
      const canMove =
        current.booking.status === 'Confirmed' || current.booking.status === 'Pending'
      if (!canMove) return

      try {
        await bookingService.updateBooking(current.booking.id, {
          arrivalDate: newArrival.toISOString(),
          departureDate: newDeparture.toISOString(),
        })
        onBookingMovedRef.current?.()
      } catch (err) {
        console.error('[Drag] updateBooking greška:', err)
      }
    }
    else if (datesChanged && roomChanged) {
      // ── PROMJENA I SOBE I DATUMA ────────────────────────────────────
      const canMove =
        current.booking.status === 'Confirmed' || current.booking.status === 'Pending'
      if (!canMove) return

      // Optimistički pomičemo u sobu
      onOptimisticAssignRef.current?.(current.booking.id, newRoomId, currentRoomId)

      try {
        // Sekvencijalno: prvo ažuriramo datume pa dodjeljujemo sobu
        await bookingService.updateBooking(current.booking.id, {
          arrivalDate: newArrival.toISOString(),
          departureDate: newDeparture.toISOString(),
        })
        await bookingService.assignRoom(current.booking.id, newRoomId)
        onBookingMovedRef.current?.()
      } catch (err) {
        console.error('[Drag] updateBooking + assignRoom greška:', err)
        onAssignErrorRef.current?.(current.booking.id, currentRoomId)
      }
    }
  }, [])

  // ── HELPERS ─────────────────────────────────────────────────────────────────
  const getDragOffset = useCallback((): { x: number; y: number } | null => {
    if (!dragState) return null
    return {
      x: dragState.currentX - dragState.startX,
      y: dragState.currentY - dragState.startY,
    }
  }, [dragState])

  const getDropTarget = useCallback((): DropTarget | null => {
    if (!dragState || dragState.targetRowIndex === null || dragState.targetRoomId === null)
      return null
    return {
      rowIndex: dragState.targetRowIndex,
      roomId: dragState.targetRoomId,
      available: dragState.dropAvailable ?? false,
    }
  }, [dragState])

  return {
    dragState,
    handleDragStart,
    handlePointerMove,
    handlePointerUp,
    getDragOffset,
    getDropTarget,
    isDragging: dragState !== null,
  }
}
