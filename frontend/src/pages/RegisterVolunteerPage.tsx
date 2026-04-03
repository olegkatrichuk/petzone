import { useMemo, useState } from 'react'
import { getApiError } from '../lib/getApiError'
import { LangLink as Link } from '../components/ui/LangLink'
import { useLangNavigate } from '../hooks/useLangNavigate'
import PageMeta from '../components/meta/PageMeta'
import { useForm, useFieldArray } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useTranslation } from 'react-i18next'
import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Paper from '@mui/material/Paper'
import Typography from '@mui/material/Typography'
import TextField from '@mui/material/TextField'
import Button from '@mui/material/Button'
import Tabs from '@mui/material/Tabs'
import Tab from '@mui/material/Tab'
import Snackbar from '@mui/material/Snackbar'
import Alert from '@mui/material/Alert'
import Dialog from '@mui/material/Dialog'
import DialogTitle from '@mui/material/DialogTitle'
import DialogContent from '@mui/material/DialogContent'
import DialogActions from '@mui/material/DialogActions'
import CircularProgress from '@mui/material/CircularProgress'
import Divider from '@mui/material/Divider'
import IconButton from '@mui/material/IconButton'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import AddIcon from '@mui/icons-material/Add'
import DeleteIcon from '@mui/icons-material/Delete'
import { useAuthStore } from '../store/authStore'
import { createVolunteerRequest } from '../api/volunteerRequests'
import type { Envelope } from '../types/api'
import type { AxiosError } from 'axios'

const CORAL = '#FF6B6B'

const fieldSx = {
  '& .MuiOutlinedInput-root.Mui-focused .MuiOutlinedInput-notchedOutline': { borderColor: CORAL },
  '& .MuiInputLabel-root.Mui-focused': { color: CORAL },
}


export default function RegisterVolunteerPage() {
  const { t } = useTranslation()
  const navigate = useLangNavigate()
  const { user } = useAuthStore()
  const [toast, setToast] = useState<string | null>(null)
  const [successOpen, setSuccessOpen] = useState(false)

  const schema = useMemo(
    () =>
      z.object({
        experience: z
          .number()
          .min(0, t('validation.experienceMin'))
          .max(100, t('validation.experienceMax')),
        motivation: z
          .string()
          .min(20, t('validation.motivationMin'))
          .max(2000, t('validation.motivationMax')),
        certificates: z
          .array(z.object({ value: z.string().min(1, t('validation.required')) }))
          .default([]),
        requisites: z
          .array(z.object({ value: z.string().min(1, t('validation.required')) }))
          .default([]),
      }),
    [t],
  )

  const {
    register,
    handleSubmit,
    control,
    formState: { errors, isSubmitting },
  } = useForm({
    resolver: zodResolver(schema),
    mode: 'onChange',
    defaultValues: { experience: 0, motivation: '', certificates: [] as { value: string }[], requisites: [] as { value: string }[] },
  })

  const {
    fields: certFields,
    append: appendCert,
    remove: removeCert,
  } = useFieldArray({ control, name: 'certificates' })

  const {
    fields: reqFields,
    append: appendReq,
    remove: removeReq,
  } = useFieldArray({ control, name: 'requisites' })

  const onSubmit = async (values: { experience: number; motivation: string; certificates: { value: string }[]; requisites: { value: string }[] }) => {
    try {
      await createVolunteerRequest({
        experience: values.experience,
        motivation: values.motivation,
        certificates: values.certificates.map((c) => c.value),
        requisites: values.requisites.map((r) => r.value),
      })
      setSuccessOpen(true)
    } catch (err) {
      setToast(getApiError(err, t))
    }
  }

  const handleSuccessClose = () => {
    setSuccessOpen(false)
    navigate('/')
  }

  // Auth guard — must be logged in to submit a volunteer request
  if (!user) {
    return (
      <Box sx={{ minHeight: '100vh', bgcolor: '#FAFAFA', py: 8 }}>
        <Container maxWidth="sm" sx={{ textAlign: 'center' }}>
          <Typography variant="h6" sx={{ mb: 2 }}>
            {t('volunteerRequest.loginRequiredText')}
          </Typography>
          <Button
            component={Link}
            to="/login"
            variant="contained"
            sx={{
              bgcolor: CORAL,
              '&:hover': { bgcolor: '#e55555' },
              textTransform: 'none',
              fontWeight: 700,
              borderRadius: 2,
              px: 4,
            }}
          >
            {t('volunteerRequest.loginBtn')}
          </Button>
        </Container>
      </Box>
    )
  }

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: '#FAFAFA', py: 4 }}>
      <PageMeta title={t('volunteerRequest.tabVolunteer')} description={t('volunteerRequest.infoText')} path="/register/volunteer" noIndex />
      <Container maxWidth="sm">
        <Button
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate('/')}
          sx={{ mb: 3, color: '#6B7280', textTransform: 'none' }}
        >
          {t('common.back')}
        </Button>

        {!user && (
          <Tabs
            value={1}
            centered
            TabIndicatorProps={{ style: { backgroundColor: CORAL } }}
            sx={{ mb: 3, '& .MuiTab-root.Mui-selected': { color: CORAL } }}
          >
            <Tab label={t('volunteerRequest.tabUser')} component={Link} to="/register" />
            <Tab label={t('volunteerRequest.tabVolunteer')} />
          </Tabs>
        )}

        <Paper
          elevation={0}
          sx={{ bgcolor: '#FFF0F0', border: '1px solid #FFCDD2', borderRadius: 2, p: 2.5, mb: 3 }}
        >
          <Typography variant="body2" color="text.secondary">
            {t('volunteerRequest.infoText')}
          </Typography>
        </Paper>

        <Box
          component="form"
          onSubmit={handleSubmit(onSubmit)}
          sx={{ display: 'flex', flexDirection: 'column', gap: 2.5 }}
        >
          {/* Applicant info (read-only) */}
          <Box sx={{ bgcolor: '#F9FAFB', border: '1px solid #E5E7EB', borderRadius: 2, p: 2 }}>
            <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', letterSpacing: 0.5, display: 'block', mb: 0.5 }}>
              {t('volunteerRequest.applicantInfo')}
            </Typography>
            <Typography variant="body2" fontWeight={600}>
              {[user.firstName, user.lastName].filter(Boolean).join(' ')}
            </Typography>
            <Typography variant="body2" color="text.secondary">{user.email}</Typography>
          </Box>

          {/* Experience */}
          <TextField
            {...register('experience', { valueAsNumber: true })}
            label={t('volunteerRequest.experience')}
            type="number"
            fullWidth
            error={!!errors.experience}
            helperText={errors.experience?.message}
            inputProps={{ min: 0, max: 100 }}
            sx={fieldSx}
          />

          {/* Motivation */}
          <TextField
            {...register('motivation')}
            label={t('volunteerRequest.motivation')}
            placeholder={t('volunteerRequest.motivationPlaceholder')}
            multiline
            minRows={4}
            fullWidth
            error={!!errors.motivation}
            helperText={errors.motivation?.message}
            sx={fieldSx}
          />

          {/* Certificates */}
          <Box>
            <Typography variant="subtitle2" sx={{ mb: 1.5, fontWeight: 600 }}>
              {t('volunteerRequest.certificates')}
            </Typography>
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1.5 }}>
              {certFields.map((field, index) => (
                <Box
                  key={field.id}
                  sx={{ display: 'flex', gap: 1, alignItems: 'flex-start' }}
                >
                  <TextField
                    {...register(`certificates.${index}.value`)}
                    placeholder={t('volunteerRequest.certificatePlaceholder')}
                    size="small"
                    fullWidth
                    error={!!errors.certificates?.[index]?.value}
                    helperText={errors.certificates?.[index]?.value?.message}
                    sx={fieldSx}
                  />
                  <IconButton
                    onClick={() => removeCert(index)}
                    size="small"
                    sx={{ mt: 0.5, color: '#6B7280', flexShrink: 0 }}
                  >
                    <DeleteIcon />
                  </IconButton>
                </Box>
              ))}
              <Button
                startIcon={<AddIcon />}
                onClick={() => appendCert({ value: '' })}
                variant="outlined"
                sx={{
                  borderColor: '#E5E7EB',
                  color: '#6B7280',
                  textTransform: 'none',
                  borderRadius: 5,
                  alignSelf: 'flex-start',
                  '&:hover': { borderColor: CORAL, color: CORAL },
                }}
              >
                {t('volunteerRequest.addCertificate')}
              </Button>
            </Box>
          </Box>

          {/* Requisites */}
          <Box>
            <Typography variant="subtitle2" sx={{ mb: 1.5, fontWeight: 600 }}>
              {t('volunteerRequest.requisites')}
            </Typography>
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1.5 }}>
              {reqFields.map((field, index) => (
                <Box
                  key={field.id}
                  sx={{ display: 'flex', gap: 1, alignItems: 'flex-start' }}
                >
                  <TextField
                    {...register(`requisites.${index}.value`)}
                    placeholder={t('volunteerRequest.requisitePlaceholder')}
                    size="small"
                    fullWidth
                    error={!!errors.requisites?.[index]?.value}
                    helperText={errors.requisites?.[index]?.value?.message}
                    sx={fieldSx}
                  />
                  <IconButton
                    onClick={() => removeReq(index)}
                    size="small"
                    sx={{ mt: 0.5, color: '#6B7280', flexShrink: 0 }}
                  >
                    <DeleteIcon />
                  </IconButton>
                </Box>
              ))}
              <Button
                startIcon={<AddIcon />}
                onClick={() => appendReq({ value: '' })}
                variant="outlined"
                sx={{
                  borderColor: '#E5E7EB',
                  color: '#6B7280',
                  textTransform: 'none',
                  borderRadius: 5,
                  alignSelf: 'flex-start',
                  '&:hover': { borderColor: CORAL, color: CORAL },
                }}
              >
                {t('volunteerRequest.addRequisite')}
              </Button>
            </Box>
          </Box>

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
              t('volunteerRequest.submit')
            )}
          </Button>
        </Box>

        {!user && (
          <>
            <Divider sx={{ my: 3 }} />
            <Typography variant="body2" color="text.secondary" textAlign="center">
              {t('volunteerRequest.alreadyRegistered')}{' '}
              <Link to="/login" style={{ color: CORAL, fontWeight: 600, textDecoration: 'none' }}>
                {t('volunteerRequest.login')}
              </Link>
            </Typography>
          </>
        )}
      </Container>

      <Dialog open={successOpen} onClose={handleSuccessClose}>
        <DialogTitle>{t('volunteerRequest.successTitle')}</DialogTitle>
        <DialogContent>
          <Typography>{t('volunteerRequest.successText')}</Typography>
        </DialogContent>
        <DialogActions>
          <Button
            onClick={handleSuccessClose}
            sx={{ color: CORAL, fontWeight: 700, textTransform: 'none' }}
          >
            {t('volunteerRequest.toHome')}
          </Button>
        </DialogActions>
      </Dialog>

      <Snackbar
        open={!!toast}
        autoHideDuration={5000}
        onClose={() => setToast(null)}
        anchorOrigin={{ vertical: 'top', horizontal: 'center' }}
      >
        <Alert severity="error" onClose={() => setToast(null)}>
          {toast}
        </Alert>
      </Snackbar>
    </Box>
  )
}
