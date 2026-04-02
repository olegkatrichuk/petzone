import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import Box from '@mui/material/Box'
import Paper from '@mui/material/Paper'
import Typography from '@mui/material/Typography'
import Button from '@mui/material/Button'
import IconButton from '@mui/material/IconButton'
import Tooltip from '@mui/material/Tooltip'
import Zoom from '@mui/material/Zoom'
import CloseIcon from '@mui/icons-material/Close'
import CompareArrowsIcon from '@mui/icons-material/CompareArrows'
import { useComparisonStore } from '../../store/comparisonStore'
import CompareDialog from './CompareDialog'

const CORAL = '#FF6B6B'

export default function CompareBar() {
  const { t } = useTranslation()
  const { pets, remove, clear } = useComparisonStore()
  const [dialogOpen, setDialogOpen] = useState(false)

  return (
    <>
      <Zoom in={pets.length > 0}>
        <Paper
          elevation={4}
          sx={{
            position: 'fixed',
            bottom: 80,
            left: '50%',
            transform: 'translateX(-50%)',
            zIndex: 1100,
            display: 'flex',
            alignItems: 'center',
            gap: 1.5,
            px: 2.5,
            py: 1.5,
            borderRadius: 4,
            bgcolor: '#1e1b4b',
            color: 'white',
            flexWrap: 'wrap',
            maxWidth: { xs: '95vw', sm: 600 },
          }}
        >
          <CompareArrowsIcon sx={{ color: CORAL, flexShrink: 0 }} />
          <Typography variant="body2" fontWeight={600} sx={{ flexShrink: 0 }}>
            {t('pets.compareBar.title')} ({pets.length}/3)
          </Typography>

          {/* Pet thumbnails */}
          <Box sx={{ display: 'flex', gap: 1, flex: 1 }}>
            {pets.map((pet) => {
              const photo =
                pet.photos.find((p) => p.isMain)?.filePath ??
                pet.photos[0]?.filePath ??
                'https://placehold.co/40x40?text=?'
              return (
                <Tooltip key={pet.id} title={pet.nickname}>
                  <Box sx={{ position: 'relative', flexShrink: 0 }}>
                    <Box
                      component="img"
                      src={photo}
                      alt={pet.nickname}
                      sx={{ width: 40, height: 40, objectFit: 'cover', borderRadius: 1.5, border: '2px solid rgba(255,255,255,0.3)' }}
                    />
                    <IconButton
                      size="small"
                      onClick={() => remove(pet.id)}
                      sx={{
                        position: 'absolute',
                        top: -8,
                        right: -8,
                        bgcolor: '#374151',
                        color: 'white',
                        width: 18,
                        height: 18,
                        '&:hover': { bgcolor: CORAL },
                      }}
                    >
                      <CloseIcon sx={{ fontSize: 12 }} />
                    </IconButton>
                  </Box>
                </Tooltip>
              )
            })}
          </Box>

          <Box sx={{ display: 'flex', gap: 1, flexShrink: 0 }}>
            <Button
              variant="text"
              size="small"
              onClick={clear}
              sx={{ color: 'rgba(255,255,255,0.6)', textTransform: 'none', '&:hover': { color: 'white' } }}
            >
              {t('pets.compareBar.clear')}
            </Button>
            <Button
              variant="contained"
              size="small"
              disabled={pets.length < 2}
              onClick={() => setDialogOpen(true)}
              sx={{
                bgcolor: CORAL,
                '&:hover': { bgcolor: '#e55555' },
                '&.Mui-disabled': { bgcolor: 'rgba(255,107,107,0.3)', color: 'rgba(255,255,255,0.5)' },
                textTransform: 'none',
                fontWeight: 700,
                borderRadius: 2,
              }}
            >
              {t('pets.compareBar.compare')}
            </Button>
          </Box>
        </Paper>
      </Zoom>

      <CompareDialog open={dialogOpen} onClose={() => setDialogOpen(false)} pets={pets} />
    </>
  )
}
