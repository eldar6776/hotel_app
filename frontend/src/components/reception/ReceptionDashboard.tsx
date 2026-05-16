'use client'

import { useState, useEffect, useCallback } from 'react'
import { ArrivalsPanel } from './ArrivalsPanel'
import { DeparturesPanel } from './DeparturesPanel'
import { InHousePanel } from './InHousePanel'
import { QuickActions } from './QuickActions'
import { CheckInModal } from './CheckInModal'
import { CheckOutModal } from './CheckOutModal'
import { bookingService } from '@/lib/bookings/booking-service'
import type { BookingDto } from '@/types/bookings'

export function ReceptionDashboard() {
  const [arrivals, setArrivals] = useState<BookingDto[]>([])
  const [departures, setDepartures] = useState<BookingDto[]>([])
  const [inHouse, setInHouse] = useState<BookingDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [activeTab, setActiveTab] = useState<'arrivals' | 'departures' | 'inhouse'>('arrivals')
  const [checkInTarget, setCheckInTarget] = useState<BookingDto | null>(null)
  const [checkOutTarget, setCheckOutTarget] = useState<BookingDto | null>(null)

  const today = new Date().toISOString().split('T')[0]

  const loadData = useCallback(async () => {
    setIsLoading(true)
    try {
      const [a, d, ih] = await Promise.all([
        bookingService.getBookings({ status: ['Confirmed'], fromDate: today }),
        bookingService.getBookings({ status: ['CheckedIn'], toDate: today }),
        bookingService.getBookings({ status: ['CheckedIn'] }),
      ])
      setArrivals(a.items.filter((b) => b.arrivalDate.split('T')[0] === today))
      setDepartures(d.items.filter((b) => b.departureDate.split('T')[0] === today))
      setInHouse(ih.items)
    } catch {
      // silent
    } finally {
      setIsLoading(false)
    }
  }, [today])

  useEffect(() => {
    // eslint-disable-next-line react-hooks/set-state-in-effect
    loadData()
    const interval = setInterval(loadData, 30000)
    return () => clearInterval(interval)
  }, [loadData])

  const handleCheckIn = (booking: BookingDto) => setCheckInTarget(booking)
  const handleCheckOut = (booking: BookingDto) => setCheckOutTarget(booking)

  return (
    <div className="space-y-4">
      <QuickActions />

      <div className="flex gap-2 border-b border-border">
        {[
          ['arrivals', 'Dolasci', arrivals.length],
          ['departures', 'Odlazak', departures.length],
          ['inhouse', 'U hotelu', inHouse.length],
        ].map(([key, label, count]) => (
          <button
            key={key}
            className={`px-4 py-2 text-sm font-medium transition ${
              activeTab === key
                ? 'border-b-2 border-primary-500 text-primary-500'
                : 'text-text-secondary hover:text-text'
            }`}
            onClick={() => setActiveTab(key as typeof activeTab)}
          >
            {label} ({count})
          </button>
        ))}
      </div>

      {isLoading ? (
        <div className="animate-pulse h-64 rounded-xl bg-surface-tertiary" />
      ) : activeTab === 'arrivals' ? (
        <ArrivalsPanel bookings={arrivals} onCheckIn={handleCheckIn} />
      ) : activeTab === 'departures' ? (
        <DeparturesPanel bookings={departures} onCheckOut={handleCheckOut} />
      ) : (
        <InHousePanel bookings={inHouse} onCheckOut={handleCheckOut} />
      )}

      {checkInTarget && (
        <CheckInModal booking={checkInTarget} onClose={() => setCheckInTarget(null)} onSuccess={loadData} />
      )}
      {checkOutTarget && (
        <CheckOutModal booking={checkOutTarget} onClose={() => setCheckOutTarget(null)} onSuccess={loadData} />
      )}
    </div>
  )
}
