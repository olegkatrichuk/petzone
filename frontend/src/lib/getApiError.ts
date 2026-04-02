import type { AxiosError } from 'axios'
import type { TFunction } from 'i18next'
import type { Envelope } from '@/types/api'

/**
 * Extracts a translated error message from an Axios API error.
 * Priority: translated errorCode → raw errorMessage → fallback
 */
export function getApiError(
  err: unknown,
  t: TFunction,
  fallback = 'errors.unknown',
): string {
  const axiosErr = err as AxiosError<Envelope<null>>
  const errorInfo = axiosErr.response?.data?.errorInfo?.[0]

  if (!errorInfo) return t(fallback)

  const { errorCode, errorMessage } = errorInfo

  if (errorCode) {
    const key = `errors.${errorCode}`
    const translated = t(key)
    if (translated !== key) return translated
  }

  return errorMessage ?? t(fallback)
}