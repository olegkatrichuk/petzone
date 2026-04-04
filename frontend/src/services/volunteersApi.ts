import { createApi } from '@reduxjs/toolkit/query/react'
import { axiosBaseQuery } from './baseQuery'
import type { Volunteer, VolunteerFilters, PaginatedResponse } from '../types/volunteer'

export interface UpdateMainInfoRequest {
  firstName: string
  lastName: string
  patronymic: string
  email: string
  generalDescription: string
  experienceYears: number
  phone: string
}

export interface SocialNetworkDto {
  name: string
  link: string
}

export interface RequisiteDto {
  name: string
  description: string
}

export const volunteersApi = createApi({
  reducerPath: 'volunteersApi',
  baseQuery: axiosBaseQuery(),
  tagTypes: ['Volunteer'],
  endpoints: (builder) => ({
    getVolunteers: builder.query<PaginatedResponse<Volunteer>, VolunteerFilters>({
      query: (filters) => ({ url: '/volunteers', params: filters }),
      providesTags: [{ type: 'Volunteer', id: 'LIST' }],
    }),

    getVolunteerById: builder.query<Volunteer, string>({
      query: (id) => ({ url: `/volunteers/${id}` }),
      providesTags: (_result, _error, id) => [{ type: 'Volunteer', id }],
    }),

    updateMainInfo: builder.mutation<Volunteer, { id: string; data: UpdateMainInfoRequest }>({
      query: ({ id, data }) => ({ url: `/volunteers/${id}/main-info`, method: 'PUT', data }),
      invalidatesTags: (_r, _e, { id }) => [{ type: 'Volunteer', id }],
    }),

    updateSocialNetworks: builder.mutation<Volunteer, { id: string; socialNetworks: SocialNetworkDto[] }>({
      query: ({ id, socialNetworks }) => ({
        url: `/volunteers/${id}/social-networks`,
        method: 'PUT',
        data: { socialNetworks },
      }),
      invalidatesTags: (_r, _e, { id }) => [{ type: 'Volunteer', id }],
    }),

    updateRequisites: builder.mutation<Volunteer, { id: string; requisites: RequisiteDto[] }>({
      query: ({ id, requisites }) => ({
        url: `/volunteers/${id}/requisites`,
        method: 'PUT',
        data: { requisites },
      }),
      invalidatesTags: (_r, _e, { id }) => [{ type: 'Volunteer', id }],
    }),

    uploadPhoto: builder.mutation<string, { id: string; file: File }>({
      query: ({ id, file }) => {
        const formData = new FormData()
        formData.append('file', file)
        return { url: `/volunteers/${id}/photo`, method: 'POST', data: formData }
      },
      invalidatesTags: (_r, _e, { id }) => [{ type: 'Volunteer', id }],
    }),
  }),
})

export const {
  useGetVolunteersQuery,
  useGetVolunteerByIdQuery,
  useUpdateMainInfoMutation,
  useUpdateSocialNetworksMutation,
  useUpdateRequisitesMutation,
  useUploadPhotoMutation,
} = volunteersApi
