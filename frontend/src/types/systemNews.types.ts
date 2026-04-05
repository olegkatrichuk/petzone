export interface SystemNewsPost {
  id: string
  title: string
  content: string
  type: string
  publishedAt: string
}

export interface SystemNewsResponse {
  items: SystemNewsPost[]
  totalCount: number
}