'use client'

import { Line } from 'react-chartjs-2'
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
  Filler,
} from 'chart.js'
import type { OccupancyTrendPoint } from '@/lib/dashboard/dashboard-service'

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
  Filler
)

export function OccupancyChart({
  data,
}: {
  data: OccupancyTrendPoint[]
}) {
  return (
    <Line
      data={{
        labels: data.map((d) => d.date),
        datasets: [
          {
            label: 'Popunjenost (%)',
            data: data.map((d) => d.occupancy),
            borderColor: 'hsl(214 90% 42%)',
            backgroundColor: 'hsla(214 90% 42% / 0.1)',
            fill: true,
            tension: 0.3,
          },
          {
            label: 'Prihod (EUR)',
            data: data.map((d) => d.revenue),
            borderColor: 'hsl(142 71% 45%)',
            backgroundColor: 'hsla(142 71% 45% / 0.1)',
            fill: true,
            tension: 0.3,
            yAxisID: 'y1',
          },
        ],
      }}
      options={{
        responsive: true,
        interaction: { intersect: false, mode: 'index' },
        plugins: {
          legend: {
            position: 'bottom',
          },
        },
        scales: {
          y: {
            type: 'linear',
            display: true,
            position: 'left',
            title: { display: true, text: 'Popunjenost (%)' },
          },
          y1: {
            type: 'linear',
            display: true,
            position: 'right',
            title: { display: true, text: 'Prihod (EUR)' },
            grid: { drawOnChartArea: false },
          },
        },
      }}
    />
  )
}
