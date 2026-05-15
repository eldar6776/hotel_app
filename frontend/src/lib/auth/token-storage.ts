const ACCESS_TOKEN_KEY = 'hotelpro_access_token'
const REFRESH_TOKEN_KEY = 'hotelpro_refresh_token'

function isBrowser(): boolean {
  return typeof window !== 'undefined'
}

function safeStorageGet(storage: Storage | null, key: string): string | null {
  try {
    return storage?.getItem(key) ?? null
  } catch {
    return null
  }
}

export const tokenStorage = {
  set(accessToken: string, refreshToken: string, persist = false) {
    if (!isBrowser()) return
    const storage = persist ? localStorage : sessionStorage
    try {
      storage.setItem(ACCESS_TOKEN_KEY, accessToken)
      storage.setItem(REFRESH_TOKEN_KEY, refreshToken)
    } catch {
      // Storage unavailable
    }
  },
  getAccessToken(): string | null {
    if (!isBrowser()) return null
    return (
      safeStorageGet(localStorage, ACCESS_TOKEN_KEY) ??
      safeStorageGet(sessionStorage, ACCESS_TOKEN_KEY)
    )
  },
  getRefreshToken(): string | null {
    if (!isBrowser()) return null
    return (
      safeStorageGet(localStorage, REFRESH_TOKEN_KEY) ??
      safeStorageGet(sessionStorage, REFRESH_TOKEN_KEY)
    )
  },
  clear() {
    if (!isBrowser()) return
    for (const storage of [localStorage, sessionStorage]) {
      try {
        storage.removeItem(ACCESS_TOKEN_KEY)
        storage.removeItem(REFRESH_TOKEN_KEY)
      } catch {
        // Storage unavailable
      }
    }
  },
}
