import { createApi } from '@reduxjs/toolkit/query/react'
import { axiosBaseQuery } from './baseQuery'
import type {
  BlogPost,
  PagedBlogPosts,
  CreateBlogPostPayload,
  UpdateBlogPostPayload,
} from '../types/blog'

export const blogApi = createApi({
  reducerPath: 'blogApi',
  baseQuery: axiosBaseQuery(),
  tagTypes: ['BlogPost'],
  endpoints: (builder) => ({
    getBlogPosts: builder.query<PagedBlogPosts, { lang?: string; page?: number; pageSize?: number }>({
      query: (params) => ({ url: '/blog', params }),
      providesTags: [{ type: 'BlogPost', id: 'LIST' }],
    }),

    getBlogPostBySlug: builder.query<BlogPost, string>({
      query: (slug) => ({ url: `/blog/${encodeURIComponent(slug)}` }),
      providesTags: (_r, _e, slug) => [{ type: 'BlogPost', id: slug }],
    }),

    createBlogPost: builder.mutation<{ id: string }, CreateBlogPostPayload>({
      query: (data) => ({ url: '/blog', method: 'POST', data }),
      invalidatesTags: [{ type: 'BlogPost', id: 'LIST' }],
    }),

    updateBlogPost: builder.mutation<void, { id: string; data: UpdateBlogPostPayload; slug: string }>({
      query: ({ id, data }) => ({ url: `/blog/${id}`, method: 'PUT', data }),
      invalidatesTags: (_r, _e, { slug }) => [{ type: 'BlogPost', id: 'LIST' }, { type: 'BlogPost', id: slug }],
    }),

    deleteBlogPost: builder.mutation<void, { id: string; slug?: string }>({
      query: ({ id }) => ({ url: `/blog/${id}`, method: 'DELETE' }),
      invalidatesTags: (_r, _e, { slug }) => [
        { type: 'BlogPost', id: 'LIST' },
        ...(slug ? [{ type: 'BlogPost' as const, id: slug }] : []),
      ],
    }),
  }),
})

export const {
  useGetBlogPostsQuery,
  useGetBlogPostBySlugQuery,
  useCreateBlogPostMutation,
  useUpdateBlogPostMutation,
  useDeleteBlogPostMutation,
} = blogApi
