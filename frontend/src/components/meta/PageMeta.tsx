import { Helmet } from 'react-helmet-async'
import { useParams } from 'react-router-dom'
import { SUPPORTED_LANGS, DEFAULT_LANG } from '../../lib/langUtils'

const SITE_URL = 'https://getpetzone.com'
const SITE_NAME = 'PetZone'

interface PageMetaProps {
  title: string
  description: string
  /** Relative path without lang prefix, e.g. "/pets" or "/pets/123" */
  path?: string
  image?: string
  type?: 'website' | 'article'
  /** Prevent indexing — use on private/auth pages */
  noIndex?: boolean
}

export default function PageMeta({ title, description, path = '', image, type = 'website', noIndex = false }: PageMetaProps) {
  const { lang } = useParams<{ lang: string }>()
  const currentLang = lang ?? DEFAULT_LANG
  const fullTitle = `${title} | ${SITE_NAME}`
  const canonicalUrl = `${SITE_URL}/${currentLang}${path}`
  const ogImage = image ?? `${SITE_URL}/og-default.svg`

  return (
    <Helmet>
      <html lang={currentLang} />
      <title>{fullTitle}</title>
      <meta name="description" content={description} />
      {noIndex && <meta name="robots" content="noindex,nofollow" />}
      <link rel="canonical" href={canonicalUrl} />

      {/* hreflang for all supported languages */}
      {SUPPORTED_LANGS.map((l) => (
        <link key={l} rel="alternate" hrefLang={l} href={`${SITE_URL}/${l}${path}`} />
      ))}
      <link rel="alternate" hrefLang="x-default" href={`${SITE_URL}/${DEFAULT_LANG}${path}`} />

      {/* Open Graph */}
      <meta property="og:type" content={type} />
      <meta property="og:title" content={fullTitle} />
      <meta property="og:description" content={description} />
      <meta property="og:url" content={canonicalUrl} />
      <meta property="og:image" content={ogImage} />
      <meta property="og:site_name" content={SITE_NAME} />
      <meta property="og:locale" content={currentLang} />

      {/* Twitter Card */}
      <meta name="twitter:card" content="summary_large_image" />
      <meta name="twitter:title" content={fullTitle} />
      <meta name="twitter:description" content={description} />
      <meta name="twitter:image" content={ogImage} />
    </Helmet>
  )
}
