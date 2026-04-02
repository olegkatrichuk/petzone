import { createApi } from '@reduxjs/toolkit/query/react'
import { axiosBaseQuery } from './baseQuery'
import type { Pet, PetFilters, PaginatedResponse } from '../types/pet'

export const petsApi = createApi({
  reducerPath: 'petsApi',
  baseQuery: axiosBaseQuery(),
  tagTypes: ['Pet'],
  endpoints: (builder) => ({
    /**
     * Fetch a paginated + filtered + sorted list of pets.
     * The full PetFilters object is forwarded as query params.
     */
    getPets: builder.query<PaginatedResponse<Pet>, PetFilters>({
      query: (filters) => ({ url: '/pets', params: filters }),
      providesTags: (result) =>
        result
          ? [
              ...result.items.map(({ id }) => ({ type: 'Pet' as const, id })),
              { type: 'Pet', id: 'LIST' },
            ]
          : [{ type: 'Pet', id: 'LIST' }],
    }),

    /**
     * Fetch a single pet by its id.
     */
    getPetById: builder.query<Pet, string>({
      query: (id) => ({ url: `/pets/${id}` }),
      providesTags: (_result, _error, id) => [{ type: 'Pet', id }],
    }),
  }),
})

export const { useGetPetsQuery, useGetPetByIdQuery } = petsApi
