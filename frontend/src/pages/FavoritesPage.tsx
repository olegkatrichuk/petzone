import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import PageMeta from '../components/meta/PageMeta'
import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Typography from '@mui/material/Typography'
import Button from '@mui/material/Button'
import Grid from '@mui/material/Grid'
import CircularProgress from '@mui/material/CircularProgress'
import StarIcon from '@mui/icons-material/Star'
import PetsIcon from '@mui/icons-material/Pets'
import { useAuthStore } from '../store/authStore'
import { useFavoritesStore } from '../store/favoritesStore'
import { useGetPetByIdQuery } from '../services/petsApi'
import PetCard from '../components/pets/PetCard'
import type { Pet } from '../types/pet'

const CORAL = '#FF6B6B'

// Loads a single pet and renders it — skips if already errored
function FavoritePetCard({ petId }: { petId: string }) {
  const { data, isLoading } = useGetPetByIdQuery(petId)

  if (isLoading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: 200 }}>
        <CircularProgress size={28} sx={{ color: CORAL }} />
      </Box>
    )
  }

  if (!data) return null

  return <PetCard pet={data as Pet} />
}

export default function FavoritesPage() {
  const { t } = useTranslation()
  const { user } = useAuthStore()
  const { ids } = useFavoritesStore()

  // Not logged in
  if (!user) {
    return (
      <Box sx={{ minHeight: '60vh', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
        <Box sx={{ textAlign: 'center' }}>
          <StarIcon sx={{ fontSize: 56, color: '#E5E7EB', mb: 2 }} />
          <Typography variant="h6" sx={{ mb: 2 }}>{t('favorites.notLoggedIn')}</Typography>
          <Button
            component={Link}
            to="/login"
            variant="contained"
            sx={{
              bgcolor: CORAL,
              '&:hover': { bgcolor: '#e55555' },
              textTransform: 'none',
              fontWeight: 700,
              borderRadius: 2,
              px: 4,
            }}
          >
            {t('favorites.loginBtn')}
          </Button>
        </Box>
      </Box>
    )
  }

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <PageMeta title={t('favorites.title')} description={t('favorites.title')} path="/favorites" noIndex />
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5, mb: 4 }}>
        <StarIcon sx={{ color: CORAL, fontSize: 28 }} />
        <Typography variant="h4" fontWeight="bold">
          {t('favorites.title')}
        </Typography>
      </Box>

      {ids.length === 0 ? (
        <Box sx={{ textAlign: 'center', py: 10 }}>
          <PetsIcon sx={{ fontSize: 72, color: '#E5E7EB', mb: 2 }} />
          <Typography variant="h6" color="text.secondary">{t('favorites.empty')}</Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mt: 1, mb: 3 }}>
            {t('favorites.emptyHint')}
          </Typography>
          <Button
            component={Link}
            to="/"
            variant="contained"
            sx={{
              bgcolor: CORAL,
              '&:hover': { bgcolor: '#e55555' },
              textTransform: 'none',
              fontWeight: 600,
              borderRadius: 2,
              px: 4,
            }}
          >
            {t('favorites.browsePets')}
          </Button>
        </Box>
      ) : (
        <Grid container spacing={3}>
          {ids.map((id) => (
            <Grid size={{ xs: 12, sm: 6, md: 4 }} key={id}>
              <FavoritePetCard petId={id} />
            </Grid>
          ))}
        </Grid>
      )}
    </Container>
  )
}
