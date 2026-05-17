'use client'

import { Shield, ShieldCheck, Key, Eye, Users } from 'lucide-react'

const roles = [
  { name: 'Admin', desc: 'Potpuni pristup svim funkcionalnostima', permissions: ['Sve dozvole'], color: 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-300' },
  { name: 'Manager', desc: 'Upravljanje hotelom, izvjestaji, cijene', permissions: ['Rezervacije', 'Folio', 'Izvjestaji', 'Postavke', 'Housekeeping'], color: 'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-300' },
  { name: 'Reception', desc: 'Check-in/out, rezervacije, gosti', permissions: ['Rezervacije', 'Gosti', 'Recepcija', 'Folio'], color: 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-300' },
  { name: 'Housekeeping', desc: 'Ciscenje soba, work orders', permissions: ['Housekeeping', 'Sobe (pregled)'], color: 'bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-300' },
]

export default function RolesPage() {
  return (
    <div className="space-y-4">
      <div>
        <h1 className="text-xl font-semibold text-text">Uloge i dozvole</h1>
        <p className="text-sm text-text-secondary mt-1">RBAC konfiguracija — role-based access control</p>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
        {roles.map((role) => {
          const Icon = role.name === 'Admin' ? ShieldCheck : role.name === 'Manager' ? Key : role.name === 'Reception' ? Eye : Users
          return (
            <div key={role.name} className="rounded-xl border border-border bg-white dark:bg-gray-900 p-5 shadow-sm">
              <div className="flex items-start gap-3 mb-3">
                <div className={`rounded-lg p-2 ${role.color}`}>
                  <Icon className="h-5 w-5" />
                </div>
                <div>
                  <h3 className="text-sm font-bold text-text">{role.name}</h3>
                  <p className="text-xs text-text-secondary mt-0.5">{role.desc}</p>
                </div>
              </div>
              <div className="flex flex-wrap gap-1">
                {role.permissions.map((p) => (
                  <span key={p} className="rounded-full bg-gray-100 dark:bg-gray-800 px-2 py-0.5 text-xs text-text-secondary">{p}</span>
                ))}
              </div>
            </div>
          )
        })}
      </div>

      <div className="rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 p-4">
        <h3 className="text-sm font-medium text-text mb-2">Politike autorizacije</h3>
        <div className="space-y-2 text-sm">
          <div className="flex justify-between py-2 border-b border-gray-200 dark:border-gray-700">
            <span className="text-text-secondary">CanManageBookings</span>
            <span className="text-text font-medium">Admin, Manager, Reception</span>
          </div>
          <div className="flex justify-between py-2 border-b border-gray-200 dark:border-gray-700">
            <span className="text-text-secondary">CanViewReports</span>
            <span className="text-text font-medium">Admin, Manager</span>
          </div>
          <div className="flex justify-between py-2 border-b border-gray-200 dark:border-gray-700">
            <span className="text-text-secondary">CanManageSettings</span>
            <span className="text-text font-medium">Admin</span>
          </div>
          <div className="flex justify-between py-2">
            <span className="text-text-secondary">CanManageHousekeeping</span>
            <span className="text-text font-medium">Admin, Manager, Housekeeping</span>
          </div>
        </div>
      </div>
    </div>
  )
}
