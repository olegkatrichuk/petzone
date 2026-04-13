import { createApi } from '@reduxjs/toolkit/query/react'
import { axiosBaseQuery } from './baseQuery'

export interface AdoptionApplicationDto {
  id: string
  petId: string
  petNickname: string
  petMainPhoto?: string
  volunteerId: string
  applicantUserId: string
  applicantName: string
  applicantPhone: string
  message?: string
  status: 'Pending' | 'Approved' | 'Rejected'
  createdAt: string
}

export interface CreateAdoptionApplicationRequest {
  applicantName: string
  applicantPhone: string
  message?: string
}

export const adoptionApi = createApi({
  reducerPath: 'adoptionApi',
  baseQuery: axiosBaseQuery(),
  tagTypes: ['Adoption'],
  endpoints: (builder) => ({
    submitApplication: builder.mutation<
      { id: string },
      { petId: string; volunteerId: string; data: CreateAdoptionApplicationRequest }
    >({
      query: ({ petId, volunteerId, data }) => ({
        url: `/adoption-applications/pets/${petId}/volunteers/${volunteerId}`,
        method: 'POST',
        data,
      }),
      invalidatesTags: [{ type: 'Adoption', id: 'LIST' }],
    }),

    getMyApplications: builder.query<AdoptionApplicationDto[], void>({
      query: () => ({ url: '/adoption-applications/my' }),
      providesTags: [{ type: 'Adoption', id: 'LIST' }],
    }),

    getVolunteerApplications: builder.query<AdoptionApplicationDto[], string>({
      query: (volunteerId) => ({ url: `/adoption-applications/volunteer/${volunteerId}` }),
      providesTags: [{ type: 'Adoption', id: 'LIST' }],
    }),

    updateApplicationStatus: builder.mutation<void, { applicationId: string; action: 'approve' | 'reject' }>({
      query: ({ applicationId, action }) => ({
        url: `/adoption-applications/${applicationId}/status`,
        method: 'PATCH',
        data: { action },
      }),
      invalidatesTags: [{ type: 'Adoption', id: 'LIST' }],
    }),
  }),
})

export const {
  useSubmitApplicationMutation,
  useGetMyApplicationsQuery,
  useGetVolunteerApplicationsQuery,
  useUpdateApplicationStatusMutation,
} = adoptionApi