import Badge from '@/components/ui/Badge'
import type { RecentBooking } from '@/lib/dashboard/dashboard-service'

const statusVariant = (status: string) => {
  switch (status?.toLowerCase()) {
    case 'confirmed':
      return 'confirmed' as const
    case 'checkedin':
    case 'checked-in':
      return 'checkedin' as const
    case 'checkedout':
    case 'checked-out':
      return 'checkedout' as const
    case 'cancelled':
      return 'cancelled' as const
    default:
      return 'confirmed' as const
  }
}

const statusLabel = (status: string) => {
  switch (status?.toLowerCase()) {
    case 'confirmed':
      return 'Potvrdeno'
    case 'checkedin':
    case 'checked-in':
      return 'Prijavljen'
    case 'checkedout':
    case 'checked-out':
      return 'Odjavljen'
    case 'cancelled':
      return 'Otkazano'
    default:
      return status
  }
}

export default function RecentBookings({
  bookings,
}: {
  bookings: RecentBooking[]
}) {
  return (
    <div className="rounded-xl bg-surface shadow-sm border border-border overflow-hidden">
      <div className="border-b border-border px-4 py-3">
        <h3 className="text-sm font-semibold text-text">Nedavne rezervacije</h3>
      </div>
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-border bg-surface-secondary dark:bg-surface-tertiary">
              <th className="px-4 py-2 text-left text-xs font-medium text-text-secondary">
                Broj
              </th>
              <th className="px-4 py-2 text-left text-xs font-medium text-text-secondary">
                Gost
              </th>
              <th className="px-4 py-2 text-left text-xs font-medium text-text-secondary">
                Soba
              </th>
              <th className="px-4 py-2 text-left text-xs font-medium text-text-secondary">
                Dolazak
              </th>
              <th className="px-4 py-2 text-left text-xs font-medium text-text-secondary">
                Odlazak
              </th>
              <th className="px-4 py-2 text-left text-xs font-medium text-text-secondary">
                Status
              </th>
              <th className="px-4 py-2 text-right text-xs font-medium text-text-secondary">
                Iznos
              </th>
            </tr>
          </thead>
          <tbody>
            {bookings.length === 0 ? (
              <tr>
                <td colSpan={7} className="px-4 py-8 text-center text-text-secondary">
                  Nema nedavnih rezervacija
                </td>
              </tr>
            ) : (
              bookings.map((b) => (
                <tr
                  key={b.id}
                  className="border-b border-border last:border-0 hover:bg-surface-secondary dark:hover:bg-surface-tertiary"
                >
                  <td className="px-4 py-2 font-medium text-text">
                    {b.bookingNumber}
                  </td>
                  <td className="px-4 py-2 text-text">{b.guestName}</td>
                  <td className="px-4 py-2 text-text-secondary">
                    {b.roomNumber}
                  </td>
                  <td className="px-4 py-2 text-text-secondary">
                    {b.arrivalDate}
                  </td>
                  <td className="px-4 py-2 text-text-secondary">
                    {b.departureDate}
                  </td>
                  <td className="px-4 py-2">
                    <Badge variant={statusVariant(b.status)}>
                      {statusLabel(b.status)}
                    </Badge>
                  </td>
                  <td className="px-4 py-2 text-right font-medium text-text">
                    {'EUR '}
                    {b.amount.toFixed(2)}
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  )
}
