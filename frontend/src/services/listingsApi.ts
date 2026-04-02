import { createApi } from '@reduxjs/toolkit/query/react'
import { axiosBaseQuery } from './baseQuery'
import type { AdoptionListing, CreateListingPayload, UpdateListingPayload } from '../types/listing'

export const listingsApi = createApi({
  reducerPath: 'listingsApi',
  baseQuery: axiosBaseQuery(),
  tagTypes: ['Listing'],
  endpoints: (builder) => ({
    getListings: builder.query<AdoptionListing[], { speciesId?: string; city?: string; page?: number; pageSize?: number }>({
      query: (params) => ({ url: '/listings', params }),
      providesTags: [{ type: 'Listing', id: 'LIST' }],
    }),

    getListingById: builder.query<AdoptionListing, string>({
      query: (id) => ({ url: `/listings/${id}` }),
      providesTags: (_r, _e, id) => [{ type: 'Listing', id }],
    }),

    getMyListings: builder.query<AdoptionListing[], { page?: number; pageSize?: number }>({
      query: (params) => ({ url: '/listings/my', params }),
      providesTags: [{ type: 'Listing', id: 'MY' }],
    }),

    createListing: builder.mutation<{ id: string }, CreateListingPayload>({
      query: (data) => ({ url: '/listings', method: 'POST', data }),
      invalidatesTags: [{ type: 'Listing', id: 'LIST' }, { type: 'Listing', id: 'MY' }],
    }),

    updateListing: builder.mutation<void, { id: string; data: UpdateListingPayload }>({
      query: ({ id, data }) => ({ url: `/listings/${id}`, method: 'PUT', data }),
      invalidatesTags: (_r, _e, { id }) => [{ type: 'Listing', id }, { type: 'Listing', id: 'LIST' }],
    }),

    deleteListing: builder.mutation<void, string>({
      query: (id) => ({ url: `/listings/${id}`, method: 'DELETE' }),
      invalidatesTags: [{ type: 'Listing', id: 'LIST' }, { type: 'Listing', id: 'MY' }],
    }),

    markAdopted: builder.mutation<void, string>({
      query: (id) => ({ url: `/listings/${id}/adopted`, method: 'PATCH' }),
      invalidatesTags: (_r, _e, id) => [{ type: 'Listing', id }, { type: 'Listing', id: 'MY' }],
    }),

    addListingPhoto: builder.mutation<void, { id: string; fileName: string }>({
      query: ({ id, fileName }) => ({ url: `/listings/${id}/photos`, method: 'POST', data: { fileName } }),
      invalidatesTags: (_r, _e, { id }) => [{ type: 'Listing', id }, { type: 'Listing', id: 'LIST' }, { type: 'Listing', id: 'MY' }],
    }),

    removeListingPhoto: builder.mutation<void, { id: string; fileName: string }>({
      query: ({ id, fileName }) => ({ url: `/listings/${id}/photos/${encodeURIComponent(fileName)}`, method: 'DELETE' }),
      invalidatesTags: (_r, _e, { id }) => [{ type: 'Listing', id }, { type: 'Listing', id: 'LIST' }, { type: 'Listing', id: 'MY' }],
    }),
  }),
})

export const {
  useGetListingsQuery,
  useGetListingByIdQuery,
  useGetMyListingsQuery,
  useCreateListingMutation,
  useUpdateListingMutation,
  useDeleteListingMutation,
  useMarkAdoptedMutation,
  useAddListingPhotoMutation,
  useRemoveListingPhotoMutation,
} = listingsApi