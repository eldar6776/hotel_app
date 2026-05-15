'use client'

import { Menu, Search, Moon, Sun, HelpCircle } from 'lucide-react'
import { useSidebar } from '@/components/layout/SidebarContext'
import { useTheme } from '@/components/providers/ThemeProvider'
import NavbarUserMenu from '@/components/layout/NavbarUserMenu'
import Breadcrumbs from '@/components/layout/Breadcrumbs'
import { HelpTooltip } from '@/components/help/HelpTooltip'

interface NavbarProps {
  onHelpToggle?: () => void
  onSearchOpen?: () => void
}

export default function Navbar({ onHelpToggle, onSearchOpen }: NavbarProps) {
  const { toggleMobile, isMobileOpen } = useSidebar()
  const { theme, toggleTheme } = useTheme()

  return (
    <header className="sticky top-0 z-30 flex h-[var(--navbar-height)] shrink-0 items-center gap-4 border-b border-border bg-surface px-4">
      <button
        onClick={toggleMobile}
        className="inline-flex items-center justify-center rounded-lg p-2 text-text-secondary hover:bg-surface-secondary dark:hover:bg-surface-tertiary lg:hidden"
        aria-label={isMobileOpen ? 'Zatvori meni' : 'Otvori meni'}
      >
        <Menu className="h-5 w-5" />
      </button>

      <Breadcrumbs />

      <div className="ml-auto flex items-center gap-1">
        <HelpTooltip id="navbar-search">
          <button
            onClick={onSearchOpen}
            className="inline-flex items-center gap-2 rounded-lg px-3 py-2 text-sm text-text-secondary hover:bg-surface-secondary dark:hover:bg-surface-tertiary transition-colors"
            id="navbar-search"
            title="Pretraga (Ctrl+K)"
          >
            <Search className="h-4 w-4" />
            <span className="hidden lg:inline">Pretraga...</span>
            <kbd className="hidden lg:inline-flex items-center rounded bg-surface-tertiary px-1.5 py-0.5 text-xs text-text-secondary">
              Ctrl+K
            </kbd>
          </button>
        </HelpTooltip>

        <button
          onClick={toggleTheme}
          className="rounded-lg p-2 text-text-secondary hover:bg-surface-secondary dark:hover:bg-surface-tertiary transition-colors"
          title={theme === 'dark' ? 'Svijetla tema' : 'Tamna tema'}
        >
          {theme === 'dark' ? (
            <Sun className="h-4 w-4" />
          ) : (
            <Moon className="h-4 w-4" />
          )}
        </button>

        <button
          onClick={onHelpToggle}
          className="rounded-lg p-2 text-text-secondary hover:bg-surface-secondary dark:hover:bg-surface-tertiary transition-colors"
          title="Pomoc"
        >
          <HelpCircle className="h-4 w-4" />
        </button>

        <NavbarUserMenu />
      </div>
    </header>
  )
}
