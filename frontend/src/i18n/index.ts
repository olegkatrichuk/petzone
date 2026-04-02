import i18n from 'i18next'
import { initReactI18next } from 'react-i18next'
import LanguageDetector from 'i18next-browser-languagedetector'
import uk from './locales/uk.json'
import en from './locales/en.json'
import pl from './locales/pl.json'
import de from './locales/de.json'
import fr from './locales/fr.json'
import ru from './locales/ru.json'

i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    resources: {
      uk: { translation: uk },
      en: { translation: en },
      pl: { translation: pl },
      de: { translation: de },
      fr: { translation: fr },
      ru: { translation: ru },
    },
    fallbackLng: 'uk',
    supportedLngs: ['uk', 'en', 'pl', 'de', 'fr', 'ru'],
    detection: {
      order: ['localStorage', 'navigator'],
      lookupLocalStorage: 'language',
      caches: ['localStorage'],
    },
    interpolation: { escapeValue: false },
  })

export default i18n
