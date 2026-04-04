import { useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { getApiError } from '../lib/getApiError'
import { LangLink as Link } from '../components/ui/LangLink'
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
import IconButton from '@mui/material/IconButton'
import InputAdornment from '@mui/material/InputAdornment'
import LockResetIcon from '@mui/icons-material/LockReset'
import VisibilityIcon from '@mui/icons-material/Visibility'
import VisibilityOffIcon from '@mui/icons-material/VisibilityOff'
import { resetPassword } from '../api/auth'

const CORAL = '#FF6B6B'

const fieldSx = {
  '& .MuiOutlinedInput-root.Mui-focused .MuiOutlinedInput-notchedOutline': { borderColor: CORAL },
  '& .MuiInputLabel-root.Mui-focused': { color: CORAL },
}

export default function ResetPasswordPage() {
  const { t } = useTranslation()
  const [searchParams] = useSearchParams()
  const [showPassword, setShowPassword] = useState(false)
  const [showConfirm, setShowConfirm] = useState(false)
  const [done, setDone] = useState(false)
  const [serverError, setServerError] = useState<string | null>(null)

  const userId = searchParams.get('userId') ?? ''
  const token = searchParams.get('token') ?? ''

  const schema = z.object({
    password: z.string().min(8, t('validation.passwordMin')),
    confirmPassword: z.string().min(1, t('validation.passwordRequired')),
  }).refine((d) => d.password === d.confirmPassword, {
    message: t('validation.passwordMismatch'),
    path: ['confirmPassword'],
  })

  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm({
    resolver: zodResolver(schema),
    defaultValues: { password: '', confirmPassword: '' },
  })

  const onSubmit = async ({ password }: { password: string; confirmPassword: string }) => {
    setServerError(null)
    try {
      await resetPassword(userId, token, password)
      setDone(true)
    } catch (err) {
      setServerError(getApiError(err, t))
    }
  }

  if (!userId || !token) {
    return (
      <Box sx={{ minHeight: '100vh', bgcolor: '#FAFAFA', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
        <Container maxWidth="xs" sx={{ textAlign: 'center' }}>
          <Alert severity="error">{t('auth.resetLinkInvalid')}</Alert>
          <Button component={Link} to="/forgot-password" sx={{ mt: 2, color: CORAL, textTransform: 'none' }}>
            {t('auth.requestNewLink')}
          </Button>
        </Container>
      </Box>
    )
  }

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: '#FAFAFA', display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', py: 4 }}>
      <PageMeta title={t('auth.resetPassword')} description={t('auth.resetPassword')} path="/reset-password" noIndex />
      <Container maxWidth="xs">
        <Paper elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 4, p: 4 }}>
          <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', mb: 3 }}>
            <Box sx={{ width: 56, height: 56, borderRadius: '50%', bgcolor: '#FFF0F0', display: 'flex', alignItems: 'center', justifyContent: 'center', mb: 1.5 }}>
              <LockResetIcon sx={{ color: CORAL, fontSize: 30 }} />
            </Box>
            <Typography variant="h5" fontWeight="bold">{t('auth.resetPassword')}</Typography>
          </Box>

          {done ? (
            <>
              <Alert severity="success" sx={{ mb: 3, borderRadius: 2 }}>
                {t('auth.resetPasswordSuccess')}
              </Alert>
              <Button component={Link} to="/login" variant="contained" fullWidth
                sx={{ bgcolor: CORAL, '&:hover': { bgcolor: '#e55555' }, borderRadius: 2, textTransform: 'none', fontWeight: 700, py: 1.4 }}>
                {t('auth.login')}
              </Button>
            </>
          ) : (
            <>
              {serverError && <Alert severity="error" sx={{ mb: 2 }}>{serverError}</Alert>}

              <Box component="form" onSubmit={handleSubmit(onSubmit)} sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                <TextField
                  {...register('password')}
                  label={t('auth.newPassword')}
                  type={showPassword ? 'text' : 'password'}
                  fullWidth
                  error={!!errors.password}
                  helperText={errors.password?.message}
                  autoFocus
                  sx={fieldSx}
                  slotProps={{
                    input: {
                      endAdornment: (
                        <InputAdornment position="end">
                          <IconButton onClick={() => setShowPassword((v) => !v)} edge="end" size="small">
                            {showPassword ? <VisibilityOffIcon /> : <VisibilityIcon />}
                          </IconButton>
                        </InputAdornment>
                      ),
                    },
                  }}
                />

                <TextField
                  {...register('confirmPassword')}
                  label={t('auth.confirmPassword')}
                  type={showConfirm ? 'text' : 'password'}
                  fullWidth
                  error={!!errors.confirmPassword}
                  helperText={errors.confirmPassword?.message}
                  sx={fieldSx}
                  slotProps={{
                    input: {
                      endAdornment: (
                        <InputAdornment position="end">
                          <IconButton onClick={() => setShowConfirm((v) => !v)} edge="end" size="small">
                            {showConfirm ? <VisibilityOffIcon /> : <VisibilityIcon />}
                          </IconButton>
                        </InputAdornment>
                      ),
                    },
                  }}
                />

                <Button type="submit" variant="contained" fullWidth disabled={isSubmitting}
                  sx={{ bgcolor: CORAL, '&:hover': { bgcolor: '#e55555' }, '&.Mui-disabled': { bgcolor: '#FFA0A0' }, borderRadius: 2, textTransform: 'none', fontWeight: 700, py: 1.4, mt: 1 }}>
                  {isSubmitting ? <CircularProgress size={22} sx={{ color: 'white' }} /> : t('auth.resetPasswordBtn')}
                </Button>
              </Box>
            </>
          )}
        </Paper>
      </Container>
    </Box>
  )
}
