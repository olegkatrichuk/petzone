import { create } from 'zustand'
import type { Pet } from '../types/pet'

const MAX = 3

interface ComparisonState {
  pets: Pet[]
  add: (pet: Pet) => void
  remove: (id: string) => void
  toggle: (pet: Pet) => void
  has: (id: string) => boolean
  clear: () => void
}

export const useComparisonStore = create<ComparisonState>((set, get) => ({
  pets: [],
  add: (pet) => {
    if (get().pets.length < MAX && !get().has(pet.id)) {
      set((s) => ({ pets: [...s.pets, pet] }))
    }
  },
  remove: (id) => set((s) => ({ pets: s.pets.filter((p) => p.id !== id) })),
  toggle: (pet) => {
    if (get().has(pet.id)) get().remove(pet.id)
    else get().add(pet)
  },
  has: (id) => get().pets.some((p) => p.id === id),
  clear: () => set({ pets: [] }),
}))
