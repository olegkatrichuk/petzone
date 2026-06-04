import i18n from 'i18next'
import { initReactI18next } from 'react-i18next'
import LanguageDetector from 'i18next-browser-languagedetector'
import uk from './locales/uk.json'

const SUPPORTED = ['uk', 'en', 'pl', 'de', 'fr', 'ru'] as const
type Lang = (typeof SUPPORTED)[number]

// Only the fallback/default locale (uk) is bundled eagerly. The other five are
// code-split and fetched on demand, keeping ~230 KB of translations out of the
// main chunk. Until a target locale finishes loading, i18next falls back to uk
// (already in memory) — so the UI never shows raw keys, at most a brief flash.
const loaders: Record<Exclude<Lang, 'uk'>, () => Promise<{ default: Record<string, unknown> }>> = {
  en: () => import('./locales/en.json'),
  pl: () => import('./locales/pl.json'),
  de: () => import('./locales/de.json'),
  fr: () => import('./locales/fr.json'),
  ru: () => import('./locales/ru.json'),
}

const loaded = new Set<Lang>(['uk'])

/**
 * Ensure a locale's resource bundle is loaded before switching to it.
 * Idempotent; safe to call on every route change.
 */
export async function ensureLanguage(lng: string): Promise<void> {
  if (!SUPPORTED.includes(lng as Lang) || loaded.has(lng as Lang)) return
  const mod = await loaders[lng as Exclude<Lang, 'uk'>]()
  i18n.addResourceBundle(lng, 'translation', mod.default, true, true)
  loaded.add(lng as Lang)
}

i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    resources: { uk: { translation: uk } },
    partialBundledLanguages: true,
    fallbackLng: 'uk',
    supportedLngs: [...SUPPORTED],
    detection: {
      order: ['localStorage', 'navigator'],
      lookupLocalStorage: 'language',
      caches: ['localStorage'],
    },
    interpolation: { escapeValue: false },
    react: { useSuspense: false },
  })

// Load the language the detector resolved at startup (may differ from uk).
void ensureLanguage(i18n.language)

export default i18n
