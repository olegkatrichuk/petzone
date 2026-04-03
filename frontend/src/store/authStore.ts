import { create } from 'zustand'
import { persist } from 'zustand/middleware'

interface UserInfo {
  id: string
  email: string
  firstName: string
  lastName: string
  role: string
}

interface AuthState {
  accessToken: string | null
  user: UserInfo | null
  setAccessToken: (token: string) => void
  logout: () => void
}

const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'

function decodeJwt(token: string): UserInfo | null {
  try {
    // JWT uses base64url encoding (no padding, - and _ instead of + and /)
    // atob() returns raw bytes as Latin-1; need TextDecoder for proper UTF-8 (Cyrillic etc.)
    const base64url = token.split('.')[1]
    const base64 = base64url.replace(/-/g, '+').replace(/_/g, '/')
    const padded = base64 + '=='.slice(0, (4 - base64.length % 4) % 4)
    const bytes = Uint8Array.from(atob(padded), (c) => c.charCodeAt(0))
    const payload = JSON.parse(new TextDecoder().decode(bytes))
    // .NET ClaimTypes.Role serializes as full URI; fall back to short "role"
    const rawRole = payload[ROLE_CLAIM] ?? payload.role ?? ''
    const role = Array.isArray(rawRole) ? rawRole[0] : rawRole
    return {
      id: payload.sub ?? '',
      email: payload.email ?? '',
      firstName: payload.given_name ?? '',
      lastName: payload.family_name ?? '',
      role,
    }
  } catch {
    return null
  }
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      accessToken: null,
      user: null,
      setAccessToken: (token) =>
        set({ accessToken: token, user: decodeJwt(token) }),
      logout: () => set({ accessToken: null, user: null }),
    }),
    {
      name: 'auth',
      partialize: (state) => ({ accessToken: state.accessToken, user: state.user }),
    }
  )
)
