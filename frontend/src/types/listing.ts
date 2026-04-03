export type ListingStatus = 'Active' | 'Adopted' | 'Removed'

export interface AdoptionListing {
  id: string
  userId: string
  userName: string
  userEmail: string
  userPhone?: string
  contactEmail?: string
  title: string
  description: string
  speciesId: string
  breedId?: string
  ageMonths: number
  color: string
  city: string
  vaccinated: boolean
  castrated: boolean
  photos: string[]
  status: ListingStatus
  createdAt: string
}

export interface CreateListingPayload {
  title: string
  description: string
  speciesId: string
  breedId?: string
  ageMonths: number
  color: string
  city: string
  vaccinated: boolean
  castrated: boolean
  phone?: string
  contactEmail?: string
}

export interface UpdateListingPayload extends CreateListingPayload {}