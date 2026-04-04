export const VolunteerRequestStatus = {
  Submitted: 0,
  OnReview: 1,
  RevisionRequired: 2,
  Rejected: 3,
  Approved: 4,
} as const
export type VolunteerRequestStatus = typeof VolunteerRequestStatus[keyof typeof VolunteerRequestStatus]

export interface VolunteerInfo {
  experience: number
  motivation: string
  certificates: string[]
  requisites: string[]
}

export interface VolunteerRequestDto {
  id: string
  userId: string
  adminId?: string
  discussionId?: string
  volunteerInfo: VolunteerInfo
  status: VolunteerRequestStatus
  rejectionComment?: string
  createdAt: string
}
