'use client'

import { useEffect, useState, useCallback } from 'react'
import apiClient from '@/lib/api/client'
import { Users, Plus, Edit2, Shield, ShieldOff } from 'lucide-react'

interface EmployeeDto {
  id: string
  firstName: string
  lastName: string
  email: string
  phone: string | null
  role: string
  isActive: boolean
  canLogin: boolean
  createdAt: string
}

const ROLE_LABELS: Record<string, string> = {
  Admin: 'Admin',
  Manager: 'Menadzer',
  Reception: 'Recepcija',
  Housekeeping: 'Sobarica',
}

export default function EmployeesPage() {
  const [employees, setEmployees] = useState<EmployeeDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [showForm, setShowForm] = useState(false)
  const [form, setForm] = useState({ firstName: '', lastName: '', email: '', phone: '', role: 'Reception', password: '' })
  const [editingId, setEditingId] = useState<string | null>(null)

  const loadData = useCallback(async () => {
    setIsLoading(true)
    try {
      const res = await apiClient.get<EmployeeDto[]>('/employees')
      setEmployees(res.data)
    } catch { /* silent */ }
    finally { setIsLoading(false) }
  }, [])

  useEffect(() => { loadData() }, [loadData])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      const dto: any = { firstName: form.firstName, lastName: form.lastName, email: form.email, role: form.role }
      if (form.phone) dto.phone = form.phone
      if (form.password) dto.password = form.password
      if (editingId) {
        await apiClient.put(`/employees/${editingId}`, dto)
      } else {
        dto.isActive = true
        dto.canLogin = true
        await apiClient.post('/employees', dto)
      }
      setShowForm(false)
      setEditingId(null)
      setForm({ firstName: '', lastName: '', email: '', phone: '', role: 'Reception', password: '' })
      loadData()
    } catch { alert('Greska pri spremanju') }
  }

  const handleToggleActive = async (id: string, current: boolean) => {
    try {
      await apiClient.patch(`/employees/${id}/toggle-active`, { isActive: !current })
      loadData()
    } catch { alert('Greska') }
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-semibold text-text">Zaposleni</h1>
          <p className="text-sm text-text-secondary mt-1">{employees.length} zaposlenih</p>
        </div>
        <button onClick={() => { setShowForm(!showForm); setEditingId(null); setForm({ firstName: '', lastName: '', email: '', phone: '', role: 'Reception', password: '' }) }} className="flex items-center gap-1 rounded-lg bg-primary-500 px-4 py-2 text-sm font-medium text-white hover:bg-primary-600">
          <Plus className="h-4 w-4" /> Novi zaposleni
        </button>
      </div>

      {showForm && (
        <form onSubmit={handleSubmit} className="rounded-xl border border-border bg-white dark:bg-gray-900 p-4 space-y-3">
          <div className="grid grid-cols-2 gap-3">
            <input required placeholder="Ime" value={form.firstName} onChange={e => setForm({ ...form, firstName: e.target.value })} className="rounded-lg border border-border bg-gray-50 dark:bg-gray-800 px-3 py-2 text-sm text-text" />
            <input required placeholder="Prezime" value={form.lastName} onChange={e => setForm({ ...form, lastName: e.target.value })} className="rounded-lg border border-border bg-gray-50 dark:bg-gray-800 px-3 py-2 text-sm text-text" />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <input required type="email" placeholder="Email" value={form.email} onChange={e => setForm({ ...form, email: e.target.value })} className="rounded-lg border border-border bg-gray-50 dark:bg-gray-800 px-3 py-2 text-sm text-text" />
            <input placeholder="Telefon" value={form.phone} onChange={e => setForm({ ...form, phone: e.target.value })} className="rounded-lg border border-border bg-gray-50 dark:bg-gray-800 px-3 py-2 text-sm text-text" />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <select value={form.role} onChange={e => setForm({ ...form, role: e.target.value })} className="rounded-lg border border-border bg-gray-50 dark:bg-gray-800 px-3 py-2 text-sm text-text">
              {Object.entries(ROLE_LABELS).map(([k, v]) => <option key={k} value={k}>{v}</option>)}
            </select>
            <input type="password" placeholder={editingId ? 'Lozinka (prazno = nepromijenjeno)' : 'Lozinka'} value={form.password} onChange={e => setForm({ ...form, password: e.target.value })} className="rounded-lg border border-border bg-gray-50 dark:bg-gray-800 px-3 py-2 text-sm text-text" />
          </div>
          <div className="flex justify-end gap-2">
            <button type="button" onClick={() => { setShowForm(false); setEditingId(null) }} className="rounded-lg px-4 py-2 text-sm text-text-secondary hover:text-text">Odustani</button>
            <button type="submit" className="rounded-lg bg-primary-500 px-4 py-2 text-sm font-medium text-white hover:bg-primary-600">{editingId ? 'Azuriraj' : 'Kreiraj'}</button>
          </div>
        </form>
      )}

      {isLoading ? (
        <div className="animate-pulse h-48 rounded-xl bg-gray-200 dark:bg-gray-700" />
      ) : employees.length === 0 ? (
        <div className="rounded-xl border border-border bg-white dark:bg-gray-900 p-12 text-center">
          <Users className="h-12 w-12 mx-auto text-text-secondary mb-3" />
          <p className="text-sm text-text-secondary">Nema zaposlenih</p>
        </div>
      ) : (
        <div className="rounded-xl border border-border bg-white dark:bg-gray-900 overflow-hidden">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-border">
                <th className="text-left py-3 px-4 text-text-secondary font-medium">Ime</th>
                <th className="text-left py-3 px-4 text-text-secondary font-medium">Email</th>
                <th className="text-left py-3 px-4 text-text-secondary font-medium">Uloga</th>
                <th className="text-left py-3 px-4 text-text-secondary font-medium">Login</th>
                <th className="text-right py-3 px-4 text-text-secondary font-medium">Akcije</th>
              </tr>
            </thead>
            <tbody>
              {employees.map(emp => (
                <tr key={emp.id} className="border-b border-border last:border-0">
                  <td className="py-3 px-4 text-text font-medium">{emp.firstName} {emp.lastName}</td>
                  <td className="py-3 px-4 text-text-secondary">{emp.email}</td>
                  <td className="py-3 px-4"><span className="rounded-full bg-primary-50 dark:bg-primary-900/20 px-2 py-0.5 text-xs font-medium text-primary-600">{ROLE_LABELS[emp.role] || emp.role}</span></td>
                  <td className="py-3 px-4">
                    <button onClick={() => handleToggleActive(emp.id, emp.isActive)} className={`rounded-full px-2 py-0.5 text-xs font-medium ${emp.isActive ? 'bg-emerald-100 text-emerald-700' : 'bg-gray-100 text-gray-500'}`}>
                      {emp.isActive ? 'Aktivan' : 'Neaktivan'}
                    </button>
                  </td>
                  <td className="py-3 px-4 text-right">
                    <div className="flex justify-end gap-1">
                      <button onClick={() => { setForm({ firstName: emp.firstName, lastName: emp.lastName, email: emp.email, phone: emp.phone || '', role: emp.role, password: '' }); setEditingId(emp.id); setShowForm(true) }} className="rounded p-1 text-text-secondary hover:text-primary-600"><Edit2 className="h-4 w-4" /></button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}
