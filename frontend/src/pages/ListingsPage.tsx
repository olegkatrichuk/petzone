import { useState, useCallback, useEffect, useRef } from 'react'
import { useSearchParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import PageMeta from '../components/meta/PageMeta'
import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Typography from '@mui/material/Typography'
import Button from '@mui/material/Button'
import CircularProgress from '@mui/material/CircularProgress'
import Chip from '@mui/material/Chip'
import TextField from '@mui/material/TextField'
import InputAdornment from '@mui/material/InputAdornment'
import Divider from '@mui/material/Divider'
import AddIcon from '@mui/icons-material/Add'
import SearchIcon from '@mui/icons-material/Search'
import LocationOnIcon from '@mui/icons-material/LocationOn'
import PhoneIcon from '@mui/icons-material/Phone'
import EmailIcon from '@mui/icons-material/Email'
import PhotoCameraIcon from '@mui/icons-material/PhotoCamera'
import { useGetListingsQuery } from '../services/listingsApi'
import { useGetSpeciesQuery } from '../services/speciesApi'
import { api } from '../lib/axios'
import type { AdoptionListing } from '../types/listing'
import { useAuthStore } from '../store/authStore'
import { useLangNavigate } from '../hooks/useLangNavigate'

const CORAL = '#FF6B6B'
const PAGE_SIZE = 12

function PhotoThumb({ fileName }: { fileName: string }) {
  const [url, setUrl] = useState<string | null>(null)
  const [visible, setVisible] = useState(false)
  const containerRef = useRef<HTMLDivElement>(null)
  const fetchedRef = useRef(false)

  useEffect(() => {
    const el = containerRef.current
    if (!el) return
    const observer = new IntersectionObserver(
      ([entry]) => { if (entry.isIntersecting) { setVisible(true); observer.disconnect() } },
      { rootMargin: '200px' }
    )
    observer.observe(el)
    return () => observer.disconnect()
  }, [])

  useEffect(() => {
    if (!visible || fetchedRef.current) return
    fetchedRef.current = true
    api.get(`/files/${encodeURIComponent(fileName)}/url`)
      .then(res => {
        const data = res.data
        setUrl(typeof data === 'string' ? data : data?.result ?? null)
      })
      .catch(() => setUrl(null))
  }, [visible, fileName])

  return (
    <Box ref={containerRef} sx={{ height: 180, borderRadius: 2, overflow: 'hidden', bgcolor: '#F3F4F6' }}>
      {url
        ? <Box component="img" src={url} alt="" loading="lazy" sx={{ width: '100%', height: '100%', objectFit: 'cover' }} />
        : visible
          ? <Box sx={{ height: '100%', display: 'flex', alignItems: 'center', justifyContent: 'center' }}><CircularProgress size={24} sx={{ color: CORAL }} /></Box>
          : null
      }
    </Box>
  )
}

function ListingCard({ listing }: { listing: AdoptionListing }) {
  const { t } = useTranslation()
  const navigate = useLangNavigate()

  const ageLabel =
    listing.ageMonths < 1
      ? t('pets.ageLessThanMonth')
      : listing.ageMonths < 12
        ? `${listing.ageMonths} ${t('pets.ageMonths')}`
        : `${Math.floor(listing.ageMonths / 12)} ${t('pets.ageYears')}`

  return (
    <Box
      onClick={() => navigate(`/listings/${listing.id}`)}
      sx={{
        bgcolor: '#fff',
        border: '1px solid #E5E7EB',
        borderRadius: 3,
        p: 2.5,
        display: 'flex',
        flexDirection: 'column',
        gap: 1.5,
        cursor: 'pointer',
        transition: 'box-shadow 0.2s, transform 0.15s',
        '&:hover': { boxShadow: '0 4px 20px rgba(0,0,0,0.10)', transform: 'translateY(-2px)' },
      }}
    >
      {/* Photos preview */}
      {listing.photos.length > 0 && (
        <Box sx={{ position: 'relative' }}>
          <PhotoThumb fileName={listing.photos[0]} />
          {listing.photos.length > 1 && (
            <Chip
              icon={<PhotoCameraIcon sx={{ fontSize: '13px !important' }} />}
              label={listing.photos.length}
              size="small"
              sx={{
                position: 'absolute', bottom: 8, right: 8,
                bgcolor: 'rgba(0,0,0,0.5)', color: 'white', fontSize: 11, height: 22,
                '& .MuiChip-icon': { color: 'white' },
              }}
            />
          )}
        </Box>
      )}

      {/* Title + badge */}
      <Box sx={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', gap: 1 }}>
        <Typography variant="subtitle1" fontWeight={700} sx={{ lineHeight: 1.3 }}>
          {listing.title}
        </Typography>
        <Chip
          label={t('listings.ownerBadge')}
          size="small"
          sx={{ bgcolor: '#FFF0F0', color: CORAL, fontWeight: 600, fontSize: 11, flexShrink: 0 }}
        />
      </Box>

      {/* Tags */}
      <Box sx={{ display: 'flex', gap: 0.75, flexWrap: 'wrap' }}>
        <Chip label={ageLabel} size="small" sx={{ bgcolor: '#F3F4F6', fontSize: 12 }} />
        <Chip label={listing.color} size="small" sx={{ bgcolor: '#F3F4F6', fontSize: 12 }} />
        {listing.vaccinated && (
          <Chip label={t('pets.vaccinated')} size="small" sx={{ bgcolor: '#D1FAE5', color: '#059669', fontSize: 12 }} />
        )}
        {listing.castrated && (
          <Chip label={t('pets.castrated')} size="small" sx={{ bgcolor: '#DBEAFE', color: '#2563EB', fontSize: 12 }} />
        )}
      </Box>

      {/* Description */}
      <Typography
        variant="body2"
        color="text.secondary"
        sx={{ display: '-webkit-box', WebkitLineClamp: 2, WebkitBoxOrient: 'vertical', overflow: 'hidden' }}
      >
        {listing.description}
      </Typography>

      {/* City */}
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, color: '#6B7280' }}>
        <LocationOnIcon sx={{ fontSize: 15 }} />
        <Typography variant="caption">{listing.city}</Typography>
      </Box>

      <Divider />

      {/* Contact */}
      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.5 }}>
        <Typography variant="caption" color="text.secondary" fontWeight={600}>
          {listing.userName}
        </Typography>
        {listing.userPhone && (
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, color: '#374151' }}>
            <PhoneIcon sx={{ fontSize: 13 }} />
            <Typography variant="caption">{listing.userPhone}</Typography>
          </Box>
        )}
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, color: '#374151' }}>
          <EmailIcon sx={{ fontSize: 13 }} />
          <Typography variant="caption">{listing.userEmail}</Typography>
        </Box>
      </Box>
    </Box>
  )
}

export default function ListingsPage() {
  const { t, i18n } = useTranslation()
  const navigate = useLangNavigate()
  const { user } = useAuthStore()
  const [searchParams, setSearchParams] = useSearchParams()
  const [page, setPage] = useState(1)
  const [allListings, setAllListings] = useState<AdoptionListing[]>([])

  const speciesId = searchParams.get('speciesId') ?? undefined
  const city = searchParams.get('city') ?? undefined
  const [cityInput, setCityInput] = useState(city ?? '')

  const { data: listings = [], isLoading, isFetching } = useGetListingsQuery(
    { speciesId, city, page, pageSize: PAGE_SIZE },
  )

  const locale = i18n.language?.slice(0, 2) || 'uk'
  const { data: speciesList = [] } = useGetSpeciesQuery(locale)

  // Reset on filter change
  useEffect(() => {
    setPage(1)
    setAllListings([])
  }, [speciesId, city])

  // Accumulate pages
  useEffect(() => {
    if (!listings.length && page === 1) { setAllListings([]); return }
    setAllListings((prev) => page === 1 ? [...listings] : [...prev, ...listings])
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [listings])

  const hasMore = listings.length === PAGE_SIZE

  const applyCity = useCallback(() => {
    const p = new URLSearchParams(searchParams)
    if (cityInput.trim()) p.set('city', cityInput.trim())
    else p.delete('city')
    setSearchParams(p, { replace: true })
  }, [cityInput, searchParams, setSearchParams])

  const setSpecies = useCallback((sid: string | undefined) => {
    const p = new URLSearchParams(searchParams)
    if (sid) p.set('speciesId', sid)
    else p.delete('speciesId')
    setSearchParams(p, { replace: true })
  }, [searchParams, setSearchParams])

  return (
    <Box sx={{ bgcolor: '#FAFAFA', minHeight: '100%', py: 4 }}>
      <PageMeta
        title={t('listings.pageTitle')}
        description={t('listings.pageDescription')}
        path="/listings"
      />
      <Container maxWidth="xl">

        {/* Header */}
        <Box sx={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', mb: 4, flexWrap: 'wrap', gap: 2 }}>
          <Box>
            <Typography variant="h4" fontWeight="bold" sx={{ color: '#1F2937' }}>
              {t('listings.pageTitle')}
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
              {t('listings.pageDescription')}
            </Typography>
          </Box>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => navigate(user ? '/listings/create' : '/login')}
            sx={{
              bgcolor: CORAL,
              '&:hover': { bgcolor: '#e55555' },
              textTransform: 'none',
              fontWeight: 700,
              borderRadius: 2,
              flexShrink: 0,
            }}
          >
            {t('listings.addBtn')}
          </Button>
        </Box>

        {/* Filters */}
        <Box sx={{ display: 'flex', gap: 2, mb: 4, flexWrap: 'wrap', alignItems: 'center' }}>
          {/* Species chips */}
          <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
            <Chip
              label={t('listings.allSpecies')}
              onClick={() => setSpecies(undefined)}
              color={!speciesId ? 'error' : 'default'}
              variant={!speciesId ? 'filled' : 'outlined'}
              sx={{ fontWeight: !speciesId ? 700 : 400, cursor: 'pointer' }}
            />
            {speciesList.map((s: { id: string; name: string }) => (
              <Chip
                key={s.id}
                label={s.name}
                onClick={() => setSpecies(s.id)}
                color={speciesId === s.id ? 'error' : 'default'}
                variant={speciesId === s.id ? 'filled' : 'outlined'}
                sx={{ fontWeight: speciesId === s.id ? 700 : 400, cursor: 'pointer' }}
              />
            ))}
          </Box>

          {/* City search */}
          <TextField
            size="small"
            placeholder={t('listings.cityFilter')}
            value={cityInput}
            onChange={(e) => setCityInput(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && applyCity()}
            InputProps={{
              startAdornment: (
                <InputAdornment position="start">
                  <SearchIcon sx={{ fontSize: 18, color: '#9CA3AF' }} />
                </InputAdornment>
              ),
            }}
            sx={{ minWidth: 200, bgcolor: 'white', borderRadius: 2 }}
          />
          <Button
            variant="outlined"
            onClick={applyCity}
            sx={{ borderRadius: 2, textTransform: 'none', borderColor: '#D1D5DB', color: '#374151' }}
          >
            {t('listings.applyFilter')}
          </Button>
          {(speciesId || city) && (
            <Button
              variant="text"
              onClick={() => { setCityInput(''); setSearchParams({}, { replace: true }) }}
              sx={{ textTransform: 'none', color: '#9CA3AF' }}
            >
              {t('listings.clearFilter')}
            </Button>
          )}
        </Box>

        {/* Grid */}
        {isLoading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
            <CircularProgress sx={{ color: CORAL }} />
          </Box>
        ) : allListings.length === 0 ? (
          <Box sx={{ textAlign: 'center', py: 10 }}>
            <Typography variant="h6" color="text.secondary" gutterBottom>
              {t('listings.empty')}
            </Typography>
            {user && (
              <Button
                variant="outlined"
                startIcon={<AddIcon />}
                onClick={() => navigate('/listings/create')}
                sx={{ mt: 2, borderColor: CORAL, color: CORAL, textTransform: 'none', borderRadius: 2 }}
              >
                {t('listings.addBtn')}
              </Button>
            )}
          </Box>
        ) : (
          <Box
            sx={{
              display: 'grid',
              gridTemplateColumns: { xs: '1fr', sm: 'repeat(2, 1fr)', md: 'repeat(3, 1fr)', lg: 'repeat(4, 1fr)' },
              gap: 2.5,
            }}
          >
            {allListings.map((l) => (
              <ListingCard key={l.id} listing={l} />
            ))}
          </Box>
        )}

        {/* Load more */}
        {!isLoading && allListings.length > 0 && (
          <Box sx={{ display: 'flex', justifyContent: 'center', mt: 5 }}>
            {hasMore ? (
              <Button
                variant="outlined"
                onClick={() => setPage((p) => p + 1)}
                disabled={isFetching}
                startIcon={isFetching ? <CircularProgress size={16} sx={{ color: 'inherit' }} /> : undefined}
                sx={{
                  borderColor: '#1e1b4b', color: '#1e1b4b', textTransform: 'none',
                  fontWeight: 600, borderRadius: 2, px: 5, py: 1,
                  '&:hover': { bgcolor: '#f0f0ff', borderColor: '#1e1b4b' },
                }}
              >
                {isFetching ? t('pets.loading') : t('pets.loadMore')}
              </Button>
            ) : (
              <Typography variant="body2" color="text.secondary">{t('pets.allLoaded')}</Typography>
            )}
          </Box>
        )}
      </Container>
    </Box>
  )
}
