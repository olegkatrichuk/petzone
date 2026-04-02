import { createApi } from '@reduxjs/toolkit/query/react'
import { axiosBaseQuery } from './baseQuery'

export interface MessageDto {
  id: string
  userId: string
  text: string
  isEdited: boolean
  createdAt: string
}

export interface DiscussionDto {
  id: string
  relationId: string
  users: string[]
  messages: MessageDto[]
  isClosed: boolean
}

export const discussionApi = createApi({
  reducerPath: 'discussionApi',
  baseQuery: axiosBaseQuery(),
  tagTypes: ['Discussion'],
  endpoints: (builder) => ({
    getDiscussion: builder.query<DiscussionDto, string>({
      query: (discussionId) => ({ url: `/discussions/${discussionId}` }),
      providesTags: (_r, _e, id) => [{ type: 'Discussion', id }],
    }),

    getDiscussionByRelation: builder.query<DiscussionDto, string>({
      query: (relationId) => ({ url: `/discussions/by-relation/${relationId}` }),
      providesTags: (_r, _e, id) => [{ type: 'Discussion', id: `rel-${id}` }],
    }),

    addMessage: builder.mutation<string, { discussionId: string; text: string }>({
      query: ({ discussionId, text }) => ({
        url: `/discussions/${discussionId}/messages`,
        method: 'POST',
        data: { text },
      }),
      invalidatesTags: (_r, _e, { discussionId }) => [{ type: 'Discussion', id: discussionId }],
    }),

    editMessage: builder.mutation<string, { discussionId: string; messageId: string; newText: string }>({
      query: ({ discussionId, messageId, newText }) => ({
        url: `/discussions/${discussionId}/messages/${messageId}`,
        method: 'PUT',
        data: { newText },
      }),
      invalidatesTags: (_r, _e, { discussionId }) => [{ type: 'Discussion', id: discussionId }],
    }),

    deleteMessage: builder.mutation<string, { discussionId: string; messageId: string }>({
      query: ({ discussionId, messageId }) => ({
        url: `/discussions/${discussionId}/messages/${messageId}`,
        method: 'DELETE',
      }),
      invalidatesTags: (_r, _e, { discussionId }) => [{ type: 'Discussion', id: discussionId }],
    }),
  }),
})

export const {
  useGetDiscussionQuery,
  useGetDiscussionByRelationQuery,
  useAddMessageMutation,
  useEditMessageMutation,
  useDeleteMessageMutation,
} = discussionApi
