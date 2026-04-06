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
import { useGetSpeciesQuery, useGetBreedsQuery } from '../services/speciesApi'
import { useCreateListingMutation, useAddListingPhotoMutation } from '../services/listingsApi'
import { useAuthStore } from '../store/authStore'
import { LangLink as Link } from '../components/ui/LangLink'
import { getApiError } from '../lib/getApiError'
import { api } from '../lib/axios'

const CORAL = '#FF6B6B'
const MAX_PHOTOS = 5
const MAX_FILE_SIZE_MB = 5
const MAX_FILE_SIZE_BYTES = MAX_FILE_SIZE_MB * 1024 * 1024
const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp', 'image/gif']

const fieldSx = {
  '& .MuiOutlinedInput-root.Mui-focused .MuiOutlinedInput-notchedOutline': { borderColor: CORAL },
  '& .MuiInputLabel-root.Mui-focused': { color: CORAL },
}

type SelectedPhoto = {
  file: File
  preview: string
}

export default function CreateListingPage() {
  const { t, i18n } = useTranslation()
  const navigate = useLangNavigate()
  const { user } = useAuthStore()
  const [toast, setToast] = useState<string | null>(null)
  const [createListing] = useCreateListingMutation()
  const [addPhoto] = useAddListingPhotoMutation()
  const fileInputRef = useRef<HTMLInputElement>(null)
  const [selectedPhotos, setSelectedPhotos] = useState<SelectedPhoto[]>([])

  const locale = i18n.language?.slice(0, 2) || 'uk'
  const { data: speciesList = [] } = useGetSpeciesQuery(locale)

  const schema = useMemo(() => z.object({
    title: z.string().min(1, t('validation.required')).max(200),
    description: z.string().min(1, t('validation.required')).max(2000),
    speciesId: z.string().min(1, t('validation.required')),
    breedId: z.string().optional().default(''),
    ageMonths: z.number().min(0),
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
  } = useForm({
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

  const handlePhotoSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = Array.from(e.target.files ?? [])
    if (fileInputRef.current) fileInputRef.current.value = ''

    const errors: string[] = []

    for (const file of files) {
      if (selectedPhotos.length + 1 > MAX_PHOTOS) {
        errors.push(t('listings.photoHint'))
        break
      }
      if (!ALLOWED_TYPES.includes(file.type)) {
        errors.push(t('common.invalidImage'))
        continue
      }
      if (file.size > MAX_FILE_SIZE_BYTES) {
        errors.push(`${file.name}: максимальний розмір ${MAX_FILE_SIZE_MB}MB`)
        continue
      }
      const preview = URL.createObjectURL(file)
      setSelectedPhotos(prev => [...prev, { file, preview }])
    }

    if (errors.length > 0) setToast(errors[0])
  }

  const handleRemoveSelected = (index: number) => {
    setSelectedPhotos(prev => {
      URL.revokeObjectURL(prev[index].preview)
      return prev.filter((_, i) => i !== index)
    })
  }

  const onSubmit = async (values: { title: string; description: string; speciesId: string; breedId?: string; ageMonths: number; color: string; city: string; vaccinated?: boolean; castrated?: boolean; phone?: string; contactEmail?: string }) => {
    try {
      const result = await createListing({
        title: values.title,
        description: values.description,
        speciesId: values.speciesId,
        breedId: values.breedId || undefined,
        ageMonths: values.ageMonths,
        color: values.color,
        city: values.city,
        vaccinated: values.vaccinated ?? false,
        castrated: values.castrated ?? false,
        phone: values.phone || undefined,
        contactEmail: values.contactEmail || undefined,
      }).unwrap()

      for (const photo of selectedPhotos) {
        try {
          const formData = new FormData()
          formData.append('file', photo.file)
          const uploadRes = await api.post('/files/upload', formData, {
            headers: { 'Content-Type': 'multipart/form-data' },
          })
          const fileName: string = uploadRes.data?.result ?? uploadRes.data
          await addPhoto({ id: result.id, fileName }).unwrap()
        } catch {
          // continue uploading remaining photos even if one fails
        }
      }

      navigate(`/listings/${result.id}`)
    } catch (err) {
      setToast(getApiError(err, t))
    }
  }

  if (!user) {
    return (
      <Box sx={{ minHeight: '100vh', bgcolor: 'background.default', py: 8 }}>
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

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'background.default', py: 4 }}>
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

            <TextField {...register('speciesId')} select label={t('pets.species')}
              fullWidth error={!!errors.speciesId} helperText={errors.speciesId?.message} sx={fieldSx}
              defaultValue="">
              <MenuItem value="">{t('pets.any')}</MenuItem>
              {speciesList.map((s) => (
                <MenuItem key={s.id} value={s.id}>{s.name}</MenuItem>
              ))}
            </TextField>

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
              error={!!errors.contactEmail} helperText={errors.contactEmail?.message || (t('listings.contactEmailHint') || 'Необов\'язково')}
              sx={fieldSx} />

            <Box sx={{ display: 'flex', gap: 3 }}>
              <Controller name="vaccinated" control={control}
                render={({ field }) => (
                  <FormControlLabel
                    control={<Switch checked={field.value} onChange={field.onChange}
                      sx={{ '& .Mui-checked': { color: CORAL }, '& .Mui-checked+.MuiSwitch-track': { bgcolor: CORAL } }} />}
                    label={t('pets.vaccinated')}
                  />
                )} />
              <Controller name="castrated" control={control}
                render={({ field }) => (
                  <FormControlLabel
                    control={<Switch checked={field.value} onChange={field.onChange}
                      sx={{ '& .Mui-checked': { color: CORAL }, '& .Mui-checked+.MuiSwitch-track': { bgcolor: CORAL } }} />}
                    label={t('pets.castrated')}
                  />
                )} />
            </Box>

            {/* Photo upload */}
            <Box>
              <Typography variant="body2" fontWeight={600} sx={{ mb: 1 }}>
                {t('listings.addPhotosTitle') || 'Фото'}{' '}
                <Typography component="span" variant="caption" color="text.secondary">
                  ({t('listings.photoHint')})
                </Typography>
              </Typography>

              {selectedPhotos.length > 0 && (
                <Box sx={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: 1.5, mb: 1.5 }}>
                  {selectedPhotos.map((photo, index) => (
                    <Box key={index} sx={{ position: 'relative', aspectRatio: '1', borderRadius: 2, overflow: 'hidden', bgcolor: 'action.hover' }}>
                      <Box
                        component="img"
                        src={photo.preview}
                        sx={{ width: '100%', height: '100%', objectFit: 'cover' }}
                      />
                      <IconButton
                        size="small"
                        onClick={() => handleRemoveSelected(index)}
                        sx={{ position: 'absolute', top: 4, right: 4, bgcolor: 'rgba(0,0,0,0.5)', color: 'white', '&:hover': { bgcolor: 'rgba(0,0,0,0.7)' } }}
                      >
                        <DeleteIcon fontSize="small" />
                      </IconButton>
                    </Box>
                  ))}
                </Box>
              )}

              {selectedPhotos.length < MAX_PHOTOS && (
                <>
                  <input
                    ref={fileInputRef}
                    type="file"
                    accept={ALLOWED_TYPES.join(',')}
                    style={{ display: 'none' }}
                    onChange={handlePhotoSelect}
                  />
                  <Button
                    variant="outlined"
                    fullWidth
                    onClick={() => fileInputRef.current?.click()}
                    startIcon={<AddPhotoAlternateIcon />}
                    sx={{ borderColor: CORAL, color: CORAL, '&:hover': { borderColor: '#e55555', bgcolor: 'rgba(255,107,107,0.04)' }, textTransform: 'none' }}
                  >
                    {t('listings.addPhotoBtn')}
                  </Button>
                </>
              )}
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
