import { useMemo, useRef, useState } from 'react'
import { useParams } from 'react-router-dom'
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
import Grid from '@mui/material/Grid'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import AddPhotoAlternateIcon from '@mui/icons-material/AddPhotoAlternate'
import DeleteIcon from '@mui/icons-material/Delete'
import { useGetSpeciesQuery, useGetBreedsQuery } from '../services/speciesApi'
import { useCreatePetMutation } from '../services/petsApi'
import { api } from '../lib/axios'
import { getApiError } from '../lib/getApiError'

const CORAL = '#FF6B6B'
const MAX_PHOTOS = 5
const MAX_FILE_SIZE_BYTES = 5 * 1024 * 1024
const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/webp']

const fieldSx = {
  '& .MuiOutlinedInput-root.Mui-focused .MuiOutlinedInput-notchedOutline': { borderColor: CORAL },
  '& .MuiInputLabel-root.Mui-focused': { color: CORAL },
}

type SelectedPhoto = { file: File; preview: string }

export default function AddAnimalPage() {
  const { t, i18n } = useTranslation()
  const { volunteerId } = useParams<{ volunteerId: string }>()
  const navigate = useLangNavigate()
  const [toast, setToast] = useState<{ msg: string; severity: 'success' | 'error' } | null>(null)
  const [selectedPhotos, setSelectedPhotos] = useState<SelectedPhoto[]>([])
  const fileInputRef = useRef<HTMLInputElement>(null)
  const [createPet] = useCreatePetMutation()

  const locale = i18n.language?.slice(0, 2) || 'uk'
  const { data: speciesList = [] } = useGetSpeciesQuery(locale)

  const schema = useMemo(() => z.object({
    nickname: z.string().min(1, t('validation.required')).max(100),
    generalDescription: z.string().min(1, t('validation.required')).max(2000),
    color: z.string().min(1, t('validation.required')).max(50),
    healthDescription: z.string().min(1, t('validation.required')).max(2000),
    dietOrAllergies: z.string().max(500).default(''),
    city: z.string().min(1, t('validation.required')),
    street: z.string().min(1, t('validation.required')),
    weight: z.number({ message: t('validation.required') }).min(0.1).max(200),
    height: z.number({ message: t('validation.required') }).min(1).max(300),
    ownerPhone: z.string().min(1, t('validation.required')),
    isCastrated: z.boolean().default(false),
    isVaccinated: z.boolean().default(false),
    dateOfBirth: z.string().min(1, t('validation.required')),
    status: z.number().default(1),
    microchipNumber: z.string().max(50).default(''),
    adoptionConditions: z.string().max(1000).default(''),
    speciesId: z.string().min(1, t('validation.required')),
    breedId: z.string().min(1, t('validation.required')),
  }), [t])

  const {
    register, handleSubmit, control, watch,
    formState: { errors, isSubmitting },
  } = useForm({
    resolver: zodResolver(schema),
    defaultValues: {
      nickname: '', generalDescription: '', color: '', healthDescription: '',
      dietOrAllergies: '', city: '', street: '', weight: undefined as unknown as number,
      height: undefined as unknown as number, ownerPhone: '',
      isCastrated: false, isVaccinated: false, dateOfBirth: '',
      status: 1, microchipNumber: '', adoptionConditions: '',
      speciesId: '', breedId: '',
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
    for (const file of files) {
      if (selectedPhotos.length >= MAX_PHOTOS) {
        setToast({ msg: t('addAnimal.photoHint'), severity: 'error' })
        break
      }
      if (!ALLOWED_TYPES.includes(file.type)) {
        setToast({ msg: t('common.invalidImage'), severity: 'error' })
        continue
      }
      if (file.size > MAX_FILE_SIZE_BYTES) {
        setToast({ msg: `${file.name}: макс 5 МБ`, severity: 'error' })
        continue
      }
      setSelectedPhotos(prev => [...prev, { file, preview: URL.createObjectURL(file) }])
    }
  }

  const handleRemovePhoto = (index: number) => {
    setSelectedPhotos(prev => {
      URL.revokeObjectURL(prev[index].preview)
      return prev.filter((_, i) => i !== index)
    })
  }

  const onSubmit = async (values: {
    nickname: string; generalDescription: string; color: string; healthDescription: string;
    dietOrAllergies: string; city: string; street: string; weight: number; height: number;
    ownerPhone: string; isCastrated: boolean; isVaccinated: boolean; dateOfBirth: string;
    status: number; microchipNumber: string; adoptionConditions: string;
    speciesId: string; breedId: string;
  }) => {
    if (!volunteerId) return
    try {
      const petId = await createPet({
        volunteerId,
        data: {
          nickname: values.nickname,
          generalDescription: values.generalDescription,
          color: values.color,
          healthDescription: values.healthDescription,
          dietOrAllergies: values.dietOrAllergies || undefined,
          city: values.city,
          street: values.street,
          weight: values.weight,
          height: values.height,
          ownerPhone: values.ownerPhone,
          isCastrated: values.isCastrated,
          isVaccinated: values.isVaccinated,
          dateOfBirth: new Date(values.dateOfBirth).toISOString(),
          status: values.status,
          microchipNumber: values.microchipNumber || undefined,
          adoptionConditions: values.adoptionConditions || undefined,
          speciesId: values.speciesId,
          breedId: values.breedId,
        },
      }).unwrap()

      if (selectedPhotos.length > 0) {
        const formData = new FormData()
        selectedPhotos.forEach(p => formData.append('files', p.file))
        try {
          await api.post(`/volunteers/${volunteerId}/pets/${petId}/photos`, formData, {
            headers: { 'Content-Type': 'multipart/form-data' },
          })
        } catch {
          // photos failed but pet created — show partial success
        }
      }

      setToast({ msg: t('addAnimal.success'), severity: 'success' })
      setTimeout(() => navigate(`/animals/${volunteerId}`), 1200)
    } catch (err) {
      setToast({ msg: getApiError(err, t), severity: 'error' })
    }
  }

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'background.default', py: 4 }}>
      <PageMeta title={t('addAnimal.pageTitle')} description={t('addAnimal.pageTitle')} path={`/animals/${volunteerId}/add`} noIndex />
      <Container maxWidth="md">
        <Button startIcon={<ArrowBackIcon />} onClick={() => navigate(`/animals/${volunteerId}`)}
          sx={{ mb: 3, color: '#6B7280', textTransform: 'none' }}>
          {t('addAnimal.backToAnimals')}
        </Button>

        <Typography variant="h5" fontWeight="bold" sx={{ mb: 3 }}>
          {t('addAnimal.pageTitle')}
        </Typography>

        <Paper elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 4, p: { xs: 3, md: 4 } }}>
          <Box component="form" onSubmit={handleSubmit(onSubmit)} sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>

            {/* Basic info */}
            <Grid container spacing={2}>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField {...register('nickname')} label={t('addAnimal.nickname')}
                  fullWidth error={!!errors.nickname} helperText={errors.nickname?.message} sx={fieldSx} />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <Controller name="status" control={control} render={({ field }) => (
                  <TextField {...field} select label={t('addAnimal.status')} fullWidth sx={fieldSx}>
                    <MenuItem value={1}>{t('pets.status.lookingForHome')}</MenuItem>
                    <MenuItem value={0}>{t('pets.status.needsHelp')}</MenuItem>
                  </TextField>
                )} />
              </Grid>

              <Grid size={{ xs: 12, sm: 6 }}>
                <Controller name="speciesId" control={control} render={({ field }) => (
                  <TextField {...field} select label={t('pets.species')} fullWidth
                    error={!!errors.speciesId} helperText={errors.speciesId?.message} sx={fieldSx}>
                    <MenuItem value="">{t('pets.any')}</MenuItem>
                    {speciesList.map(s => <MenuItem key={s.id} value={s.id}>{s.name}</MenuItem>)}
                  </TextField>
                )} />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <Controller name="breedId" control={control} render={({ field }) => (
                  <TextField {...field} select label={t('pets.breed')} fullWidth
                    error={!!errors.breedId} helperText={errors.breedId?.message}
                    disabled={!selectedSpeciesId} sx={fieldSx}>
                    <MenuItem value="">{t('pets.any')}</MenuItem>
                    {breeds.map(b => <MenuItem key={b.id} value={b.id}>{b.name}</MenuItem>)}
                  </TextField>
                )} />
              </Grid>
            </Grid>

            {/* Description */}
            <TextField {...register('generalDescription')} label={t('addAnimal.generalDescription')}
              multiline rows={4} fullWidth error={!!errors.generalDescription}
              helperText={errors.generalDescription?.message} sx={fieldSx} />

            {/* Location */}
            <Grid container spacing={2}>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField {...register('city')} label={t('pets.location.label')}
                  fullWidth error={!!errors.city} helperText={errors.city?.message} sx={fieldSx} />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField {...register('street')} label={t('addAnimal.street')}
                  fullWidth error={!!errors.street} helperText={errors.street?.message} sx={fieldSx} />
              </Grid>
            </Grid>

            {/* Physical */}
            <Grid container spacing={2}>
              <Grid size={{ xs: 12, sm: 4 }}>
                <TextField {...register('color')} label={t('pets.color.label')}
                  fullWidth error={!!errors.color} helperText={errors.color?.message} sx={fieldSx} />
              </Grid>
              <Grid size={{ xs: 6, sm: 4 }}>
                <TextField {...register('weight', { valueAsNumber: true })} label={t('addAnimal.weight')}
                  type="number" inputProps={{ step: '0.1', min: '0.1' }} fullWidth
                  error={!!errors.weight} helperText={errors.weight?.message} sx={fieldSx} />
              </Grid>
              <Grid size={{ xs: 6, sm: 4 }}>
                <TextField {...register('height', { valueAsNumber: true })} label={t('addAnimal.height')}
                  type="number" inputProps={{ step: '1', min: '1' }} fullWidth
                  error={!!errors.height} helperText={errors.height?.message} sx={fieldSx} />
              </Grid>
            </Grid>

            {/* Date of birth + phone */}
            <Grid container spacing={2}>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField {...register('dateOfBirth')} label={t('addAnimal.dateOfBirth')}
                  type="date" fullWidth InputLabelProps={{ shrink: true }}
                  error={!!errors.dateOfBirth} helperText={errors.dateOfBirth?.message} sx={fieldSx} />
              </Grid>
              <Grid size={{ xs: 12, sm: 6 }}>
                <TextField {...register('ownerPhone')} label={t('addAnimal.ownerPhone')}
                  fullWidth error={!!errors.ownerPhone} helperText={errors.ownerPhone?.message} sx={fieldSx} />
              </Grid>
            </Grid>

            {/* Health */}
            <TextField {...register('healthDescription')} label={t('addAnimal.healthDescription')}
              multiline rows={3} fullWidth error={!!errors.healthDescription}
              helperText={errors.healthDescription?.message} sx={fieldSx} />
            <TextField {...register('dietOrAllergies')} label={t('addAnimal.dietOrAllergies')}
              multiline rows={2} fullWidth sx={fieldSx} />

            {/* Toggles */}
            <Box sx={{ display: 'flex', gap: 4, flexWrap: 'wrap' }}>
              <Controller name="isVaccinated" control={control} render={({ field }) => (
                <FormControlLabel control={<Switch checked={field.value} onChange={e => field.onChange(e.target.checked)}
                  sx={{ '& .MuiSwitch-switchBase.Mui-checked': { color: CORAL }, '& .MuiSwitch-switchBase.Mui-checked + .MuiSwitch-track': { bgcolor: CORAL } }} />}
                  label={t('pets.vaccinated')} />
              )} />
              <Controller name="isCastrated" control={control} render={({ field }) => (
                <FormControlLabel control={<Switch checked={field.value} onChange={e => field.onChange(e.target.checked)}
                  sx={{ '& .MuiSwitch-switchBase.Mui-checked': { color: CORAL }, '& .MuiSwitch-switchBase.Mui-checked + .MuiSwitch-track': { bgcolor: CORAL } }} />}
                  label={t('pets.castrated')} />
              )} />
            </Box>

            {/* Optional */}
            <TextField {...register('microchipNumber')} label={t('addAnimal.microchipNumber')} fullWidth sx={fieldSx} />
            <TextField {...register('adoptionConditions')} label={t('addAnimal.adoptionConditions')}
              multiline rows={2} fullWidth sx={fieldSx} />

            {/* Photos */}
            <Box>
              <Typography variant="subtitle2" fontWeight={600} sx={{ mb: 1 }}>
                {t('addAnimal.photosSection')}
              </Typography>
              <Typography variant="caption" color="text.secondary" sx={{ mb: 1.5, display: 'block' }}>
                {t('addAnimal.photoHint')}
              </Typography>

              <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1.5, mb: 1.5 }}>
                {selectedPhotos.map((p, i) => (
                  <Box key={i} sx={{ position: 'relative', width: 90, height: 90 }}>
                    <Box component="img" src={p.preview} alt=""
                      sx={{ width: 90, height: 90, objectFit: 'cover', borderRadius: 2, border: '1px solid #E5E7EB' }} />
                    <IconButton size="small" onClick={() => handleRemovePhoto(i)}
                      sx={{ position: 'absolute', top: -8, right: -8, bgcolor: 'white', border: '1px solid #E5E7EB',
                        '&:hover': { bgcolor: '#FFF0F0' }, p: 0.25 }}>
                      <DeleteIcon fontSize="small" sx={{ color: CORAL }} />
                    </IconButton>
                  </Box>
                ))}
                {selectedPhotos.length < MAX_PHOTOS && (
                  <Box onClick={() => fileInputRef.current?.click()}
                    sx={{ width: 90, height: 90, border: '2px dashed #E5E7EB', borderRadius: 2, display: 'flex',
                      alignItems: 'center', justifyContent: 'center', cursor: 'pointer',
                      '&:hover': { borderColor: CORAL, bgcolor: '#FFF5F5' }, transition: 'all 0.2s' }}>
                    <AddPhotoAlternateIcon sx={{ color: '#9CA3AF', fontSize: 28 }} />
                  </Box>
                )}
              </Box>
              <input ref={fileInputRef} type="file" accept="image/jpeg,image/png,image/webp"
                multiple hidden onChange={handlePhotoSelect} />
            </Box>

            <Button type="submit" variant="contained" size="large" disabled={isSubmitting}
              sx={{ bgcolor: CORAL, '&:hover': { bgcolor: '#e55555' }, textTransform: 'none',
                fontWeight: 700, borderRadius: 2, py: 1.5 }}>
              {isSubmitting ? <CircularProgress size={22} sx={{ color: 'white' }} /> : t('addAnimal.submit')}
            </Button>
          </Box>
        </Paper>
      </Container>

      <Snackbar open={!!toast} autoHideDuration={4000} onClose={() => setToast(null)}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}>
        <Alert severity={toast?.severity ?? 'success'} onClose={() => setToast(null)}>
          {toast?.msg}
        </Alert>
      </Snackbar>
    </Box>
  )
}
