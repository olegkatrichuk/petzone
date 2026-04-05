export interface SystemNewsPost {
  id: string
  type: string
  publishedAt: string
  lookingForHome: number
  needsHelp: number
  foundHomeThisWeek: number
  totalVolunteers: number
  factEn: string
}

export interface SystemNewsResponse {
  items: SystemNewsPost[]
  totalCount: number
}