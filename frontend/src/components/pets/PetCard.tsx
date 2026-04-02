import { useTranslation } from 'react-i18next'
import { useParams } from 'react-router-dom'
import { useFavoritesStore } from '../../store/favoritesStore'
import { useComparisonStore } from '../../store/comparisonStore'
import { useLangNavigate } from '../../hooks/useLangNavigate'
import { DEFAULT_LANG } from '../../lib/langUtils'
import { toast } from '../../store/toastStore'
import Card from '@mui/material/Card'
import CardMedia from '@mui/material/CardMedia'
import CardContent from '@mui/material/CardContent'
import CardActions from '@mui/material/CardActions'
import Typography from '@mui/material/Typography'
import Chip from '@mui/material/Chip'
import Button from '@mui/material/Button'
import IconButton from '@mui/material/IconButton'
import Box from '@mui/material/Box'
import Tooltip from '@mui/material/Tooltip'
import Divider from '@mui/material/Divider'
import StarBorderIcon from '@mui/icons-material/StarBorder'
import StarIcon from '@mui/icons-material/Star'
import ShareIcon from '@mui/icons-material/Share'
import CompareArrowsIcon from '@mui/icons-material/CompareArrows'
import VaccinesIcon from '@mui/icons-material/Vaccines'
import ContentCutIcon from '@mui/icons-material/ContentCut'
import LocationOnIcon from '@mui/icons-material/LocationOn'
import ScaleIcon from '@mui/icons-material/Scale'
import HeightIcon from '@mui/icons-material/Height'
import PersonIcon from '@mui/icons-material/Person'
import type { Pet, PetStatus } from '../../types/pet'

const CORAL = '#FF6B6B'
const CORAL_LIGHT = '#FFF0F0'

const STATUS_COLORS: Record<PetStatus, { bg: string; text: string; labelKey: string }> = {
  0: { bg: '#FEF3C7', text: '#D97706', labelKey: 'pets.status.needsHelp' },
  1: { bg: '#DBEAFE', text: '#2563EB', labelKey: 'pets.status.lookingForHome' },
  2: { bg: '#D1FAE5', text: '#059669', labelKey: 'pets.status.foundHome' },
}

function calcAge(dateOfBirth: string, t: (k: string) => string): string {
  const dob = new Date(dateOfBirth)
  const now = new Date()
  let years = now.getFullYear() - dob.getFullYear()
  let months = now.getMonth() - dob.getMonth()
  if (months < 0) { years--; months += 12 }
  const y = years > 0 ? `${years} ${t('pets.ageYears')}` : ''
  const m = months > 0 ? `${months} ${t('pets.ageMonths')}` : ''
  return [y, m].filter(Boolean).join(' ') || t('pets.ageLessThanMonth')
}

interface Props {
  pet: Pet
}

export default function PetCard({ pet }: Props) {
  const navigate = useLangNavigate()
  const { t } = useTranslation()
  const { lang } = useParams<{ lang: string }>()
  const { toggle, has } = useFavoritesStore()
  const { toggle: toggleCompare, has: inCompare, pets: comparePets } = useComparisonStore()
  const saved = has(pet.id)
  const compared = inCompare(pet.id)
  const compareMaxed = comparePets.length >= 3 && !compared

  const handleShare = async () => {
    const url = `${window.location.origin}/${lang ?? DEFAULT_LANG}/pets/${pet.id}`
    const shareData = {
      title: pet.nickname,
      text: pet.generalDescription ? `${pet.nickname} — ${pet.generalDescription.slice(0, 80)}…` : pet.nickname,
      url,
    }
    if (navigator.share && navigator.canShare?.(shareData)) {
      try { await navigator.share(shareData) } catch { /* user cancelled */ }
    } else {
      await navigator.clipboard.writeText(url)
      toast.success(t('pets.linkCopied'))
    }
  }

  const handleToggleFavorite = () => {
    toggle(pet.id)
    toast.success(saved ? t('pets.removedFromFavorites') : t('pets.addedToFavorites'))
  }

  const mainPhoto =
    pet.photos.find((p) => p.isMain)?.filePath ??
    pet.photos[0]?.filePath ??
    'https://placehold.co/400x280?text=Photo'

  const statusCfg = STATUS_COLORS[pet.status]

  return (
    <Card
      elevation={0}
      sx={{
        border: '1px solid #E5E7EB',
        borderRadius: 3,
        display: 'flex',
        flexDirection: 'column',
        transition: 'box-shadow 0.2s, transform 0.2s',
        '&:hover': {
          boxShadow: '0 8px 24px rgba(255,107,107,0.15)',
          transform: 'translateY(-2px)',
        },
      }}
    >
      <CardMedia
        component="img"
        height={220}
        image={mainPhoto}
        alt={pet.nickname}
        loading="lazy"
        sx={{ objectFit: 'cover' }}
      />

      <CardContent sx={{ flex: 1, pb: 1 }}>
        {/* Name + age + status */}
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 1.5 }}>
          <Typography variant="h6" fontWeight="bold">
            {pet.nickname}
            <Typography component="span" variant="body2" color="text.secondary" sx={{ ml: 0.5 }}>
              · {calcAge(pet.dateOfBirth, t)}
            </Typography>
          </Typography>
          <Chip
            label={t(statusCfg.labelKey)}
            size="small"
            sx={{ bgcolor: statusCfg.bg, color: statusCfg.text, fontWeight: 600, fontSize: 11, ml: 1, flexShrink: 0 }}
          />
        </Box>

        {/* Color chip */}
        <Box sx={{ mb: 1.5 }}>
          <Chip
            label={`# ${pet.color}`}
            size="small"
            sx={{ bgcolor: CORAL_LIGHT, color: CORAL, fontWeight: 500, fontSize: 11 }}
          />
        </Box>

        {/* Main info rows */}
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.6, mb: 1.5 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <LocationOnIcon sx={{ fontSize: 17, color: '#6B7280' }} />
            <Typography variant="body2" color="text.secondary">{pet.city}</Typography>
          </Box>

          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <VaccinesIcon sx={{ fontSize: 17, color: pet.isVaccinated ? '#22C55E' : '#9CA3AF' }} />
            <Typography variant="body2" color="text.secondary">
              {pet.isVaccinated ? t('pets.vaccinated') : t('pets.notVaccinated')}
            </Typography>
          </Box>

          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <ContentCutIcon sx={{ fontSize: 17, color: pet.isCastrated ? '#22C55E' : '#9CA3AF' }} />
            <Typography variant="body2" color="text.secondary">
              {pet.isCastrated ? t('pets.castrated') : t('pets.notCastrated')}
            </Typography>
          </Box>
        </Box>

        {/* Weight + Height */}
        <Box sx={{ display: 'flex', gap: 2, mb: 1.5 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
            <ScaleIcon sx={{ fontSize: 16, color: '#9CA3AF' }} />
            <Typography variant="body2" color="text.secondary">{pet.weight} {t('pets.kg')}</Typography>
          </Box>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
            <HeightIcon sx={{ fontSize: 16, color: '#9CA3AF' }} />
            <Typography variant="body2" color="text.secondary">{pet.height} {t('pets.cm')}</Typography>
          </Box>
        </Box>

        {/* Description preview */}
        {pet.generalDescription && (
          <Typography
            variant="body2"
            color="text.secondary"
            sx={{
              overflow: 'hidden',
              display: '-webkit-box',
              WebkitLineClamp: 2,
              WebkitBoxOrient: 'vertical',
              mb: 1.5,
            }}
          >
            {pet.generalDescription}
          </Typography>
        )}

        <Divider sx={{ mb: 1.5 }} />

        {/* Volunteer link */}
        <Box
          sx={{ display: 'flex', alignItems: 'center', gap: 0.5, cursor: 'pointer' }}
          onClick={() => navigate(`/volunteers/${pet.volunteerId}`)}
        >
          <PersonIcon sx={{ fontSize: 16, color: '#6B7280' }} />
          <Typography variant="caption" color="text.secondary" sx={{ '&:hover': { color: CORAL } }}>
            {t('pets.volunteer')}
          </Typography>
        </Box>
      </CardContent>

      <CardActions sx={{ px: 2, pb: 2, justifyContent: 'space-between' }}>
        <Button
          variant="contained"
          size="small"
          onClick={() => navigate(`/pets/${pet.id}`)}
          sx={{
            bgcolor: CORAL,
            '&:hover': { bgcolor: '#e55555' },
            borderRadius: 2,
            textTransform: 'none',
            fontWeight: 600,
          }}
        >
          {t('pets.details')}
        </Button>

        <Box sx={{ display: 'flex' }}>
          <Tooltip title={t('pets.share')}>
            <IconButton onClick={handleShare} sx={{ color: '#9CA3AF', '&:hover': { color: CORAL } }}>
              <ShareIcon />
            </IconButton>
          </Tooltip>

          <Tooltip title={compareMaxed ? t('pets.compareBar.hint') : compared ? t('pets.removeFromCompare') : t('pets.addToCompare')}>
            <span>
              <IconButton
                onClick={() => toggleCompare(pet)}
                disabled={compareMaxed}
                sx={{ color: compared ? '#6366F1' : '#9CA3AF', '&:hover': { color: '#6366F1' }, '&.Mui-disabled': { color: '#D1D5DB' } }}
              >
                <CompareArrowsIcon />
              </IconButton>
            </span>
          </Tooltip>

          <Tooltip title={saved ? t('pets.removeFromFavorites') : t('pets.saveToFavorites')}>
            <IconButton onClick={handleToggleFavorite} sx={{ color: saved ? CORAL : '#9CA3AF' }}>
              {saved ? <StarIcon /> : <StarBorderIcon />}
            </IconButton>
          </Tooltip>
        </Box>
      </CardActions>
    </Card>
  )
}
