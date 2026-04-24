import { useCallback, useState, useEffect, useMemo } from 'react'
import { useSearchParams, useParams } from "react-router-dom"
import { useTranslation } from 'react-i18next'
import PageMeta from '../components/meta/PageMeta'
import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Typography from '@mui/material/Typography'
import Alert from '@mui/material/Alert'
import Button from '@mui/material/Button'
import IconButton from '@mui/material/IconButton'
import Drawer from '@mui/material/Drawer'
import Tooltip from '@mui/material/Tooltip'
import CircularProgress from '@mui/material/CircularProgress'
import RefreshIcon from '@mui/icons-material/Refresh'
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutline'
import FilterAltIcon from '@mui/icons-material/FilterAlt'
import CloseIcon from '@mui/icons-material/Close'
import AddIcon from '@mui/icons-material/Add'
import SearchIcon from '@mui/icons-material/Search'
import InputAdornment from '@mui/material/InputAdornment'
import TextField from '@mui/material/TextField'
import { useGetPetsQuery, useGetPetByIdQuery } from '../services/petsApi'
import type { PetFilters, Pet } from '../types/pet'
import PetFiltersPanel from '../components/pets/PetFilters'
import PetsList from '../components/pets/PetsList'
import PetCard from '../components/pets/PetCard'
import { useRecentlyViewedStore } from '../store/recentlyViewedStore'
import { useLangNavigate } from '../hooks/useLangNavigate'
import { useAuthStore } from '../store/authStore'
import { useGeoSource } from '../hooks/useGeoSource'

const CORAL = '#FF6B6B'

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
  const navigate = useLangNavigate()
  const { accessToken } = useAuthStore()
  const [searchParams, setSearchParams] = useSearchParams()
  const [drawerOpen, setDrawerOpen] = useState(false)
  const [page, setPage] = useState(1)
  const [allItems, setAllItems] = useState<Pet[]>([])
  const [searchInput, setSearchInput] = useState(() => new URLSearchParams(window.location.search).get('nickname') ?? '')

  const geoSource = useGeoSource()
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

  const filters: PetFilters = { ...baseFilters, page, source: geoSource }
  const { data, isLoading, isFetching, isError, refetch } = useGetPetsQuery(filters, { refetchOnMountOrArgChange: true })

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

  const { lang } = useParams<{ lang: string }>()
  const currentLang = lang ?? 'uk'
  const SITE_URL = 'https://getpetzone.com'

  const breadcrumbLd = {
    '@context': 'https://schema.org',
    '@type': 'BreadcrumbList',
    itemListElement: [
      { '@type': 'ListItem', position: 1, name: t('nav.home'), item: `${SITE_URL}/${currentLang}` },
      { '@type': 'ListItem', position: 2, name: t('pets.pageTitle'), item: `${SITE_URL}/${currentLang}/pets` },
    ],
  }

  const itemListLd = allItems.length > 0 ? {
    '@context': 'https://schema.org',
    '@type': 'ItemList',
    name: t('pets.pageTitle'),
    numberOfItems: data?.totalCount ?? allItems.length,
    itemListElement: allItems.slice(0, 10).map((pet, i) => ({
      '@type': 'ListItem',
      position: i + 1,
      name: pet.nickname,
      url: `${SITE_URL}/${currentLang}/pets/${pet.id}`,
    })),
  } : null

  return (
    <Box sx={{ bgcolor: 'background.default', minHeight: '100%', py: 4 }}>
      <PageMeta title={t('pets.pageTitle')} description={t('pets.metaDesc')} path="/pets" />
      <script type="application/ld+json" dangerouslySetInnerHTML={{ __html: JSON.stringify(breadcrumbLd) }} />
      {itemListLd && (
        <script type="application/ld+json" dangerouslySetInnerHTML={{ __html: JSON.stringify(itemListLd) }} />
      )}
      <Container maxWidth="xl">
        <Box sx={{ mb: 4 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 1 }}>
            <Typography variant="h1" fontSize="2rem" fontWeight="bold" sx={{ color: '#1F2937' }}>
              {t('pets.pageTitle')}
            </Typography>
            <Box sx={{ display: 'flex', gap: 1.5 }}>
            {accessToken && (
              <Button
                variant="contained"
                startIcon={<AddIcon />}
                onClick={() => navigate('/listings/create')}
                sx={{
                  bgcolor: CORAL,
                  '&:hover': { bgcolor: '#e55555' },
                  textTransform: 'none',
                  fontWeight: 600,
                  borderRadius: 2,
                }}
              >
                {t('listings.addPet')}
              </Button>
            )}
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
          </Box>
          <Typography variant="body2" color="text.secondary" sx={{ mt: 1, maxWidth: 720 }}>
            {t('pets.seoText')}
          </Typography>
        </Box>

        {isError && (
          <Alert
            severity="error"
            sx={{ mb: 3 }}
            action={
              <Button color="inherit" startIcon={<RefreshIcon />} onClick={() => refetch()} sx={{ minHeight: 44 }}>
                {t('errors.retry')}
              </Button>
            }
          >
            {t('pets.loadError')}
          </Alert>
        )}

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

        <RecentlyViewedSection />

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
