// City-level shelter page titles/descriptions.
// MUST stay in sync with cityShelterTitle/cityShelterDesc in scripts/prerender.mjs
// so the prerendered <head> and the hydrated SPA emit identical meta.

const SHELTER_WORD: Record<string, string> = {
  uk: 'Притулки для тварин',
  en: 'Animal Shelters',
  pl: 'Schroniska dla zwierząt',
  de: 'Tierheime',
  fr: 'Refuges pour animaux',
  ru: 'Приюты для животных',
}

const IN_CITY: Record<string, (c: string) => string> = {
  uk: (c) => `у місті ${c}`,
  en: (c) => `in ${c}`,
  pl: (c) => `w ${c}`,
  de: (c) => `in ${c}`,
  fr: (c) => `à ${c}`,
  ru: (c) => `в городе ${c}`,
}

export function cityShelterTitle(lang: string, city: string, countryName: string): string {
  if (lang === 'de') return `Tierheim ${city} — Tierheime in ${city} (${countryName})`
  const word = SHELTER_WORD[lang] ?? SHELTER_WORD.en
  const inCity = (IN_CITY[lang] ?? IN_CITY.en)(city)
  return `${word} ${inCity} (${countryName})`
}

export function cityShelterDesc(lang: string, city: string, countryName: string, count: number): string {
  // Lead with the count only when plural — avoids "1 Tierheime" / "1 shelters".
  const n = count > 1 ? `${count} ` : ''
  const inCity = (IN_CITY[lang] ?? IN_CITY.en)(city)
  switch (lang) {
    case 'de': return `${n}Tierheime und Tierschutzorganisationen in ${city} (${countryName}) — Adressen, Telefon, Website und Social Media.`
    case 'ru': return `${n}приюты и зоозащитные организации ${inCity} (${countryName}) — адреса, телефоны, сайты и соцсети.`
    case 'uk': return `${n}притулки та зоозахисні організації ${inCity} (${countryName}) — адреси, телефони, сайти.`
    case 'pl': return `${n}schroniska i organizacje ochrony zwierząt ${inCity} (${countryName}) — adresy, telefony, strony.`
    case 'fr': return `${n}refuges et organisations de protection animale ${inCity} (${countryName}) — adresses, téléphones, sites.`
    default: return `${n}animal shelters and rescue organizations ${inCity} (${countryName}) — addresses, phone, websites.`
  }
}
