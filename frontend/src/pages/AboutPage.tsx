import { useTranslation } from 'react-i18next'
import { useLangNavigate } from '../hooks/useLangNavigate'
import PageMeta from '../components/meta/PageMeta'
import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Typography from '@mui/material/Typography'
import Button from '@mui/material/Button'
import Paper from '@mui/material/Paper'
import Grid from '@mui/material/Grid'
import PetsIcon from '@mui/icons-material/Pets'
import VisibilityIcon from '@mui/icons-material/Visibility'
import FavoriteIcon from '@mui/icons-material/Favorite'
import GroupsIcon from '@mui/icons-material/Groups'
import LockOpenIcon from '@mui/icons-material/LockOpen'
import ErrorOutlineIcon from '@mui/icons-material/ErrorOutline'
import LightbulbIcon from '@mui/icons-material/Lightbulb'
import CheckCircleOutlineIcon from '@mui/icons-material/CheckCircleOutline'

const CORAL = '#FF6B6B'
const DARK = '#1e1b4b'

export default function AboutPage() {
  const { t } = useTranslation()
  const navigate = useLangNavigate()

  const missionCards = [
    {
      icon: <ErrorOutlineIcon sx={{ fontSize: 32, color: CORAL }} />,
      titleKey: 'about.mission.card1.title',
      descKey: 'about.mission.card1.desc',
    },
    {
      icon: <LightbulbIcon sx={{ fontSize: 32, color: '#F59E0B' }} />,
      titleKey: 'about.mission.card2.title',
      descKey: 'about.mission.card2.desc',
    },
    {
      icon: <CheckCircleOutlineIcon sx={{ fontSize: 32, color: '#22C55E' }} />,
      titleKey: 'about.mission.card3.title',
      descKey: 'about.mission.card3.desc',
    },
  ]

  const values = [
    { icon: <VisibilityIcon />, titleKey: 'about.values.v1.title', descKey: 'about.values.v1.desc' },
    { icon: <FavoriteIcon />, titleKey: 'about.values.v2.title', descKey: 'about.values.v2.desc' },
    { icon: <GroupsIcon />, titleKey: 'about.values.v3.title', descKey: 'about.values.v3.desc' },
    { icon: <LockOpenIcon />, titleKey: 'about.values.v4.title', descKey: 'about.values.v4.desc' },
  ]

  const orgJsonLd = {
    '@context': 'https://schema.org',
    '@type': 'Organization',
    name: 'PetZone',
    url: 'https://getpetzone.com',
    logo: 'https://getpetzone.com/og-default.svg',
    description: t('about.metaDesc'),
    foundingDate: '2024',
    sameAs: [],
  }

  return (
    <Box sx={{ bgcolor: 'background.default' }}>
      <PageMeta
        title={t('about.pageTitle')}
        description={t('about.metaDesc')}
        path="/about"
      />
      <script type="application/ld+json" dangerouslySetInnerHTML={{ __html: JSON.stringify(orgJsonLd) }} />

      {/* ── HERO ──────────────────────────────────────────── */}
      <Box
        sx={{
          background: `linear-gradient(135deg, ${DARK} 0%, #312e81 60%, #4c1d95 100%)`,
          color: 'white',
          py: { xs: 8, md: 12 },
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
            <PetsIcon sx={{ fontSize: 16, color: CORAL }} />
            <Typography variant="caption" sx={{ color: CORAL, fontWeight: 600, letterSpacing: 1, textTransform: 'uppercase' }}>
              {t('about.hero.badge')}
            </Typography>
          </Box>

          <Typography
            variant="h3"
            fontWeight="bold"
            sx={{ mb: 2.5, fontSize: { xs: '2rem', md: '3rem' }, lineHeight: 1.2 }}
          >
            {t('about.hero.title')}
          </Typography>

          <Typography
            variant="h6"
            sx={{ opacity: 0.8, maxWidth: 640, mx: 'auto', lineHeight: 1.7, fontWeight: 400, fontSize: { xs: '1rem', md: '1.15rem' } }}
          >
            {t('about.hero.subtitle')}
          </Typography>
        </Container>
      </Box>

      {/* ── MISSION ───────────────────────────────────────── */}
      <Container maxWidth="lg" sx={{ py: { xs: 6, md: 10 } }}>
        <Typography
          variant="h4"
          fontWeight="bold"
          textAlign="center"
          sx={{ mb: 5, fontSize: { xs: '1.6rem', md: '2.2rem' } }}
        >
          {t('about.mission.title')}
        </Typography>

        <Grid container spacing={3}>
          {missionCards.map((card) => (
            <Grid key={card.titleKey} size={{ xs: 12, md: 4 }}>
              <Paper
                elevation={0}
                sx={{
                  border: '1px solid #E5E7EB',
                  borderRadius: 3,
                  p: 3.5,
                  height: '100%',
                  display: 'flex',
                  flexDirection: 'column',
                  gap: 1.5,
                }}
              >
                {card.icon}
                <Typography variant="h6" fontWeight="bold">{t(card.titleKey)}</Typography>
                <Typography variant="body2" color="text.secondary" sx={{ lineHeight: 1.8 }}>
                  {t(card.descKey)}
                </Typography>
              </Paper>
            </Grid>
          ))}
        </Grid>
      </Container>

      {/* ── VALUES ────────────────────────────────────────── */}
      <Box sx={{ bgcolor: 'background.paper', py: { xs: 6, md: 10 } }}>
        <Container maxWidth="lg">
          <Typography
            variant="h4"
            fontWeight="bold"
            textAlign="center"
            sx={{ mb: 5, fontSize: { xs: '1.6rem', md: '2.2rem' } }}
          >
            {t('about.values.title')}
          </Typography>

          <Grid container spacing={3}>
            {values.map((v) => (
              <Grid key={v.titleKey} size={{ xs: 12, sm: 6 }}>
                <Box sx={{ display: 'flex', gap: 2, alignItems: 'flex-start' }}>
                  <Box
                    sx={{
                      width: 44,
                      height: 44,
                      borderRadius: 2,
                      bgcolor: '#FFF0F0',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      color: CORAL,
                      flexShrink: 0,
                    }}
                  >
                    {v.icon}
                  </Box>
                  <Box>
                    <Typography variant="subtitle1" fontWeight="bold" sx={{ mb: 0.5 }}>
                      {t(v.titleKey)}
                    </Typography>
                    <Typography variant="body2" color="text.secondary" sx={{ lineHeight: 1.7 }}>
                      {t(v.descKey)}
                    </Typography>
                  </Box>
                </Box>
              </Grid>
            ))}
          </Grid>
        </Container>
      </Box>

      {/* ── TEAM ──────────────────────────────────────────── */}
      <Container maxWidth="sm" sx={{ py: { xs: 6, md: 10 }, textAlign: 'center' }}>
        <Typography
          variant="h4"
          fontWeight="bold"
          sx={{ mb: 2, fontSize: { xs: '1.6rem', md: '2.2rem' } }}
        >
          {t('about.team.title')}
        </Typography>
        <Typography
          variant="body1"
          color="text.secondary"
          sx={{ mb: 5, lineHeight: 1.8 }}
        >
          {t('about.team.desc')}
        </Typography>

      </Container>

      {/* ── CTA ───────────────────────────────────────────── */}
      <Box sx={{ background: `linear-gradient(135deg, ${CORAL} 0%, #e55555 100%)`, py: { xs: 6, md: 8 } }}>
        <Container maxWidth="md" sx={{ textAlign: 'center' }}>
          <Typography variant="h4" fontWeight="bold" color="white" sx={{ mb: 1.5, fontSize: { xs: '1.6rem', md: '2.2rem' } }}>
            {t('about.cta.title')}
          </Typography>
          <Typography variant="h6" sx={{ color: 'rgba(255,255,255,0.85)', mb: 4, fontWeight: 400, fontSize: { xs: '1rem', md: '1.15rem' } }}>
            {t('about.cta.subtitle')}
          </Typography>
          <Button
            variant="contained"
            size="large"
            onClick={() => navigate('/register/volunteer')}
            sx={{
              bgcolor: 'background.paper',
              color: CORAL,
              fontWeight: 700,
              px: 4,
              py: 1.5,
              borderRadius: 3,
              fontSize: '1rem',
              textTransform: 'none',
              '&:hover': { bgcolor: '#fff5f5' },
            }}
          >
            {t('about.cta.btn')}
          </Button>
        </Container>
      </Box>
    </Box>
  )
}
