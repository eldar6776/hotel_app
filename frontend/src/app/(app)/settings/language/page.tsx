'use client'

import { Globe, Languages } from 'lucide-react'

const languages = [
  { code: 'hr', name: 'Hrvatski', flag: '🇭🇷', status: 'Kompletan', progress: 100 },
  { code: 'en', name: 'English', flag: '🇬🇧', status: 'Kompletan', progress: 100 },
  { code: 'de', name: 'Deutsch', flag: '🇩🇪', status: 'Djelomican', progress: 65 },
  { code: 'it', name: 'Italiano', flag: '🇮🇹', status: 'Djelomican', progress: 45 },
]

export default function LanguagePage() {
  return (
    <div className="space-y-4">
      <div>
        <h1 className="text-xl font-semibold text-text">Jezik i lokalizacija</h1>
        <p className="text-sm text-text-secondary mt-1">Prevodi i podrzani jezici</p>
      </div>

      <div className="rounded-xl border border-border bg-white dark:bg-gray-900 p-5 shadow-sm">
        <div className="flex items-center gap-3 mb-4">
          <div className="rounded-lg bg-primary-50 dark:bg-primary-900/20 p-2"><Globe className="h-5 w-5 text-primary-600" /></div>
          <h3 className="text-sm font-bold text-text">Podrzani jezici</h3>
        </div>
        <div className="space-y-3">
          {languages.map((lang) => (
            <div key={lang.code} className="flex items-center justify-between py-3 border-b border-border last:border-0">
              <div className="flex items-center gap-3">
                <span className="text-2xl">{lang.flag}</span>
                <div>
                  <p className="text-sm font-medium text-text">{lang.name}</p>
                  <p className="text-xs text-text-secondary">{lang.code}</p>
                </div>
              </div>
              <div className="flex items-center gap-3">
                <div className="w-32 h-2 rounded-full bg-gray-100 dark:bg-gray-700 overflow-hidden">
                  <div className="h-full rounded-full bg-primary-500" style={{ width: `${lang.progress}%` }} />
                </div>
                <span className="text-xs text-text-secondary w-20 text-right">{lang.progress}%</span>
                <span className={`rounded-full px-2 py-0.5 text-xs font-medium ${lang.progress === 100 ? 'bg-emerald-100 text-emerald-700' : 'bg-amber-100 text-amber-700'}`}>
                  {lang.status}
                </span>
              </div>
            </div>
          ))}
        </div>
      </div>

      <div className="rounded-xl border border-border bg-white dark:bg-gray-900 p-5 shadow-sm">
        <div className="flex items-center gap-3 mb-3">
          <div className="rounded-lg bg-emerald-50 dark:bg-emerald-900/20 p-2"><Languages className="h-5 w-5 text-emerald-600" /></div>
          <h3 className="text-sm font-bold text-text">Formati</h3>
        </div>
        <div className="space-y-2 text-sm">
          <div className="flex justify-between py-2 border-b border-border"><span className="text-text-secondary">Datum:</span><span className="text-text font-medium">DD.MM.YYYY</span></div>
          <div className="flex justify-between py-2 border-b border-border"><span className="text-text-secondary">Vrijeme:</span><span className="text-text font-medium">24h (HH:mm)</span></div>
          <div className="flex justify-between py-2 border-b border-border"><span className="text-text-secondary">Valuta:</span><span className="text-text font-medium">EUR (€)</span></div>
          <div className="flex justify-between py-2"><span className="text-text-secondary">Timezone:</span><span className="text-text font-medium">Europe/Zagreb (CET)</span></div>
        </div>
      </div>
    </div>
  )
}
