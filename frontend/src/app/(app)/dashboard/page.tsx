'use client'

import { useEffect, useState, useMemo } from 'react'
import {
  BedDouble,
  DollarSign,
  TrendingUp,
  LogIn,
  LogOut,
  Receipt,
  DoorOpen,
  AlertTriangle,
} from 'lucide-react'
import KpiCard from '@/components/dashboard/KpiCard'
import { OccupancyChart } from '@/components/dashboard/OccupancyChart'
import RecentBookings from '@/components/dashboard/RecentBookings'
import UpcomingCheckins from '@/components/dashboard/UpcomingCheckins'
import Skeleton from '@/components/ui/Skeleton'
import Alert from '@/components/ui/Alert'
import Button from '@/components/ui/Button'
import {
  dashboardService,
  type KpiData,
  type OccupancyTrendPoint,
  type RecentBooking,
  type UpcomingCheckin,
} from '@/lib/dashboard/dashboard-service'

export default function DashboardPage() {
  const [kpi, setKpi] = useState<KpiData | null>(null)
  const [trend, setTrend] = useState<OccupancyTrendPoint[]>([])
  const [bookings, setBookings] = useState<RecentBooking[]>([])
  const [checkins, setCheckins] = useState<UpcomingCheckin[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    let cancelled = false

    async function load() {
      try {
        const [kpiData, trendData, bookingsData, checkinsData] =
          await Promise.all([
            dashboardService.getKpi(),
            dashboardService.getOccupancyTrend(),
            dashboardService.getRecentBookings(),
            dashboardService.getUpcomingCheckIns(),
          ])
        if (!cancelled) {
          setKpi(kpiData)
          setTrend(trendData)
          setBookings(bookingsData)
          setCheckins(checkinsData)
          setError('')
        }
      } catch {
        if (!cancelled) {
          setError('Nije moguce ucitati podatke. Provjerite konekciju.')
        }
      } finally {
        if (!cancelled) {
          setLoading(false)
        }
      }
    }

    load()

    return () => {
      cancelled = true
    }
  }, [])

  useEffect(() => {
    let cancelled = false
    let interval: ReturnType<typeof setInterval> | null = null

    const startInterval = () => {
      if (interval) return
      interval = setInterval(() => {
        dashboardService.getKpi().then((data) => {
          if (!cancelled) setKpi(data)
        }).catch(() => {})
      }, 60000)
    }

    const stopInterval = () => {
      if (interval) {
        clearInterval(interval)
        interval = null
      }
    }

    const handleVisibility = () => {
      if (document.visibilityState === 'visible') {
        dashboardService.getKpi().then((data) => {
          if (!cancelled) setKpi(data)
        }).catch(() => {})
        startInterval()
      } else {
        stopInterval()
      }
    }

    if (document.visibilityState === 'visible') {
      startInterval()
    }

    document.addEventListener('visibilitychange', handleVisibility)
    return () => {
      cancelled = true
      stopInterval()
      document.removeEventListener('visibilitychange', handleVisibility)
    }
  }, [])

  const kpiCards = useMemo(() => {
    if (!kpi) return []
    return [
      {
        icon: BedDouble,
        label: 'Popunjenost',
        value: `${kpi.occupancyPercent}%`,
        trend: { direction: 'up' as const, percent: 5 },
        color: 'primary' as const,
        helpId: 'dashboard-occupancy',
      },
      {
        icon: DollarSign,
        label: 'ADR',
        value: `EUR ${kpi.adr.toFixed(2)}`,
        color: 'success' as const,
        helpId: 'dashboard-adr',
      },
      {
        icon: TrendingUp,
        label: 'RevPAR',
        value: `EUR ${kpi.revpar.toFixed(2)}`,
        color: 'accent' as const,
        helpId: 'dashboard-revpar',
      },
      {
        icon: LogIn,
        label: 'Check-in danas',
        value: kpi.todayCheckIns,
        color: 'success' as const,
      },
      {
        icon: LogOut,
        label: 'Check-out danas',
        value: kpi.todayCheckOuts,
        color: 'warning' as const,
      },
      {
        icon: Receipt,
        label: 'Otvoreni racuni',
        value: kpi.openFolios,
        color: 'primary' as const,
      },
      {
        icon: DoorOpen,
        label: 'Slobodnih soba',
        value: `${kpi.freeRooms}/${kpi.totalRooms}`,
        color: 'success' as const,
      },
      {
        icon: AlertTriangle,
        label: 'Prljave sobe',
        value: kpi.dirtyRooms,
        color: 'error' as const,
        trend:
          kpi.dirtyRooms > 0
            ? { direction: 'up' as const, percent: 10 }
            : undefined,
      },
    ]
  }, [kpi])

  if (loading) {
    return (
      <div className="space-y-6">
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {Array.from({ length: 6 }).map((_, i) => (
            <div
              key={i}
              className="rounded-xl bg-surface p-4 shadow-sm border border-border"
            >
              <div className="flex items-center gap-3">
                <Skeleton className="h-10 w-10 rounded-lg" />
                <div className="flex-1 space-y-2">
                  <Skeleton className="h-3 w-20" />
                  <Skeleton className="h-6 w-16" />
                </div>
              </div>
            </div>
          ))}
        </div>
        <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
          <div className="lg:col-span-2 rounded-xl bg-surface p-4 shadow-sm border border-border">
            <Skeleton className="h-64 w-full rounded-lg" />
          </div>
          <Skeleton className="h-64 w-full rounded-xl" />
        </div>
        <Skeleton className="h-48 w-full rounded-xl" />
      </div>
    )
  }

  if (error) {
    return (
      <div className="space-y-4">
        <Alert type="error">{error}</Alert>
        <Button variant="secondary" onClick={() => window.location.reload()}>
          Pokusaj ponovo
        </Button>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-xl font-semibold text-text">Dashboard</h1>
        <p className="text-sm text-text-secondary">
          Pregled stanja hotela za danas
        </p>
      </div>

      <div data-help-id="dashboard-kpi" className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {kpiCards.map((card) => (
          <KpiCard key={card.label} {...card} />
        ))}
      </div>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
        <div className="lg:col-span-2 rounded-xl bg-surface p-4 shadow-sm border border-border">
          <h3 className="mb-4 text-sm font-semibold text-text">
            Trend popunjenosti i prihoda
          </h3>
          {trend.length > 0 ? (
            <OccupancyChart data={trend} />
          ) : (
            <p className="py-8 text-center text-sm text-text-secondary">
              Nema dostupnih podataka
            </p>
          )}
        </div>
        <UpcomingCheckins checkins={checkins} />
      </div>

      <RecentBookings bookings={bookings} />
    </div>
  )
}
