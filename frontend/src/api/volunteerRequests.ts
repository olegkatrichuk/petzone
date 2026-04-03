import { api } from '../lib/axios'

export interface CreateVolunteerRequestDto {
  experience: number
  motivation: string
  certificates: string[]
  requisites: string[]
}

export const createVolunteerRequest = async (data: CreateVolunteerRequestDto): Promise<void> => {
  await api.post('/volunteerrequests', data)
}
