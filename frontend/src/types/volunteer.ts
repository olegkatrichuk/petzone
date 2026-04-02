export interface SocialNetwork {
  name: string
  link: string
}

export interface Volunteer {
  id: string
  firstName: string
  lastName: string
  patronymic: string
  email: string
  phone: string
  experienceYears: number
  generalDescription: string
  petsCount: number
  isDeleted: boolean
  /** Optional — will be returned once the backend exposes it */
  socialNetworks?: SocialNetwork[]
  /** Optional profile photo path */
  photoPath?: string
}

export interface VolunteerFilters {
  page: number
  pageSize: number
}

export type { PaginatedResponse } from './api'
