import { createApi } from '@reduxjs/toolkit/query/react'
import { axiosBaseQuery } from './baseQuery'
import type { SystemNewsPost, SystemNewsResponse } from '../types/systemNews.types'

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
    getNewsPostById: builder.query<NewsPostDto, string>({
      query: (id) => ({ url: `/news/${id}` }),
      providesTags: (_, __, id) => [{ type: 'News', id }],
    }),

    getNewsByVolunteer: builder.query<NewsPostDto[], string>({
      query: (volunteerId) => ({ url: `/news/volunteer/${volunteerId}` }),
      providesTags: (_, __, volunteerId) => [{ type: 'News', id: volunteerId }],
    }),

    createNewsPost: builder.mutation<string, CreateNewsPostRequest>({
      query: (body) => ({ url: '/news', method: 'POST', data: body }),
      invalidatesTags: ['News'],
    }),

    updateNewsPost: builder.mutation<string, { id: string; body: UpdateNewsPostRequest }>({
      query: ({ id, body }) => ({ url: `/news/${id}`, method: 'PUT', data: body }),
      invalidatesTags: ['News'],
    }),

    deleteNewsPost: builder.mutation<string, string>({
      query: (id) => ({ url: `/news/${id}`, method: 'DELETE' }),
      invalidatesTags: ['News'],
    }),

    getSystemNews: builder.query<SystemNewsResponse, { page?: number; pageSize?: number }>({
      query: ({ page = 1, pageSize = 10 } = {}) => ({
        url: `/news/system?page=${page}&pageSize=${pageSize}`,
      }),
      providesTags: ['News'],
    }),
    getTodayDigest: builder.query<SystemNewsPost, void>({
      query: () => ({ url: '/news/system/today' }),
      providesTags: ['News'],
    }),
  }),
})

export const {
  useGetNewsPostByIdQuery,
  useGetNewsByVolunteerQuery,
  useCreateNewsPostMutation,
  useUpdateNewsPostMutation,
  useDeleteNewsPostMutation,
  useGetSystemNewsQuery,
  useGetTodayDigestQuery,
} = newsApi
