export interface PetPhoto {
  filePath: string
  isMain: boolean
}

export const PetStatus = {
  NeedsHelp: 0,
  LookingForHome: 1,
  FoundHome: 2,
} as const
export type PetStatus = typeof PetStatus[keyof typeof PetStatus]

export interface Pet {
  id: string
  volunteerId: string
  nickname: string
  color: string
  generalDescription: string
  city: string
  street: string
  weight: number
  height: number
  isCastrated: boolean
  isVaccinated: boolean
  dateOfBirth: string
  status: PetStatus
  microchipNumber?: string
  adoptionConditions?: string
  speciesId: string
  breedId: string
  position: number
  isDeleted: boolean
  photos: PetPhoto[]
}

export interface PetFilters {
  page: number
  pageSize: number
  volunteerId?: string
  nickname?: string
  color?: string
  city?: string
  speciesId?: string
  breedId?: string
  minAge?: number
  maxAge?: number
  minWeight?: number
  maxWeight?: number
  isCastrated?: boolean
  isVaccinated?: boolean
  status?: number
  sortBy?: string
  sortDescending?: boolean
}

export type { PaginatedResponse } from './api'
