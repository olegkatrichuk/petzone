import { useTranslation } from 'react-i18next'
import { useLangNavigate } from '../hooks/useLangNavigate'
import PageMeta from '../components/meta/PageMeta'
import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Typography from '@mui/material/Typography'
import Button from '@mui/material/Button'
import PetsIcon from '@mui/icons-material/Pets'
import HomeIcon from '@mui/icons-material/Home'
import SearchIcon from '@mui/icons-material/Search'

const CORAL = '#FF6B6B'

export default function NotFoundPage() {
  const { t } = useTranslation()
  const navigate = useLangNavigate()

  return (
    <Box sx={{ minHeight: '70vh', display: 'flex', alignItems: 'center', bgcolor: 'background.default' }}>
      <PageMeta title={t('notFound.title')} description={t('notFound.subtitle')} path="/404" />
      <Container maxWidth="sm" sx={{ textAlign: 'center', py: 8 }}>
        {/* Big 404 */}
        <Box sx={{ position: 'relative', display: 'inline-block', mb: 2 }}>
          <Typography
            sx={{
              fontSize: { xs: '7rem', md: '10rem' },
              fontWeight: 900,
              lineHeight: 1,
              color: '#F3F4F6',
              userSelect: 'none',
            }}
          >
            404
          </Typography>
          <PetsIcon sx={{
            position: 'absolute',
            top: '50%', left: '50%',
            transform: 'translate(-50%, -50%)',
            fontSize: { xs: 56, md: 80 },
            color: CORAL,
          }} />
        </Box>

        <Typography variant="h5" fontWeight="bold" sx={{ mb: 1.5, color: '#1F2937' }}>
          {t('notFound.title')}
        </Typography>

        <Typography variant="body1" color="text.secondary" sx={{ mb: 5, lineHeight: 1.8 }}>
          {t('notFound.subtitle')}
        </Typography>

        <Box sx={{ display: 'flex', gap: 2, justifyContent: 'center', flexWrap: 'wrap' }}>
          <Button
            variant="contained"
            startIcon={<HomeIcon />}
            onClick={() => navigate('/')}
            sx={{
              bgcolor: CORAL, '&:hover': { bgcolor: '#e55555' },
              textTransform: 'none', fontWeight: 700,
              borderRadius: 2, px: 3,
            }}
          >
            {t('notFound.goHome')}
          </Button>
          <Button
            variant="outlined"
            startIcon={<SearchIcon />}
            onClick={() => navigate('/pets')}
            sx={{
              borderColor: '#E5E7EB', color: '#6B7280',
              textTransform: 'none', borderRadius: 2, px: 3,
              '&:hover': { borderColor: CORAL, color: CORAL },
            }}
          >
            {t('notFound.browsePets')}
          </Button>
        </Box>
      </Container>
    </Box>
  )
}
