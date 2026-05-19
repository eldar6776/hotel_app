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
}

export interface DropTarget {
  rowIndex: number
  roomId: string
  available: boolean
}

function isRoomAvailable(room: GanttRoom, booking: GanttBooking): boolean {
  if (room.id === '__unassigned__') return true
  return !room.bookings.some(
    (b) =>
      b.id !== booking.id &&
      b.arrivalDate < booking.departureDate &&
      b.departureDate > booking.arrivalDate,
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
      }
      dragRef.current = state
      setDragState(state)
    },
    [],
  )

  // ── POINTER MOVE — React handler, ide na grid div ───────────────────────────
  // Pointer capture na GanttBaru garantuje da eventi bubblauju do grida čak i
  // kad je miš izvan grid elementa.
  const handlePointerMove = useCallback((e: React.PointerEvent) => {
    if (!dragRef.current) return
    e.preventDefault()

    const current = dragRef.current
    const dx = e.clientX - current.startX
    const dy = e.clientY - current.startY

    const updated: DragState = { ...current, currentX: e.clientX, currentY: e.clientY }

    // Vertikalni drag: mora biti dominantno vertikalan I minimalno 10px pomaka
    const isVertical = Math.abs(dy) > Math.abs(dx) && Math.abs(dy) > 10

    if (isVertical) {
      const grid = gridRef.current
      if (grid) {
        const rect = grid.getBoundingClientRect()
        const relY = e.clientY - rect.top + grid.scrollTop
        const rh = rowHeightRef.current
        const rowIndex = Math.max(
          0,
          Math.min(Math.floor(relY / rh), ganttDataRef.current.length - 1),
        )
        const targetRoom = ganttDataRef.current[rowIndex]
        if (targetRoom) {
          updated.targetRowIndex = rowIndex
          updated.targetRoomId = targetRoom.id
          updated.dropAvailable = isRoomAvailable(targetRoom, current.booking)
        }
      }
    } else {
      updated.targetRowIndex = null
      updated.targetRoomId = null
      updated.dropAvailable = null
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

    const isVertical = Math.abs(dy) > Math.abs(dx) && Math.abs(dy) > 10

    if (isVertical) {
      // ── VERTIKALNI DROP: dodjela / oslobađanje sobe ─────────────────
      const targetRoomId = current.targetRoomId
      if (!targetRoomId || !current.dropAvailable) return

      const currentRoomId = current.booking.roomId ?? null
      const newRoomId = targetRoomId === '__unassigned__' ? null : targetRoomId
      if (newRoomId === currentRoomId) return

      const canAssign =
        current.booking.status === 'Confirmed' ||
        current.booking.status === 'Pending' ||
        current.booking.status === 'CheckedIn'
      if (!canAssign) return

      // Odmah pomijeri u UI bez čekanja API-ja
      onOptimisticAssignRef.current?.(current.booking.id, newRoomId, currentRoomId)

      try {
        await bookingService.assignRoom(current.booking.id, newRoomId)
        onBookingMovedRef.current?.()
      } catch (err) {
        console.error('[Drag] assignRoom greška:', err)
        onAssignErrorRef.current?.(current.booking.id, currentRoomId)
      }
    } else {
      // ── HORIZONTALNI DROP: pomjeranje datuma ────────────────────────
      const cw = columnWidthRef.current
      const daysDiff = Math.round(dx / cw)
      const canMove =
        daysDiff !== 0 &&
        (current.booking.status === 'Confirmed' || current.booking.status === 'Pending')

      if (!canMove) return

      const newArrival = new Date(current.booking.arrivalDate)
      newArrival.setDate(newArrival.getDate() + daysDiff)
      const newDeparture = new Date(current.booking.departureDate)
      newDeparture.setDate(newDeparture.getDate() + daysDiff)

      const today = new Date()
      today.setHours(0, 0, 0, 0)
      if (newArrival < today) return

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
