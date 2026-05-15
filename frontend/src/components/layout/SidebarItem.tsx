'use client'

import Link from 'next/link'
import { usePathname } from 'next/navigation'
import { type LucideIcon } from 'lucide-react'
import { useSidebar } from '@/components/layout/SidebarContext'

export interface SidebarItemData {
  href: string
  icon: LucideIcon
  label: string
  helpId?: string
}

interface SidebarItemProps {
  item: SidebarItemData
}

export default function SidebarItem({ item }: SidebarItemProps) {
  const pathname = usePathname()
  const { isExpanded } = useSidebar()
  const isActive = pathname === item.href || pathname.startsWith(item.href + '/')
  const Icon = item.icon

  return (
    <Link
      href={item.href}
      id={item.helpId}
      className={`group flex items-center gap-3 rounded-r-lg px-3 py-2.5 text-sm font-medium transition-colors ${
        isActive
          ? 'bg-primary-50 text-primary-700 dark:bg-primary-900/20 dark:text-primary-300'
          : 'text-text-secondary hover:bg-surface-secondary dark:hover:bg-surface-tertiary hover:text-text'
      } ${!isExpanded ? 'justify-center px-2' : ''}`}
      title={!isExpanded ? item.label : undefined}
    >
      <Icon className="h-5 w-5 shrink-0" />
      {isExpanded && <span className="truncate">{item.label}</span>}
    </Link>
  )
}
