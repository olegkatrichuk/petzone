import { createApi } from '@reduxjs/toolkit/query/react'
import { axiosBaseQuery } from './baseQuery'
import type { VolunteerRequestDto, VolunteerRequestStatus } from '../types/volunteerRequest'
import type { PaginatedResponse } from '../types/api'

interface GetMyRequestsParams {
  status?: VolunteerRequestStatus
  page: number
  pageSize: number
}

interface UpdateRequestDto {
  experience: number
  certificates: string[]
  requisites: string[]
}

export const volunteerRequestsApi = createApi({
  reducerPath: 'volunteerRequestsApi',
  baseQuery: axiosBaseQuery(),
  tagTypes: ['VolunteerRequest'],
  endpoints: (builder) => ({
    getMyRequests: builder.query<PaginatedResponse<VolunteerRequestDto>, GetMyRequestsParams>({
      query: (params) => ({ url: '/volunteerrequests/my', params }),
      providesTags: [{ type: 'VolunteerRequest', id: 'LIST' }],
    }),

    updateRequest: builder.mutation<VolunteerRequestDto, { requestId: string; data: UpdateRequestDto }>({
      query: ({ requestId, data }) => ({
        url: `/volunteerrequests/${requestId}`,
        method: 'PUT',
        data,
      }),
      invalidatesTags: [{ type: 'VolunteerRequest', id: 'LIST' }],
    }),
  }),
})

export const { useGetMyRequestsQuery, useUpdateRequestMutation } = volunteerRequestsApi
