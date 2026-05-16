'use client'

import { useState } from 'react'
import type { BookingDto } from '@/types/bookings'
import apiClient from '@/lib/api/client'

interface Props {
  booking: BookingDto
  onClose: () => void
  onSuccess: () => void
}

export function CheckOutModal({ booking, onClose, onSuccess }: Props) {
  const [paymentMethod, setPaymentMethod] = useState('Cash')
  const [lateCheckout, setLateCheckout] = useState(false)
  const [isLoading, setIsLoading] = useState(false)
  const [result, setResult] = useState<{
    totalAmount: number
    stayCharges: number
    folioCharges: number
    lateCheckoutFee: number
    discountAmount: number
    folioNumber: string
  } | null>(null)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setIsLoading(true)
    try {
      const res = await apiClient.post('/v2/reception/check-out', {
        bookingId: booking.id,
        paymentMethod,
        paymentReference: null,
        lateCheckout,
        applyDiscounts: true,
      })
      setResult(res.data)
    } catch {
      alert('Greška pri check-outu')
    } finally {
      setIsLoading(false)
    }
  }

  if (result) {
    return (
      <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50" onClick={onClose}>
        <div className="w-full max-w-md rounded-xl bg-surface p-6 shadow-xl" onClick={(e) => e.stopPropagation()}>
          <h2 className="text-lg font-bold text-text mb-2">Check-out završen</h2>
          <div className="space-y-2 text-sm mb-4">
            <div className="flex justify-between"><span className="text-text-secondary">Noćenja:</span><span className="text-text">{result.stayCharges.toFixed(2)} €</span></div>
            <div className="flex justify-between"><span className="text-text-secondary">Folio troškovi:</span><span className="text-text">{result.folioCharges.toFixed(2)} €</span></div>
            {result.lateCheckoutFee > 0 && (
              <div className="flex justify-between"><span className="text-text-secondary">Late check-out:</span><span className="text-text">{result.lateCheckoutFee.toFixed(2)} €</span></div>
            )}
            {result.discountAmount > 0 && (
              <div className="flex justify-between"><span className="text-text-secondary">Popust:</span><span className="text-text text-emerald-600">-{result.discountAmount.toFixed(2)} €</span></div>
            )}
            <div className="flex justify-between border-t border-border pt-2 font-bold"><span>Ukupno:</span><span className="text-text">{result.totalAmount.toFixed(2)} €</span></div>
            <div className="text-xs text-text-secondary">Folio: {result.folioNumber} · Plaćanje: {paymentMethod}</div>
          </div>
          <button onClick={() => { onSuccess(); onClose() }} className="w-full rounded-lg bg-primary-500 px-4 py-2 text-sm font-medium text-white">
            Zatvori
          </button>
        </div>
      </div>
    )
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50" onClick={onClose}>
      <div className="w-full max-w-md rounded-xl bg-surface p-6 shadow-xl" onClick={(e) => e.stopPropagation()}>
        <h2 className="text-lg font-bold text-text mb-2">Check-out: {booking.guestName}</h2>
        <p className="text-sm text-text-secondary mb-4">Soba {booking.rooms?.[0]?.roomNumber || 'N/A'} · {booking.nights}n</p>

        <form onSubmit={handleSubmit} className="space-y-3">
          <select
            value={paymentMethod}
            onChange={(e) => setPaymentMethod(e.target.value)}
            className="w-full rounded-lg border border-border bg-surface px-3 py-2 text-sm text-text"
          >
            <option value="Cash">Gotovina</option>
            <option value="Card">Kartica</option>
            <option value="Invoice">Faktura</option>
            <option value="BankTransfer">Bankovni transfer</option>
          </select>
          <label className="flex items-center gap-2 text-sm text-text cursor-pointer">
            <input type="checkbox" checked={lateCheckout} onChange={(e) => setLateCheckout(e.target.checked)} className="rounded" />
            Late check-out (nakon 12:00)
          </label>
          <div className="flex gap-2 justify-end">
            <button type="button" onClick={onClose} className="rounded-lg px-4 py-2 text-sm text-text-secondary hover:text-text">Odustani</button>
            <button type="submit" disabled={isLoading} className="rounded-lg bg-amber-500 px-4 py-2 text-sm font-medium text-white hover:bg-amber-600 disabled:opacity-50">
              {isLoading ? 'Obračun...' : 'Check-out'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}
