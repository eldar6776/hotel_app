'use client'

import { useState } from 'react'
import { ToggleLeft, CheckCircle, XCircle } from 'lucide-react'

interface Feature {
  name: string
  description: string
  enabled: boolean
  category: string
}

const initialFeatures: Feature[] = [
  { name: 'AllowOverbooking', description: 'Dozvoli overbooking rezervacija', enabled: false, category: 'Rezervacije' },
  { name: 'EmailNotifications', description: 'Slanje email potvrda za rezervacije', enabled: true, category: 'Notifikacije' },
  { name: 'NightAudit', description: 'Automatski night audit proces', enabled: true, category: 'Automatizacija' },
  { name: 'NoShowDetection', description: 'Detekcija no-show gostiju', enabled: true, category: 'Automatizacija' },
  { name: 'DynamicPricing', description: 'Dinamicke cijene bazirane na popunjenosti', enabled: false, category: 'Revenue' },
  { name: 'ChannelManager', description: 'Sinhronizacija sa Booking.com, Airbnb', enabled: false, category: 'Integracije' },
  { name: 'IoTDevices', description: 'Smart hotel IoT senzori i brave', enabled: false, category: 'Integracije' },
  { name: 'GuestPortal', description: 'Guest self-service portal', enabled: false, category: 'Integracije' },
  { name: 'PaymentGateway', description: 'Stripe payment processing', enabled: false, category: 'Integracije' },
  { name: 'FiscalPrinter', description: 'Integracija sa fiskalnim printerom', enabled: false, category: 'Hardware' },
]

export default function FeaturesPage() {
  const [features, setFeatures] = useState(initialFeatures)

  const toggleFeature = (name: string) => {
    setFeatures(features.map(f => f.name === name ? { ...f, enabled: !f.enabled } : f))
  }

  const categories = [...new Set(features.map(f => f.category))]

  return (
    <div className="space-y-4">
      <div>
        <h1 className="text-xl font-semibold text-text">Feature flags</h1>
        <p className="text-sm text-text-secondary mt-1">Ukljuci ili iskljuci funkcionalnosti bez deploy-a</p>
      </div>

      {categories.map((category) => (
        <div key={category}>
          <h2 className="text-sm font-medium text-text-secondary uppercase tracking-wide mb-3">{category}</h2>
          <div className="rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 overflow-hidden">
            {features.filter(f => f.category === category).map((feature, i, arr) => (
              <div key={feature.name} className={`flex items-center justify-between p-4 ${i < arr.length - 1 ? 'border-b border-gray-200 dark:border-gray-700' : ''}`}>
                <div className="flex items-center gap-3">
                  {feature.enabled ? <CheckCircle className="h-5 w-5 text-emerald-500" /> : <XCircle className="h-5 w-5 text-gray-400" />}
                  <div>
                    <p className="text-sm font-medium text-text">{feature.name}</p>
                    <p className="text-xs text-text-secondary">{feature.description}</p>
                  </div>
                </div>
                <button
                  onClick={() => toggleFeature(feature.name)}
                  className={`relative w-12 h-6 rounded-full transition-colors ${feature.enabled ? 'bg-primary-500' : 'bg-gray-300 dark:bg-gray-600'}`}
                >
                  <div className={`absolute top-0.5 w-5 h-5 rounded-full bg-white shadow-sm transition-transform ${feature.enabled ? 'left-6' : 'left-0.5'}`} />
                </button>
              </div>
            ))}
          </div>
        </div>
      ))}

      <div className="rounded-xl border border-amber-200 dark:border-amber-800 bg-amber-50 dark:bg-amber-900/10 p-4">
        <div className="flex items-start gap-3">
          <ToggleLeft className="h-5 w-5 text-amber-600 mt-0.5" />
          <div>
            <p className="text-sm font-medium text-amber-800 dark:text-amber-300">Promjene se primjenjuju odmah</p>
            <p className="text-xs text-amber-700 dark:text-amber-400 mt-1">Feature flags se procitavaju pri svakom zahtjevu. Nije potreban restart aplikacije.</p>
          </div>
        </div>
      </div>
    </div>
  )
}
