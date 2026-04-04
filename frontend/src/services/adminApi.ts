import { createApi } from '@reduxjs/toolkit/query/react'
import { axiosBaseQuery } from './baseQuery'
import type { VolunteerRequestDto, VolunteerRequestStatus } from '../types/volunteerRequest'
import type { PaginatedResponse } from '../types/api'
import type { UserListItemDto } from '../types/account'

export interface VolunteerRequestStats {
  total: number
  submitted: number
  onReview: number
  revisionRequired: number
  approved: number
  rejected: number
}

interface GetUnreviewedParams {
  page: number
  pageSize: number
}

interface GetAdminRequestsParams {
  page: number
  pageSize: number
  status?: VolunteerRequestStatus
}

interface GetUsersParams {
  page: number
  pageSize: number
  search?: string
}

interface PagedResult<T> {
  items: T[]
  totalCount: number
}

export const adminApi = createApi({
  reducerPath: 'adminApi',
  baseQuery: axiosBaseQuery(),
  tagTypes: ['AdminRequest', 'AdminUser'],
  endpoints: (builder) => ({
    getStats: builder.query<VolunteerRequestStats, void>({
      query: () => ({ url: '/volunteerrequests/stats' }),
      providesTags: [{ type: 'AdminRequest', id: 'STATS' }],
    }),

    getUnreviewedRequests: builder.query<PaginatedResponse<VolunteerRequestDto>, GetUnreviewedParams>({
      query: (params) => ({ url: '/volunteerrequests/unreviewed', params }),
      providesTags: [{ type: 'AdminRequest', id: 'UNREVIEWED' }],
    }),

    getAdminRequests: builder.query<PaginatedResponse<VolunteerRequestDto>, GetAdminRequestsParams>({
      query: (params) => ({ url: '/volunteerrequests/admin', params }),
      providesTags: [{ type: 'AdminRequest', id: 'MINE' }],
    }),

    takeOnReview: builder.mutation<string, string>({
      query: (requestId) => ({ url: `/volunteerrequests/${requestId}/review`, method: 'PUT' }),
      invalidatesTags: [{ type: 'AdminRequest', id: 'UNREVIEWED' }, { type: 'AdminRequest', id: 'MINE' }],
    }),

    approveRequest: builder.mutation<string, string>({
      query: (requestId) => ({ url: `/volunteerrequests/${requestId}/approve`, method: 'PUT' }),
      invalidatesTags: [{ type: 'AdminRequest', id: 'MINE' }],
    }),

    sendForRevision: builder.mutation<string, { requestId: string; comment: string }>({
      query: ({ requestId, comment }) => ({
        url: `/volunteerrequests/${requestId}/revision`,
        method: 'PUT',
        data: { comment },
      }),
      invalidatesTags: [{ type: 'AdminRequest', id: 'MINE' }],
    }),

    rejectRequest: builder.mutation<string, { requestId: string; comment: string }>({
      query: ({ requestId, comment }) => ({
        url: `/volunteerrequests/${requestId}/reject`,
        method: 'PUT',
        data: { comment },
      }),
      invalidatesTags: [{ type: 'AdminRequest', id: 'MINE' }],
    }),

    getUsers: builder.query<PagedResult<UserListItemDto>, GetUsersParams>({
      query: (params) => ({ url: '/admin/users', params }),
      providesTags: [{ type: 'AdminUser', id: 'LIST' }],
    }),

    banUser: builder.mutation<void, string>({
      query: (userId) => ({ url: `/admin/users/${userId}/ban`, method: 'POST' }),
      invalidatesTags: [{ type: 'AdminUser', id: 'LIST' }],
    }),

    unbanUser: builder.mutation<void, string>({
      query: (userId) => ({ url: `/admin/users/${userId}/unban`, method: 'POST' }),
      invalidatesTags: [{ type: 'AdminUser', id: 'LIST' }],
    }),
  }),
})

export const {
  useGetStatsQuery,
  useGetUnreviewedRequestsQuery,
  useGetAdminRequestsQuery,
  useTakeOnReviewMutation,
  useApproveRequestMutation,
  useSendForRevisionMutation,
  useRejectRequestMutation,
  useGetUsersQuery,
  useBanUserMutation,
  useUnbanUserMutation,
} = adminApi
