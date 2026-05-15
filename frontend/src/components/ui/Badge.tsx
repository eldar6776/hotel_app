interface BadgeProps {
  variant: 'confirmed' | 'checkedin' | 'checkedout' | 'cancelled' | 'today'
  children: React.ReactNode
}

const variants: Record<string, string> = {
  confirmed: 'bg-primary-100 text-primary-700 dark:bg-primary-900/30 dark:text-primary-300',
  checkedin: 'bg-success/10 text-success',
  checkedout: 'bg-surface-tertiary text-text-secondary',
  cancelled: 'bg-error/10 text-error',
  today: 'bg-accent-100 text-accent-700 dark:bg-accent-900/30 dark:text-accent-300',
}

export default function Badge({ variant, children }: BadgeProps) {
  return (
    <span
      className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${variants[variant]}`}
    >
      {children}
    </span>
  )
}
