import { useTranslation } from 'react-i18next'
import Grid from '@mui/material/Grid'
import Typography from '@mui/material/Typography'
import Box from '@mui/material/Box'
import CircularProgress from '@mui/material/CircularProgress'
import PetCard from './PetCard'
import PetCardSkeleton from './PetCardSkeleton'
import type { Pet } from '../../types/pet'

interface Props {
  pets: Pet[]
  /** True on the very first load (no cached data yet) — shows a full-page spinner */
  isLoading: boolean
  /**
   * True whenever a background request is in flight (e.g. the user changed filters
   * and RTK Query is refetching while still showing the previous result).
   * Shows a small spinner in the top-right corner + slight opacity on the list.
   */
  isFetching?: boolean
}

export default function PetsList({ pets, isLoading, isFetching = false }: Props) {
  const { t } = useTranslation()

  if (isLoading) {
    return (
      <Grid container spacing={3}>
        {Array.from({ length: 9 }).map((_, i) => (
          <Grid key={i} size={{ xs: 12, sm: 6, lg: 4 }} sx={{ display: 'flex' }}>
            <PetCardSkeleton />
          </Grid>
        ))}
      </Grid>
    )
  }

  if (pets.length === 0) {
    return (
      <Box sx={{ textAlign: 'center', py: 10 }}>
        <Typography variant="h6" color="text.secondary">
          {t('pets.notFound')}
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
          {t('pets.notFoundHint')}
        </Typography>
      </Box>
    )
  }

  return (
    <Box sx={{ position: 'relative' }}>
      {/* Background-refetch indicator — visible when filter/page changes trigger a new request */}
      {isFetching && (
        <Box
          sx={{
            position: 'absolute',
            top: -8,
            right: 0,
            zIndex: 1,
            display: 'flex',
            alignItems: 'center',
            gap: 0.75,
            bgcolor: 'rgba(255,255,255,0.85)',
            borderRadius: 2,
            px: 1.5,
            py: 0.5,
          }}
        >
          <CircularProgress size={14} sx={{ color: '#FF6B6B' }} />
          <Typography variant="caption" color="text.secondary">
            {t('pets.updating')}
          </Typography>
        </Box>
      )}

      <Box sx={{ opacity: isFetching ? 0.55 : 1, transition: 'opacity 0.2s ease' }}>
        <Grid container spacing={3}>
          {pets.map((pet) => (
            <Grid key={pet.id} size={{ xs: 12, sm: 6, lg: 4 }} sx={{ display: 'flex' }}>
              <PetCard pet={pet} />
            </Grid>
          ))}
        </Grid>
      </Box>
    </Box>
  )
}
