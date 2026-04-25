import { useState, useMemo } from 'react'
import { useParams, Navigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useLangNavigate } from '../hooks/useLangNavigate'
import PageMeta from '../components/meta/PageMeta'
import ShelterCard from '../components/shelters/ShelterCard'
import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Typography from '@mui/material/Typography'
import Grid from '@mui/material/Grid'
import Button from '@mui/material/Button'
import TextField from '@mui/material/TextField'
import Chip from '@mui/material/Chip'
import IconButton from '@mui/material/IconButton'
import InputAdornment from '@mui/material/InputAdornment'
import Divider from '@mui/material/Divider'
import SearchIcon from '@mui/icons-material/Search'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import ClearIcon from '@mui/icons-material/Clear'
import PetsIcon from '@mui/icons-material/Pets'
import Pagination from '../components/ui/Pagination'
import sheltersData from '../data/shelters.json'
import { SHELTER_COUNTRIES } from '../data/shelterCountries'
import type { ShelterItem } from '../components/shelters/ShelterCard'
import { DEFAULT_LANG } from '../lib/langUtils'

const CORAL = '#FF6B6B'
const DARK = '#1e1b4b'
const PAGE_SIZE = 24
const SITE_URL = 'https://getpetzone.com'

const allShelters = sheltersData as ShelterItem[]

export default function SheltersCountryPage() {
  const { lang, countryCode } = useParams<{ lang: string; countryCode: string }>()
  const { t } = useTranslation()
  const navigate = useLangNavigate()
  const [search, setSearch] = useState('')
  const [cityFilter, setCityFilter] = useState('')
  const [page, setPage] = useState(1)

  const code = (countryCode ?? '').toUpperCase()
  const meta = SHELTER_COUNTRIES[code]
  const currentLang = lang ?? DEFAULT_LANG

  if (!meta) return <Navigate to={`/${currentLang}/shelters`} replace />

  const countryShelters = useMemo(
    () => allShelters.filter((s) => (s.country ?? '') === code),
    [code],
  )

  const uniqueCities = useMemo(
    () => [...new Set(countryShelters.map((s) => s.city).filter(Boolean))].sort(),
    [countryShelters],
  )

  const topCities = useMemo(
    () =>
      uniqueCities
        .map((city) => ({ city, count: countryShelters.filter((s) => s.city === city).length }))
        .sort((a, b) => b.count - a.count)
        .slice(0, 8)
        .map((c) => c.city),
    [uniqueCities, countryShelters],
  )

  const filtered = useMemo(() => {
    const q = search.trim().toLowerCase()
    return countryShelters.filter((s) => {
      const matchSearch = !q || s.name.toLowerCase().includes(q) || s.city.toLowerCase().includes(q)
      const matchCity = !cityFilter || s.city === cityFilter
      return matchSearch && matchCity
    })
  }, [countryShelters, search, cityFilter])

  const totalCount = filtered.length
  const paged = filtered.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE)

  const handleSearch = (v: string) => { setSearch(v); setPage(1) }
  const handleCity = (city: string) => { setCityFilter(city === cityFilter ? '' : city); setPage(1) }

  const pageTitle = meta.pageTitle[currentLang] ?? meta.pageTitle['en']
  const pageDesc = meta.pageDesc[currentLang] ?? meta.pageDesc['en']
  const countryPath = `/shelters/${code.toLowerCase()}`

  // Schema.org: BreadcrumbList
  const breadcrumbSchema = {
    '@context': 'https://schema.org',
    '@type': 'BreadcrumbList',
    itemListElement: [
      { '@type': 'ListItem', position: 1, name: t('nav.home'), item: `${SITE_URL}/${currentLang}` },
      { '@type': 'ListItem', position: 2, name: t('shelters.pageTitle'), item: `${SITE_URL}/${currentLang}/shelters` },
      { '@type': 'ListItem', position: 3, name: meta.name[currentLang] ?? meta.name['en'], item: `${SITE_URL}/${currentLang}${countryPath}` },
    ],
  }

  // Schema.org: ItemList with LocalBusiness entries (top 50)
  const itemListSchema = {
    '@context': 'https://schema.org',
    '@type': 'ItemList',
    name: pageTitle,
    description: pageDesc,
    numberOfItems: countryShelters.length,
    url: `${SITE_URL}/${currentLang}${countryPath}`,
    itemListElement: countryShelters.slice(0, 50).map((s, i) => ({
      '@type': 'ListItem',
      position: i + 1,
      item: {
        '@type': 'LocalBusiness',
        name: s.name,
        ...(s.city ? { address: { '@type': 'PostalAddress', addressLocality: s.city, addressCountry: code } } : {}),
        ...(s.url ? { url: s.url } : {}),
        ...(s.phone ? { telephone: s.phone } : {}),
        ...(s.email ? { email: s.email } : {}),
      },
    })),
  }

  return (
    <Box sx={{ bgcolor: 'background.default' }}>
      <PageMeta title={pageTitle} description={pageDesc} path={countryPath} />
      <script type="application/ld+json" dangerouslySetInnerHTML={{ __html: JSON.stringify(breadcrumbSchema) }} />
      <script type="application/ld+json" dangerouslySetInnerHTML={{ __html: JSON.stringify(itemListSchema) }} />

      {/* ── HERO ─────────────────────────────────────────────── */}
      <Box
        sx={{
          background: `linear-gradient(135deg, ${DARK} 0%, #1e3a5f 55%, #0f4c81 100%)`,
          color: 'white',
          py: { xs: 7, md: 10 },
          textAlign: 'center',
        }}
      >
        <Container maxWidth="md">
          <Box
            sx={{
              display: 'inline-flex',
              alignItems: 'center',
              gap: 1,
              bgcolor: 'rgba(255,107,107,0.15)',
              border: '1px solid rgba(255,107,107,0.35)',
              borderRadius: 5,
              px: 2,
              py: 0.75,
              mb: 3,
            }}
          >
            <PetsIcon sx={{ fontSize: 15, color: CORAL }} />
            <Typography
              variant="caption"
              sx={{ color: CORAL, fontWeight: 600, letterSpacing: 1, textTransform: 'uppercase' }}
            >
              {meta.flag} {meta.name[currentLang] ?? meta.name['en']}
            </Typography>
          </Box>

          <Typography
            variant="h1"
            fontWeight="bold"
            sx={{ mb: 2, fontSize: { xs: '1.8rem', md: '2.8rem' }, lineHeight: 1.2 }}
          >
            {pageTitle}
          </Typography>

          <Typography
            variant="h6"
            sx={{ opacity: 0.8, fontWeight: 400, mb: 5, fontSize: { xs: '1rem', md: '1.1rem' }, maxWidth: 640, mx: 'auto' }}
          >
            {pageDesc}
          </Typography>

          {/* Stats bar */}
          <Box
            sx={{
              display: 'inline-flex',
              gap: { xs: 3, md: 5 },
              bgcolor: 'rgba(255,255,255,0.08)',
              border: '1px solid rgba(255,255,255,0.15)',
              borderRadius: 3,
              px: { xs: 3, md: 5 },
              py: 2,
              flexWrap: 'wrap',
              justifyContent: 'center',
            }}
          >
            <Box sx={{ textAlign: 'center' }}>
              <Typography variant="h5" fontWeight="bold" sx={{ color: CORAL, lineHeight: 1 }}>
                {countryShelters.length}
              </Typography>
              <Typography variant="caption" sx={{ opacity: 0.7 }}>{t('shelters.stats.orgs')}</Typography>
            </Box>
            <Divider orientation="vertical" flexItem sx={{ borderColor: 'rgba(255,255,255,0.2)' }} />
            <Box sx={{ textAlign: 'center' }}>
              <Typography variant="h5" fontWeight="bold" sx={{ color: '#60A5FA', lineHeight: 1 }}>
                {uniqueCities.length}
              </Typography>
              <Typography variant="caption" sx={{ opacity: 0.7 }}>{t('shelters.stats.cities')}</Typography>
            </Box>
          </Box>
        </Container>
      </Box>

      <Container maxWidth="xl" sx={{ py: { xs: 4, md: 6 } }}>
        {/* ── BACK LINK ──────────────────────────────────────── */}
        <Button
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate('/shelters')}
          sx={{ mb: 3, textTransform: 'none', color: '#6B7280', '&:hover': { color: CORAL } }}
        >
          {t('shelters.allCountries')}
        </Button>

        {/* ── SEARCH & CITY FILTERS ───────────────────────────── */}
        <Box sx={{ display: 'flex', gap: 2, mb: 3, flexWrap: 'wrap', alignItems: 'center' }}>
          <TextField
            value={search}
            onChange={(e) => handleSearch(e.target.value)}
            placeholder={t('shelters.searchPlaceholder')}
            size="small"
            sx={{ minWidth: 280, bgcolor: 'background.paper', borderRadius: 2, flexShrink: 0 }}
            slotProps={{
              input: {
                startAdornment: (
                  <InputAdornment position="start">
                    <SearchIcon sx={{ color: '#9CA3AF', fontSize: 20 }} />
                  </InputAdornment>
                ),
                endAdornment: search ? (
                  <InputAdornment position="end">
                    <IconButton size="small" onClick={() => handleSearch('')}>
                      <ClearIcon sx={{ fontSize: 16 }} />
                    </IconButton>
                  </InputAdornment>
                ) : null,
              },
            }}
          />

          <Box sx={{ display: 'flex', gap: 0.75, flexWrap: 'wrap' }}>
            {topCities.map((city) => (
              <Chip
                key={city}
                label={city}
                size="small"
                onClick={() => handleCity(city)}
                variant={cityFilter === city ? 'filled' : 'outlined'}
                sx={{
                  cursor: 'pointer',
                  bgcolor: cityFilter === city ? CORAL : 'transparent',
                  color: cityFilter === city ? 'white' : 'text.secondary',
                  borderColor: cityFilter === city ? CORAL : '#E5E7EB',
                  fontWeight: cityFilter === city ? 600 : 400,
                  '&:hover': { bgcolor: cityFilter === city ? '#e55555' : '#FFF0F0', borderColor: CORAL, color: cityFilter === city ? 'white' : CORAL },
                }}
              />
            ))}
            {cityFilter && (
              <Button
                size="small"
                onClick={() => { setCityFilter(''); setPage(1) }}
                sx={{ textTransform: 'none', color: '#9CA3AF', minWidth: 0, fontSize: 13 }}
              >
                {t('shelters.clearSearch')}
              </Button>
            )}
          </Box>

          <Typography variant="body2" color="text.secondary" sx={{ ml: 'auto' }}>
            {totalCount} {t('shelters.stats.orgs')}
          </Typography>
        </Box>

        {/* ── GRID ───────────────────────────────────────────── */}
        {paged.length === 0 ? (
          <Box sx={{ textAlign: 'center', py: 10 }}>
            <Typography variant="h6" color="text.secondary">{t('shelters.notFound')}</Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
              {t('shelters.notFoundHint')}
            </Typography>
          </Box>
        ) : (
          <Grid container spacing={2.5}>
            {paged.map((shelter) => (
              <Grid key={shelter.url} size={{ xs: 12, sm: 6, md: 4, lg: 3 }}>
                <ShelterCard shelter={shelter} showFlag={false} />
              </Grid>
            ))}
          </Grid>
        )}

        {totalCount > PAGE_SIZE && (
          <Pagination
            page={page}
            pageSize={PAGE_SIZE}
            totalCount={totalCount}
            onChange={(p) => { setPage(p); window.scrollTo({ top: 0, behavior: 'smooth' }) }}
            ofLabel={t('shelters.of')}
          />
        )}
      </Container>
    </Box>
  )
}
