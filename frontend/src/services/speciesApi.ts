import { createApi } from '@reduxjs/toolkit/query/react'
import { skipToken } from '@reduxjs/toolkit/query'
import { axiosBaseQuery } from './baseQuery'
import type { Species, Breed } from '../types/species'

export const speciesApi = createApi({
  reducerPath: 'speciesApi',
  baseQuery: axiosBaseQuery(),
  endpoints: (builder) => ({
    /**
     * Fetch all available species (used to populate the species filter dropdown).
     * Cached globally — no arguments, no invalidation needed.
     */
    getSpecies: builder.query<Species[], void>({
      query: () => ({ url: '/species' }),
    }),

    /**
     * Fetch breeds for a given species id.
     * Pass skipToken when no species is selected to avoid making the request.
     */
    getBreeds: builder.query<Breed[], string>({
      query: (speciesId) => ({ url: `/species/${speciesId}/breeds` }),
    }),
  }),
})

// Re-export skipToken so consumers can import it from one place
export { skipToken }

export const { useGetSpeciesQuery, useGetBreedsQuery } = speciesApi
