export interface PaginatedResponse<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
}

export interface ErrorInfo {
  errorCode: string | null
  errorMessage: string | null
  invalidField: string | null
}

export interface Envelope<T> {
  result: T
  errorInfo: ErrorInfo[]
  timeGenerated: string
}
