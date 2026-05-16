'use client'

import { useState, useCallback, useRef, useEffect } from 'react'
import type { GanttBooking } from '@/types/bookings'
import { bookingService } from '@/lib/bookings/booking-service'

interface DragState {
  booking: GanttBooking
  startX: number
  startY: number
  currentX: number
  currentY: number
}

export function useDragAndDrop(
  columnWidth: number,
  rowHeight: number,
  dateRange: string[],
  onBookingMoved?: () => void
) {
  const [dragState, setDragState] = useState<DragState | null>(null)
  const dragRef = useRef<DragState | null>(null)
  const gridElRef = useRef<HTMLDivElement | null>(null)

  const setGridRef = useCallback((el: HTMLDivElement | null) => {
    gridElRef.current = el
  }, [])

  const handleDragStart = useCallback(
    (booking: GanttBooking, e: React.PointerEvent) => {
      e.preventDefault()
      const el = e.currentTarget as HTMLElement
      el.setPointerCapture(e.pointerId)

      const state: DragState = {
        booking,
        startX: e.clientX,
        startY: e.clientY,
        currentX: e.clientX,
        currentY: e.clientY,
      }
      dragRef.current = state
      setDragState(state)
    },
    []
  )

  useEffect(() => {
    const handleMove = (e: globalThis.PointerEvent) => {
      if (!dragRef.current) return
      dragRef.current = {
        ...dragRef.current,
        currentX: e.clientX,
        currentY: e.clientY,
      }
      setDragState({ ...dragRef.current })
    }

    const handleUp = async (e: globalThis.PointerEvent) => {
      const current = dragRef.current
      if (!current) return

      const el = e.target as HTMLElement
      if (el && typeof el.releasePointerCapture === 'function') {
        el.releasePointerCapture(e.pointerId)
      }

      dragRef.current = null
      const dx = e.clientX - current.startX
      const dy = e.clientY - current.startY

      if (Math.abs(dx) < 10 && Math.abs(dy) < 10) {
        setDragState(null)
        return
      }

      const daysDiff = Math.round(dx / columnWidth)
      const canMove = daysDiff !== 0 && (
        current.booking.status === 'Confirmed' || current.booking.status === 'Pending'
      )

      if (canMove) {
        try {
          const oldArrival = new Date(current.booking.arrivalDate)
          const oldDeparture = new Date(current.booking.departureDate)
          const newArrival = new Date(oldArrival)
          newArrival.setDate(newArrival.getDate() + daysDiff)
          const newDeparture = new Date(oldDeparture)
          newDeparture.setDate(newDeparture.getDate() + daysDiff)

          await bookingService.updateBooking(current.booking.id, {
            arrivalDate: newArrival.toISOString(),
            departureDate: newDeparture.toISOString(),
          })

          onBookingMoved?.()
        } catch {
          // API failure - silently revert
        }
      }

      setDragState(null)
    }

    window.addEventListener('pointermove', handleMove)
    window.addEventListener('pointerup', handleUp)

    return () => {
      window.removeEventListener('pointermove', handleMove)
      window.removeEventListener('pointerup', handleUp)
    }
  }, [columnWidth, onBookingMoved])

  const getDragOffset = useCallback((): { x: number; y: number } | null => {
    if (!dragState) return null
    return {
      x: dragState.currentX - dragState.startX,
      y: dragState.currentY - dragState.startY,
    }
  }, [dragState])

  return {
    dragState,
    setGridRef,
    handleDragStart,
    getDragOffset,
    isDragging: dragState !== null,
  }
}
