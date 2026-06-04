/**
 * Lightweight prerender for static routes.
 *
 * Why: SPA on a fresh domain — Googlebot's first pass reads bare HTML, then
 * queues a JS render later (often with delay). Bing/Yandex barely run JS at
 * all. Without prerender, our static landing pages look empty to crawlers.
 *
 * What this does: for each (language × route), copy dist/index.html and
 * inject lang-specific <title>, description, OG/Twitter tags, canonical,
 * hreflang, and a <noscript> H1+intro block. Crawlers see content
 * immediately; React still hydrates the SPA on top for users.
 */

import { readFile, writeFile, mkdir } from 'node:fs/promises'
import { readFileSync } from 'node:fs'
import { dirname, join } from 'node:path'
import { fileURLToPath } from 'node:url'

const __dirname = dirname(fileURLToPath(import.meta.url))
const ROOT = join(__dirname, '..')
const DIST = join(ROOT, 'dist')
const SITE_URL = 'https://getpetzone.com'

const LANGS = ['uk', 'en', 'pl', 'de', 'fr', 'ru']
const DEFAULT_LANG = 'uk'

// Single source of truth shared with the SPA (src/data/shelterCountries.ts).
// Per-country titles like "Tierheime in der Slowakei" / "Приюты для животных в
// Швейцарии" — far stronger for geo queries than the generic "Shelters — X".
const COUNTRY_META = JSON.parse(
  readFileSync(join(ROOT, 'src/data/shelterCountries.json'), 'utf-8'),
)
const SHELTERS = JSON.parse(
  readFileSync(join(ROOT, 'src/data/shelters.json'), 'utf-8'),
)

// ASCII slug for a city name, e.g. "Liptovský Mikuláš" -> "liptovsky-mikulas".
// MUST stay identical to citySlug() in src/lib/citySlug.ts so prerendered URLs
// match what the SPA resolves at runtime.
function slugifyCity(city) {
  return city
    .normalize('NFD')
    .replace(/[̀-ͯ]/g, '')
    .toLowerCase()
    .trim()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-+|-+$/g, '')
}

// Cities with shelters in a country, most-populated first.
function citiesForCountry(code) {
  const counts = {}
  for (const s of SHELTERS) {
    if ((s.country ?? '') !== code || !s.city) continue
    counts[s.city] = (counts[s.city] ?? 0) + 1
  }
  return Object.entries(counts)
    .map(([city, count]) => ({ city, count, slug: slugifyCity(city) }))
    .sort((a, b) => b.count - a.count)
}

const SHELTER_COUNTRIES = [
  'be', 'bg', 'ch', 'cz', 'de', 'ee', 'es', 'gb',
  'hu', 'lt', 'pl', 'pt', 'ro', 'se', 'sk', 'ua',
]

const COUNTRY_NAMES = {
  uk: { be: 'Бельгія', bg: 'Болгарія', ch: 'Швейцарія', cz: 'Чехія', de: 'Німеччина', ee: 'Естонія', es: 'Іспанія', gb: 'Велика Британія', hu: 'Угорщина', lt: 'Литва', pl: 'Польща', pt: 'Португалія', ro: 'Румунія', se: 'Швеція', sk: 'Словаччина', ua: 'Україна' },
  en: { be: 'Belgium', bg: 'Bulgaria', ch: 'Switzerland', cz: 'Czechia', de: 'Germany', ee: 'Estonia', es: 'Spain', gb: 'United Kingdom', hu: 'Hungary', lt: 'Lithuania', pl: 'Poland', pt: 'Portugal', ro: 'Romania', se: 'Sweden', sk: 'Slovakia', ua: 'Ukraine' },
  pl: { be: 'Belgia', bg: 'Bułgaria', ch: 'Szwajcaria', cz: 'Czechy', de: 'Niemcy', ee: 'Estonia', es: 'Hiszpania', gb: 'Wielka Brytania', hu: 'Węgry', lt: 'Litwa', pl: 'Polska', pt: 'Portugalia', ro: 'Rumunia', se: 'Szwecja', sk: 'Słowacja', ua: 'Ukraina' },
  de: { be: 'Belgien', bg: 'Bulgarien', ch: 'Schweiz', cz: 'Tschechien', de: 'Deutschland', ee: 'Estland', es: 'Spanien', gb: 'Vereinigtes Königreich', hu: 'Ungarn', lt: 'Litauen', pl: 'Polen', pt: 'Portugal', ro: 'Rumänien', se: 'Schweden', sk: 'Slowakei', ua: 'Ukraine' },
  fr: { be: 'Belgique', bg: 'Bulgarie', ch: 'Suisse', cz: 'Tchéquie', de: 'Allemagne', ee: 'Estonie', es: 'Espagne', gb: 'Royaume-Uni', hu: 'Hongrie', lt: 'Lituanie', pl: 'Pologne', pt: 'Portugal', ro: 'Roumanie', se: 'Suède', sk: 'Slovaquie', ua: 'Ukraine' },
  ru: { be: 'Бельгия', bg: 'Болгария', ch: 'Швейцария', cz: 'Чехия', de: 'Германия', ee: 'Эстония', es: 'Испания', gb: 'Великобритания', hu: 'Венгрия', lt: 'Литва', pl: 'Польша', pt: 'Португалия', ro: 'Румыния', se: 'Швеция', sk: 'Словакия', ua: 'Украина' },
}

const SPECIES = ['dogs', 'cats', 'rabbits', 'parrots']
const SPECIES_LABELS = {
  uk: { dogs: 'Собаки',    cats: 'Коти',    rabbits: 'Кролі',    parrots: 'Папуги'   },
  en: { dogs: 'Dogs',      cats: 'Cats',    rabbits: 'Rabbits',  parrots: 'Parrots'  },
  pl: { dogs: 'Psy',       cats: 'Koty',    rabbits: 'Króliki',  parrots: 'Papugi'   },
  de: { dogs: 'Hunde',     cats: 'Katzen',  rabbits: 'Kaninchen', parrots: 'Papageien' },
  fr: { dogs: 'Chiens',    cats: 'Chats',   rabbits: 'Lapins',   parrots: 'Perroquets' },
  ru: { dogs: 'Собаки',    cats: 'Кошки',   rabbits: 'Кролики',  parrots: 'Попугаи'  },
}

// Must match backend SitemapController citySlugs and frontend PetsSpeciesPage CITIES
const CITY_SLUGS = ['kyiv', 'kharkiv', 'lviv', 'odesa', 'dnipro', 'zaporizhzhia']

// Per-locale city phrasing. Slavic languages use locative case ("у Києві"),
// the rest just use the city name with a preposition.
// Format: { slug: { name, in } } — `in` is the phrase used after species
// (e.g. "in Kyiv", "у Києві", "in Krakau"). Both go into <title>/H1.
const CITY_LABELS = {
  uk: {
    kyiv:         { name: 'Київ',       in: 'у Києві' },
    kharkiv:      { name: 'Харків',     in: 'у Харкові' },
    lviv:         { name: 'Львів',      in: 'у Львові' },
    odesa:        { name: 'Одеса',      in: 'в Одесі' },
    dnipro:       { name: 'Дніпро',     in: 'у Дніпрі' },
    zaporizhzhia: { name: 'Запоріжжя',  in: 'у Запоріжжі' },
  },
  ru: {
    kyiv:         { name: 'Киев',       in: 'в Киеве' },
    kharkiv:      { name: 'Харьков',    in: 'в Харькове' },
    lviv:         { name: 'Львов',      in: 'во Львове' },
    odesa:        { name: 'Одесса',     in: 'в Одессе' },
    dnipro:       { name: 'Днепр',      in: 'в Днепре' },
    zaporizhzhia: { name: 'Запорожье',  in: 'в Запорожье' },
  },
  en: {
    kyiv:         { name: 'Kyiv',       in: 'in Kyiv' },
    kharkiv:      { name: 'Kharkiv',    in: 'in Kharkiv' },
    lviv:         { name: 'Lviv',       in: 'in Lviv' },
    odesa:        { name: 'Odesa',      in: 'in Odesa' },
    dnipro:       { name: 'Dnipro',     in: 'in Dnipro' },
    zaporizhzhia: { name: 'Zaporizhzhia', in: 'in Zaporizhzhia' },
  },
  pl: {
    kyiv:         { name: 'Kijów',      in: 'w Kijowie' },
    kharkiv:      { name: 'Charków',    in: 'w Charkowie' },
    lviv:         { name: 'Lwów',       in: 'we Lwowie' },
    odesa:        { name: 'Odessa',     in: 'w Odessie' },
    dnipro:       { name: 'Dniepr',     in: 'w Dnieprze' },
    zaporizhzhia: { name: 'Zaporoże',   in: 'w Zaporożu' },
  },
  de: {
    kyiv:         { name: 'Kiew',       in: 'in Kiew' },
    kharkiv:      { name: 'Charkiw',    in: 'in Charkiw' },
    lviv:         { name: 'Lwiw',       in: 'in Lwiw' },
    odesa:        { name: 'Odessa',     in: 'in Odessa' },
    dnipro:       { name: 'Dnipro',     in: 'in Dnipro' },
    zaporizhzhia: { name: 'Saporischschja', in: 'in Saporischschja' },
  },
  fr: {
    kyiv:         { name: 'Kiev',       in: 'à Kiev' },
    kharkiv:      { name: 'Kharkiv',    in: 'à Kharkiv' },
    lviv:         { name: 'Lviv',       in: 'à Lviv' },
    odesa:        { name: 'Odessa',     in: 'à Odessa' },
    dnipro:       { name: 'Dnipro',     in: 'à Dnipro' },
    zaporizhzhia: { name: 'Zaporijia',  in: 'à Zaporijia' },
  },
}

async function loadLocale(lang) {
  const raw = await readFile(join(ROOT, 'src/i18n/locales', `${lang}.json`), 'utf-8')
  return JSON.parse(raw)
}

function escapeHtml(s) {
  return String(s ?? '')
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#39;')
}

function buildRoutes(locale, lang) {
  const cn = COUNTRY_NAMES[lang]
  const sl = SPECIES_LABELS[lang]

  const routes = [
    {
      path: '',
      title: locale.home?.hero?.title ?? 'PetZone',
      description: locale.home?.hero?.subtitle ?? '',
    },
    {
      path: '/pets',
      title: locale.pets?.pageTitle ?? 'Pets',
      description: locale.pets?.metaDesc ?? '',
    },
    {
      path: '/shelters',
      title: locale.shelters?.pageTitle ?? 'Shelters',
      description: locale.shelters?.metaDesc ?? '',
    },
    {
      path: '/volunteers',
      title: locale.volunteers?.pageTitle ?? 'Volunteers',
      description: locale.volunteers?.metaDesc ?? '',
    },
    {
      path: '/about',
      title: locale.about?.pageTitle ?? 'About',
      description: locale.about?.metaDesc ?? '',
    },
    {
      path: '/faq',
      title: locale.faq?.pageTitle ?? 'FAQ',
      description: locale.faq?.metaDesc ?? '',
    },
    {
      path: '/blog',
      title: locale.blog?.pageTitle ?? 'Blog',
      description: locale.blog?.metaDesc ?? '',
    },
  ]

  const cl = CITY_LABELS[lang]

  for (const sp of SPECIES) {
    const speciesLabel = sl[sp]
    routes.push({
      path: `/pets/${sp}`,
      title: speciesLabel,
      description: `${speciesLabel} — ${locale.pets?.metaDesc ?? ''}`,
    })

    // City-scoped species pages — high local-SEO value
    // (e.g. "Собаки у Києві", "Dogs in Kyiv", "Psy w Kijowie").
    for (const cs of CITY_SLUGS) {
      const city = cl[cs]
      routes.push({
        path: `/pets/${sp}/${cs}`,
        title: `${speciesLabel} ${city.in}`,
        description: `${speciesLabel} ${city.in} — ${locale.pets?.metaDesc ?? ''}`,
      })
    }
  }

  for (const cc of SHELTER_COUNTRIES) {
    const code = cc.toUpperCase()
    const meta = COUNTRY_META[code]
    if (!meta) continue

    const countryName = meta.name?.[lang] ?? cn[cc]
    // Cap to the 15 best-covered cities per country: enough to capture local
    // queries (e.g. "tierheim bratislava") without spawning hundreds of thin
    // single-shelter pages across 16 countries × 6 languages.
    const cities = citiesForCountry(code).slice(0, 15)
    // Top shelter names for crawlable body text on the country page.
    const topShelters = SHELTERS
      .filter((s) => (s.country ?? '') === code)
      .slice(0, 12)
      .map((s) => s.name)

    routes.push({
      path: `/shelters/${cc}`,
      title: meta.pageTitle?.[lang] ?? meta.pageTitle?.en ?? `Shelters — ${countryName}`,
      description: meta.pageDesc?.[lang] ?? meta.pageDesc?.en ?? `${countryName}: ${locale.shelters?.metaDesc ?? ''}`,
      cityLinks: cities.map((c) => ({ label: c.city, href: `/${lang}/shelters/${cc}/${c.slug}` })),
      bodyItems: topShelters,
    })

    // City-level shelter pages — these match queries like "tierheim bratislava".
    for (const c of cities) {
      routes.push({
        path: `/shelters/${cc}/${c.slug}`,
        title: cityShelterTitle(lang, c.city, countryName),
        description: cityShelterDesc(lang, c.city, countryName, c.count),
        bodyItems: SHELTERS
          .filter((s) => (s.country ?? '') === code && s.city === c.city)
          .map((s) => s.name),
      })
    }
  }

  return routes
}

// "Tierheim Bratislava", "Приюты для животных в Bratislava (Швейцария)" etc.
// Front-loads the city + the locale word for "shelter" to match local queries.
const SHELTER_WORD = {
  uk: 'Притулки для тварин',
  en: 'Animal Shelters',
  pl: 'Schroniska dla zwierząt',
  de: 'Tierheime',
  fr: 'Refuges pour animaux',
  ru: 'Приюты для животных',
}
const IN_CITY = {
  uk: (c) => `у місті ${c}`,
  en: (c) => `in ${c}`,
  pl: (c) => `w ${c}`,
  de: (c) => `in ${c}`,
  fr: (c) => `à ${c}`,
  ru: (c) => `в городе ${c}`,
}

function cityShelterTitle(lang, city, countryName) {
  if (lang === 'de') return `Tierheim ${city} — Tierheime in ${city} (${countryName})`
  const word = SHELTER_WORD[lang] ?? SHELTER_WORD.en
  const inCity = (IN_CITY[lang] ?? IN_CITY.en)(city)
  return `${word} ${inCity} (${countryName})`
}

function cityShelterDesc(lang, city, countryName, count) {
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

function buildHead({ lang, path, title, description }) {
  const siteName = 'PetZone'
  const fullTitle = path === '' ? `${title} — ${siteName}` : `${title} | ${siteName}`
  const canonical = `${SITE_URL}/${lang}${path}`
  const ogImage = `${SITE_URL}/og-default.png`

  const hreflangs = LANGS
    .map((l) => `    <link rel="alternate" hreflang="${l}" href="${SITE_URL}/${l}${path}" />`)
    .join('\n')

  return `    <title>${escapeHtml(fullTitle)}</title>
    <meta name="description" content="${escapeHtml(description)}" />
    <link rel="canonical" href="${canonical}" />
${hreflangs}
    <link rel="alternate" hreflang="x-default" href="${SITE_URL}/${DEFAULT_LANG}${path}" />

    <meta property="og:type" content="website" />
    <meta property="og:site_name" content="${siteName}" />
    <meta property="og:title" content="${escapeHtml(fullTitle)}" />
    <meta property="og:description" content="${escapeHtml(description)}" />
    <meta property="og:url" content="${canonical}" />
    <meta property="og:image" content="${ogImage}" />
    <meta property="og:image:width" content="1200" />
    <meta property="og:image:height" content="630" />
    <meta property="og:locale" content="${lang}" />

    <meta name="twitter:card" content="summary_large_image" />
    <meta name="twitter:title" content="${escapeHtml(fullTitle)}" />
    <meta name="twitter:description" content="${escapeHtml(description)}" />
    <meta name="twitter:image" content="${ogImage}" />`
}

function buildNoscript({ title, description, cityLinks, bodyItems }) {
  const parts = [`<h1>${escapeHtml(title)}</h1>`, `<p>${escapeHtml(description)}</p>`]

  if (cityLinks?.length) {
    const links = cityLinks
      .map((c) => `<li><a href="${escapeHtml(c.href)}">${escapeHtml(c.label)}</a></li>`)
      .join('')
    parts.push(`<ul>${links}</ul>`)
  }

  if (bodyItems?.length) {
    const items = bodyItems.map((name) => `<li>${escapeHtml(name)}</li>`).join('')
    parts.push(`<ul>${items}</ul>`)
  }

  return `<noscript>
      ${parts.join('\n      ')}
    </noscript>`
}

function applyTemplate(template, { lang, path, title, description, cityLinks, bodyItems }) {
  let html = template

  // Set <html lang="...">
  html = html.replace(/<html\s+lang="[^"]*"/, `<html lang="${lang}"`)

  // Replace existing default <title>
  html = html.replace(/<title>[\s\S]*?<\/title>/, '__PETZONE_HEAD__')

  // Strip the meta block we are going to overwrite (description + og + twitter
  // emitted by index.html) so we do not end up with duplicates.
  html = html.replace(/\s*<meta\s+name="description"[^>]*>/i, '')
  html = html.replace(/\s*<meta\s+property="og:[^"]+"[^>]*>/gi, '')
  html = html.replace(/\s*<meta\s+name="twitter:[^"]+"[^>]*>/gi, '')

  const head = buildHead({ lang, path, title, description })
  html = html.replace('__PETZONE_HEAD__', head)

  // Inject <noscript> fallback right after <div id="root">
  const noscript = buildNoscript({ title, description, cityLinks, bodyItems })
  html = html.replace(
    /<div id="root"><\/div>/,
    `<div id="root"></div>\n    ${noscript}`,
  )

  return html
}

async function writeRoute(template, lang, route) {
  const outDir = join(DIST, lang, route.path.replace(/^\//, ''))
  await mkdir(outDir, { recursive: true })
  const html = applyTemplate(template, {
    lang,
    path: route.path,
    title: route.title,
    description: route.description,
    cityLinks: route.cityLinks,
    bodyItems: route.bodyItems,
  })
  await writeFile(join(outDir, 'index.html'), html, 'utf-8')
}

// XML sitemap for the city-level shelter pages (one <url> per city, with
// hreflang alternates for every language). Countries are already covered by
// public/sitemap-static.xml; this file carries the new city delta.
async function writeShelterCitySitemap() {
  const entries = []
  for (const cc of SHELTER_COUNTRIES) {
    const code = cc.toUpperCase()
    if (!COUNTRY_META[code]) continue
    for (const c of citiesForCountry(code).slice(0, 15)) {
      entries.push(`/shelters/${cc}/${c.slug}`)
    }
  }

  const urls = entries
    .map((path) => {
      const alts = LANGS
        .map((l) => `    <xhtml:link rel="alternate" hreflang="${l}" href="${SITE_URL}/${l}${path}"/>`)
        .join('\n')
      return `  <url>
    <loc>${SITE_URL}/${DEFAULT_LANG}${path}</loc>
${alts}
    <xhtml:link rel="alternate" hreflang="x-default" href="${SITE_URL}/${DEFAULT_LANG}${path}"/>
    <changefreq>weekly</changefreq><priority>0.6</priority>
  </url>`
    })
    .join('\n')

  const xml = `<?xml version="1.0" encoding="UTF-8"?>
<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9"
        xmlns:xhtml="http://www.w3.org/1999/xhtml">
${urls}
</urlset>
`
  await writeFile(join(DIST, 'sitemap-shelters-cities.xml'), xml, 'utf-8')
  return entries.length
}

async function main() {
  const template = await readFile(join(DIST, 'index.html'), 'utf-8')

  let count = 0
  for (const lang of LANGS) {
    const locale = await loadLocale(lang)
    const routes = buildRoutes(locale, lang)
    for (const route of routes) {
      await writeRoute(template, lang, route)
      count++
    }
  }

  const cityUrls = await writeShelterCitySitemap()

  console.log(`Prerendered ${count} routes (${LANGS.length} langs × ${count / LANGS.length} per lang)`)
  console.log(`Wrote sitemap-shelters-cities.xml with ${cityUrls} city URLs`)
}

main().catch((err) => {
  console.error('Prerender failed:', err)
  process.exit(1)
})