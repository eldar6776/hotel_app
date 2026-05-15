import { type LucideIcon } from 'lucide-react'

interface KpiCardProps {
  icon: LucideIcon
  label: string
  value: string | number
  trend?: { direction: 'up' | 'down'; percent: number }
  color: 'primary' | 'success' | 'warning' | 'error' | 'accent'
  helpId?: string
}

const colorClasses: Record<string, { bg: string; text: string }> = {
  primary: { bg: 'bg-primary-100 dark:bg-primary-900/30', text: 'text-primary-600 dark:text-primary-400' },
  success: { bg: 'bg-success/10', text: 'text-success' },
  warning: { bg: 'bg-warning/10', text: 'text-warning' },
  error: { bg: 'bg-error/10', text: 'text-error' },
  accent: { bg: 'bg-accent-100 dark:bg-accent-900/30', text: 'text-accent-600 dark:text-accent-400' },
}

export default function KpiCard({ icon: Icon, label, value, trend, color, helpId }: KpiCardProps) {
  return (
    <div
      data-help-id={helpId}
      className="rounded-xl bg-surface p-4 shadow-sm border border-border"
    >
      <div className="flex items-center gap-3">
        <div className={`rounded-lg p-2 ${colorClasses[color].bg} ${colorClasses[color].text}`}>
          <Icon className="h-5 w-5" />
        </div>
        <div className="flex-1 min-w-0">
          <p className="text-sm text-text-secondary truncate">{label}</p>
          <p className="text-2xl font-semibold text-text">{value}</p>
        </div>
        {trend && (
          <span
            className={`flex items-center gap-0.5 text-sm font-medium ${
              trend.direction === 'up' ? 'text-success' : 'text-error'
            }`}
          >
            {trend.direction === 'up' ? '\u2191' : '\u2193'} {trend.percent}%
          </span>
        )}
      </div>
    </div>
  )
}
