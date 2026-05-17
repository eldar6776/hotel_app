import Link from 'next/link'
import {
  Receipt,
  Sparkles,
  Building2,
  BedDouble,
  Users,
  Shield,
  Mail,
  Globe,
  ToggleLeft,
  Key,
} from 'lucide-react'

interface SettingsSection {
  title: string
  description: string
  items: { href: string; icon: typeof Receipt; label: string; desc: string }[]
}

const sections: SettingsSection[] = [
  {
    title: 'Hotel konfiguracija',
    description: 'Osnovne postavke hotela i soba',
    items: [
      { href: '/settings/tariffs', icon: Receipt, label: 'Tarife', desc: 'Cjenovnici i rate planovi' },
      { href: '/settings/amenities', icon: Sparkles, label: 'Sadrzaji', desc: 'Udobnosti i oprema soba' },
      { href: '/settings/buildings', icon: Building2, label: 'Zgrade', desc: 'Objekti i etaže' },
      { href: '/settings/room-types', icon: BedDouble, label: 'Tipovi soba', desc: 'Definicije tipova soba' },
    ],
  },
  {
    title: 'Korisnici i sigurnost',
    description: 'Zaposleni, uloge i pristup',
    items: [
      { href: '/settings/employees', icon: Users, label: 'Zaposleni', desc: 'Tim i raspored smjena' },
      { href: '/settings/roles', icon: Shield, label: 'Uloge i dozvole', desc: 'RBAC konfiguracija' },
      { href: '/settings/security', icon: Key, label: 'Sigurnost', desc: 'JWT, PIN, sesije' },
    ],
  },
  {
    title: 'Sistem i integracije',
    description: 'Email, jezik, feature flags',
    items: [
      { href: '/settings/email', icon: Mail, label: 'Email (SMTP)', desc: 'Slanje notifikacija i racuna' },
      { href: '/settings/language', icon: Globe, label: 'Jezik', desc: 'Prevodi i lokalizacija' },
      { href: '/settings/features', icon: ToggleLeft, label: 'Feature flags', desc: 'Ukljuci/iskljuci funkcionalnosti' },
    ],
  },
]

export default function SettingsPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-xl font-semibold text-text">Postavke</h1>
        <p className="text-sm text-text-secondary mt-1">Konfiguracija sistema, hotela i integracija</p>
      </div>

      {sections.map((section) => (
        <div key={section.title}>
          <h2 className="text-sm font-medium text-text-secondary uppercase tracking-wide mb-3">{section.title}</h2>
          <p className="text-xs text-text-secondary mb-3">{section.description}</p>
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
            {section.items.map((item) => {
              const Icon = item.icon
              return (
                <Link
                  key={item.href}
                  href={item.href}
                  className="rounded-xl border border-border bg-white dark:bg-gray-900 p-5 shadow-sm hover:shadow-md hover:border-primary-300 transition-all group"
                >
                  <div className="flex items-start gap-3">
                    <div className="rounded-lg bg-primary-50 dark:bg-primary-900/20 p-2 text-primary-600 group-hover:bg-primary-100 dark:group-hover:bg-primary-900/30 transition-colors">
                      <Icon className="h-5 w-5" />
                    </div>
                    <div>
                      <h3 className="text-sm font-semibold text-text group-hover:text-primary-600 transition-colors">{item.label}</h3>
                      <p className="text-xs text-text-secondary mt-0.5">{item.desc}</p>
                    </div>
                  </div>
                </Link>
              )
            })}
          </div>
        </div>
      ))}
    </div>
  )
}
