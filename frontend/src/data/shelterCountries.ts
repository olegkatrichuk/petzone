import shelterCountriesData from './shelterCountries.json'

export interface ShelterCountryMeta {
  flag: string
  /** Nominative form per UI lang */
  name: Record<string, string>
  /** Page <title> per UI lang */
  pageTitle: Record<string, string>
  /** Meta description per UI lang */
  pageDesc: Record<string, string>
}

// Single source of truth lives in shelterCountries.json so the build-time
// prerender (scripts/prerender.mjs) and the SPA share identical titles/descriptions.
export const SHELTER_COUNTRIES = shelterCountriesData as Record<string, ShelterCountryMeta>

export const COUNTRY_CODES = Object.keys(SHELTER_COUNTRIES)
