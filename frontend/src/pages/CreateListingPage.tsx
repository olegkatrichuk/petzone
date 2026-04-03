import { useMemo, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useLangNavigate } from '../hooks/useLangNavigate'
import PageMeta from '../components/meta/PageMeta'
import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Paper from '@mui/material/Paper'
import Typography from '@mui/material/Typography'
import TextField from '@mui/material/TextField'
import Button from '@mui/material/Button'
import MenuItem from '@mui/material/MenuItem'
import FormControlLabel from '@mui/material/FormControlLabel'
import Switch from '@mui/material/Switch'
import CircularProgress from '@mui/material/CircularProgress'
import Snackbar from '@mui/material/Snackbar'
import Alert from '@mui/material/Alert'
import IconButton from '@mui/material/IconButton'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import AddPhotoAlternateIcon from '@mui/icons-material/AddPhotoAlternate'
import DeleteIcon from '@mui/icons-material/Delete'
import CheckCircleIcon from '@mui/icons-material/CheckCircle'
import { useGetSpeciesQuery, useGetBreedsQuery } from '../services/speciesApi'
import { useCreateListingMutation, useAddListingPhotoMutation, useRemoveListingPhotoMutation } from '../services/listingsApi'
import { useAuthStore } from '../store/authStore'
import { Link } from 'react-router-dom'
import { getApiError } from '../lib/getApiError'
import { api } from '../lib/axios'

const CORAL = '#FF6B6B'

const fieldSx = {
  '& .MuiOutlinedInput-root.Mui-focused .MuiOutlinedInput-notchedOutline': { borderColor: CORAL },
  '& .MuiInputLabel-root.Mui-focused': { color: CORAL },
}

type FormValues = {
  title: string
  description: string
  speciesId: string
  breedId: string
  ageMonths: number
  color: string
  city: string
  vaccinated: boolean
  castrated: boolean
  phone: string
  contactEmail: string
}

export default function CreateListingPage() {
  const { t, i18n } = useTranslation()
  const navigate = useLangNavigate()
  const { user } = useAuthStore()
  const [toast, setToast] = useState<string | null>(null)
  const [createListing] = useCreateListingMutation()
  const [addPhoto] = useAddListingPhotoMutation()
  const [removePhoto] = useRemoveListingPhotoMutation()
  const fileInputRef = useRef<HTMLInputElement>(null)

  // After creation — show photo upload step
  const [createdId, setCreatedId] = useState<string | null>(null)
  const [photos, setPhotos] = useState<string[]>([])
  const [uploading, setUploading] = useState(false)

  const locale = i18n.language?.slice(0, 2) || 'uk'
  const { data: speciesList = [] } = useGetSpeciesQuery(locale)

  const schema = useMemo(() => z.object({
    title: z.string().min(1, t('validation.required')).max(200),
    description: z.string().min(1, t('validation.required')).max(2000),
    speciesId: z.string().min(1, t('validation.required')),
    breedId: z.string().optional().default(''),
    ageMonths: z.number({ invalid_type_error: t('validation.required') }).min(0),
    color: z.string().min(1, t('validation.required')),
    city: z.string().min(1, t('validation.required')),
    vaccinated: z.boolean().default(false),
    castrated: z.boolean().default(false),
    phone: z.string().optional().default(''),
    contactEmail: z.string().email(t('validation.invalidEmail')).optional().or(z.literal('')).default(''),
  }), [t])

  const {
    register, handleSubmit, control, watch,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      title: '', description: '', speciesId: '', breedId: '',
      ageMonths: 0, color: '', city: '', vaccinated: false, castrated: false,
      phone: '', contactEmail: '',
    },
  })

  const selectedSpeciesId = watch('speciesId')
  const { data: breeds = [] } = useGetBreedsQuery(
    { speciesId: selectedSpeciesId, locale },
    { skip: !selectedSpeciesId }
  )

  const onSubmit = async (values: FormValues) => {
    try {
      const result = await createListing({
        title: values.title,
        description: values.description,
        speciesId: values.speciesId,
        breedId: values.breedId || undefined,
        ageMonths: values.ageMonths,
        color: values.color,
        city: values.city,
        vaccinated: values.vaccinated,
        castrated: values.castrated,
        phone: values.phone || undefined,
        contactEmail: values.contactEmail || undefined,
      }).unwrap()
      setCreatedId(result.id)
    } catch (err) {
      setToast(getApiError(err, t))
    }
  }

  const handlePhotoUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    if (!file || !createdId) return
    if (!file.type.startsWith('image/')) {
      setToast(t('common.invalidImage'))
      return
    }
    if (photos.length >= 5) {
      setToast(t('listings.photoHint'))
      return
    }
    setUploading(true)
    try {
      const formData = new FormData()
      formData.append('file', file)
      const uploadRes = await api.post('/files/upload', formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
      const fileName: string = uploadRes.data?.result ?? uploadRes.data
      await addPhoto({ id: createdId, fileName }).unwrap()
      setPhotos(prev => [...prev, fileName])
    } catch (err) {
      setToast(getApiError(err, t))
    } finally {
      setUploading(false)
      if (fileInputRef.current) fileInputRef.current.value = ''
    }
  }

  const handleRemovePhoto = async (fileName: string) => {
    if (!createdId) return
    try {
      await removePhoto({ id: createdId, fileName }).unwrap()
      setPhotos(prev => prev.filter(p => p !== fileName))
    } catch (err) {
      setToast(getApiError(err, t))
    }
  }

  if (!user) {
    return (
      <Box sx={{ minHeight: '100vh', bgcolor: '#FAFAFA', py: 8 }}>
        <Container maxWidth="sm" sx={{ textAlign: 'center' }}>
          <Typography variant="h6" sx={{ mb: 2 }}>{t('listings.loginRequired')}</Typography>
          <Button component={Link} to="/login" variant="contained"
            sx={{ bgcolor: CORAL, '&:hover': { bgcolor: '#e55555' }, textTransform: 'none', fontWeight: 700, borderRadius: 2 }}>
            {t('auth.login')}
          </Button>
        </Container>
      </Box>
    )
  }

  // Photo upload step after successful creation
  if (createdId) {
    return (
      <Box sx={{ minHeight: '100vh', bgcolor: '#FAFAFA', py: 4 }}>
        <PageMeta title={t('listings.createTitle')} description={t('listings.createTitle')} path="/listings/create" noIndex />
        <Container maxWidth="sm">
          <Typography variant="h5" fontWeight="bold" sx={{ mb: 1 }}>
            {t('listings.addPhotosTitle') || 'Додайте фото'}
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
            {t('listings.photoHint')}
          </Typography>

          <Paper elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 4, p: 4 }}>
            {photos.length > 0 && (
              <Box sx={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: 1.5, mb: 2 }}>
                {photos.map((fileName) => (
                  <Box key={fileName} sx={{ position: 'relative', aspectRatio: '1', borderRadius: 2, overflow: 'hidden', bgcolor: '#F3F4F6' }}>
                    <Box
                      component="img"
                      src={`/api/files/presigned?fileName=${encodeURIComponent(fileName)}`}
                      sx={{ width: '100%', height: '100%', objectFit: 'cover' }}
                      onError={(e) => { (e.target as HTMLImageElement).style.display = 'none' }}
                    />
                    <IconButton
                      size="small"
                      onClick={() => handleRemovePhoto(fileName)}
                      sx={{ position: 'absolute', top: 4, right: 4, bgcolor: 'rgba(0,0,0,0.5)', color: 'white', '&:hover': { bgcolor: 'rgba(0,0,0,0.7)' } }}
                    >
                      <DeleteIcon fontSize="small" />
                    </IconButton>
                  </Box>
                ))}
              </Box>
            )}

            {photos.length < 5 && (
              <>
                <input
                  ref={fileInputRef}
                  type="file"
                  accept="image/*"
                  style={{ display: 'none' }}
                  onChange={handlePhotoUpload}
                />
                <Button
                  variant="outlined"
                  fullWidth
                  disabled={uploading}
                  onClick={() => fileInputRef.current?.click()}
                  startIcon={uploading ? <CircularProgress size={16} /> : <AddPhotoAlternateIcon />}
                  sx={{ borderColor: CORAL, color: CORAL, '&:hover': { borderColor: '#e55555', bgcolor: 'rgba(255,107,107,0.04)' }, textTransform: 'none', mb: 1 }}
                >
                  {uploading ? t('listings.photoUploading') : t('listings.addPhotoBtn')}
                </Button>
                <Typography variant="caption" color="text.secondary" display="block" textAlign="center">
                  {t('listings.photoHint')}
                </Typography>
              </>
            )}
          </Paper>

          <Button
            variant="contained"
            fullWidth
            onClick={() => navigate(`/listings/${createdId}`)}
            startIcon={<CheckCircleIcon />}
            sx={{ mt: 3, bgcolor: CORAL, '&:hover': { bgcolor: '#e55555' }, borderRadius: 2, textTransform: 'none', fontWeight: 700, py: 1.4 }}
          >
            {t('listings.doneBtn') || 'Готово — переглянути оголошення'}
          </Button>
        </Container>

        <Snackbar open={!!toast} autoHideDuration={5000} onClose={() => setToast(null)}
          anchorOrigin={{ vertical: 'top', horizontal: 'center' }}>
          <Alert severity="error" onClose={() => setToast(null)}>{toast}</Alert>
        </Snackbar>
      </Box>
    )
  }

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: '#FAFAFA', py: 4 }}>
      <PageMeta title={t('listings.createTitle')} description={t('listings.createTitle')} path="/listings/create" noIndex />
      <Container maxWidth="sm">
        <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/listings')}
          sx={{ mb: 3, color: '#6B7280', textTransform: 'none' }}>
          {t('common.back')}
        </Button>

        <Typography variant="h5" fontWeight="bold" sx={{ mb: 3 }}>
          {t('listings.createTitle')}
        </Typography>

        <Paper elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 4, p: 4 }}>
          <Box component="form" onSubmit={handleSubmit(onSubmit)} sx={{ display: 'flex', flexDirection: 'column', gap: 2.5 }}>

            <TextField {...register('title')} label={t('listings.titleLabel')}
              fullWidth error={!!errors.title} helperText={errors.title?.message} sx={fieldSx} />

            <TextField {...register('description')} label={t('listings.descriptionLabel')}
              multiline rows={4} fullWidth error={!!errors.description}
              helperText={errors.description?.message} sx={fieldSx} />

            {/* Species */}
            <TextField {...register('speciesId')} select label={t('pets.species')}
              fullWidth error={!!errors.speciesId} helperText={errors.speciesId?.message} sx={fieldSx}
              defaultValue="">
              <MenuItem value="">{t('pets.any')}</MenuItem>
              {speciesList.map((s) => (
                <MenuItem key={s.id} value={s.id}>{s.name}</MenuItem>
              ))}
            </TextField>

            {/* Breed */}
            {breeds.length > 0 && (
              <TextField {...register('breedId')} select label={t('pets.breed')}
                fullWidth sx={fieldSx} defaultValue="">
                <MenuItem value="">{t('pets.any')}</MenuItem>
                {breeds.map((b) => (
                  <MenuItem key={b.id} value={b.id}>{b.name}</MenuItem>
                ))}
              </TextField>
            )}

            <TextField {...register('ageMonths', { valueAsNumber: true })} label={t('listings.ageMonthsLabel')}
              type="number" fullWidth inputProps={{ min: 0 }}
              error={!!errors.ageMonths} helperText={errors.ageMonths?.message} sx={fieldSx} />

            <TextField {...register('color')} label={t('pets.color.label')}
              fullWidth error={!!errors.color} helperText={errors.color?.message} sx={fieldSx} />

            <TextField {...register('city')} label={t('petDetail.city')}
              fullWidth error={!!errors.city} helperText={errors.city?.message} sx={fieldSx} />

            <TextField {...register('phone')} label={t('listings.phoneLabel')}
              fullWidth helperText={t('listings.phoneHint')} sx={fieldSx} />

            <TextField {...register('contactEmail')} label={t('listings.contactEmailLabel') || 'Email для зв\'язку'}
              type="email" fullWidth
              error={!!errors.contactEmail} helperText={errors.contactEmail?.message || (t('listings.contactEmailHint') || 'Необов\'язково — альтернативний email для контакту')}
              sx={fieldSx} />

            <Box sx={{ display: 'flex', gap: 3 }}>
              <Controller name="vaccinated" control={control}
                render={({ field }) => (
                  <FormControlLabel
                    control={<Switch checked={field.value} onChange={field.onChange} sx={{ '& .Mui-checked': { color: CORAL }, '& .Mui-checked+.MuiSwitch-track': { bgcolor: CORAL } }} />}
                    label={t('pets.vaccinated')}
                  />
                )} />
              <Controller name="castrated" control={control}
                render={({ field }) => (
                  <FormControlLabel
                    control={<Switch checked={field.value} onChange={field.onChange} sx={{ '& .Mui-checked': { color: CORAL }, '& .Mui-checked+.MuiSwitch-track': { bgcolor: CORAL } }} />}
                    label={t('pets.castrated')}
                  />
                )} />
            </Box>

            <Button type="submit" variant="contained" fullWidth disabled={isSubmitting}
              sx={{
                bgcolor: CORAL, '&:hover': { bgcolor: '#e55555' },
                '&.Mui-disabled': { bgcolor: '#FFA0A0' },
                borderRadius: 2, textTransform: 'none', fontWeight: 700, py: 1.4, mt: 1,
              }}>
              {isSubmitting ? <CircularProgress size={22} sx={{ color: 'white' }} /> : t('listings.submitBtn')}
            </Button>
          </Box>
        </Paper>
      </Container>

      <Snackbar open={!!toast} autoHideDuration={5000} onClose={() => setToast(null)}
        anchorOrigin={{ vertical: 'top', horizontal: 'center' }}>
        <Alert severity="error" onClose={() => setToast(null)}>{toast}</Alert>
      </Snackbar>
    </Box>
  )
}
