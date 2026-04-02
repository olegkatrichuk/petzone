import type { BaseQueryFn } from '@reduxjs/toolkit/query'
import type { AxiosRequestConfig, AxiosError } from 'axios'
import { api } from '../lib/axios'

export type AxiosBaseQueryArgs = {
  url: string
  method?: AxiosRequestConfig['method']
  data?: unknown
  params?: unknown
}

export type AxiosBaseQueryError = {
  status: number | undefined
  data: unknown
}

/**
 * Custom RTK Query baseQuery that delegates all requests to the shared
 * axios instance. This means every request automatically gets:
 *  - Bearer token injection (request interceptor)
 *  - Silent access-token refresh on 401 (response interceptor)
 */
export const axiosBaseQuery =
  (): BaseQueryFn<AxiosBaseQueryArgs, unknown, AxiosBaseQueryError> =>
  async ({ url, method = 'GET', data, params }) => {
    try {
      const result = await api({ url, method, data, params })
      // Backend wraps all responses in { result, errorInfo, timeGenerated }
      const payload = result.data
      return { data: payload?.result !== undefined ? payload.result : payload }
    } catch (axiosError) {
      const err = axiosError as AxiosError
      return {
        error: {
          status: err.response?.status,
          data: err.response?.data ?? err.message,
        },
      }
    }
  }
