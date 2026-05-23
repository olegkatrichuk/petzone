export interface BlogPost {
  id: string
  slug: string
  language: string
  title: string
  summary: string
  contentMarkdown: string
  coverImageUrl?: string | null
  authorUserId: string
  createdAt: string
  updatedAt?: string | null
}

export interface BlogPostListItem {
  id: string
  slug: string
  language: string
  title: string
  summary: string
  coverImageUrl?: string | null
  createdAt: string
}

export interface PagedBlogPosts {
  items: BlogPostListItem[]
  total: number
  page: number
  pageSize: number
}

export interface CreateBlogPostPayload {
  slug: string
  language: string
  title: string
  summary: string
  contentMarkdown: string
  coverImageUrl?: string
}

export interface UpdateBlogPostPayload {
  title: string
  summary: string
  contentMarkdown: string
  coverImageUrl?: string
}
