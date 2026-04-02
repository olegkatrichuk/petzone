import { api } from '../lib/axios'
import type { Envelope } from '../types/api'

interface LoginRequest {
  email: string
  password: string
}

interface LoginResponse {
  accessToken: string
}

export interface RegisterUserRequest {
  firstName: string
  lastName: string
  email: string
  password: string
}


export const loginUser = async (data: LoginRequest): Promise<string> => {
  const res = await api.post<Envelope<LoginResponse>>('/accounts/login', data)
  return res.data.result.accessToken
}

export const registerUser = async (data: RegisterUserRequest): Promise<void> => {
  await api.post('/accounts/register', data)
}


export const refreshTokens = async (): Promise<string> => {
  const res = await api.post<Envelope<LoginResponse>>('/accounts/refresh')
  return res.data.result.accessToken
}

export const logoutUser = (): Promise<void> =>
  api.post('/accounts/logout').then(() => undefined)
