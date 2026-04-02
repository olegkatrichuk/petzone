export enum VolunteerRequestStatus {
  Submitted = 0,
  OnReview = 1,
  RevisionRequired = 2,
  Rejected = 3,
  Approved = 4,
}

export interface VolunteerInfo {
  experience: number
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
