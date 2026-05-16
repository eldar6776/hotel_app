'use client'

import { useState, useCallback, createContext, useContext, useEffect } from 'react'
import { Joyride, type Step, type EventData, STATUS } from 'react-joyride'

const TourStarter = createContext<() => void>(() => {})
export const useTour = () => useContext(TourStarter)

const defaultSteps: Step[] = [
  {
    target: '[data-help-id="sidebar-dashboard"]',
    content: 'Ovo je vas dashboard. Ovdje vidite osnovne KPI podatke o stanju hotela.',
    placement: 'right',
    title: 'Dobrodosli u HotelPRO!',
  },
  {
    target: '[data-help-id="sidebar-rooms"]',
    content: 'Pregledajte sobe, njihov status i upravljajte njima.',
    placement: 'right',
    title: 'Pregled soba',
  },
  {
    target: '[data-help-id="sidebar-bookings"]',
    content: 'Kreirajte nove rezervacije i upravljajte postojecom.',
    placement: 'right',
    title: 'Rezervacije',
  },
  {
    target: '#navbar-search',
    content: 'Brza pretraga: pritisnite Ctrl+K ili kliknite ovdje za pretragu gostiju, rezervacija i soba.',
    placement: 'bottom',
    title: 'Pretraga',
  },
  {
    target: '[data-help-id="dashboard-kpi"]',
    content: 'Ovdje su kljucni pokazatelji — popunjenost, ADR, RevPAR, check-in/out brojevi.',
    placement: 'top',
    title: 'KPI pokazatelji',
  },
]

export function TourProvider({ children }: { children: React.ReactNode }) {
  const [run, setRun] = useState(false)
  const [steps] = useState<Step[]>(defaultSteps)
  const [mounted, setMounted] = useState(false)

  useEffect(() => { setMounted(true) }, [])

  const startTour = useCallback(() => { setRun(true) }, [])

  const handleEvent = (data: EventData) => {
    if (data.status === STATUS.FINISHED || data.status === STATUS.SKIPPED) {
      setRun(false)
      localStorage.setItem('hotelpro_tour_completed', 'true')
    }
  }

  return (
    <>
      {mounted && (
        <Joyride
          steps={steps}
          run={run}
          continuous
          onEvent={handleEvent}
          locale={{
            back: 'Nazad',
            close: 'Zatvori',
            last: 'Zavrsi',
            next: 'Dalje',
            skip: 'Preskoci',
          }}
          options={{
            primaryColor: 'hsl(214 90% 42%)',
            zIndex: 100,
            showProgress: true,
          }}
        />
      )}
      <TourStarter.Provider value={startTour}>
        {children}
      </TourStarter.Provider>
    </>
  )
}
