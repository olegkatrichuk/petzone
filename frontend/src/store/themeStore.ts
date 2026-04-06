import { create } from 'zustand'
import { persist } from 'zustand/middleware'

interface ThemeState {
  mode: 'light' | 'dark'
  toggle: () => void
}

export const useThemeStore = create<ThemeState>()(
  persist(
    (set, get) => ({
      mode: 'light',
      toggle: () => set({ mode: get().mode === 'light' ? 'dark' : 'light' }),
    }),
    { name: 'petzone-theme' }
  )
)