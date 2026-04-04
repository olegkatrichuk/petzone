import { createApi } from '@reduxjs/toolkit/query/react'
import { skipToken } from '@reduxjs/toolkit/query'
import { axiosBaseQuery } from './baseQuery'
import type { Species, Breed } from '../types/species'

export const speciesApi = createApi({
  reducerPath: 'speciesApi',
  baseQuery: axiosBaseQuery(),
  tagTypes: ['Species', 'Breed'],
  endpoints: (builder) => ({
    getSpecies: builder.query<Species[], string>({
      query: (locale) => ({ url: '/species', params: { locale } }),
      providesTags: [{ type: 'Species', id: 'LIST' }],
    }),

    getBreeds: builder.query<Breed[], { speciesId: string; locale: string }>({
      query: ({ speciesId, locale }) => ({ url: `/species/${speciesId}/breeds`, params: { locale } }),
      providesTags: (_r, _e, { speciesId }) => [{ type: 'Breed', id: speciesId }],
    }),

    createSpecies: builder.mutation<Species, { translations: Record<string, string> }>({
      query: (body) => ({ url: '/species', method: 'POST', data: body }),
      invalidatesTags: [{ type: 'Species', id: 'LIST' }],
    }),

    deleteSpecies: builder.mutation<void, string>({
      query: (speciesId) => ({ url: `/species/${speciesId}`, method: 'DELETE' }),
      invalidatesTags: [{ type: 'Species', id: 'LIST' }],
    }),

    createBreed: builder.mutation<Breed, { speciesId: string; translations: Record<string, string> }>({
      query: ({ speciesId, translations }) => ({
        url: `/species/${speciesId}/breeds`,
        method: 'POST',
        data: { translations },
      }),
      invalidatesTags: (_r, _e, { speciesId }) => [
        { type: 'Breed', id: speciesId },
        { type: 'Species', id: 'LIST' },
      ],
    }),

    deleteBreed: builder.mutation<void, { speciesId: string; breedId: string }>({
      query: ({ speciesId, breedId }) => ({
        url: `/species/${speciesId}/breeds/${breedId}`,
        method: 'DELETE',
      }),
      invalidatesTags: (_r, _e, { speciesId }) => [
        { type: 'Breed', id: speciesId },
        { type: 'Species', id: 'LIST' },
      ],
    }),
  }),
})

// Re-export skipToken so consumers can import it from one place
export { skipToken }

export const {
  useGetSpeciesQuery,
  useGetBreedsQuery,
  useCreateSpeciesMutation,
  useDeleteSpeciesMutation,
  useCreateBreedMutation,
  useDeleteBreedMutation,
} = speciesApi
