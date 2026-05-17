'use client'

import { useState, useCallback } from 'react'
import apiClient from '@/lib/api/client'
import { BarChart3, Calendar, DollarSign, Users, TrendingUp, Download } from 'lucide-react'

interface DailyReport {
  date: string
  totalRooms: number
  occupiedRooms: number
  occupancyRate: number
  adr: number
  revPar: number
  arrivals: number
  departures: number
  inHouse: number
}

interface ChannelData {
  channel: string
  bookings: number
  revenue: number
}

const CHANNEL_LABELS: Record<string, string> = {
  Direct: 'Direktno',
  BookingCom: 'Booking.com',
  Expedia: 'Expedia',
  HotelWebsite: 'Web stranica',
  Phone: 'Telefon',
  WalkIn: 'Walk-in',
  Corporate: 'Corporate',
  TravelAgency: 'Agencija',
}

export default function ReportsPage() {
  const [activeTab, setActiveTab] = useState<'daily' | 'financial' | 'guestbook' | 'channel'>('daily')
  const [isLoading, setIsLoading] = useState(false)
  const [dailyReport, setDailyReport] = useState<DailyReport | null>(null)
  const [channelData, setChannelData] = useState<ChannelData[]>([])
  const [reportDate, setReportDate] = useState(new Date().toISOString().split('T')[0])
  const [financialFrom, setFinancialFrom] = useState(new Date(new Date().getFullYear(), new Date().getMonth(), 1).toISOString().split('T')[0])
  const [financialTo, setFinancialTo] = useState(new Date().toISOString().split('T')[0])
  const [financialData, setFinancialData] = useState<any>(null)
  const [guestBookData, setGuestBookData] = useState<any>(null)

  const loadDailyReport = useCallback(async () => {
    setIsLoading(true)
    try {
      const res = await apiClient.get<DailyReport>('/reports/daily', { params: { date: reportDate } })
      setDailyReport(res.data)
    } catch {
      // silent
    } finally {
      setIsLoading(false)
    }
  }, [reportDate])

  const loadChannelReport = useCallback(async () => {
    setIsLoading(true)
    try {
      const res = await apiClient.get<{ channels: ChannelData[] }>('/reports/revenue-by-channel', {
        params: { from: financialFrom, to: financialTo },
      })
      setChannelData(res.data.channels)
    } catch {
      // silent
    } finally {
      setIsLoading(false)
    }
  }, [financialFrom, financialTo])

  const loadFinancialReport = useCallback(async () => {
    setIsLoading(true)
    try {
      const res = await apiClient.get('/reports/financial', { params: { from: financialFrom, to: financialTo } })
      setFinancialData(res.data)
    } catch {
      // silent
    } finally {
      setIsLoading(false)
    }
  }, [financialFrom, financialTo])

  const loadGuestBook = useCallback(async () => {
    setIsLoading(true)
    try {
      const res = await apiClient.get('/reports/guest-book', { params: { date: reportDate } })
      setGuestBookData(res.data)
    } catch {
      // silent
    } finally {
      setIsLoading(false)
    }
  }, [reportDate])

  const handleLoad = () => {
    if (activeTab === 'daily') loadDailyReport()
    else if (activeTab === 'financial') loadFinancialReport()
    else if (activeTab === 'guestbook') loadGuestBook()
    else if (activeTab === 'channel') loadChannelReport()
  }

  return (
    <div className="space-y-4">
      <div>
        <h1 className="text-xl font-semibold text-text">Izvjestaji</h1>
        <p className="text-sm text-text-secondary mt-1">Dnevni, finansijski i statisticki izvjestaji</p>
      </div>

      <div className="flex gap-2 border-b border-gray-200 dark:border-gray-700">
        {([
          { key: 'daily' as const, label: 'Dnevni izvjestaj', Icon: BarChart3 },
          { key: 'financial' as const, label: 'Finansijski', Icon: DollarSign },
          { key: 'guestbook' as const, label: 'Knjiga gostiju', Icon: Users },
          { key: 'channel' as const, label: 'Prihod po kanalu', Icon: TrendingUp },
        ]).map(({ key, label, Icon }) => (
          <button
            key={key}
            className={`flex items-center gap-1 px-4 py-2 text-sm font-medium transition ${activeTab === key ? 'border-b-2 border-primary-500 text-primary-500' : 'text-text-secondary hover:text-text'}`}
            onClick={() => setActiveTab(key)}
          >
            <Icon className="h-4 w-4" /> {label}
          </button>
        ))}
      </div>

      <div className="flex items-center gap-3">
        {activeTab === 'daily' || activeTab === 'guestbook' ? (
          <div className="flex items-center gap-2">
            <Calendar className="h-4 w-4 text-text-secondary" />
            <input
              type="date"
              value={reportDate}
              onChange={(e) => setReportDate(e.target.value)}
              className="rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 px-3 py-2 text-sm text-text"
            />
          </div>
        ) : (
          <div className="flex items-center gap-2">
            <Calendar className="h-4 w-4 text-text-secondary" />
            <input type="date" value={financialFrom} onChange={(e) => setFinancialFrom(e.target.value)} className="rounded-lg border border-border bg-white dark:bg-gray-800 px-3 py-2 text-sm text-text" />
            <span className="text-sm text-text-secondary">do</span>
            <input type="date" value={financialTo} onChange={(e) => setFinancialTo(e.target.value)} className="rounded-lg border border-border bg-white dark:bg-gray-800 px-3 py-2 text-sm text-text" />
          </div>
        )}
        <button onClick={handleLoad} disabled={isLoading} className="rounded-lg bg-primary-500 px-4 py-2 text-sm font-medium text-white hover:bg-primary-600 disabled:opacity-50">
          {isLoading ? 'Ucitavam...' : 'Generisi'}
        </button>
      </div>

      {isLoading && <div className="animate-pulse h-48 rounded-xl bg-gray-200 dark:bg-gray-700" />}

      {!isLoading && activeTab === 'daily' && dailyReport && (
        <div className="space-y-4">
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
            {[
              { label: 'Popunjenost', value: `${dailyReport.occupancyRate}%`, icon: BarChart3, color: 'text-primary-600' },
              { label: 'ADR', value: `${dailyReport.adr.toFixed(2)} €`, icon: DollarSign, color: 'text-emerald-600' },
              { label: 'RevPAR', value: `${dailyReport.revPar.toFixed(2)} €`, icon: TrendingUp, color: 'text-accent-600' },
              { label: 'U hotelu', value: dailyReport.inHouse.toString(), icon: Users, color: 'text-blue-600' },
            ].map((item) => {
              const Icon = item.icon
              return (
                <div key={item.label} className="rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 p-4 shadow-sm">
                  <Icon className={`h-5 w-5 ${item.color} mb-2`} />
                  <p className="text-xs text-text-secondary">{item.label}</p>
                  <p className="text-xl font-bold text-text">{item.value}</p>
                </div>
              )
            })}
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div className="rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 p-4">
              <p className="text-sm text-text-secondary">Dolasci danas</p>
              <p className="text-2xl font-bold text-text">{dailyReport.arrivals}</p>
            </div>
            <div className="rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 p-4">
              <p className="text-sm text-text-secondary">Odlasci danas</p>
              <p className="text-2xl font-bold text-text">{dailyReport.departures}</p>
            </div>
          </div>
          <div className="rounded-xl border border-border bg-white dark:bg-gray-900 p-4">
            <p className="text-sm font-medium text-text mb-2">Detalji</p>
            <div className="grid grid-cols-2 gap-3 text-sm">
              <div><span className="text-text-secondary">Ukupno soba:</span> <span className="text-text font-medium ml-2">{dailyReport.totalRooms}</span></div>
              <div><span className="text-text-secondary">Zauzete:</span> <span className="text-text font-medium ml-2">{dailyReport.occupiedRooms}</span></div>
              <div><span className="text-text-secondary">Slobodne:</span> <span className="text-text font-medium ml-2">{dailyReport.totalRooms - dailyReport.occupiedRooms}</span></div>
              <div><span className="text-text-secondary">Datum:</span> <span className="text-text font-medium ml-2">{new Date(dailyReport.date).toLocaleDateString()}</span></div>
            </div>
          </div>
        </div>
      )}

      {!isLoading && activeTab === 'financial' && financialData && (
        <div className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div className="rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 p-4">
              <p className="text-sm text-text-secondary">Ukupno naplaceno</p>
              <p className="text-2xl font-bold text-emerald-600">{financialData.totalRevenue?.toFixed(2) || '0.00'} €</p>
            </div>
            <div className="rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 p-4">
              <p className="text-sm text-text-secondary">Ukupno fakturisano</p>
              <p className="text-2xl font-bold text-text">{financialData.totalInvoiced?.toFixed(2) || '0.00'} €</p>
            </div>
          </div>
          {financialData.payments?.length > 0 && (
            <div className="rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 p-4">
              <p className="text-sm font-medium text-text mb-3">Placanja po metodi</p>
              <div className="space-y-2">
                {financialData.payments.map((p: any) => (
                  <div key={p.method} className="flex justify-between text-sm">
                    <span className="text-text-secondary">{p.method}</span>
                    <span className="text-text font-medium">{p.total?.toFixed(2)} € ({p.count})</span>
                  </div>
                ))}
              </div>
            </div>
          )}
          {financialData.invoices?.length > 0 && (
            <div className="rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 p-4">
              <p className="text-sm font-medium text-text mb-3">Fakture po statusu</p>
              <div className="space-y-2">
                {financialData.invoices.map((i: any) => (
                  <div key={i.status} className="flex justify-between text-sm">
                    <span className="text-text-secondary">{i.status}</span>
                    <span className="text-text font-medium">{i.total?.toFixed(2)} € ({i.count})</span>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>
      )}

      {!isLoading && activeTab === 'guestbook' && guestBookData && (
        <div className="space-y-4">
          <div className="rounded-xl border border-border bg-white dark:bg-gray-900 p-4">
            <p className="text-sm font-medium text-text mb-1">Knjiga gostiju — {new Date(guestBookData.date).toLocaleDateString()}</p>
            <p className="text-xs text-text-secondary mb-4">{guestBookData.totalGuests} gostiju u hotelu</p>
            {guestBookData.guests?.length > 0 ? (
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b border-border">
                      <th className="text-left py-2 px-3 text-text-secondary font-medium">Ime i prezime</th>
                      <th className="text-left py-2 px-3 text-text-secondary font-medium">Soba</th>
                      <th className="text-left py-2 px-3 text-text-secondary font-medium">Dolazak</th>
                      <th className="text-left py-2 px-3 text-text-secondary font-medium">Odlazak</th>
                    </tr>
                  </thead>
                  <tbody>
                    {guestBookData.guests.map((g: any, i: number) => (
                      <tr key={i} className="border-b border-border">
                        <td className="py-2 px-3 text-text">{g.firstName} {g.lastName}</td>
                        <td className="py-2 px-3 text-text">{g.roomNumber || '-'}</td>
                        <td className="py-2 px-3 text-text">{new Date(g.arrivalDate).toLocaleDateString()}</td>
                        <td className="py-2 px-3 text-text">{new Date(g.departureDate).toLocaleDateString()}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            ) : (
              <p className="text-sm text-text-secondary text-center py-8">Nema gostiju za odabrani datum</p>
            )}
          </div>
        </div>
      )}

      {!isLoading && activeTab === 'channel' && channelData.length > 0 && (
        <div className="space-y-4">
          <div className="rounded-xl border border-border bg-white dark:bg-gray-900 p-4">
            <p className="text-sm font-medium text-text mb-3">Prihod po kanalu</p>
            <div className="space-y-3">
              {channelData.map((ch) => {
                const maxRevenue = Math.max(...channelData.map(c => c.revenue), 1)
                const width = (ch.revenue / maxRevenue) * 100
                return (
                  <div key={ch.channel}>
                    <div className="flex justify-between text-sm mb-1">
                      <span className="text-text font-medium">{CHANNEL_LABELS[ch.channel] || ch.channel}</span>
                      <span className="text-text-secondary">{ch.bookings} rezervacija · {ch.revenue.toFixed(2)} €</span>
                    </div>
                    <div className="h-3 rounded-full bg-gray-100 dark:bg-gray-700 overflow-hidden">
                      <div className="h-full rounded-full bg-primary-500 transition-all" style={{ width: `${width}%` }} />
                    </div>
                  </div>
                )
              })}
            </div>
          </div>
        </div>
      )}

      {!isLoading && !dailyReport && !financialData && !guestBookData && channelData.length === 0 && activeTab !== 'daily' && (
        <div className="rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 p-12 text-center">
          <BarChart3 className="h-12 w-12 mx-auto text-text-secondary mb-3" />
          <p className="text-sm text-text-secondary">Klikni "Generisi" za ucitavanje izvjestaja</p>
        </div>
      )}
    </div>
  )
}
