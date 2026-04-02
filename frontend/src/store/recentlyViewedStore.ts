import { create } from 'zustand'
import { persist } from 'zustand/middleware'

const MAX_ITEMS = 6

interface RecentlyViewedState {
  ids: string[]
  add: (id: string) => void
  clear: () => void
}

export const useRecentlyViewedStore = create<RecentlyViewedState>()(
  persist(
    (set) => ({
      ids: [],
      add: (id) =>
        set((state) => {
          const filtered = state.ids.filter((x) => x !== id)
          return { ids: [id, ...filtered].slice(0, MAX_ITEMS) }
        }),
      clear: () => set({ ids: [] }),
    }),
    { name: 'recently-viewed' },
  ),
)
