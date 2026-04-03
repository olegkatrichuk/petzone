import { useState } from 'react'
import { getApiError } from '../lib/getApiError'
import { LangLink as Link } from '../components/ui/LangLink'
import { useLangNavigate } from '../hooks/useLangNavigate'
import PageMeta from '../components/meta/PageMeta'
import Tabs from '@mui/material/Tabs'
import Tab from '@mui/material/Tab'
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
import Snackbar from '@mui/material/Snackbar'
import Alert from '@mui/material/Alert'
import CircularProgress from '@mui/material/CircularProgress'
import Divider from '@mui/material/Divider'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import VisibilityIcon from '@mui/icons-material/Visibility'
import VisibilityOffIcon from '@mui/icons-material/VisibilityOff'
import PetsIcon from '@mui/icons-material/Pets'
import { registerUser } from '../api/auth'
import type { Envelope } from '../types/api'
import type { AxiosError } from 'axios'

const CORAL = '#FF6B6B'

const fieldSx = {
  '& .MuiOutlinedInput-root.Mui-focused .MuiOutlinedInput-notchedOutline': { borderColor: CORAL },
  '& .MuiInputLabel-root.Mui-focused': { color: CORAL },
}

function buildSchema(t: (k: string) => string) {
  return z
    .object({
      firstName: z.string().min(1, t('validation.firstNameRequired')),
      lastName: z.string().min(1, t('validation.lastNameRequired')),
      email: z.string().min(1, t('validation.emailRequired')).email(t('validation.emailInvalid')),
      password: z.string().min(6, t('validation.passwordMin')),
      confirmPassword: z.string().min(1, t('validation.confirmPasswordRequired')),
    })
    .refine((d) => d.password === d.confirmPassword, {
      message: t('validation.passwordsMismatch'),
      path: ['confirmPassword'],
    })
}

type FormValues = z.infer<ReturnType<typeof buildSchema>>

interface PasswordFieldProps {
  label: string
  error?: string
  registration: object
  autoComplete?: string
}

function PasswordField({ label, error, registration, autoComplete }: PasswordFieldProps) {
  const [show, setShow] = useState(false)
  return (
    <TextField
      {...registration}
      label={label}
      type={show ? 'text' : 'password'}
      fullWidth
      error={!!error}
      helperText={error}
      autoComplete={autoComplete}
      slotProps={{
        input: {
          endAdornment: (
            <InputAdornment position="end">
              <IconButton onClick={() => setShow((v) => !v)} edge="end" size="small">
                {show ? <VisibilityOffIcon /> : <VisibilityIcon />}
              </IconButton>
            </InputAdornment>
          ),
        },
      }}
      sx={fieldSx}
    />
  )
}

export default function RegisterPage() {
  const { t } = useTranslation()
  const navigate = useLangNavigate()
  const [toast, setToast] = useState<string | null>(null)

  const schema = buildSchema(t)
  const { register, handleSubmit, formState: { errors, isSubmitting } } =
    useForm<FormValues>({ resolver: zodResolver(schema), mode: 'onChange' })

  const onSubmit = async (values: FormValues) => {
    try {
      await registerUser({
        firstName: values.firstName,
        lastName: values.lastName,
        email: values.email,
        password: values.password,
      })
      navigate('/login')
    } catch (err) {
      setToast(getApiError(err, t))
    }
  }

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: '#FAFAFA', display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', py: 4 }}>
      <PageMeta title={t('pages.register.title')} description={t('pages.register.title')} path="/register" noIndex />
      <Container maxWidth="xs">
        <Button
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate('/')}
          sx={{ mb: 3, color: '#6B7280', textTransform: 'none' }}
        >
          {t('nav.home')}
        </Button>

        <Tabs
          value={0}
          centered
          TabIndicatorProps={{ style: { backgroundColor: CORAL } }}
          sx={{ mb: 3, '& .MuiTab-root.Mui-selected': { color: CORAL } }}
        >
          <Tab label={t('register.tabUser')} />
          <Tab label={t('register.tabVolunteer')} component={Link} to="/register/volunteer" />
        </Tabs>

        <Paper elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 4, p: 4 }}>
          {/* Logo */}
          <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', mb: 3 }}>
            <Box sx={{ width: 56, height: 56, borderRadius: '50%', bgcolor: '#FFF0F0', display: 'flex', alignItems: 'center', justifyContent: 'center', mb: 1.5 }}>
              <PetsIcon sx={{ color: CORAL, fontSize: 30 }} />
            </Box>
            <Typography variant="h5" fontWeight="bold">{t('pages.register.title')}</Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>PetZone</Typography>
          </Box>

          <Box component="form" onSubmit={handleSubmit(onSubmit)} sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
            <Box sx={{ display: 'flex', gap: 2, flexDirection: { xs: 'column', sm: 'row' } }}>
              <TextField
                {...register('firstName')}
                label={t('auth.firstName')}
                fullWidth error={!!errors.firstName} helperText={errors.firstName?.message} sx={fieldSx}
              />
              <TextField
                {...register('lastName')}
                label={t('auth.lastName')}
                fullWidth error={!!errors.lastName} helperText={errors.lastName?.message} sx={fieldSx}
              />
            </Box>

            <TextField
              {...register('email')}
              label={t('auth.email')}
              type="email" fullWidth
              error={!!errors.email} helperText={errors.email?.message}
              autoComplete="email" sx={fieldSx}
            />

            <PasswordField
              label={t('auth.password')}
              error={errors.password?.message}
              registration={register('password')}
              autoComplete="new-password"
            />

            <PasswordField
              label={t('auth.confirmPassword')}
              error={errors.confirmPassword?.message}
              registration={register('confirmPassword')}
              autoComplete="new-password"
            />

            <Button
              type="submit" variant="contained" fullWidth disabled={isSubmitting}
              sx={{ bgcolor: CORAL, '&:hover': { bgcolor: '#e55555' }, '&.Mui-disabled': { bgcolor: '#FFA0A0' }, borderRadius: 2, textTransform: 'none', fontWeight: 700, py: 1.4, mt: 1 }}
            >
              {isSubmitting
                ? <CircularProgress size={22} sx={{ color: 'white' }} />
                : t('auth.registerBtn')}
            </Button>
          </Box>

          <Divider sx={{ my: 3 }} />

          <Typography variant="body2" color="text.secondary" textAlign="center">
            {t('auth.hasAccount')}{' '}
            <Link to="/login" style={{ color: CORAL, fontWeight: 600, textDecoration: 'none' }}>
              {t('auth.login')}
            </Link>
          </Typography>
        </Paper>
      </Container>

      <Snackbar
        open={!!toast}
        autoHideDuration={5000}
        onClose={() => setToast(null)}
        anchorOrigin={{ vertical: 'top', horizontal: 'center' }}
      >
        <Alert severity="error" onClose={() => setToast(null)}>{toast}</Alert>
      </Snackbar>
    </Box>
  )
}
