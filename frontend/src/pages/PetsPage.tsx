import { useCallback, useState, useEffect, useMemo } from 'react'
import { useSearchParams } from "react-router-dom"
import { useTranslation } from 'react-i18next'
import PageMeta from '../components/meta/PageMeta'
import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Typography from '@mui/material/Typography'
import Alert from '@mui/material/Alert'
import Button from '@mui/material/Button'
import IconButton from '@mui/material/IconButton'
import Drawer from '@mui/material/Drawer'
import Chip from '@mui/material/Chip'
import Tooltip from '@mui/material/Tooltip'
import CircularProgress from '@mui/material/CircularProgress'
import Divider from '@mui/material/Divider'
import RefreshIcon from '@mui/icons-material/Refresh'
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutline'
import PhotoCameraIcon from '@mui/icons-material/PhotoCamera'
import FilterAltIcon from '@mui/icons-material/FilterAlt'
import CloseIcon from '@mui/icons-material/Close'
import ArrowForwardIcon from '@mui/icons-material/ArrowForward'
import LocationOnIcon from '@mui/icons-material/LocationOn'
import PhoneIcon from '@mui/icons-material/Phone'
import EmailIcon from '@mui/icons-material/Email'
import SearchIcon from '@mui/icons-material/Search'
import InputAdornment from '@mui/material/InputAdornment'
import TextField from '@mui/material/TextField'
import { useGetPetsQuery, useGetPetByIdQuery } from '../services/petsApi'
import { useGetListingsQuery } from '../services/listingsApi'
import type { PetFilters, Pet } from '../types/pet'
import type { AdoptionListing } from '../types/listing'
import PetFiltersPanel from '../components/pets/PetFilters'
import PetsList from '../components/pets/PetsList'
import PetCard from '../components/pets/PetCard'
import { useRecentlyViewedStore } from '../store/recentlyViewedStore'
import { useLangNavigate } from '../hooks/useLangNavigate'

const CORAL = '#FF6B6B'

function ListingCard({ listing }: { listing: AdoptionListing }) {
  const { t } = useTranslation()
  const navigate = useLangNavigate()
  const ageLabel = listing.ageMonths < 1
    ? t('pets.ageLessThanMonth')
    : listing.ageMonths < 12
      ? `${listing.ageMonths} ${t('pets.ageMonths')}`
      : `${Math.floor(listing.ageMonths / 12)} ${t('pets.ageYears')}`

  return (
    <Box
      onClick={() => navigate(`/listings/${listing.id}`)}
      sx={{
        bgcolor: 'background.paper',
        border: '1px solid #E5E7EB',
        borderRadius: 3,
        p: 2.5,
        display: 'flex',
        flexDirection: 'column',
        gap: 1,
        cursor: 'pointer',
        transition: 'box-shadow 0.2s',
        '&:hover': { boxShadow: '0 4px 16px rgba(0,0,0,0.10)' },
      }}>
      <Box sx={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', gap: 1 }}>
        <Typography variant="subtitle1" fontWeight={700} sx={{ lineHeight: 1.3 }}>
          {listing.title}
        </Typography>
        <Box sx={{ display: 'flex', gap: 0.5, alignItems: 'center', flexShrink: 0 }}>
          {listing.photos.length > 0 && (
            <Chip
              icon={<PhotoCameraIcon sx={{ fontSize: '14px !important' }} />}
              label={listing.photos.length}
              size="small"
              sx={{ bgcolor: 'action.hover', fontSize: 11, height: 22 }}
            />
          )}
          <Chip
            label={t('listings.ownerBadge')}
            size="small"
            sx={{ bgcolor: '#FFF0F0', color: CORAL, fontWeight: 600, fontSize: 11 }}
          />
        </Box>
      </Box>

      <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
        <Chip label={ageLabel} size="small" sx={{ bgcolor: 'action.hover' }} />
        <Chip label={listing.color} size="small" sx={{ bgcolor: 'action.hover' }} />
        {listing.vaccinated && <Chip label={t('pets.vaccinated')} size="small" sx={{ bgcolor: '#D1FAE5', color: '#059669' }} />}
        {listing.castrated && <Chip label={t('pets.castrated')} size="small" sx={{ bgcolor: '#DBEAFE', color: '#2563EB' }} />}
      </Box>

      <Typography variant="body2" color="text.secondary" sx={{
        display: '-webkit-box', WebkitLineClamp: 2, WebkitBoxOrient: 'vertical', overflow: 'hidden',
      }}>
        {listing.description}
      </Typography>

      <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, color: '#6B7280' }}>
        <LocationOnIcon sx={{ fontSize: 16 }} />
        <Typography variant="caption">{listing.city}</Typography>
      </Box>

      <Divider />

      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.5 }}>
        <Typography variant="caption" color="text.secondary" fontWeight={600}>{listing.userName}</Typography>
        {listing.userPhone && (
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, color: '#374151' }}>
            <PhoneIcon sx={{ fontSize: 14 }} />
            <Typography variant="caption">{listing.userPhone}</Typography>
          </Box>
        )}
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, color: '#374151' }}>
          <EmailIcon sx={{ fontSize: 14 }} />
          <Typography variant="caption">{listing.userEmail}</Typography>
        </Box>
      </Box>
    </Box>
  )
}

function ListingsTeaser({ speciesId }: { speciesId?: string }) {
  const { t } = useTranslation()
  const navigate = useLangNavigate()
  const { data: listingsData, isLoading } = useGetListingsQuery(
    { speciesId, pageSize: 3 },
    { skip: false }
  )
  const listings = listingsData?.items ?? []

  if (isLoading || listings.length === 0) return null

  return (
    <Box sx={{ mt: 6 }}>
      <Divider sx={{ mb: 4 }} />
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 3 }}>
        <Box>
          <Typography variant="h5" fontWeight="bold" sx={{ color: '#1F2937' }}>
            {t('listings.sectionTitle')}
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
            {t('listings.sectionSubtitle')}
          </Typography>
        </Box>
        <Button
          variant="outlined"
          endIcon={<ArrowForwardIcon />}
          onClick={() => navigate('/listings')}
          sx={{ borderColor: CORAL, color: CORAL, textTransform: 'none', fontWeight: 600, borderRadius: 2, flexShrink: 0 }}
        >
          {t('listings.showAll')}
        </Button>
      </Box>
      <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: 'repeat(2, 1fr)', md: 'repeat(3, 1fr)' }, gap: 2 }}>
        {listings.map((l) => <ListingCard key={l.id} listing={l} />)}
      </Box>
    </Box>
  )
}

// Loads one pet and renders it in the recently viewed strip
function RecentPetCard({ petId }: { petId: string }) {
  const { data } = useGetPetByIdQuery(petId)
  if (!data) return null
  return (
    <Box sx={{ minWidth: { xs: 220, sm: 260 }, maxWidth: { xs: 220, sm: 260 }, flexShrink: 0 }}>
      <PetCard pet={data as Pet} />
    </Box>
  )
}

function RecentlyViewedSection() {
  const { t } = useTranslation()
  const { ids, clear } = useRecentlyViewedStore()

  if (ids.length === 0) return null

  return (
    <Box sx={{ mb: 4 }}>
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 1.5 }}>
        <Typography variant="h6" fontWeight="bold">
          {t('pets.recentlyViewed')}
        </Typography>
        <Tooltip title={t('pets.clearRecent')}>
          <IconButton size="small" onClick={clear} sx={{ color: '#9CA3AF', '&:hover': { color: '#FF6B6B' } }}>
            <DeleteOutlineIcon fontSize="small" />
          </IconButton>
        </Tooltip>
      </Box>
      <Box
        sx={{
          display: 'flex',
          gap: 2,
          overflowX: 'auto',
          pb: 1,
          scrollbarWidth: 'none',
          '&::-webkit-scrollbar': { display: 'none' },
        }}
      >
        {ids.map((id) => <RecentPetCard key={id} petId={id} />)}
      </Box>
    </Box>
  )
}

const PAGE_SIZE = 9

function paramsToFilters(params: URLSearchParams): Omit<PetFilters, 'page'> {
  const num = (key: string) => { const v = params.get(key); return v !== null && v !== '' ? Number(v) : undefined }
  const bool = (key: string) => { const v = params.get(key); return v === 'true' ? true : v === 'false' ? false : undefined }
  return {
    pageSize: PAGE_SIZE,
    nickname: params.get('nickname') ?? undefined,
    color: params.get('color') ?? undefined,
    city: params.get('city') ?? undefined,
    speciesId: params.get('speciesId') ?? undefined,
    breedId: params.get('breedId') ?? undefined,
    minAge: num('minAge'),
    maxAge: num('maxAge'),
    minWeight: num('minWeight'),
    maxWeight: num('maxWeight'),
    isCastrated: bool('isCastrated'),
    isVaccinated: bool('isVaccinated'),
    status: num('status'),
    sortBy: params.get('sortBy') ?? undefined,
    sortDescending: bool('sortDescending') ?? false,
  }
}

function filtersToParams(filters: Omit<PetFilters, 'page'>): Record<string, string> {
  const p: Record<string, string> = {}
  if (filters.nickname) p.nickname = filters.nickname
  if (filters.color) p.color = filters.color
  if (filters.city) p.city = filters.city
  if (filters.speciesId) p.speciesId = filters.speciesId
  if (filters.breedId) p.breedId = filters.breedId
  if (filters.minAge !== undefined) p.minAge = String(filters.minAge)
  if (filters.maxAge !== undefined) p.maxAge = String(filters.maxAge)
  if (filters.minWeight !== undefined) p.minWeight = String(filters.minWeight)
  if (filters.maxWeight !== undefined) p.maxWeight = String(filters.maxWeight)
  if (filters.isCastrated !== undefined) p.isCastrated = String(filters.isCastrated)
  if (filters.isVaccinated !== undefined) p.isVaccinated = String(filters.isVaccinated)
  if (filters.status !== undefined) p.status = String(filters.status)
  if (filters.sortBy) p.sortBy = filters.sortBy
  if (filters.sortDescending) p.sortDescending = 'true'
  return p
}

export default function PetsPage() {
  const { t } = useTranslation()
  const [searchParams, setSearchParams] = useSearchParams()
  const [drawerOpen, setDrawerOpen] = useState(false)
  const [page, setPage] = useState(1)
  const [allItems, setAllItems] = useState<Pet[]>([])
  const [searchInput, setSearchInput] = useState(() => new URLSearchParams(window.location.search).get('nickname') ?? '')

  const baseFilters = useMemo(() => paramsToFilters(searchParams), [searchParams])

  // Stringify of filters for detecting changes (reset page when filters change)
  const filterKey = useMemo(() => new URLSearchParams(filtersToParams(baseFilters)).toString(), [baseFilters])

  useEffect(() => {
    setPage(1)
    setAllItems([])
  }, [filterKey])

  // Sync search input when URL changes externally (e.g. filter reset)
  useEffect(() => {
    setSearchInput(baseFilters.nickname ?? '')
  }, [baseFilters.nickname])

  // Debounce: update URL nickname param 400ms after user stops typing
  useEffect(() => {
    const timer = setTimeout(() => {
      const current = searchParams.get('nickname') ?? ''
      if (searchInput === current) return
      const newParams = filtersToParams(baseFilters)
      if (searchInput) newParams.nickname = searchInput
      else delete newParams.nickname
      setSearchParams(newParams, { replace: true })
    }, 400)
    return () => clearTimeout(timer)
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [searchInput])

  const filters: PetFilters = { ...baseFilters, page }
  const { data, isLoading, isFetching, isError, refetch } = useGetPetsQuery(filters)

  // Accumulate items across pages
  useEffect(() => {
    if (!data?.items) return
    setAllItems((prev) => page === 1 ? [...data.items] : [...prev, ...data.items])
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [data])

  const hasMore = allItems.length < (data?.totalCount ?? 0)

  const handleApply = useCallback((newFilters: PetFilters) => {
    setSearchParams(filtersToParams(newFilters), { replace: true })
  }, [setSearchParams])

  const handleApplyMobile = useCallback((newFilters: PetFilters) => {
    setSearchParams(filtersToParams(newFilters), { replace: true })
    setDrawerOpen(false)
  }, [setSearchParams])

  return (
    <Box sx={{ bgcolor: 'background.default', minHeight: '100%', py: 4 }}>
      <PageMeta title={t('pets.pageTitle')} description={t('pets.pageTitle')} path="/pets" />
      <Container maxWidth="xl">
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 4 }}>
          <Typography variant="h4" fontWeight="bold" sx={{ color: '#1F2937' }}>
            {t('pets.pageTitle')}
          </Typography>
          {/* Filter button — mobile only */}
          <Button
            variant="outlined"
            startIcon={<FilterAltIcon />}
            onClick={() => setDrawerOpen(true)}
            sx={{
              display: { xs: 'flex', lg: 'none' },
              borderColor: '#1e1b4b',
              color: '#1e1b4b',
              textTransform: 'none',
              fontWeight: 600,
              borderRadius: 2,
              '&:hover': { bgcolor: '#f0f0ff', borderColor: '#1e1b4b' },
            }}
          >
            {t('pets.filters')}
          </Button>
        </Box>

        {isError && (
          <Alert
            severity="error"
            sx={{ mb: 3 }}
            action={
              <Button color="inherit" size="small" startIcon={<RefreshIcon />} onClick={() => refetch()}>
                {t('errors.retry')}
              </Button>
            }
          >
            {t('pets.loadError')}
          </Alert>
        )}

        <RecentlyViewedSection />

        <Box sx={{ display: 'flex', gap: 4, alignItems: 'flex-start' }}>
          {/* Desktop sidebar filters */}
          <Box sx={{ width: 280, flexShrink: 0, display: { xs: 'none', lg: 'block' } }}>
            <PetFiltersPanel initialFilters={filters} onApply={handleApply} />
          </Box>

          <Box sx={{ flex: 1, minWidth: 0 }}>
            <TextField
              fullWidth
              size="small"
              placeholder={t('pets.searchByName')}
              value={searchInput}
              onChange={(e) => setSearchInput(e.target.value)}
              slotProps={{
                input: {
                  startAdornment: (
                    <InputAdornment position="start">
                      <SearchIcon sx={{ color: '#9CA3AF', fontSize: 20 }} />
                    </InputAdornment>
                  ),
                },
              }}
              sx={{ mb: 3, '& .MuiOutlinedInput-root': { borderRadius: 2 } }}
            />
            <PetsList pets={allItems} isLoading={isLoading} isFetching={false} />

            {/* Load more */}
            {!isLoading && (
              <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}>
                {hasMore ? (
                  <Button
                    variant="outlined"
                    onClick={() => setPage((p) => p + 1)}
                    disabled={isFetching}
                    startIcon={isFetching ? <CircularProgress size={16} sx={{ color: 'inherit' }} /> : undefined}
                    sx={{
                      borderColor: '#1e1b4b',
                      color: '#1e1b4b',
                      textTransform: 'none',
                      fontWeight: 600,
                      borderRadius: 2,
                      px: 4,
                      py: 1,
                      '&:hover': { bgcolor: '#f0f0ff', borderColor: '#1e1b4b' },
                    }}
                  >
                    {isFetching ? t('pets.loading') : t('pets.loadMore')}
                  </Button>
                ) : allItems.length > 0 ? (
                  <Typography variant="body2" color="text.secondary">{t('pets.allLoaded')}</Typography>
                ) : null}
              </Box>
            )}
          </Box>
        </Box>

        <ListingsTeaser speciesId={baseFilters.speciesId} />
      </Container>

      {/* Mobile filters drawer */}
      <Drawer
        anchor="left"
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
        PaperProps={{ sx: { width: { xs: '100%', sm: 340 }, p: 2 } }}
      >
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 1 }}>
          <Typography variant="h6" fontWeight="bold">{t('pets.filters')}</Typography>
          <IconButton onClick={() => setDrawerOpen(false)}>
            <CloseIcon />
          </IconButton>
        </Box>
        <PetFiltersPanel initialFilters={filters} onApply={handleApplyMobile} />
      </Drawer>
    </Box>
  )
}
