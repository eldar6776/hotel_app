import { HelpProvider } from '@/components/help/HelpProvider'

export default function AuthLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <HelpProvider>
      <div className="flex min-h-screen items-center justify-center bg-surface-secondary">
        {children}
      </div>
    </HelpProvider>
  )
}
