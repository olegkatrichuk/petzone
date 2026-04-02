import { useState } from 'react'
import { getApiError } from '../lib/getApiError'
import { Link } from 'react-router-dom'
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
import IconButton from '@mui/material/IconButton'
import InputAdornment from '@mui/material/InputAdornment'
import Alert from '@mui/material/Alert'
import CircularProgress from '@mui/material/CircularProgress'
import Divider from '@mui/material/Divider'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import VisibilityIcon from '@mui/icons-material/Visibility'
import VisibilityOffIcon from '@mui/icons-material/VisibilityOff'
import PetsIcon from '@mui/icons-material/Pets'
import { loginUser } from '../api/auth'
import { useAuthStore } from '../store/authStore'
import type { Envelope } from '../types/api'
import type { AxiosError } from 'axios'

const CORAL = '#FF6B6B'

function buildSchema(t: (key: string) => string) {
  return z.object({
    email: z
      .string()
      .min(1, t('validation.emailRequired'))
      .email(t('validation.emailInvalid')),
    password: z
      .string()
      .min(1, t('validation.passwordRequired')),
  })
}

type FormValues = { email: string; password: string }

export default function LoginPage() {
  const { t } = useTranslation()
  const navigate = useLangNavigate()
  const setAccessToken = useAuthStore((s) => s.setAccessToken)

  const [showPassword, setShowPassword] = useState(false)
  const [serverError, setServerError] = useState<string | null>(null)

  const schema = buildSchema(t)

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    mode: 'onChange',
  })

  const onSubmit = async (values: FormValues) => {
    setServerError(null)
    try {
      const token = await loginUser(values)
      setAccessToken(token)
      navigate('/')
    } catch (err) {
      setServerError(getApiError(err, t))
    }
  }

  return (
    <Box
      sx={{
        minHeight: '100vh',
        bgcolor: '#FAFAFA',
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        py: 4,
      }}
    >
      <PageMeta title={t('pages.login.title')} description={t('pages.login.title')} path="/login" noIndex />
      <Container maxWidth="xs">
        {/* Back button */}
        <Button
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate('/')}
          sx={{ mb: 3, color: '#6B7280', textTransform: 'none' }}
        >
          {t('nav.home')}
        </Button>

        <Paper
          elevation={0}
          sx={{ border: '1px solid #E5E7EB', borderRadius: 4, p: 4 }}
        >
          {/* Logo */}
          <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', mb: 3 }}>
            <Box
              sx={{
                width: 56, height: 56, borderRadius: '50%',
                bgcolor: '#FFF0F0', display: 'flex', alignItems: 'center', justifyContent: 'center', mb: 1.5,
              }}
            >
              <PetsIcon sx={{ color: CORAL, fontSize: 30 }} />
            </Box>
            <Typography variant="h5" fontWeight="bold">
              {t('pages.login.title')}
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
              PetZone
            </Typography>
          </Box>

          {/* Server error */}
          {serverError && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {serverError}
            </Alert>
          )}

          <Box
            component="form"
            onSubmit={handleSubmit(onSubmit)}
            sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}
          >
            {/* Email */}
            <TextField
              {...register('email')}
              label={t('auth.email')}
              type="email"
              fullWidth
              error={!!errors.email}
              helperText={errors.email?.message}
              autoComplete="email"
              autoFocus
              sx={{
                '& .MuiOutlinedInput-root.Mui-focused .MuiOutlinedInput-notchedOutline': {
                  borderColor: CORAL,
                },
                '& .MuiInputLabel-root.Mui-focused': { color: CORAL },
              }}
            />

            {/* Password */}
            <TextField
              {...register('password')}
              label={t('auth.password')}
              type={showPassword ? 'text' : 'password'}
              fullWidth
              error={!!errors.password}
              helperText={errors.password?.message}
              autoComplete="current-password"
              slotProps={{
                input: {
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton
                        onClick={() => setShowPassword((v) => !v)}
                        edge="end"
                        size="small"
                      >
                        {showPassword ? <VisibilityOffIcon /> : <VisibilityIcon />}
                      </IconButton>
                    </InputAdornment>
                  ),
                },
              }}
              sx={{
                '& .MuiOutlinedInput-root.Mui-focused .MuiOutlinedInput-notchedOutline': {
                  borderColor: CORAL,
                },
                '& .MuiInputLabel-root.Mui-focused': { color: CORAL },
              }}
            />

            {/* Submit */}
            <Button
              type="submit"
              variant="contained"
              fullWidth
              disabled={isSubmitting}
              sx={{
                bgcolor: CORAL,
                '&:hover': { bgcolor: '#e55555' },
                '&.Mui-disabled': { bgcolor: '#FFA0A0' },
                borderRadius: 2,
                textTransform: 'none',
                fontWeight: 700,
                py: 1.4,
                mt: 1,
              }}
            >
              {isSubmitting ? (
                <CircularProgress size={22} sx={{ color: 'white' }} />
              ) : (
                t('auth.login')
              )}
            </Button>
          </Box>

          <Divider sx={{ my: 3 }} />

          {/* Register link */}
          <Typography variant="body2" color="text.secondary" textAlign="center">
            {t('auth.noAccount')}{' '}
            <Link
              to="/register"
              style={{ color: CORAL, fontWeight: 600, textDecoration: 'none' }}
            >
              {t('auth.register')}
            </Link>
          </Typography>
        </Paper>
      </Container>
    </Box>
  )
}
