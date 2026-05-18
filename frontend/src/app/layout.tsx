import type { Metadata } from 'next'
import Script from 'next/script'
import { Inter } from 'next/font/google'
import './globals.css'
import { ThemeProvider } from '@/components/providers/ThemeProvider'

const inter = Inter({
  variable: '--font-inter',
  subsets: ['latin'],
  weight: ['300', '400', '500', '600', '700'],
})

export const metadata: Metadata = {
  title: 'HotelPRO',
  description: 'Hotel Management System',
}

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode
}>) {
  return (
    <html
      lang="hr"
      className={`${inter.variable} h-full antialiased`}
      suppressHydrationWarning
    >
      <head>
        <Script id="theme-init" strategy="beforeInteractive">
          {`try{const t=localStorage.getItem('theme');document.documentElement.classList.toggle('dark',t==='dark')}catch(e){}`}
        </Script>
      </head>
      <body className="min-h-full flex flex-col font-sans">
        <ThemeProvider>{children}</ThemeProvider>
      </body>
    </html>
  )
}
