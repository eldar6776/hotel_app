'use client'

import { useEffect, useState, useCallback } from 'react'
import { useRouter } from 'next/navigation'
import { Search } from 'lucide-react'

interface Command {
  id: string
  label: string
  category: string
  action: () => void
}

export function CommandPalette({
  open,
  onClose,
}: {
  open: boolean
  onClose: () => void
}) {
  const [query, setQuery] = useState('')
  const router = useRouter()

  const commands: Command[] = [
    {
      id: 'nav-dashboard',
      label: 'Idi na Dashboard',
      category: 'Navigacija',
      action: () => router.push('/dashboard'),
    },
    {
      id: 'nav-rooms',
      label: 'Idi na Sobe',
      category: 'Navigacija',
      action: () => router.push('/rooms'),
    },
    {
      id: 'nav-bookings',
      label: 'Idi na Rezervacije',
      category: 'Navigacija',
      action: () => router.push('/bookings'),
    },
    {
      id: 'nav-guests',
      label: 'Idi na Goste',
      category: 'Navigacija',
      action: () => router.push('/guests'),
    },
    {
      id: 'nav-folio',
      label: 'Idi na Folio',
      category: 'Navigacija',
      action: () => router.push('/folio'),
    },
    {
      id: 'nav-reports',
      label: 'Idi na Izvjestaje',
      category: 'Navigacija',
      action: () => router.push('/reports'),
    },
    {
      id: 'nav-settings',
      label: 'Idi na Postavke',
      category: 'Navigacija',
      action: () => router.push('/settings'),
    },
  ]

  const reset = useCallback(() => {
    setQuery('')
    onClose()
  }, [onClose])

  useEffect(() => {
    const handler = (e: KeyboardEvent) => {
      if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
        e.preventDefault()
        if (open) {
          reset()
        } else {
          onClose()
        }
      }
      if (e.key === 'Escape' && open) {
        reset()
      }
    }
    window.addEventListener('keydown', handler)
    return () => window.removeEventListener('keydown', handler)
  }, [open, reset, onClose])

  if (!open) return null

  const filtered = commands.filter((c) =>
    c.label.toLowerCase().includes(query.toLowerCase())
  )

  const grouped = new Map<string, Command[]>()
  for (const cmd of filtered) {
    const group = grouped.get(cmd.category) || []
    group.push(cmd)
    grouped.set(cmd.category, group)
  }

  return (
    <div className="fixed inset-0 z-50 flex items-start justify-center pt-[15vh]">
      <div
        className="fixed inset-0 bg-black/50 backdrop-blur-sm"
        onClick={reset}
      />
      <div className="relative z-10 w-full max-w-xl rounded-xl bg-white dark:bg-gray-900 shadow-2xl border border-gray-200 dark:border-gray-700 overflow-hidden">
        <div className="flex items-center gap-3 border-b border-gray-200 dark:border-gray-700 px-4 py-3">
          <Search className="h-4 w-4 text-text-secondary" />
          <input
            autoFocus
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            placeholder="Pretrazi komande..."
            className="flex-1 bg-transparent text-sm text-text outline-none placeholder:text-text-secondary/50"
          />
          <kbd className="rounded bg-gray-100 dark:bg-gray-800 px-2 py-0.5 text-xs text-text-secondary">
            ESC
          </kbd>
        </div>
        <div className="max-h-72 overflow-y-auto p-2">
          {filtered.length === 0 ? (
            <p className="px-3 py-4 text-center text-sm text-text-secondary">
              Nema rezultata
            </p>
          ) : (
            Array.from(grouped.entries()).map(([category, cmds]) => (
              <div key={category}>
                <p className="px-3 pt-2 pb-1 text-xs font-medium text-text-secondary">
                  {category}
                </p>
                {cmds.map((cmd) => (
                  <button
                    key={cmd.id}
                    onClick={() => {
                      cmd.action()
                      reset()
                    }}
                    className="flex w-full items-center gap-3 rounded-lg px-3 py-2 text-sm text-text hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors"
                  >
                    {cmd.label}
                    <span className="ml-auto text-xs text-text-secondary">
                      {cmd.category}
                    </span>
                  </button>
                ))}
              </div>
            ))
          )}
        </div>
        <div className="border-t border-gray-200 dark:border-gray-700 px-4 py-2">
          <div className="flex items-center gap-4 text-xs text-text-secondary">
            <span>
              <kbd className="rounded bg-gray-100 dark:bg-gray-800 px-1 py-0.5">
                Ctrl+K
              </kbd>{' '}
              Otvori
            </span>
            <span>
              <kbd className="rounded bg-gray-100 dark:bg-gray-800 px-1 py-0.5">
                ESC
              </kbd>{' '}
              Zatvori
            </span>
          </div>
        </div>
      </div>
    </div>
  )
}
