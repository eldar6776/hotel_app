'use client'

import { Mail, Send, AlertTriangle } from 'lucide-react'

export default function EmailPage() {
  return (
    <div className="space-y-4">
      <div>
        <h1 className="text-xl font-semibold text-text">Email (SMTP)</h1>
        <p className="text-sm text-text-secondary mt-1">Konfiguracija slanja email notifikacija i racuna</p>
      </div>

      <div className="rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 p-5 shadow-sm">
        <div className="flex items-center gap-3 mb-4">
          <div className="rounded-lg bg-primary-50 dark:bg-primary-900/20 p-2"><Mail className="h-5 w-5 text-primary-600" /></div>
          <h3 className="text-sm font-bold text-text">SMTP postavke</h3>
        </div>
        <div className="space-y-3">
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-text-secondary">SMTP Host</label>
              <input defaultValue="smtp.gmail.com" className="w-full rounded-lg border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 px-3 py-2 text-sm text-text" />
            </div>
            <div>
              <label className="text-xs text-text-secondary">SMTP Port</label>
              <input defaultValue="587" type="number" className="w-full rounded-lg border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 px-3 py-2 text-sm text-text" />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-text-secondary">Username</label>
              <input defaultValue="" placeholder="email@hotel.com" className="w-full rounded-lg border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 px-3 py-2 text-sm text-text" />
            </div>
            <div>
              <label className="text-xs text-text-secondary">Password</label>
              <input type="password" defaultValue="" placeholder="••••••••" className="w-full rounded-lg border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 px-3 py-2 text-sm text-text" />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="text-xs text-text-secondary">From Address</label>
              <input defaultValue="noreply@hotelpro.local" className="w-full rounded-lg border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 px-3 py-2 text-sm text-text" />
            </div>
            <div>
              <label className="text-xs text-text-secondary">From Name</label>
              <input defaultValue="HotelPRO" className="w-full rounded-lg border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 px-3 py-2 text-sm text-text" />
            </div>
          </div>
          <div className="flex items-center justify-between pt-2">
            <label className="flex items-center gap-2 text-sm text-text cursor-pointer">
              <input type="checkbox" defaultChecked className="rounded" />
              Use TLS
            </label>
            <div className="flex gap-2">
              <button className="flex items-center gap-1 rounded-lg border border-gray-200 dark:border-gray-700 px-4 py-2 text-sm text-text-secondary hover:text-text">
                <Send className="h-4 w-4" /> Testiraj
              </button>
              <button className="rounded-lg bg-primary-500 px-4 py-2 text-sm font-medium text-white hover:bg-primary-600">
                Spremi
              </button>
            </div>
          </div>
        </div>
      </div>

      <div className="rounded-xl border border-amber-200 dark:border-amber-800 bg-amber-50 dark:bg-amber-900/10 p-4">
        <div className="flex items-start gap-3">
          <AlertTriangle className="h-5 w-5 text-amber-600 mt-0.5" />
          <div>
            <p className="text-sm font-medium text-amber-800 dark:text-amber-300">SMTP nije konfiguriran</p>
            <p className="text-xs text-amber-700 dark:text-amber-400 mt-1">Email notifikacije za rezervacije i racune nece biti poslane dok se SMTP ne konfigurise.</p>
          </div>
        </div>
      </div>
    </div>
  )
}
