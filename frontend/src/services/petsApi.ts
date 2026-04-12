import { createApi } from '@reduxjs/toolkit/query/react'
import { axiosBaseQuery } from './baseQuery'
import type { Pet, PetFilters, PaginatedResponse } from '../types/pet'

export interface CreatePetPayload {
  nickname: string
  generalDescription: string
  color: string
  healthDescription: string
  dietOrAllergies?: string
  city: string
  street: string
  weight: number
  height: number
  ownerPhone: string
  isCastrated: boolean
  dateOfBirth: string
  isVaccinated: boolean
  status: number
  microchipNumber?: string
  adoptionConditions?: string
  speciesId: string
  breedId: string
}

export const petsApi = createApi({
  reducerPath: 'petsApi',
  baseQuery: axiosBaseQuery(),
  tagTypes: ['Pet'],
  endpoints: (builder) => ({
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

    getPetById: builder.query<Pet, string>({
      query: (id) => ({ url: `/pets/${id}` }),
      providesTags: (_result, _error, id) => [{ type: 'Pet', id }],
    }),

    createPet: builder.mutation<string, { volunteerId: string; data: CreatePetPayload }>({
      query: ({ volunteerId, data }) => ({
        url: `/volunteers/${volunteerId}/pets`,
        method: 'POST',
        data,
      }),
      invalidatesTags: [{ type: 'Pet', id: 'LIST' }],
    }),
  }),
})

export const { useGetPetsQuery, useGetPetByIdQuery, useCreatePetMutation } = petsApi
