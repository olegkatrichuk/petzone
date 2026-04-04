import { useTranslation } from 'react-i18next'
import { useLangNavigate } from '../hooks/useLangNavigate'
import PageMeta from '../components/meta/PageMeta'
import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Typography from '@mui/material/Typography'
import Button from '@mui/material/Button'
import Paper from '@mui/material/Paper'
import Grid from '@mui/material/Grid'
import Skeleton from '@mui/material/Skeleton'
import PetsIcon from '@mui/icons-material/Pets'
import FavoriteIcon from '@mui/icons-material/Favorite'
import VolunteerActivismIcon from '@mui/icons-material/VolunteerActivism'
import SearchIcon from '@mui/icons-material/Search'
import ArrowForwardIcon from '@mui/icons-material/ArrowForward'
import { useGetPetsQuery } from '../services/petsApi'
import { useGetVolunteersQuery } from '../services/volunteersApi'
import PetCard from '../components/pets/PetCard'

const CORAL = '#FF6B6B'

function StatCard({ value, label, icon }: { value: string | number; label: string; icon: React.ReactNode }) {
  return (
    <Paper elevation={0} sx={{ p: { xs: 2, sm: 3 }, textAlign: 'center', border: '1px solid #E5E7EB', borderRadius: 3, flex: 1, minWidth: { xs: 100, sm: 140 } }}>
      <Box sx={{ color: CORAL, mb: 1 }}>{icon}</Box>
      <Typography variant="h4" fontWeight="bold" sx={{ color: '#1F2937' }}>{value}</Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>{label}</Typography>
    </Paper>
  )
}

function StepCard({ step, titleKey, descKey }: { step: number; titleKey: string; descKey: string }) {
  const { t } = useTranslation()
  return (
    <Box sx={{ flex: 1, minWidth: { xs: '80%', sm: 220 }, textAlign: 'center', px: { xs: 1, sm: 2 } }}>
      <Box sx={{
        width: 56, height: 56, borderRadius: '50%',
        bgcolor: '#FFF0F0', color: CORAL,
        display: 'flex', alignItems: 'center', justifyContent: 'center',
        mx: 'auto', mb: 2, fontSize: 22, fontWeight: 700,
      }}>
        {step}
      </Box>
      <Typography variant="h6" fontWeight="bold" sx={{ mb: 1 }}>{t(titleKey)}</Typography>
      <Typography variant="body2" color="text.secondary" sx={{ lineHeight: 1.7 }}>{t(descKey)}</Typography>
    </Box>
  )
}

export default function HomePage() {
  const { t } = useTranslation()
  const navigate = useLangNavigate()

  const { data: petsData } = useGetPetsQuery({ page: 1, pageSize: 1 })
  const { data: volunteersData } = useGetVolunteersQuery({ page: 1, pageSize: 1 })
  const { data: featuredPetsData, isLoading: featuredLoading } = useGetPetsQuery({ page: 1, pageSize: 6 })

  const websiteJsonLd = {
    '@context': 'https://schema.org',
    '@type': 'WebSite',
    name: 'PetZone',
    url: 'https://getpetzone.com',
    potentialAction: {
      '@type': 'SearchAction',
      target: {
        '@type': 'EntryPoint',
        urlTemplate: 'https://getpetzone.com/uk/pets?nickname={search_term_string}',
      },
      'query-input': 'required name=search_term_string',
    },
  }

  return (
    <Box>
      <PageMeta title={t('home.hero.title')} description={t('home.hero.subtitle')} path="/" />
      <script type="application/ld+json" dangerouslySetInnerHTML={{ __html: JSON.stringify(websiteJsonLd) }} />
      {/* Hero */}
      <Box sx={{
        background: 'linear-gradient(135deg, #1e1b4b 0%, #312e81 50%, #4338ca 100%)',
        color: 'white',
        py: { xs: 8, md: 12 },
        px: 3,
        position: 'relative',
        overflow: 'hidden',
      }}>
        {/* Decorative blobs */}
        <Box sx={{ position: 'absolute', top: -60, right: -60, width: 300, height: 300, borderRadius: '50%', bgcolor: 'rgba(255,107,107,0.1)' }} />
        <Box sx={{ position: 'absolute', bottom: -40, left: -40, width: 200, height: 200, borderRadius: '50%', bgcolor: 'rgba(99,102,241,0.2)' }} />

        <Container maxWidth="md" sx={{ position: 'relative', textAlign: 'center' }}>
          <Box sx={{ display: 'inline-flex', alignItems: 'center', gap: 1, bgcolor: 'rgba(255,107,107,0.15)', border: '1px solid rgba(255,107,107,0.3)', borderRadius: 5, px: 2, py: 0.5, mb: 3 }}>
            <PetsIcon sx={{ fontSize: 16, color: CORAL }} />
            <Typography variant="caption" sx={{ color: CORAL, fontWeight: 600, letterSpacing: 0.5 }}>
              PetZone
            </Typography>
          </Box>

          <Typography variant="h2" fontWeight="bold" sx={{ mb: 2, lineHeight: 1.2, fontSize: { xs: '2rem', md: '3rem' } }}>
            {t('home.hero.title')}
          </Typography>

          <Typography variant="h6" sx={{ mb: 5, opacity: 0.8, fontWeight: 400, maxWidth: 560, mx: 'auto', lineHeight: 1.6 }}>
            {t('home.hero.subtitle')}
          </Typography>

          <Box sx={{ display: 'flex', gap: 2, justifyContent: 'center', flexWrap: 'wrap' }}>
            <Button
              variant="contained"
              size="large"
              startIcon={<SearchIcon />}
              onClick={() => navigate('/pets')}
              sx={{
                bgcolor: CORAL, '&:hover': { bgcolor: '#e55555' },
                textTransform: 'none', fontWeight: 700, borderRadius: 3,
                px: 4, py: 1.5, fontSize: '1rem',
              }}
            >
              {t('home.hero.browsePets')}
            </Button>
            <Button
              variant="outlined"
              size="large"
              startIcon={<VolunteerActivismIcon />}
              onClick={() => navigate('/register/volunteer')}
              sx={{
                color: 'white', borderColor: 'rgba(255,255,255,0.4)',
                textTransform: 'none', fontWeight: 600, borderRadius: 3,
                px: 4, py: 1.5, fontSize: '1rem',
                '&:hover': { borderColor: CORAL, color: CORAL, bgcolor: 'transparent' },
              }}
            >
              {t('home.hero.becomeVolunteer')}
            </Button>
          </Box>
        </Container>
      </Box>

      {/* Stats */}
      <Box sx={{ bgcolor: 'white', py: 5, borderBottom: '1px solid #E5E7EB' }}>
        <Container maxWidth="sm">
          <Box sx={{ display: 'flex', gap: 3, flexWrap: 'wrap', justifyContent: 'center' }}>
            <StatCard
              value={petsData?.totalCount ?? '—'}
              label={t('home.stats.pets')}
              icon={<PetsIcon sx={{ fontSize: 32 }} />}
            />
            <StatCard
              value={volunteersData?.totalCount ?? '—'}
              label={t('home.stats.volunteers')}
              icon={<VolunteerActivismIcon sx={{ fontSize: 32 }} />}
            />
            <StatCard
              value="100%"
              label={t('home.stats.free')}
              icon={<FavoriteIcon sx={{ fontSize: 32 }} />}
            />
          </Box>
        </Container>
      </Box>

      {/* How it works */}
      <Box sx={{ bgcolor: '#FAFAFA', py: 8 }}>
        <Container maxWidth="md">
          <Typography variant="h4" fontWeight="bold" textAlign="center" sx={{ mb: 6, color: '#1F2937' }}>
            {t('home.how.title')}
          </Typography>
          <Box sx={{ display: 'flex', gap: 4, flexWrap: 'wrap', justifyContent: 'center' }}>
            <StepCard step={1} titleKey="home.how.step1.title" descKey="home.how.step1.desc" />
            <StepCard step={2} titleKey="home.how.step2.title" descKey="home.how.step2.desc" />
            <StepCard step={3} titleKey="home.how.step3.title" descKey="home.how.step3.desc" />
          </Box>
        </Container>
      </Box>

      {/* Featured Pets */}
      <Box sx={{ bgcolor: 'white', py: { xs: 6, md: 9 } }}>
        <Container maxWidth="lg">
          <Box sx={{ textAlign: 'center', mb: 6 }}>
            <Typography variant="h4" fontWeight="bold" sx={{ mb: 1.5, color: '#1F2937' }}>
              {t('home.featured.title')}
            </Typography>
            <Typography variant="body1" color="text.secondary" sx={{ maxWidth: 520, mx: 'auto' }}>
              {t('home.featured.subtitle')}
            </Typography>
          </Box>

          {featuredLoading ? (
            <Grid container spacing={3}>
              {Array.from({ length: 6 }).map((_, i) => (
                <Grid key={i} size={{ xs: 12, sm: 6, md: 4 }}>
                  <Skeleton variant="rounded" height={420} sx={{ borderRadius: 3 }} />
                </Grid>
              ))}
            </Grid>
          ) : (
            <Grid container spacing={3}>
              {(featuredPetsData?.items ?? []).map((pet) => (
                <Grid key={pet.id} size={{ xs: 12, sm: 6, md: 4 }}>
                  <PetCard pet={pet} />
                </Grid>
              ))}
            </Grid>
          )}

          <Box sx={{ textAlign: 'center', mt: 5 }}>
            <Button
              variant="outlined"
              size="large"
              endIcon={<ArrowForwardIcon />}
              onClick={() => navigate('/pets')}
              sx={{
                borderColor: CORAL, color: CORAL,
                textTransform: 'none', fontWeight: 600, borderRadius: 3,
                px: 4, py: 1.4,
                '&:hover': { bgcolor: '#FFF0F0', borderColor: CORAL },
              }}
            >
              {t('home.featured.viewAll')}
            </Button>
          </Box>
        </Container>
      </Box>

      {/* CTA Banner */}
      <Box sx={{ background: 'linear-gradient(135deg, #FF6B6B 0%, #ee5a24 100%)', py: 7, px: 3 }}>
        <Container maxWidth="sm" sx={{ textAlign: 'center' }}>
          <VolunteerActivismIcon sx={{ fontSize: 48, color: 'white', mb: 2, opacity: 0.9 }} />
          <Typography variant="h4" fontWeight="bold" sx={{ color: 'white', mb: 1.5 }}>
            {t('home.cta.title')}
          </Typography>
          <Typography variant="body1" sx={{ color: 'rgba(255,255,255,0.85)', mb: 4, lineHeight: 1.7 }}>
            {t('home.cta.subtitle')}
          </Typography>
          <Button
            variant="contained"
            size="large"
            onClick={() => navigate('/register/volunteer')}
            sx={{
              bgcolor: 'white', color: CORAL,
              '&:hover': { bgcolor: 'rgba(255,255,255,0.9)' },
              textTransform: 'none', fontWeight: 700,
              borderRadius: 3, px: 5, py: 1.5, fontSize: '1rem',
            }}
          >
            {t('home.cta.btn')}
          </Button>
        </Container>
      </Box>
    </Box>
  )
}
