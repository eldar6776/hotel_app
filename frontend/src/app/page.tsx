export default function Home() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-surface-secondary">
      <div className="w-full max-w-2xl rounded-xl bg-surface p-12 shadow-lg border border-border">
        <div className="mb-8 text-center">
          <h1 className="text-4xl font-semibold text-primary-700">HotelPRO</h1>
          <p className="mt-3 text-lg text-text-secondary">Hotel Management System</p>
        </div>

        <div className="grid grid-cols-2 gap-4 mb-8">
          <div className="rounded-lg bg-primary-50 p-4 border border-primary-200">
            <p className="text-sm text-primary-700 font-medium">Primary</p>
            <p className="text-xs text-primary-600 mt-1">hsl(214 90% 42%)</p>
          </div>
          <div className="rounded-lg bg-accent-50 p-4 border border-accent-200">
            <p className="text-sm text-accent-700 font-medium">Accent</p>
            <p className="text-xs text-accent-600 mt-1">hsl(42 90% 38%)</p>
          </div>
          <div className="rounded-lg bg-success/10 p-4 border border-success/30">
            <p className="text-sm text-success font-medium">Success</p>
            <p className="text-xs text-success-dark mt-1">hsl(142 71% 45%)</p>
          </div>
          <div className="rounded-lg bg-error/10 p-4 border border-error/30">
            <p className="text-sm text-error font-medium">Error</p>
            <p className="text-xs text-error-dark mt-1">hsl(0 84% 60%)</p>
          </div>
        </div>

        <div className="text-center">
          <p className="text-sm text-text-secondary">
            Font: Inter | Next.js 15 | Tailwind CSS v4
          </p>
          <p className="text-xs text-text-secondary mt-2">
            Status: Buildable ✓ | Dark Mode: class strategy ✓
          </p>
        </div>
      </div>
    </div>
  );
}
