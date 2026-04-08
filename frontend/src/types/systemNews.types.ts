export interface TopBreed {
  name: string
  count: number
}

export interface SystemNewsPost {
  id: string
  type: string
  publishedAt: string
  lookingForHome: number
  needsHelp: number
  foundHomeThisWeek: number
  totalVolunteers: number
  factEn: string
  topBreedsJson: string
  topCity: string | null
  featuredPetNickname: string | null
  featuredPetPhotoUrl: string | null
  featuredPetDescription: string | null
  featuredPetBreed: string | null
  featuredPetCity: string | null
}

export interface SystemNewsResponse {
  items: SystemNewsPost[]
  totalCount: number
}
