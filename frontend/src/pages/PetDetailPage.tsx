import { useState, useEffect } from 'react'
import { useParams } from 'react-router-dom'
import { useLangNavigate } from '../hooks/useLangNavigate'
import PageMeta from '../components/meta/PageMeta'
import { useTranslation } from 'react-i18next'
import { useFavoritesStore } from '../store/favoritesStore'
import { toast } from '../store/toastStore'
import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Paper from '@mui/material/Paper'
import Typography from '@mui/material/Typography'
import Button from '@mui/material/Button'
import Chip from '@mui/material/Chip'
import Divider from '@mui/material/Divider'
import Alert from '@mui/material/Alert'
import PetDetailSkeleton from '../components/skeletons/PetDetailSkeleton'
import IconButton from '@mui/material/IconButton'
import Tooltip from '@mui/material/Tooltip'
import Grid from '@mui/material/Grid'
import VaccinesIcon from '@mui/icons-material/Vaccines'
import ContentCutIcon from '@mui/icons-material/ContentCut'
import LocationOnIcon from '@mui/icons-material/LocationOn'
import ScaleIcon from '@mui/icons-material/Scale'
import HeightIcon from '@mui/icons-material/Height'
import PersonIcon from '@mui/icons-material/Person'
import StarBorderIcon from '@mui/icons-material/StarBorder'
import StarIcon from '@mui/icons-material/Star'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import { useGetPetByIdQuery, useGetPetsQuery } from '../services/petsApi'
import { PetStatus } from '../types/pet'
import PetCard from '../components/pets/PetCard'
import Skeleton from '@mui/material/Skeleton'
import AppBreadcrumbs from '../components/ui/AppBreadcrumbs'
import { useRecentlyViewedStore } from '../store/recentlyViewedStore'
import ShareButton from '../components/ui/ShareButton'

const CORAL = '#FF6B6B'

const STATUS_COLORS: Record<PetStatus, { bg: string; text: string; labelKey: string }> = {
  [PetStatus.NeedsHelp]: { bg: '#FEF3C7', text: '#D97706', labelKey: 'pets.status.needsHelp' },
  [PetStatus.LookingForHome]: { bg: '#DBEAFE', text: '#2563EB', labelKey: 'pets.status.lookingForHome' },
  [PetStatus.FoundHome]: { bg: '#D1FAE5', text: '#059669', labelKey: 'pets.status.foundHome' },
}

export default function PetDetailPage() {
  const { t } = useTranslation()
  const { petId } = useParams<{ petId: string }>()
  const navigate = useLangNavigate()
  const { toggle, has } = useFavoritesStore()
  const [activePhoto, setActivePhoto] = useState(0)

  const { data: pet, isLoading, isError } = useGetPetByIdQuery(petId ?? '', { skip: !petId })

  if (isLoading) {
    return <PetDetailSkeleton />
  }

  if (isError || !pet) {
    return (
      <Container maxWidth="sm" sx={{ py: 8 }}>
        <Alert severity="error" sx={{ mb: 3 }}>{t('petDetail.notFound')}</Alert>
        <Button
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate('/')}
          sx={{ color: CORAL, textTransform: 'none' }}
        >
          {t('petDetail.backToList')}
        </Button>
      </Container>
    )
  }

  // Track recently viewed
  const addViewed = useRecentlyViewedStore((s) => s.add)
  // eslint-disable-next-line react-hooks/rules-of-hooks
  useEffect(() => { if (pet) addViewed(pet.id) }, [pet?.id])

  // Similar pets — same species, LookingForHome, max 5 (we'll exclude current)
  // eslint-disable-next-line react-hooks/rules-of-hooks
  const { data: similarData, isLoading: similarLoading } = useGetPetsQuery(
    { page: 1, pageSize: 5, speciesId: pet.speciesId, status: PetStatus.LookingForHome },
    { skip: !pet.speciesId },
  )
  const similarPets = (similarData?.items ?? []).filter((p) => p.id !== pet.id).slice(0, 4)

  const photos = pet.photos.length > 0 ? pet.photos : []
  const mainPhotoFirst = [...photos].sort((a, b) => (b.isMain ? 1 : 0) - (a.isMain ? 1 : 0))
  const displayPhoto =
    mainPhotoFirst[activePhoto]?.filePath ?? 'https://placehold.co/800x500?text=Photo'

  const statusCfg = STATUS_COLORS[pet.status]

  return (
    <Box sx={{ bgcolor: 'background.default', minHeight: '100%', py: 4 }}>
      <PageMeta
        title={pet.nickname}
        description={pet.generalDescription ?? t('pets.pageTitle')}
        path={`/pets/${petId}`}
        image={mainPhotoFirst[0]?.filePath}
        type="article"
      />
      <script type="application/ld+json" dangerouslySetInnerHTML={{ __html: JSON.stringify({
        '@context': 'https://schema.org',
        '@type': 'Animal',
        name: pet.nickname,
        description: pet.generalDescription ?? '',
        image: mainPhotoFirst[0]?.filePath ?? '',
        url: `https://getpetzone.com/uk/pets/${petId}`,
        identifier: pet.id,
        locationCreated: { '@type': 'Place', name: pet.city },
      }) }} />
      <Container maxWidth="lg">
        <AppBreadcrumbs items={[
          { label: t('nav.home'), path: '/' },
          { label: t('nav.pets'), path: '/pets' },
          { label: pet.nickname },
        ]} />

        <Grid container spacing={4}>
          {/* ── LEFT: Photos ─────────────────────────────── */}
          <Grid size={{ xs: 12, md: 6 }}>
            <Paper
              elevation={0}
              sx={{ border: '1px solid #E5E7EB', borderRadius: 3, overflow: 'hidden' }}
            >
              {/* Main photo */}
              <Box
                component="img"
                src={displayPhoto}
                alt={pet.nickname}
                loading="lazy"
                sx={{ width: '100%', height: { xs: 240, sm: 340, md: 420 }, objectFit: 'cover', display: 'block' }}
              />

              {/* Thumbnail strip */}
              {mainPhotoFirst.length > 1 && (
                <Box
                  sx={{
                    display: 'flex',
                    gap: 1,
                    p: 1.5,
                    overflowX: 'auto',
                    scrollbarWidth: 'none',
                    '&::-webkit-scrollbar': { display: 'none' },
                  }}
                >
                  {mainPhotoFirst.map((photo, i) => (
                    <Box
                      key={photo.filePath}
                      component="img"
                      src={photo.filePath}
                      alt={`${t('petDetail.photos')} ${i + 1}`}
                      loading="lazy"
                      onClick={() => setActivePhoto(i)}
                      sx={{
                        width: 72,
                        height: 56,
                        objectFit: 'cover',
                        borderRadius: 1.5,
                        flexShrink: 0,
                        cursor: 'pointer',
                        border: i === activePhoto ? `2px solid ${CORAL}` : '2px solid transparent',
                        opacity: i === activePhoto ? 1 : 0.65,
                        transition: 'opacity 0.2s, border-color 0.2s',
                        '&:hover': { opacity: 1 },
                      }}
                    />
                  ))}
                </Box>
              )}
            </Paper>
          </Grid>

          {/* ── RIGHT: Info ──────────────────────────────── */}
          <Grid size={{ xs: 12, md: 6 }}>
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2.5 }}>

              {/* Name + status + favorite */}
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                <Box>
                  <Typography variant="h4" fontWeight="bold" sx={{ lineHeight: 1.2 }}>
                    {pet.nickname}
                  </Typography>
                  <Chip
                    label={t(statusCfg.labelKey)}
                    size="small"
                    sx={{ mt: 0.5, bgcolor: statusCfg.bg, color: statusCfg.text, fontWeight: 600 }}
                  />
                </Box>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                <ShareButton title={pet.nickname} text={pet.generalDescription ?? ''} />
                <Tooltip title={has(petId ?? '') ? t('petDetail.removeFromFavorites') : t('petDetail.saveToFavorites')}>
                  <IconButton
                    onClick={() => {
                      const wasSaved = has(petId ?? '')
                      toggle(petId ?? '')
                      toast.success(wasSaved ? t('pets.removedFromFavorites') : t('pets.addedToFavorites'))
                    }}
                    sx={{ color: has(petId ?? '') ? CORAL : '#9CA3AF', mt: -0.5 }}
                  >
                    {has(petId ?? '') ? <StarIcon /> : <StarBorderIcon />}
                  </IconButton>
                </Tooltip>
                </Box>
              </Box>

              {/* Characteristics */}
              <Paper elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 2, p: 2 }}>
                <Typography variant="subtitle2" fontWeight="bold" color="text.secondary"
                  sx={{ mb: 1.5, textTransform: 'uppercase', fontSize: 11, letterSpacing: 0.5 }}>
                  {t('petDetail.characteristics')}
                </Typography>

                <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr' }, gap: 1.2 }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <LocationOnIcon sx={{ fontSize: 17, color: '#6B7280' }} />
                    <Typography variant="body2">{pet.city}</Typography>
                  </Box>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <ScaleIcon sx={{ fontSize: 17, color: '#6B7280' }} />
                    <Typography variant="body2">{pet.weight} {t('pets.kg')}</Typography>
                  </Box>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <HeightIcon sx={{ fontSize: 17, color: '#6B7280' }} />
                    <Typography variant="body2">{pet.height} {t('pets.cm')}</Typography>
                  </Box>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <Chip
                      label={`# ${pet.color}`}
                      size="small"
                      sx={{ bgcolor: '#FFF0F0', color: CORAL, fontWeight: 500, fontSize: 11 }}
                    />
                  </Box>
                </Box>
              </Paper>

              {/* Health */}
              <Paper elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 2, p: 2 }}>
                <Typography variant="subtitle2" fontWeight="bold" color="text.secondary"
                  sx={{ mb: 1.5, textTransform: 'uppercase', fontSize: 11, letterSpacing: 0.5 }}>
                  {t('petDetail.health')}
                </Typography>
                <Box sx={{ display: 'flex', gap: 1.5, flexWrap: 'wrap' }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                    <VaccinesIcon sx={{ fontSize: 17, color: pet.isVaccinated ? '#22C55E' : '#9CA3AF' }} />
                    <Typography variant="body2">
                      {pet.isVaccinated ? t('petDetail.vaccinated') : t('petDetail.notVaccinated')}
                    </Typography>
                  </Box>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                    <ContentCutIcon sx={{ fontSize: 17, color: pet.isCastrated ? '#22C55E' : '#9CA3AF' }} />
                    <Typography variant="body2">
                      {pet.isCastrated ? t('petDetail.castrated') : t('petDetail.notCastrated')}
                    </Typography>
                  </Box>
                </Box>
              </Paper>

              {/* About */}
              {pet.generalDescription && (
                <Paper elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 2, p: 2 }}>
                  <Typography variant="subtitle2" fontWeight="bold" color="text.secondary"
                    sx={{ mb: 1, textTransform: 'uppercase', fontSize: 11, letterSpacing: 0.5 }}>
                    {t('petDetail.about')}
                  </Typography>
                  <Typography variant="body2" color="text.secondary" sx={{ lineHeight: 1.8 }}>
                    {pet.generalDescription}
                  </Typography>
                </Paper>
              )}

              {/* Adoption conditions */}
              {pet.adoptionConditions && (
                <Paper elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 2, p: 2 }}>
                  <Typography variant="subtitle2" fontWeight="bold" color="text.secondary"
                    sx={{ mb: 1, textTransform: 'uppercase', fontSize: 11, letterSpacing: 0.5 }}>
                    {t('petDetail.adoptionConditions')}
                  </Typography>
                  <Typography variant="body2" color="text.secondary" sx={{ lineHeight: 1.8 }}>
                    {pet.adoptionConditions}
                  </Typography>
                </Paper>
              )}

              {/* Microchip */}
              {pet.microchipNumber && (
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <Typography variant="body2" color="text.secondary">
                    {t('petDetail.microchip')}:
                  </Typography>
                  <Typography variant="body2" fontWeight="mono" sx={{ fontFamily: 'monospace' }}>
                    {pet.microchipNumber}
                  </Typography>
                </Box>
              )}

              <Divider />

              {/* Volunteer link */}
              <Button
                startIcon={<PersonIcon />}
                variant="outlined"
                onClick={() => navigate(`/volunteers/${pet.volunteerId}`)}
                sx={{
                  borderColor: CORAL,
                  color: CORAL,
                  textTransform: 'none',
                  fontWeight: 600,
                  '&:hover': { borderColor: '#e55555', bgcolor: '#FFF0F0' },
                }}
              >
                {t('petDetail.viewVolunteer')}
              </Button>
            </Box>
          </Grid>
        </Grid>
      </Container>

      {/* ── SIMILAR PETS ───────────────────────────────── */}
      {(similarLoading || similarPets.length > 0) && (
        <Box sx={{ bgcolor: 'background.paper', borderTop: '1px solid #E5E7EB', py: 5, mt: 2 }}>
          <Container maxWidth="lg">
            <Typography variant="h5" fontWeight="bold" sx={{ mb: 3 }}>
              {t('petDetail.similarPets')}
            </Typography>

            {similarLoading ? (
              <Box sx={{ display: 'flex', gap: 2, overflow: 'hidden' }}>
                {[0, 1, 2, 3].map((i) => (
                  <Skeleton
                    key={i}
                    variant="rectangular"
                    sx={{ minWidth: { xs: 220, sm: 260, md: 290 }, height: 380, borderRadius: 3, flexShrink: 0 }}
                  />
                ))}
              </Box>
            ) : (
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
                {similarPets.map((p) => (
                  <Box
                    key={p.id}
                    sx={{ minWidth: { xs: 220, sm: 260, md: 290 }, maxWidth: { xs: 220, sm: 260, md: 290 }, flexShrink: 0 }}
                  >
                    <PetCard pet={p} />
                  </Box>
                ))}
              </Box>
            )}
          </Container>
        </Box>
      )}
    </Box>
  )
}
