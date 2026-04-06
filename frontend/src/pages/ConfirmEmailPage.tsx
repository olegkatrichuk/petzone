import { useEffect, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { LangLink as Link } from '../components/ui/LangLink'
import PageMeta from '../components/meta/PageMeta'
import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Paper from '@mui/material/Paper'
import Typography from '@mui/material/Typography'
import Button from '@mui/material/Button'
import CircularProgress from '@mui/material/CircularProgress'
import CheckCircleOutlineIcon from '@mui/icons-material/CheckCircleOutline'
import ErrorOutlineIcon from '@mui/icons-material/ErrorOutline'
import { confirmEmail } from '../api/auth'

const CORAL = '#FF6B6B'

export default function ConfirmEmailPage() {
  const { t } = useTranslation()
  const [searchParams] = useSearchParams()
  const [status, setStatus] = useState<'loading' | 'success' | 'error'>('loading')

  const userId = searchParams.get('userId') ?? ''
  const token = searchParams.get('token') ?? ''

  useEffect(() => {
    if (!userId || !token) {
      setStatus('error')
      return
    }

    confirmEmail(userId, token)
      .then(() => setStatus('success'))
      .catch(() => setStatus('error'))
  }, [userId, token])

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'background.default', display: 'flex', alignItems: 'center', justifyContent: 'center', py: 4 }}>
      <PageMeta title={t('auth.confirmEmail')} description={t('auth.confirmEmail')} path="/confirm-email" noIndex />
      <Container maxWidth="xs">
        <Paper elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 4, p: 5, textAlign: 'center' }}>

          {status === 'loading' && (
            <>
              <CircularProgress sx={{ color: CORAL, mb: 2 }} />
              <Typography variant="body1" color="text.secondary">
                {t('auth.confirmingEmail')}
              </Typography>
            </>
          )}

          {status === 'success' && (
            <>
              <CheckCircleOutlineIcon sx={{ fontSize: 64, color: '#10B981', mb: 2 }} />
              <Typography variant="h5" fontWeight="bold" sx={{ mb: 1 }}>
                {t('auth.emailConfirmed')}
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
                {t('auth.emailConfirmedHint')}
              </Typography>
              <Button
                component={Link}
                to="/login"
                variant="contained"
                fullWidth
                sx={{ bgcolor: CORAL, '&:hover': { bgcolor: '#e55555' }, borderRadius: 2, textTransform: 'none', fontWeight: 700, py: 1.4 }}
              >
                {t('auth.login')}
              </Button>
            </>
          )}

          {status === 'error' && (
            <>
              <ErrorOutlineIcon sx={{ fontSize: 64, color: '#EF4444', mb: 2 }} />
              <Typography variant="h5" fontWeight="bold" sx={{ mb: 1 }}>
                {t('auth.confirmEmailFailed')}
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
                {t('auth.confirmEmailFailedHint')}
              </Typography>
              <Button
                component={Link}
                to="/"
                variant="outlined"
                fullWidth
                sx={{ borderColor: CORAL, color: CORAL, borderRadius: 2, textTransform: 'none', fontWeight: 600, py: 1.2 }}
              >
                {t('nav.home')}
              </Button>
            </>
          )}

        </Paper>
      </Container>
    </Box>
  )
}
