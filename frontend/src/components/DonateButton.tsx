import { useTranslation } from 'react-i18next'
import Button from '@mui/material/Button'
import FavoriteIcon from '@mui/icons-material/Favorite'

const DONATE_URL = 'https://www.privat24.ua/send/jypcd'

export function DonateButton() {
  const { t } = useTranslation()

  return (
    <Button
      href={DONATE_URL}
      target="_blank"
      rel="noopener noreferrer"
      variant="contained"
      startIcon={<FavoriteIcon />}
      sx={{
        bgcolor: '#FF6B6B',
        color: 'white',
        textTransform: 'none',
        fontWeight: 600,
        borderRadius: 2,
        px: 2.5,
        '&:hover': { bgcolor: '#ff5252' },
      }}
    >
      {t('footer.support')}
    </Button>
  )
}
