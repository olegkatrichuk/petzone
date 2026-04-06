import { useState } from 'react'
import { getApiError } from '../lib/getApiError'
import { LangLink as Link } from '../components/ui/LangLink'
import { useLangNavigate } from '../hooks/useLangNavigate'
import PageMeta from '../components/meta/PageMeta'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useTranslation } from 'react-i18next'
import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Paper from '@mui/material/Paper'
import Typography from '@mui/material/Typography'
import TextField from '@mui/material/TextField'
import Button from '@mui/material/Button'
import Alert from '@mui/material/Alert'
import CircularProgress from '@mui/material/CircularProgress'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import LockResetIcon from '@mui/icons-material/LockReset'
import { forgotPassword } from '../api/auth'

const CORAL = '#FF6B6B'

const fieldSx = {
  '& .MuiOutlinedInput-root.Mui-focused .MuiOutlinedInput-notchedOutline': { borderColor: CORAL },
  '& .MuiInputLabel-root.Mui-focused': { color: CORAL },
}

export default function ForgotPasswordPage() {
  const { t } = useTranslation()
  const navigate = useLangNavigate()
  const [sent, setSent] = useState(false)
  const [serverError, setServerError] = useState<string | null>(null)

  const schema = z.object({
    email: z.string().min(1, t('validation.emailRequired')).email(t('validation.emailInvalid')),
  })

  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm({
    resolver: zodResolver(schema),
    defaultValues: { email: '' },
  })

  const onSubmit = async ({ email }: { email: string }) => {
    setServerError(null)
    try {
      await forgotPassword(email)
      setSent(true)
    } catch (err) {
      setServerError(getApiError(err, t))
    }
  }

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'background.default', display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', py: 4 }}>
      <PageMeta title={t('auth.forgotPassword')} description={t('auth.forgotPassword')} path="/forgot-password" noIndex />
      <Container maxWidth="xs">
        <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/login')}
          sx={{ mb: 3, color: '#6B7280', textTransform: 'none' }}>
          {t('auth.backToLogin')}
        </Button>

        <Paper elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 4, p: 4 }}>
          <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', mb: 3 }}>
            <Box sx={{ width: 56, height: 56, borderRadius: '50%', bgcolor: '#FFF0F0', display: 'flex', alignItems: 'center', justifyContent: 'center', mb: 1.5 }}>
              <LockResetIcon sx={{ color: CORAL, fontSize: 30 }} />
            </Box>
            <Typography variant="h5" fontWeight="bold">{t('auth.forgotPassword')}</Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5, textAlign: 'center' }}>
              {t('auth.forgotPasswordHint')}
            </Typography>
          </Box>

          {sent ? (
            <Alert severity="success" sx={{ borderRadius: 2 }}>
              {t('auth.forgotPasswordSent')}
            </Alert>
          ) : (
            <>
              {serverError && <Alert severity="error" sx={{ mb: 2 }}>{serverError}</Alert>}

              <Box component="form" onSubmit={handleSubmit(onSubmit)} sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                <TextField
                  {...register('email')}
                  label={t('auth.email')}
                  type="email"
                  fullWidth
                  error={!!errors.email}
                  helperText={errors.email?.message}
                  autoComplete="email"
                  autoFocus
                  sx={fieldSx}
                />

                <Button type="submit" variant="contained" fullWidth disabled={isSubmitting}
                  sx={{ bgcolor: CORAL, '&:hover': { bgcolor: '#e55555' }, '&.Mui-disabled': { bgcolor: '#FFA0A0' }, borderRadius: 2, textTransform: 'none', fontWeight: 700, py: 1.4, mt: 1 }}>
                  {isSubmitting ? <CircularProgress size={22} sx={{ color: 'white' }} /> : t('auth.sendResetLink')}
                </Button>
              </Box>
            </>
          )}

          <Box sx={{ mt: 3, textAlign: 'center' }}>
            <Typography variant="body2" color="text.secondary">
              {t('auth.rememberPassword')}{' '}
              <Link to="/login" style={{ color: CORAL, fontWeight: 600, textDecoration: 'none' }}>
                {t('auth.login')}
              </Link>
            </Typography>
          </Box>
        </Paper>
      </Container>
    </Box>
  )
}
