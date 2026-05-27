import { useState, useMemo } from 'react'
import { useTranslation } from 'react-i18next'
import { useLangNavigate } from '../hooks/useLangNavigate'
import PageMeta from '../components/meta/PageMeta'
import ShelterCard from '../components/shelters/ShelterCard'
import { safeJsonLd } from '../lib/safeJsonLd'
import type { ShelterItem } from '../components/shelters/ShelterCard'
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
import PetsIcon from '@mui/icons-material/Pets'
import ClearIcon from '@mui/icons-material/Clear'
import Pagination from '../components/ui/Pagination'
import sheltersData from '../data/shelters.json'
import { SHELTER_COUNTRIES } from '../data/shelterCountries'

const CORAL = '#FF6B6B'
const DARK = '#1e1b4b'
const PAGE_SIZE = 24

const allShelters = sheltersData as ShelterItem[]

export default function SheltersPage() {
  const { t } = useTranslation()
  const navigate = useLangNavigate()
  const [search, setSearch] = useState('')
  const [cityFilter, setCityFilter] = useState<string>('')
  const [countryFilter, setCountryFilter] = useState<string>('')
  const [page, setPage] = useState(1)

  const countries = useMemo(() => {
    const present = new Set(allShelters.map((s) => s.country ?? '').filter(Boolean))
    return Object.entries(SHELTER_COUNTRIES)
      .filter(([code]) => present.has(code))
      .map(([code, meta]) => ({ code, meta }))
      .sort((a, b) => {
        if (a.code === 'UA') return -1
        if (b.code === 'UA') return 1
        return (a.meta.name['uk'] ?? '').localeCompare(b.meta.name['uk'] ?? '', 'uk')
      })
  }, [])

  const uniqueCities = useMemo(() => {
    const base = countryFilter
      ? allShelters.filter((s) => (s.country ?? '') === countryFilter)
      : allShelters
    return [...new Set(base.map((s) => s.city).filter(Boolean))].sort()
  }, [countryFilter])

  const filtered = useMemo(() => {
    const q = search.trim().toLowerCase()
    return allShelters.filter((s) => {
      const matchSearch = !q || s.name.toLowerCase().includes(q) || s.city.toLowerCase().includes(q)
      const matchCity = !cityFilter || s.city === cityFilter
      const matchCountry = !countryFilter || (s.country ?? '') === countryFilter
      return matchSearch && matchCity && matchCountry
    })
  }, [search, cityFilter, countryFilter])

  const totalCount = filtered.length
  const paged = filtered.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE)

  const topCities = useMemo(
    () =>
      uniqueCities
        .map((city) => ({
          city,
          count: (countryFilter
            ? allShelters.filter((s) => (s.country ?? '') === countryFilter)
            : allShelters
          ).filter((s) => s.city === city).length,
        }))
        .sort((a, b) => b.count - a.count)
        .slice(0, 8)
        .map((c) => c.city),
    [uniqueCities, countryFilter],
  )

  const handleSearch = (v: string) => { setSearch(v); setPage(1) }
  const handleCity = (city: string) => { setCityFilter(city === cityFilter ? '' : city); setPage(1) }
  const handleCountryClick = (code: string) => {
    if (countryFilter === code) {
      setCountryFilter(''); setCityFilter(''); setPage(1)
    } else {
      navigate(`/shelters/${code.toLowerCase()}`)
    }
  }

  return (
    <Box sx={{ bgcolor: 'background.default' }}>
      <PageMeta
        title={t('shelters.pageTitle')}
        description={t('shelters.metaDesc')}
        path="/shelters"
      />
      <script type="application/ld+json" dangerouslySetInnerHTML={{ __html: safeJsonLd({
        '@context': 'https://schema.org',
        '@type': 'BreadcrumbList',
        itemListElement: [
          { '@type': 'ListItem', position: 1, name: t('nav.home'), item: 'https://getpetzone.com/uk' },
          { '@type': 'ListItem', position: 2, name: t('shelters.pageTitle'), item: 'https://getpetzone.com/uk/shelters' },
        ],
      }) }} />
      <script type="application/ld+json" dangerouslySetInnerHTML={{ __html: safeJsonLd({
        '@context': 'https://schema.org',
        '@type': 'ItemList',
        name: t('shelters.pageTitle'),
        description: t('shelters.metaDesc'),
        numberOfItems: allShelters.length,
        url: 'https://getpetzone.com/uk/shelters',
      }) }} />

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
              {t('shelters.badge')}
            </Typography>
          </Box>

          <Typography
            variant="h1"
            fontWeight="bold"
            sx={{ mb: 2, fontSize: { xs: '1.8rem', md: '2.8rem' }, lineHeight: 1.2 }}
          >
            {t('shelters.hero.title')}
          </Typography>

          <Typography
            variant="h6"
            sx={{ opacity: 0.8, fontWeight: 400, mb: 5, fontSize: { xs: '1rem', md: '1.1rem' }, maxWidth: 640, mx: 'auto' }}
          >
            {t('shelters.hero.subtitle')}
          </Typography>

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
                {allShelters.length}
              </Typography>
              <Typography variant="caption" sx={{ opacity: 0.7 }}>{t('shelters.stats.orgs')}</Typography>
            </Box>
            <Divider orientation="vertical" flexItem sx={{ borderColor: 'rgba(255,255,255,0.2)' }} />
            <Box sx={{ textAlign: 'center' }}>
              <Typography variant="h5" fontWeight="bold" sx={{ color: '#60A5FA', lineHeight: 1 }}>
                {countries.length}
              </Typography>
              <Typography variant="caption" sx={{ opacity: 0.7 }}>країн</Typography>
            </Box>
            <Divider orientation="vertical" flexItem sx={{ borderColor: 'rgba(255,255,255,0.2)' }} />
            <Box sx={{ textAlign: 'center' }}>
              <Typography variant="h5" fontWeight="bold" sx={{ color: '#34D399', lineHeight: 1 }}>
                {t('shelters.stats.source')}
              </Typography>
              <Typography variant="caption" sx={{ opacity: 0.7 }}>{t('shelters.attribution')}</Typography>
            </Box>
          </Box>
        </Container>
      </Box>

      <Container maxWidth="xl" sx={{ py: { xs: 4, md: 6 } }}>
        {/* ── COUNTRY CHIPS ──────────────────────────────────── */}
        <Box sx={{ display: 'flex', gap: 0.75, flexWrap: 'wrap', mb: 2 }}>
          <Chip
            label={t('shelters.allCountries')}
            size="small"
            onClick={() => { setCountryFilter(''); setCityFilter(''); setPage(1) }}
            variant={!countryFilter ? 'filled' : 'outlined'}
            sx={{
              cursor: 'pointer',
              bgcolor: !countryFilter ? CORAL : 'transparent',
              color: !countryFilter ? 'white' : 'text.secondary',
              borderColor: !countryFilter ? CORAL : '#E5E7EB',
              fontWeight: !countryFilter ? 600 : 400,
              '&:hover': { bgcolor: !countryFilter ? '#e55555' : '#FFF0F0', borderColor: CORAL, color: !countryFilter ? 'white' : CORAL },
            }}
          />
          {countries.map(({ code, meta }) => (
            <Chip
              key={code}
              label={`${meta.flag} ${meta.name['uk']}`}
              size="small"
              onClick={() => handleCountryClick(code)}
              variant="outlined"
              sx={{
                cursor: 'pointer',
                bgcolor: 'transparent',
                color: 'text.secondary',
                borderColor: '#E5E7EB',
                '&:hover': { bgcolor: '#FFF0F0', borderColor: CORAL, color: CORAL },
              }}
            />
          ))}
        </Box>

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
                <ShelterCard shelter={shelter} />
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

        {/* ── CTA ────────────────────────────────────────────── */}
        <Box
          sx={{
            mt: 8,
            textAlign: 'center',
            border: '1px solid #E5E7EB',
            borderRadius: 4,
            p: { xs: 3, md: 5 },
            bgcolor: 'background.paper',
          }}
        >
          <Typography variant="h6" fontWeight="bold" sx={{ mb: 1 }}>
            {t('shelters.cta.title')}
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 3, maxWidth: 480, mx: 'auto' }}>
            {t('shelters.cta.subtitle')}
          </Typography>
          <Button
            variant="contained"
            onClick={() => navigate('/register/volunteer')}
            sx={{
              bgcolor: CORAL,
              '&:hover': { bgcolor: '#e55555' },
              textTransform: 'none',
              fontWeight: 600,
              borderRadius: 2,
              px: 3,
            }}
          >
            {t('shelters.cta.btn')}
          </Button>
        </Box>
      </Container>
    </Box>
  )
}
