import { createApi } from '@reduxjs/toolkit/query/react'
import { axiosBaseQuery } from './baseQuery'
import type { UserDto } from '../types/account'

export const accountsApi = createApi({
  reducerPath: 'accountsApi',
  baseQuery: axiosBaseQuery(),
  tagTypes: ['User'],
  endpoints: (builder) => ({
    getUserById: builder.query<UserDto, string>({
      query: (userId) => ({ url: `/accounts/${userId}` }),
      providesTags: (_result, _error, userId) => [{ type: 'User', id: userId }],
    }),
  }),
})

export const { useGetUserByIdQuery } = accountsApi
