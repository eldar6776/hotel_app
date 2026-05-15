interface AlertProps {
  type: 'error' | 'success' | 'warning' | 'info'
  children: React.ReactNode
}

const styles: Record<string, string> = {
  error: 'bg-error/10 border-error/30 text-error',
  success: 'bg-success/10 border-success/30 text-success',
  warning: 'bg-warning/10 border-warning/30 text-warning',
  info: 'bg-primary-50 border-primary-200 text-primary-700 dark:bg-primary-900/20 dark:border-primary-700 dark:text-primary-300',
}

export default function Alert({ type, children }: AlertProps) {
  return (
    <div
      className={`rounded-lg border px-4 py-3 text-sm ${styles[type]}`}
      role="alert"
    >
      {children}
    </div>
  )
}
