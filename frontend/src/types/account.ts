export interface ParticipantAccountDto {
  id: string
  favoritePets: string[]
}

export interface VolunteerAccountDto {
  id: string
  experience: number
  certificates: string[]
  requisites: string[]
}

export interface AdminAccountDto {
  id: string
}

export interface UserDto {
  id: string
  email: string
  firstName: string
  lastName: string
  participantAccount?: ParticipantAccountDto
  volunteerAccount?: VolunteerAccountDto
  adminAccount?: AdminAccountDto
}

export interface UserListItemDto {
  id: string
  email: string
  firstName: string
  lastName: string
  role: 'Admin' | 'Volunteer' | 'Participant'
  isLocked: boolean
  lockoutEnd?: string
}
