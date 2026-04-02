export const SUPPORTED_LANGS = ['uk', 'en', 'pl', 'de', 'fr', 'ru'] as const
export type SupportedLang = (typeof SUPPORTED_LANGS)[number]

export const DEFAULT_LANG: SupportedLang = 'uk'

export function isValidLang(lang: string | undefined): lang is SupportedLang {
  return SUPPORTED_LANGS.includes(lang as SupportedLang)
}

/** Detects preferred language from browser, falls back to DEFAULT_LANG */
export function detectBrowserLang(): SupportedLang {
  const langs = navigator.languages ?? [navigator.language]
  for (const l of langs) {
    const code = l.split('-')[0].toLowerCase()
    if (isValidLang(code)) return code
  }
  return DEFAULT_LANG
}

/** Reads current lang from URL path (first segment) */
export function getLangFromPath(pathname: string): SupportedLang {
  const segment = pathname.split('/')[1]
  return isValidLang(segment) ? segment : DEFAULT_LANG
}
