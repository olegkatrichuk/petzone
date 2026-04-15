import { useState } from 'react'
import { useParams } from 'react-router-dom'
import { useLangNavigate } from '../hooks/useLangNavigate'
import PageMeta from '../components/meta/PageMeta'
import { useTranslation } from 'react-i18next'
import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Typography from '@mui/material/Typography'
import Button from '@mui/material/Button'
import Tabs from '@mui/material/Tabs'
import Tab from '@mui/material/Tab'
import Grid from '@mui/material/Grid'
import Skeleton from '@mui/material/Skeleton'
import Alert from '@mui/material/Alert'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import { useGetPetsQuery } from '../services/petsApi'
import { PetStatus } from '../types/pet'
import PetCard from '../components/pets/PetCard'
import Pagination from '../components/ui/Pagination'

const CORAL = '#FF6B6B'
const PAGE_SIZE = 12

const STATUS_TABS = [undefined, PetStatus.LookingForHome, PetStatus.NeedsHelp] as const

export default function VolunteerAnimalsPage() {
  const { t } = useTranslation()
  const { volunteerId } = useParams<{ volunteerId: string }>()
  const navigate = useLangNavigate()
  const [tab, setTab] = useState(0)
  const [page, setPage] = useState(1)

  const status = STATUS_TABS[tab]

  const { data, isLoading, isError, refetch } = useGetPetsQuery(
    { page, pageSize: PAGE_SIZE, volunteerId: volunteerId ?? '', ...(status !== undefined ? { status } : {}) },
    { skip: !volunteerId },
  )

  const handleTabChange = (_: React.SyntheticEvent, newTab: number) => {
    setTab(newTab)
    setPage(1)
  }

  const handlePageChange = (newPage: number) => {
    setPage(newPage)
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <PageMeta title={t('volunteerAnimals.title')} description={t('volunteerAnimals.title')} path={`/animals/${volunteerId}`} />
      <Button
        startIcon={<ArrowBackIcon />}
        onClick={() => navigate(`/volunteers/${volunteerId}`)}
        sx={{ mb: 3, color: '#6B7280', textTransform: 'none' }}
      >
        {t('volunteerAnimals.backToProfile')}
      </Button>

      <Typography variant="h4" fontWeight="bold" sx={{ mb: 3 }}>
        {t('volunteerAnimals.title')}
      </Typography>

      <Tabs
        value={tab}
        onChange={handleTabChange}
        TabIndicatorProps={{ style: { backgroundColor: CORAL } }}
        sx={{ mb: 3, '& .MuiTab-root.Mui-selected': { color: CORAL } }}
      >
        <Tab label={t('volunteerAnimals.all')} />
        <Tab label={t('volunteerAnimals.lookingForHome')} />
        <Tab label={t('volunteerAnimals.needsHelp')} />
      </Tabs>

      {isLoading && (
        <Grid container spacing={3}>
          {Array.from({ length: 6 }).map((_, i) => (
            <Grid size={{ xs: 12, sm: 6, md: 4 }} key={i}>
              <Box sx={{ border: '1px solid #E5E7EB', borderRadius: 3, overflow: 'hidden' }}>
                <Skeleton variant="rectangular" height={200} />
                <Box sx={{ p: 2, display: 'flex', flexDirection: 'column', gap: 1 }}>
                  <Skeleton width="60%" height={22} />
                  <Skeleton width="40%" height={16} />
                  <Box sx={{ display: 'flex', gap: 0.75, mt: 0.5 }}>
                    <Skeleton width={55} height={24} sx={{ borderRadius: 8 }} />
                    <Skeleton width={70} height={24} sx={{ borderRadius: 8 }} />
                  </Box>
                </Box>
              </Box>
            </Grid>
          ))}
        </Grid>
      )}

      {isError && (
        <Alert
          severity="error"
          action={
            <Button size="small" onClick={refetch} sx={{ color: CORAL, textTransform: 'none' }}>
              {t('errors.retry')}
            </Button>
          }
        >
          {t('pets.loadError')}
        </Alert>
      )}

      {!isLoading && !isError && (data?.items.length ?? 0) === 0 && (
        <Box sx={{ textAlign: 'center', py: 10 }}>
          <Typography color="text.secondary">{t('volunteerAnimals.noPets')}</Typography>
        </Box>
      )}

      {!isLoading && (data?.items.length ?? 0) > 0 && (
        <>
          <Grid container spacing={3}>
            {data!.items.map((pet) => (
              <Grid size={{ xs: 12, sm: 6, md: 4 }} key={pet.id}>
                <PetCard pet={pet} />
              </Grid>
            ))}
          </Grid>

          <Pagination
            page={page}
            pageSize={PAGE_SIZE}
            totalCount={data!.totalCount}
            onChange={handlePageChange}
            ofLabel={t('pets.of')}
          />
        </>
      )}
    </Container>
  )
}
