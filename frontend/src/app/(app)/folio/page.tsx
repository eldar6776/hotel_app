'use client'

import { useEffect, useState, useCallback } from 'react'
import { folioService } from '@/lib/folios/folio-service'
import type { FolioDto, CreateFolioChargeDto, FolioStayNightDto } from '@/types/folios'
import { CHARGE_TYPES } from '@/types/folios'
import {
  Plus,
  X,
  Trash2,
  RotateCcw,
  CheckCircle,
  Receipt,
  ChevronDown,
  ChevronUp,
} from 'lucide-react'
import Button from '@/components/ui/Button'

const money = (value: number | null | undefined) => (value ?? 0).toFixed(2)
const stayNightAmount = (night: FolioStayNightDto) => night.tariffAmount ?? night.roomPrice ?? 0

export default function FolioPage() {
  const [folios, setFolios] = useState<FolioDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [expandedFolio, setExpandedFolio] = useState<string | null>(null)
  const [chargeFormFolio, setChargeFormFolio] = useState<string | null>(null)
  const [chargeForm, setChargeForm] = useState<CreateFolioChargeDto>({
    chargeType: 'Room',
    description: '',
    quantity: 1,
    unitPrice: 0,
    chargeDate: new Date().toISOString().split('T')[0],
    posReference: null,
  })
  const [stornoReason, setStornoReason] = useState<{ chargeId: string; reason: string } | null>(null)

  const loadFolios = useCallback(async () => {
    setIsLoading(true)
    try {
      const data = await folioService.getOpenFolios()
      setFolios(data)
    } catch {
      // silent
    } finally {
      setIsLoading(false)
    }
  }, [])

  useEffect(() => {
    loadFolios()
  }, [loadFolios])

  const handleAddCharge = async (folioId: string) => {
    try {
      await folioService.addCharge(folioId, chargeForm)
      setChargeFormFolio(null)
      setChargeForm({ chargeType: 'Room', description: '', quantity: 1, unitPrice: 0, chargeDate: new Date().toISOString().split('T')[0], posReference: null })
      loadFolios()
    } catch {
      alert('Greska pri dodavanju troska')
    }
  }

  const handleDeleteCharge = async (chargeId: string) => {
    if (!confirm('Obrisati ovaj trosak?')) return
    try {
      await folioService.deleteCharge(chargeId)
      loadFolios()
    } catch {
      alert('Greska pri brisanju')
    }
  }

  const handleStornoCharge = async (chargeId: string) => {
    if (!stornoReason || !stornoReason.reason.trim()) return
    try {
      await folioService.stornoCharge(chargeId, stornoReason.reason)
      setStornoReason(null)
      loadFolios()
    } catch {
      alert('Greska pri stornu')
    }
  }

  const handleCloseFolio = async (folioId: string) => {
    if (!confirm('Zatvoriti ovaj folio?')) return
    try {
      await folioService.closeFolio(folioId)
      loadFolios()
    } catch {
      alert('Greska pri zatvaranju folija')
    }
  }

  const totalBalance = folios.reduce((sum, f) => sum + (f.balance ?? 0), 0)

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-semibold text-text">Folio — Otvoreni racuni</h1>
          <p className="text-sm text-text-secondary mt-1">
            {folios.length} otvorenih folija · Ukupno: <span className="font-bold text-text">{money(totalBalance)} EUR</span>
          </p>
        </div>
        <Button variant="secondary" onClick={loadFolios}>
          Osvjezi
        </Button>
      </div>

      {isLoading ? (
        <div className="animate-pulse h-64 rounded-xl bg-surface-tertiary" />
      ) : folios.length === 0 ? (
        <div className="rounded-xl border border-border bg-surface p-12 text-center">
          <Receipt className="h-12 w-12 mx-auto text-text-secondary mb-3" />
          <p className="text-sm text-text-secondary">Nema otvorenih folija</p>
        </div>
      ) : (
        <div className="space-y-3">
          {folios.map((folio) => {
            const isExpanded = expandedFolio === folio.id
            const isChargeOpen = chargeFormFolio === folio.id
            const charges = folio.charges ?? []
            const stayNights = folio.stayNights ?? []
            const totalCharges = charges.reduce((s, c) => s + (c.totalPrice ?? 0), 0)
            const totalNights = stayNights.reduce((s, n) => s + stayNightAmount(n), 0)

            return (
              <div key={folio.id} className="rounded-xl border border-border bg-surface shadow-sm overflow-hidden">
                <button
                  onClick={() => setExpandedFolio(isExpanded ? null : folio.id)}
                  className="w-full flex items-center justify-between p-4 hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors"
                >
                  <div className="flex items-center gap-4">
                    <div className="rounded-lg bg-primary-50 dark:bg-primary-900/20 p-2">
                      <Receipt className="h-5 w-5 text-primary-600" />
                    </div>
                    <div className="text-left">
                      <p className="text-sm font-semibold text-text">{folio.folioNumber}</p>
                      <p className="text-xs text-text-secondary">{folio.guestName || 'Bez gosta'}</p>
                    </div>
                  </div>
                  <div className="flex items-center gap-6">
                    <div className="text-right">
                      <p className="text-xs text-text-secondary">Troskovi</p>
                      <p className="text-sm font-medium text-text">{money(totalCharges)} €</p>
                    </div>
                    <div className="text-right">
                      <p className="text-xs text-text-secondary">Nocenja</p>
                      <p className="text-sm font-medium text-text">{money(totalNights)} €</p>
                    </div>
                    <div className="text-right">
                      <p className="text-xs text-text-secondary">Stanje</p>
                      <p className={`text-sm font-bold ${(folio.balance ?? 0) > 0 ? 'text-red-600' : 'text-emerald-600'}`}>
                        {money(folio.balance)} €
                      </p>
                    </div>
                    {isExpanded ? <ChevronUp className="h-4 w-4 text-text-secondary" /> : <ChevronDown className="h-4 w-4 text-text-secondary" />}
                  </div>
                </button>

                {isExpanded && (
                  <div className="border-t border-border px-4 py-4 space-y-4">
                    <div className="flex items-center justify-between">
                      <h3 className="text-sm font-medium text-text">Troskovi ({charges.length})</h3>
                      <button
                        onClick={() => setChargeFormFolio(isChargeOpen ? null : folio.id)}
                        className="flex items-center gap-1 rounded-lg bg-primary-500 px-3 py-1.5 text-xs font-medium text-white hover:bg-primary-600"
                      >
                        <Plus className="h-3 w-3" /> Dodaj trosak
                      </button>
                    </div>

                    {isChargeOpen && (
                      <div className="rounded-lg border border-border bg-surface-secondary p-4 space-y-3">
                        <div className="grid grid-cols-2 gap-3">
                          <select
                            value={chargeForm.chargeType}
                            onChange={(e) => setChargeForm({ ...chargeForm, chargeType: e.target.value })}
                            className="rounded-lg border border-border bg-surface px-3 py-2 text-sm text-text"
                          >
                            {CHARGE_TYPES.map((ct) => (
                              <option key={ct.value} value={ct.value}>{ct.label}</option>
                            ))}
                          </select>
                          <input
                            type="date"
                            value={chargeForm.chargeDate}
                            onChange={(e) => setChargeForm({ ...chargeForm, chargeDate: e.target.value })}
                            className="rounded-lg border border-border bg-surface px-3 py-2 text-sm text-text"
                          />
                        </div>
                        <input
                          placeholder="Opis troska"
                          value={chargeForm.description}
                          onChange={(e) => setChargeForm({ ...chargeForm, description: e.target.value })}
                          className="w-full rounded-lg border border-border bg-surface px-3 py-2 text-sm text-text"
                        />
                        <div className="grid grid-cols-2 gap-3">
                          <input
                            type="number"
                            min="1"
                            value={chargeForm.quantity}
                            onChange={(e) => setChargeForm({ ...chargeForm, quantity: Number(e.target.value) })}
                            className="rounded-lg border border-border bg-surface px-3 py-2 text-sm text-text"
                            placeholder="Kolicina"
                          />
                          <input
                            type="number"
                            step="0.01"
                            min="0"
                            value={chargeForm.unitPrice}
                            onChange={(e) => setChargeForm({ ...chargeForm, unitPrice: Number(e.target.value) })}
                            className="rounded-lg border border-border bg-surface px-3 py-2 text-sm text-text"
                            placeholder="Cijena po komadu"
                          />
                        </div>
                        <div className="flex justify-end gap-2">
                          <button onClick={() => setChargeFormFolio(null)} className="rounded-lg px-3 py-1.5 text-xs text-text-secondary hover:text-text">
                            Odustani
                          </button>
                          <button
                            onClick={() => handleAddCharge(folio.id)}
                            disabled={!chargeForm.description || chargeForm.unitPrice <= 0}
                            className="rounded-lg bg-primary-500 px-4 py-1.5 text-xs font-medium text-white hover:bg-primary-600 disabled:opacity-50"
                          >
                            Dodaj ({money(chargeForm.quantity * chargeForm.unitPrice)} €)
                          </button>
                        </div>
                      </div>
                    )}

                    {charges.length === 0 ? (
                      <p className="text-xs text-text-secondary text-center py-4">Nema troskova</p>
                    ) : (
                      <div className="space-y-1">
                        {charges.map((charge) => (
                          <div key={charge.id} className="flex items-center justify-between rounded-lg bg-surface-secondary px-3 py-2 text-sm">
                            <div className="flex-1">
                              <p className="text-text font-medium">{charge.description}</p>
                              <p className="text-xs text-text-secondary">
                                {charge.chargeType} · {charge.quantity} x {money(charge.unitPrice)} € · {new Date(charge.chargeDate).toLocaleDateString()}
                              </p>
                            </div>
                            <div className="flex items-center gap-3">
                              <span className={`font-bold ${(charge.totalPrice ?? 0) < 0 ? 'text-emerald-600' : 'text-text'}`}>
                                {money(charge.totalPrice)} €
                              </span>
                              <div className="flex gap-1">
                                {stornoReason?.chargeId === charge.id ? (
                                  <div className="flex items-center gap-1">
                                    <input
                                      placeholder="Razlog storna"
                                      value={stornoReason.reason}
                                      onChange={(e) => setStornoReason({ chargeId: charge.id, reason: e.target.value })}
                                      className="rounded border border-border bg-surface px-2 py-1 text-xs text-text w-32"
                                    />
                                    <button onClick={() => handleStornoCharge(charge.id)} className="rounded bg-amber-500 px-2 py-1 text-xs text-white">
                                      OK
                                    </button>
                                    <button onClick={() => setStornoReason(null)} className="rounded px-1 text-text-secondary">
                                      <X className="h-3 w-3" />
                                    </button>
                                  </div>
                                ) : (
                                  <>
                                    <button onClick={() => setStornoReason({ chargeId: charge.id, reason: '' })} title="Storno" className="rounded p-1 text-amber-600 hover:bg-amber-50 dark:hover:bg-amber-900/20">
                                      <RotateCcw className="h-3.5 w-3.5" />
                                    </button>
                                    <button onClick={() => handleDeleteCharge(charge.id)} title="Obrisi" className="rounded p-1 text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20">
                                      <Trash2 className="h-3.5 w-3.5" />
                                    </button>
                                  </>
                                )}
                              </div>
                            </div>
                          </div>
                        ))}
                      </div>
                    )}

                    {stayNights.length > 0 && (
                      <div>
                        <h3 className="text-sm font-medium text-text mb-2">Nocenja ({stayNights.length})</h3>
                        <div className="space-y-1">
                          {stayNights.map((night) => (
                            <div key={night.id} className="flex items-center justify-between rounded-lg bg-surface-secondary px-3 py-2 text-sm">
                              <div>
                                <p className="text-text font-medium">Nocenje</p>
                                <p className="text-xs text-text-secondary">{new Date(night.date).toLocaleDateString()}{night.isComp ? ' · Komp' : ''}</p>
                              </div>
                              <span className="font-bold text-text">{money(stayNightAmount(night))} €</span>
                            </div>
                          ))}
                        </div>
                      </div>
                    )}

                    <div className="flex justify-end pt-2 border-t border-border">
                      <button
                        onClick={() => handleCloseFolio(folio.id)}
                        className="flex items-center gap-1 rounded-lg bg-emerald-500 px-4 py-2 text-xs font-medium text-white hover:bg-emerald-600"
                      >
                        <CheckCircle className="h-3.5 w-3.5" /> Zatvori folio
                      </button>
                    </div>
                  </div>
                )}
              </div>
            )
          })}
        </div>
      )}
    </div>
  )
}
