import { useState, useMemo } from 'react'
import { useTranslation } from 'react-i18next'
import { useLangNavigate } from '../hooks/useLangNavigate'
import PageMeta from '../components/meta/PageMeta'
import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Typography from '@mui/material/Typography'
import Grid from '@mui/material/Grid'
import Card from '@mui/material/Card'
import CardContent from '@mui/material/CardContent'
import CardActions from '@mui/material/CardActions'
import Avatar from '@mui/material/Avatar'
import Button from '@mui/material/Button'
import TextField from '@mui/material/TextField'
import Chip from '@mui/material/Chip'
import IconButton from '@mui/material/IconButton'
import InputAdornment from '@mui/material/InputAdornment'
import Divider from '@mui/material/Divider'
import Tooltip from '@mui/material/Tooltip'
import SearchIcon from '@mui/icons-material/Search'
import LocationOnIcon from '@mui/icons-material/LocationOn'
import PhoneIcon from '@mui/icons-material/Phone'
import EmailIcon from '@mui/icons-material/Email'
import LanguageIcon from '@mui/icons-material/Language'
import PetsIcon from '@mui/icons-material/Pets'
import OpenInNewIcon from '@mui/icons-material/OpenInNew'
import ClearIcon from '@mui/icons-material/Clear'
import FacebookIcon from '@mui/icons-material/Facebook'
import InstagramIcon from '@mui/icons-material/Instagram'
import Pagination from '../components/ui/Pagination'
import sheltersData from '../data/shelters.json'

const CORAL = '#FF6B6B'
const DARK = '#1e1b4b'
const PAGE_SIZE = 24

interface Shelter {
  url: string
  name: string
  city: string
  photo: string | null
  phone: string | null
  email: string | null
  facebook: string | null
  instagram: string | null
  website: string | null
  description: string | null
}

const allShelters = sheltersData as Shelter[]
const uniqueCities = [...new Set(allShelters.map((s) => s.city).filter(Boolean))].sort()

function ShelterCard({ shelter }: { shelter: Shelter }) {
  const { t } = useTranslation()
  const initials = shelter.name
    .split(/[\s,]+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((w) => w[0])
    .join('')
    .toUpperCase()

  return (
    <Card
      elevation={0}
      sx={{
        border: '1px solid #E5E7EB',
        borderRadius: 3,
        height: '100%',
        display: 'flex',
        flexDirection: 'column',
        transition: 'box-shadow 0.2s, transform 0.2s',
        '&:hover': {
          boxShadow: '0 8px 24px rgba(255,107,107,0.12)',
          transform: 'translateY(-2px)',
        },
      }}
    >
      <CardContent sx={{ flex: 1, pb: 1 }}>
        {/* Avatar + name */}
        <Box sx={{ display: 'flex', gap: 2, alignItems: 'center', mb: 2 }}>
          <Avatar
            src={shelter.photo ?? undefined}
            imgProps={{ referrerPolicy: 'no-referrer' }}
            sx={{ width: 56, height: 56, bgcolor: '#FFF0F0', color: CORAL, fontSize: 18, fontWeight: 700, flexShrink: 0 }}
          >
            {!shelter.photo && initials}
          </Avatar>
          <Box sx={{ minWidth: 0 }}>
            <Typography
              variant="subtitle1"
              fontWeight="bold"
              sx={{ lineHeight: 1.3, wordBreak: 'break-word', fontSize: '0.9rem' }}
            >
              {shelter.name}
            </Typography>
            {shelter.city && (
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.4, mt: 0.25, color: '#6B7280' }}>
                <LocationOnIcon sx={{ fontSize: 13 }} />
                <Typography variant="caption">{shelter.city}</Typography>
              </Box>
            )}
          </Box>
        </Box>

        {/* Description */}
        {shelter.description && (
          <Typography
            variant="body2"
            color="text.secondary"
            sx={{
              mb: 1.5,
              overflow: 'hidden',
              display: '-webkit-box',
              WebkitLineClamp: 2,
              WebkitBoxOrient: 'vertical',
              lineHeight: 1.55,
              fontSize: '0.8rem',
            }}
          >
            {shelter.description}
          </Typography>
        )}

        <Divider sx={{ mb: 1.5 }} />

        {/* Contacts */}
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.6 }}>
          {shelter.phone && (
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.75 }}>
              <PhoneIcon sx={{ fontSize: 14, color: '#9CA3AF', flexShrink: 0 }} />
              <Typography
                variant="caption"
                component="a"
                href={`tel:${shelter.phone}`}
                sx={{ color: '#374151', textDecoration: 'none', '&:hover': { color: CORAL } }}
              >
                {shelter.phone}
              </Typography>
            </Box>
          )}
          {shelter.email && (
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.75 }}>
              <EmailIcon sx={{ fontSize: 14, color: '#9CA3AF', flexShrink: 0 }} />
              <Typography
                variant="caption"
                component="a"
                href={`mailto:${shelter.email}`}
                sx={{
                  color: CORAL,
                  textDecoration: 'none',
                  '&:hover': { textDecoration: 'underline' },
                  overflow: 'hidden',
                  textOverflow: 'ellipsis',
                  whiteSpace: 'nowrap',
                }}
              >
                {shelter.email}
              </Typography>
            </Box>
          )}
        </Box>

        {/* Social icons */}
        {(shelter.facebook || shelter.instagram || shelter.website) && (
          <Box sx={{ display: 'flex', gap: 0.5, mt: 1.5 }}>
            {shelter.facebook && (
              <Tooltip title={t('shelters.facebook')}>
                <IconButton
                  size="small"
                  component="a"
                  href={shelter.facebook}
                  target="_blank"
                  rel="noopener noreferrer"
                  sx={{ color: '#1877F2', p: 0.5 }}
                >
                  <FacebookIcon sx={{ fontSize: 20 }} />
                </IconButton>
              </Tooltip>
            )}
            {shelter.instagram && (
              <Tooltip title={t('shelters.instagram')}>
                <IconButton
                  size="small"
                  component="a"
                  href={shelter.instagram}
                  target="_blank"
                  rel="noopener noreferrer"
                  sx={{ color: '#E1306C', p: 0.5 }}
                >
                  <InstagramIcon sx={{ fontSize: 20 }} />
                </IconButton>
              </Tooltip>
            )}
            {shelter.website && (
              <Tooltip title={t('shelters.website')}>
                <IconButton
                  size="small"
                  component="a"
                  href={shelter.website}
                  target="_blank"
                  rel="noopener noreferrer"
                  sx={{ color: '#6B7280', p: 0.5 }}
                >
                  <LanguageIcon sx={{ fontSize: 20 }} />
                </IconButton>
              </Tooltip>
            )}
          </Box>
        )}
      </CardContent>

      <CardActions sx={{ px: 2, pb: 2, pt: 0 }}>
        <Button
          variant="outlined"
          size="small"
          fullWidth
          endIcon={<OpenInNewIcon sx={{ fontSize: '14px !important' }} />}
          component="a"
          href={shelter.url}
          target="_blank"
          rel="noopener noreferrer"
          sx={{
            borderColor: '#E5E7EB',
            color: '#6B7280',
            borderRadius: 2,
            textTransform: 'none',
            fontWeight: 500,
            fontSize: 12,
            '&:hover': { borderColor: CORAL, color: CORAL, bgcolor: '#FFF0F0' },
          }}
        >
          {t('shelters.viewOnHappyPaw')}
        </Button>
      </CardActions>
    </Card>
  )
}

export default function SheltersPage() {
  const { t } = useTranslation()
  const navigate = useLangNavigate()
  const [search, setSearch] = useState('')
  const [cityFilter, setCityFilter] = useState<string>('')
  const [page, setPage] = useState(1)

  const filtered = useMemo(() => {
    const q = search.trim().toLowerCase()
    return allShelters.filter((s) => {
      const matchSearch = !q || s.name.toLowerCase().includes(q) || s.city.toLowerCase().includes(q)
      const matchCity = !cityFilter || s.city === cityFilter
      return matchSearch && matchCity
    })
  }, [search, cityFilter])

  const totalCount = filtered.length
  const paged = filtered.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE)

  const handleSearch = (v: string) => {
    setSearch(v)
    setPage(1)
  }

  const handleCity = (city: string) => {
    setCityFilter(city === cityFilter ? '' : city)
    setPage(1)
  }

  const topCities = useMemo(
    () =>
      uniqueCities
        .map((city) => ({ city, count: allShelters.filter((s) => s.city === city).length }))
        .sort((a, b) => b.count - a.count)
        .slice(0, 10)
        .map((c) => c.city),
    [],
  )

  const uniqueCitiesCount = uniqueCities.length

  return (
    <Box sx={{ bgcolor: 'background.default' }}>
      <PageMeta
        title={t('shelters.pageTitle')}
        description={t('shelters.metaDesc')}
        path="/shelters"
      />

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
                {allShelters.length}
              </Typography>
              <Typography variant="caption" sx={{ opacity: 0.7 }}>{t('shelters.stats.orgs')}</Typography>
            </Box>
            <Divider orientation="vertical" flexItem sx={{ borderColor: 'rgba(255,255,255,0.2)' }} />
            <Box sx={{ textAlign: 'center' }}>
              <Typography variant="h5" fontWeight="bold" sx={{ color: '#60A5FA', lineHeight: 1 }}>
                {uniqueCitiesCount}
              </Typography>
              <Typography variant="caption" sx={{ opacity: 0.7 }}>{t('shelters.stats.cities')}</Typography>
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
        {/* ── SEARCH & FILTERS ───────────────────────────────── */}
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

          {/* Top cities */}
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
                  '&:hover': {
                    bgcolor: cityFilter === city ? '#e55555' : '#FFF0F0',
                    borderColor: CORAL,
                    color: cityFilter === city ? 'white' : CORAL,
                  },
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

        {/* ── PAGINATION ─────────────────────────────────────── */}
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
