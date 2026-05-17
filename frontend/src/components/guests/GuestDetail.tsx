'use client'

import { useEffect, useState } from 'react'
import apiClient from '@/lib/api/client'
import type { GuestDto } from '@/types/guests'

interface GuestProfile {
  guest: GuestDto
  stayHistory: { bookingId: string; checkedInAt: string; checkedOutAt: string | null; roomNumber: string; nights: number }[]
  bookingHistory: { bookingId: string; arrival: string; departure: string; status: string; nights: number; totalPrice: number; roomTypeName: string }[]
  totalStays: number
  totalSpent: number
}

export function GuestDetail({ guest, onClose }: { guest: GuestDto; onClose: () => void }) {
  const [profile, setProfile] = useState<GuestProfile | null>(null)
  const [activeTab, setActiveTab] = useState<'info' | 'stays' | 'bookings'>('info')

  useEffect(() => {
    apiClient.get(`/v2/guests/${guest.id}/profile`).then(r => setProfile(r.data)).catch(() => {})
  }, [guest.id])

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm" onClick={onClose}>
      <div className="w-full max-w-3xl rounded-xl bg-white dark:bg-gray-900 p-6 shadow-2xl border border-gray-200 dark:border-gray-700 max-h-[90vh] overflow-y-auto" onClick={e => e.stopPropagation()}>
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-xl font-bold text-text">{guest.firstName} {guest.lastName}</h2>
          <button onClick={onClose} className="text-text-secondary hover:text-text">&times;</button>
        </div>

        <div className="flex gap-2 border-b border-border mb-4">
          {(['info', 'stays', 'bookings'] as const).map(tab => (
            <button key={tab} className={`px-4 py-2 text-sm font-medium ${activeTab === tab ? 'border-b-2 border-primary-500 text-primary-500' : 'text-text-secondary'}`} onClick={() => setActiveTab(tab)}>
              {{ info: 'Podaci', stays: 'Istorija boravaka', bookings: 'Rezervacije' }[tab]}
            </button>
          ))}
        </div>

        {activeTab === 'info' && (
          <div className="grid grid-cols-2 gap-3 text-sm">
            <div><span className="text-text-secondary">Email:</span> <p className="text-text">{guest.email || '-'}</p></div>
            <div><span className="text-text-secondary">Telefon:</span> <p className="text-text">{guest.phone || '-'}</p></div>
            <div><span className="text-text-secondary">Grad:</span> <p className="text-text">{guest.city || '-'}</p></div>
            <div><span className="text-text-secondary">Drzava:</span> <p className="text-text">{guest.countryName || '-'}</p></div>
            <div><span className="text-text-secondary">Datum rodjenja:</span> <p className="text-text">{guest.dateOfBirth ? new Date(guest.dateOfBirth).toLocaleDateString() : '-'}</p></div>
            <div><span className="text-text-secondary">Pol:</span> <p className="text-text">{guest.gender || '-'}</p></div>
            <div><span className="text-text-secondary">Tip:</span> <p className="text-text">{guest.isCompany ? 'Pravno lice' : 'Fizicko lice'}</p></div>
            <div><span className="text-text-secondary">GDPR:</span> <p className="text-text">{guest.gdprConsentGiven ? '✅ Dat' : '❌ Nije dat'}</p></div>
            {profile && (
              <>
                <div><span className="text-text-secondary">Ukupno boravaka:</span> <p className="text-text">{profile.totalStays}</p></div>
                <div><span className="text-text-secondary">Ukupno potroseno:</span> <p className="text-text font-bold">{profile.totalSpent.toFixed(2)} €</p></div>
              </>
            )}
            {guest.documents.length > 0 && (
              <div className="col-span-2 mt-2">
                <p className="text-text-secondary mb-1">Dokumenti:</p>
                {guest.documents.map((d, i) => (
                  <p key={i} className="text-xs text-text">
                    {d.documentType}: {d.documentNumber} {d.isVerified ? '✅' : '⚠️'}
                    {d.expiryDate && ` (do ${new Date(d.expiryDate).toLocaleDateString()})`}
                  </p>
                ))}
              </div>
            )}
          </div>
        )}

        {activeTab === 'stays' && profile && (
          <div className="space-y-2">
            {profile.stayHistory.length === 0 ? (
              <p className="text-sm text-text-secondary text-center py-8">Nema istorije boravaka</p>
            ) : (
              profile.stayHistory.map(s => (
                <div key={s.bookingId} className="rounded-lg border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 p-3 flex justify-between items-center">
                  <div>
                    <p className="text-sm font-medium text-text">Soba {s.roomNumber} · {s.nights}n</p>
                    <p className="text-xs text-text-secondary">
                      {new Date(s.checkedInAt).toLocaleDateString()} {s.checkedOutAt ? `→ ${new Date(s.checkedOutAt).toLocaleDateString()}` : '(trenutno)'}
                    </p>
                  </div>
                  {!s.checkedOutAt && <span className="rounded-full bg-blue-500 px-2 py-0.5 text-xs text-white">Aktivan</span>}
                </div>
              ))
            )}
          </div>
        )}

        {activeTab === 'bookings' && profile && (
          <div className="space-y-2">
            {profile.bookingHistory.length === 0 ? (
              <p className="text-sm text-text-secondary text-center py-8">Nema rezervacija</p>
            ) : (
              profile.bookingHistory.map(b => (
                <div key={b.bookingId} className="rounded-lg border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 p-3 flex justify-between items-center">
                  <div>
                    <p className="text-sm font-medium text-text">{b.roomTypeName} · {b.nights}n</p>
                    <p className="text-xs text-text-secondary">{new Date(b.arrival).toLocaleDateString()} → {new Date(b.departure).toLocaleDateString()}</p>
                  </div>
                  <div className="text-right">
                    <p className="text-sm font-bold text-text">{b.totalPrice.toFixed(0)} €</p>
                    <p className="text-xs text-text-secondary">{b.status}</p>
                  </div>
                </div>
              ))
            )}
          </div>
        )}
      </div>
    </div>
  )
}
