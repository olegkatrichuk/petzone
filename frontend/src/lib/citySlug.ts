// ASCII slug for a city name, e.g. "Liptovský Mikuláš" -> "liptovsky-mikulas".
// MUST stay identical to slugifyCity() in scripts/prerender.mjs so prerendered
// shelter-city URLs resolve to the same city the SPA filters by.
export function citySlug(city: string): string {
  return city
    .normalize('NFD')
    .replace(/[̀-ͯ]/g, '')
    .toLowerCase()
    .trim()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-+|-+$/g, '')
}
