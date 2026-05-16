import Link from 'next/link'

export default function Home() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-surface-secondary">
      <div className="w-full max-w-2xl rounded-xl bg-surface p-12 shadow-lg border border-border text-center">
        <h1 className="text-4xl font-semibold text-primary-700">HotelPRO</h1>
        <p className="mt-3 text-lg text-text-secondary">Hotel Management System</p>
        <Link
          href="/login"
          className="mt-8 inline-block rounded-lg bg-primary-500 px-6 py-3 text-sm font-medium text-white hover:bg-primary-600"
        >
          Prijava u sistem
        </Link>
      </div>
    </div>
  )
}
