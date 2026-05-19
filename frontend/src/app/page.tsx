import Link from 'next/link'

export default function Home() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-surface-secondary">
      <div className="w-full max-w-2xl rounded-xl bg-surface p-12 shadow-lg border border-border text-center text-text">
        <h1 className="text-4xl font-semibold">HotelPRO</h1>
        <p className="mt-3 text-lg">Hotel Management System</p>
        <Link
          href="/login"
          className="mt-8 inline-block text-sm font-medium text-inherit hover:underline focus-visible:outline-none"
        >
          Prijava u sistem
        </Link>
      </div>
    </div>
  )
}
