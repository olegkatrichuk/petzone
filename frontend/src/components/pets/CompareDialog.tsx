import { useTranslation } from 'react-i18next'
import Dialog from '@mui/material/Dialog'
import DialogTitle from '@mui/material/DialogTitle'
import DialogContent from '@mui/material/DialogContent'
import IconButton from '@mui/material/IconButton'
import Box from '@mui/material/Box'
import Typography from '@mui/material/Typography'
import Chip from '@mui/material/Chip'
import CloseIcon from '@mui/icons-material/Close'
import CheckIcon from '@mui/icons-material/Check'
import CloseRoundedIcon from '@mui/icons-material/CloseRounded'
import type { Pet } from '../../types/pet'

const CORAL = '#FF6B6B'

const STATUS_LABELS: Record<number, string> = {
  0: 'pets.status.needsHelp',
  1: 'pets.status.lookingForHome',
  2: 'pets.status.foundHome',
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

interface RowProps {
  label: string
  values: React.ReactNode[]
  highlight?: boolean
}

function Row({ label, values, highlight }: RowProps) {
  return (
    <Box
      sx={{
        display: 'grid',
        gridTemplateColumns: `160px repeat(${values.length}, 1fr)`,
        borderBottom: '1px solid #F3F4F6',
        bgcolor: highlight ? '#FFFBF0' : 'transparent',
        '&:hover': { bgcolor: '#FAFAFA' },
      }}
    >
      <Box sx={{ p: 1.5, borderRight: '1px solid #F3F4F6' }}>
        <Typography variant="body2" color="text.secondary" fontWeight={500}>{label}</Typography>
      </Box>
      {values.map((val, i) => (
        <Box key={i} sx={{ p: 1.5, borderRight: i < values.length - 1 ? '1px solid #F3F4F6' : 'none', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
          {val}
        </Box>
      ))}
    </Box>
  )
}

interface Props {
  open: boolean
  onClose: () => void
  pets: Pet[]
}

export default function CompareDialog({ open, onClose, pets }: Props) {
  const { t } = useTranslation()

  if (pets.length === 0) return null

  const mainPhoto = (pet: Pet) =>
    pet.photos.find((p) => p.isMain)?.filePath ?? pet.photos[0]?.filePath ?? 'https://placehold.co/200x160?text=Photo'

  const BoolIcon = ({ val }: { val: boolean }) =>
    val
      ? <CheckIcon sx={{ color: '#22C55E', fontSize: 20 }} />
      : <CloseRoundedIcon sx={{ color: '#9CA3AF', fontSize: 20 }} />

  return (
    <Dialog open={open} onClose={onClose} maxWidth="lg" fullWidth scroll="body">
      <DialogTitle sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', pb: 1 }}>
        <Typography variant="h6" fontWeight="bold">{t('pets.compareDialog.title')}</Typography>
        <IconButton onClick={onClose}><CloseIcon /></IconButton>
      </DialogTitle>

      <DialogContent sx={{ p: 0, overflowX: 'auto' }}>
        <Box sx={{ minWidth: 500 }}>
          {/* Photo header row */}
          <Box
            sx={{
              display: 'grid',
              gridTemplateColumns: `160px repeat(${pets.length}, 1fr)`,
              bgcolor: '#F9FAFB',
              borderBottom: '2px solid #E5E7EB',
            }}
          >
            <Box sx={{ p: 2, borderRight: '1px solid #E5E7EB' }} />
            {pets.map((pet) => (
              <Box key={pet.id} sx={{ p: 2, textAlign: 'center', borderRight: '1px solid #E5E7EB' }}>
                <Box
                  component="img"
                  src={mainPhoto(pet)}
                  alt={pet.nickname}
                  sx={{ width: '100%', maxWidth: 160, height: 120, objectFit: 'cover', borderRadius: 2, mb: 1 }}
                />
                <Typography variant="subtitle1" fontWeight="bold">{pet.nickname}</Typography>
              </Box>
            ))}
          </Box>

          {/* Comparison rows */}
          <Row
            label={t('pets.compareDialog.status')}
            values={pets.map((p) => (
              <Chip
                key={p.id}
                label={t(STATUS_LABELS[p.status])}
                size="small"
                sx={{
                  bgcolor: p.status === 0 ? '#FEF3C7' : p.status === 1 ? '#DBEAFE' : '#D1FAE5',
                  color: p.status === 0 ? '#D97706' : p.status === 1 ? '#2563EB' : '#059669',
                  fontWeight: 600,
                }}
              />
            ))}
          />
          <Row
            label={t('pets.compareDialog.age')}
            values={pets.map((p) => (
              <Typography key={p.id} variant="body2">{calcAge(p.dateOfBirth, t)}</Typography>
            ))}
          />
          <Row
            label={t('pets.compareDialog.weight')}
            values={pets.map((p) => (
              <Typography key={p.id} variant="body2">{p.weight} {t('pets.kg')}</Typography>
            ))}
          />
          <Row
            label={t('pets.compareDialog.height')}
            values={pets.map((p) => (
              <Typography key={p.id} variant="body2">{p.height} {t('pets.cm')}</Typography>
            ))}
          />
          <Row
            label={t('pets.compareDialog.color')}
            values={pets.map((p) => (
              <Chip key={p.id} label={`# ${p.color}`} size="small" sx={{ bgcolor: '#FFF0F0', color: CORAL, fontWeight: 500, fontSize: 11 }} />
            ))}
          />
          <Row
            label={t('pets.compareDialog.city')}
            values={pets.map((p) => (
              <Typography key={p.id} variant="body2">{p.city}</Typography>
            ))}
          />
          <Row
            label={t('pets.compareDialog.vaccinated')}
            values={pets.map((p) => <BoolIcon key={p.id} val={p.isVaccinated} />)}
          />
          <Row
            label={t('pets.compareDialog.castrated')}
            values={pets.map((p) => <BoolIcon key={p.id} val={p.isCastrated} />)}
          />
        </Box>
      </DialogContent>
    </Dialog>
  )
}
