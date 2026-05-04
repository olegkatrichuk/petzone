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
import { dirname, join } from 'node:path'
import { fileURLToPath } from 'node:url'

const __dirname = dirname(fileURLToPath(import.meta.url))
const ROOT = join(__dirname, '..')
const DIST = join(ROOT, 'dist')
const SITE_URL = 'https://getpetzone.com'

const LANGS = ['uk', 'en', 'pl', 'de', 'fr', 'ru']
const DEFAULT_LANG = 'uk'

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
  ]

  for (const sp of SPECIES) {
    const speciesLabel = sl[sp]
    routes.push({
      path: `/pets/${sp}`,
      title: speciesLabel,
      description: `${speciesLabel} — ${locale.pets?.metaDesc ?? ''}`,
    })
  }

  for (const cc of SHELTER_COUNTRIES) {
    const countryName = cn[cc]
    routes.push({
      path: `/shelters/${cc}`,
      title: `${locale.shelters?.pageTitle ?? 'Shelters'} — ${countryName}`,
      description: `${countryName}: ${locale.shelters?.metaDesc ?? ''}`,
    })
  }

  return routes
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

function buildNoscript({ title, description }) {
  return `<noscript>
      <h1>${escapeHtml(title)}</h1>
      <p>${escapeHtml(description)}</p>
    </noscript>`
}

function applyTemplate(template, { lang, path, title, description }) {
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
  const noscript = buildNoscript({ title, description })
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
  })
  await writeFile(join(outDir, 'index.html'), html, 'utf-8')
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

  console.log(`Prerendered ${count} routes (${LANGS.length} langs × ${count / LANGS.length} per lang)`)
}

main().catch((err) => {
  console.error('Prerender failed:', err)
  process.exit(1)
})