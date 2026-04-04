export interface Species {
  id: string
  name: string
  breedsCount: number
  translations: Record<string, string>
}

export interface Breed {
  id: string
  name: string
  translations: Record<string, string>
}

export const SPECIES_LOCALES = ['uk', 'en', 'ru', 'de', 'pl', 'fr'] as const
export type SpeciesLocale = typeof SPECIES_LOCALES[number]
