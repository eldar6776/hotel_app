'use client'

import { Key, Shield, Clock, Globe } from 'lucide-react'

export default function SecurityPage() {
  return (
    <div className="space-y-4">
      <div>
        <h1 className="text-xl font-semibold text-text">Sigurnost</h1>
        <p className="text-sm text-text-secondary mt-1">JWT, PIN kodovi, sesije i sigurnosne postavke</p>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
        <div className="rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 p-5 shadow-sm">
          <div className="flex items-center gap-3 mb-3">
            <div className="rounded-lg bg-primary-50 dark:bg-primary-900/20 p-2"><Key className="h-5 w-5 text-primary-600" /></div>
            <h3 className="text-sm font-bold text-text">JWT Tokeni</h3>
          </div>
          <div className="space-y-2 text-sm">
            <div className="flex justify-between"><span className="text-text-secondary">Access token expiry:</span><span className="text-text font-medium">60 minuta</span></div>
            <div className="flex justify-between"><span className="text-text-secondary">Refresh token expiry:</span><span className="text-text font-medium">7 dana</span></div>
            <div className="flex justify-between"><span className="text-text-secondary">Algorithm:</span><span className="text-text font-medium">HS256</span></div>
          </div>
        </div>

        <div className="rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 p-5 shadow-sm">
          <div className="flex items-center gap-3 mb-3">
            <div className="rounded-lg bg-emerald-50 dark:bg-emerald-900/20 p-2"><Shield className="h-5 w-5 text-emerald-600" /></div>
            <h3 className="text-sm font-bold text-text">Password Hashing</h3>
          </div>
          <div className="space-y-2 text-sm">
            <div className="flex justify-between"><span className="text-text-secondary">Algorithm:</span><span className="text-text font-medium">BCrypt</span></div>
            <div className="flex justify-between"><span className="text-text-secondary">PIN hashing:</span><span className="text-text font-medium">PBKDF2 (SHA256)</span></div>
            <div className="flex justify-between"><span className="text-text-secondary">Iterations:</span><span className="text-text font-medium">10,000</span></div>
          </div>
        </div>

        <div className="rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 p-5 shadow-sm">
          <div className="flex items-center gap-3 mb-3">
            <div className="rounded-lg bg-amber-50 dark:bg-amber-900/20 p-2"><Clock className="h-5 w-5 text-amber-600" /></div>
            <h3 className="text-sm font-bold text-text">Rate Limiting</h3>
          </div>
          <div className="space-y-2 text-sm">
            <div className="flex justify-between"><span className="text-text-secondary">Auth endpoint:</span><span className="text-text font-medium">5/min</span></div>
            <div className="flex justify-between"><span className="text-text-secondary">Staff API:</span><span className="text-text font-medium">100/min</span></div>
            <div className="flex justify-between"><span className="text-text-secondary">Guest API:</span><span className="text-text font-medium">30/min</span></div>
          </div>
        </div>

        <div className="rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 p-5 shadow-sm">
          <div className="flex items-center gap-3 mb-3">
            <div className="rounded-lg bg-blue-50 dark:bg-blue-900/20 p-2"><Globe className="h-5 w-5 text-blue-600" /></div>
            <h3 className="text-sm font-bold text-text">Multi-Tenant</h3>
          </div>
          <div className="space-y-2 text-sm">
            <div className="flex justify-between"><span className="text-text-secondary">Tenant header:</span><span className="text-text font-medium">X-Hotel-Code</span></div>
            <div className="flex justify-between"><span className="text-text-secondary">Isolation:</span><span className="text-text font-medium">Query filters</span></div>
            <div className="flex justify-between"><span className="text-text-secondary">Hotel code:</span><span className="text-text font-medium">HVA</span></div>
          </div>
        </div>
      </div>
    </div>
  )
}
