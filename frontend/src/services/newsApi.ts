import { createApi } from '@reduxjs/toolkit/query/react'
import { axiosBaseQuery } from './baseQuery'

export interface NewsPostDto {
  id: string
  volunteerId: string
  title: string
  content: string
  createdAt: string
  updatedAt: string | null
}

export interface CreateNewsPostRequest {
  title: string
  content: string
}

export interface UpdateNewsPostRequest {
  title: string
  content: string
}

export const newsApi = createApi({
  reducerPath: 'newsApi',
  baseQuery: axiosBaseQuery(),
  tagTypes: ['News'],
  endpoints: (builder) => ({
    getNewsByVolunteer: builder.query<NewsPostDto[], string>({
      query: (volunteerId) => ({ url: `/news/volunteer/${volunteerId}` }),
      providesTags: (_, __, volunteerId) => [{ type: 'News', id: volunteerId }],
    }),

    createNewsPost: builder.mutation<string, CreateNewsPostRequest>({
      query: (body) => ({ url: '/news', method: 'POST', data: body }),
      invalidatesTags: (_result, _error, _arg, meta) => [{ type: 'News', id: meta?.arg as unknown as string }],
    }),

    updateNewsPost: builder.mutation<string, { id: string; body: UpdateNewsPostRequest }>({
      query: ({ id, body }) => ({ url: `/news/${id}`, method: 'PUT', data: body }),
      invalidatesTags: ['News'],
    }),

    deleteNewsPost: builder.mutation<string, string>({
      query: (id) => ({ url: `/news/${id}`, method: 'DELETE' }),
      invalidatesTags: ['News'],
    }),
  }),
})

export const {
  useGetNewsByVolunteerQuery,
  useCreateNewsPostMutation,
  useUpdateNewsPostMutation,
  useDeleteNewsPostMutation,
} = newsApi
