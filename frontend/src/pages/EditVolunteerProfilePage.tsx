import { useState } from 'react'
import { useParams } from 'react-router-dom'
import { useLangNavigate } from '../hooks/useLangNavigate'
import PageMeta from '../components/meta/PageMeta'
import { useTranslation } from 'react-i18next'
import { useForm, useFieldArray } from 'react-hook-form'
import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Paper from '@mui/material/Paper'
import Typography from '@mui/material/Typography'
import Button from '@mui/material/Button'
import TextField from '@mui/material/TextField'
import Tabs from '@mui/material/Tabs'
import Tab from '@mui/material/Tab'
import CircularProgress from '@mui/material/CircularProgress'
import Alert from '@mui/material/Alert'
import Snackbar from '@mui/material/Snackbar'
import IconButton from '@mui/material/IconButton'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import AddIcon from '@mui/icons-material/Add'
import DeleteIcon from '@mui/icons-material/Delete'
import SaveIcon from '@mui/icons-material/Save'
import { useAuthStore } from '../store/authStore'
import {
  useGetVolunteerByIdQuery,
  useUpdateMainInfoMutation,
  useUpdateSocialNetworksMutation,
  useUpdateRequisitesMutation,
} from '../services/volunteersApi'
import { getApiError } from '../lib/getApiError'

const CORAL = '#FF6B6B'

const fieldSx = {
  '& .MuiOutlinedInput-root.Mui-focused .MuiOutlinedInput-notchedOutline': { borderColor: CORAL },
  '& .MuiInputLabel-root.Mui-focused': { color: CORAL },
}

// ─── Main Info Tab ─────────────────────────────────────
interface MainInfoValues {
  firstName: string
  lastName: string
  patronymic: string
  email: string
  phone: string
  generalDescription: string
  experienceYears: number
}

function MainInfoTab({ volunteerId, initial }: { volunteerId: string; initial: MainInfoValues }) {
  const { t } = useTranslation()
  const [updateMainInfo] = useUpdateMainInfoMutation()
  const [toast, setToast] = useState<{ msg: string; severity: 'success' | 'error' } | null>(null)

  const { register, handleSubmit, formState: { isSubmitting } } = useForm<MainInfoValues>({ defaultValues: initial })

  const onSubmit = async (values: MainInfoValues) => {
    try {
      await updateMainInfo({ id: volunteerId, data: { ...values } }).unwrap()
      setToast({ msg: t('editVolunteer.saveSuccess'), severity: 'success' })
    } catch (err) {
      setToast({ msg: getApiError(err, t), severity: 'error' })
    }
  }

  return (
    <Box component="form" onSubmit={handleSubmit(onSubmit)} sx={{ display: 'flex', flexDirection: 'column', gap: 2.5 }}>
      <Box sx={{ display: 'flex', gap: 2 }}>
        <TextField {...register('firstName')} label={t('editVolunteer.firstName')} fullWidth sx={fieldSx} />
        <TextField {...register('lastName')} label={t('editVolunteer.lastName')} fullWidth sx={fieldSx} />
      </Box>
      <TextField {...register('patronymic')} label={t('editVolunteer.patronymic')} fullWidth sx={fieldSx} />
      <TextField {...register('email')} label={t('editVolunteer.email')} type="email" fullWidth sx={fieldSx} />
      <TextField {...register('phone')} label={t('editVolunteer.phone')} fullWidth sx={fieldSx} />
      <TextField {...register('generalDescription')} label={t('editVolunteer.description')} multiline rows={4} fullWidth sx={fieldSx} />
      <TextField
        {...register('experienceYears', { valueAsNumber: true })}
        label={t('editVolunteer.experience')}
        type="number"
        inputProps={{ min: 0, max: 100 }}
        sx={{ ...fieldSx, maxWidth: 200 }}
      />
      <SaveButton isSubmitting={isSubmitting} label={t('editVolunteer.saveBtn')} />

      <Snackbar open={!!toast} autoHideDuration={3000} onClose={() => setToast(null)} anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}>
        <Alert severity={toast?.severity} onClose={() => setToast(null)}>{toast?.msg}</Alert>
      </Snackbar>
    </Box>
  )
}

// ─── Social Networks Tab ───────────────────────────────
interface SocialNetworkValues {
  socialNetworks: { name: string; link: string }[]
}

function SocialNetworksTab({ volunteerId, initial }: { volunteerId: string; initial: SocialNetworkValues }) {
  const { t } = useTranslation()
  const [updateSocialNetworks] = useUpdateSocialNetworksMutation()
  const [toast, setToast] = useState<{ msg: string; severity: 'success' | 'error' } | null>(null)

  const { register, handleSubmit, control, formState: { isSubmitting } } = useForm<SocialNetworkValues>({ defaultValues: initial })
  const { fields, append, remove } = useFieldArray({ control, name: 'socialNetworks' })

  const onSubmit = async (values: SocialNetworkValues) => {
    try {
      await updateSocialNetworks({ id: volunteerId, socialNetworks: values.socialNetworks }).unwrap()
      setToast({ msg: t('editVolunteer.saveSuccess'), severity: 'success' })
    } catch (err) {
      setToast({ msg: getApiError(err, t), severity: 'error' })
    }
  }

  return (
    <Box component="form" onSubmit={handleSubmit(onSubmit)} sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
      {fields.map((field, i) => (
        <Box key={field.id} sx={{ display: 'flex', gap: 1, alignItems: 'flex-start', bgcolor: 'background.default', border: '1px solid #E5E7EB', borderRadius: 2, p: 1.5 }}>
          <TextField {...register(`socialNetworks.${i}.name`)} label={t('editVolunteer.linkName')} size="small" sx={{ flex: 1, ...fieldSx }} />
          <TextField {...register(`socialNetworks.${i}.link`)} label={t('editVolunteer.linkUrl')} size="small" sx={{ flex: 2, ...fieldSx }} />
          <IconButton onClick={() => remove(i)} size="small" sx={{ mt: 0.5, color: '#6B7280' }}><DeleteIcon /></IconButton>
        </Box>
      ))}

      <Button
        startIcon={<AddIcon />}
        onClick={() => append({ name: '', link: '' })}
        variant="outlined"
        sx={{ borderColor: '#E5E7EB', color: '#6B7280', textTransform: 'none', borderRadius: 5, alignSelf: 'flex-start', '&:hover': { borderColor: CORAL, color: CORAL } }}
      >
        {t('editVolunteer.addLink')}
      </Button>

      <SaveButton isSubmitting={isSubmitting} label={t('editVolunteer.saveBtn')} />

      <Snackbar open={!!toast} autoHideDuration={3000} onClose={() => setToast(null)} anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}>
        <Alert severity={toast?.severity} onClose={() => setToast(null)}>{toast?.msg}</Alert>
      </Snackbar>
    </Box>
  )
}

// ─── Requisites Tab ────────────────────────────────────
interface RequisitesValues {
  requisites: { name: string; description: string }[]
}

function RequisitesTab({ volunteerId, initial }: { volunteerId: string; initial: RequisitesValues }) {
  const { t } = useTranslation()
  const [updateRequisites] = useUpdateRequisitesMutation()
  const [toast, setToast] = useState<{ msg: string; severity: 'success' | 'error' } | null>(null)

  const { register, handleSubmit, control, formState: { isSubmitting } } = useForm<RequisitesValues>({ defaultValues: initial })
  const { fields, append, remove } = useFieldArray({ control, name: 'requisites' })

  const onSubmit = async (values: RequisitesValues) => {
    try {
      await updateRequisites({ id: volunteerId, requisites: values.requisites }).unwrap()
      setToast({ msg: t('editVolunteer.saveSuccess'), severity: 'success' })
    } catch (err) {
      setToast({ msg: getApiError(err, t), severity: 'error' })
    }
  }

  return (
    <Box component="form" onSubmit={handleSubmit(onSubmit)} sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
      {fields.map((field, i) => (
        <Box key={field.id} sx={{ display: 'flex', gap: 1, alignItems: 'flex-start', bgcolor: 'background.default', border: '1px solid #E5E7EB', borderRadius: 2, p: 1.5 }}>
          <TextField {...register(`requisites.${i}.name`)} label={t('editVolunteer.requisiteName')} size="small" sx={{ flex: 1, ...fieldSx }} />
          <TextField {...register(`requisites.${i}.description`)} label={t('editVolunteer.requisiteDescription')} size="small" sx={{ flex: 2, ...fieldSx }} />
          <IconButton onClick={() => remove(i)} size="small" sx={{ mt: 0.5, color: '#6B7280' }}><DeleteIcon /></IconButton>
        </Box>
      ))}

      <Button
        startIcon={<AddIcon />}
        onClick={() => append({ name: '', description: '' })}
        variant="outlined"
        sx={{ borderColor: '#E5E7EB', color: '#6B7280', textTransform: 'none', borderRadius: 5, alignSelf: 'flex-start', '&:hover': { borderColor: CORAL, color: CORAL } }}
      >
        {t('editVolunteer.addRequisite')}
      </Button>

      <SaveButton isSubmitting={isSubmitting} label={t('editVolunteer.saveBtn')} />

      <Snackbar open={!!toast} autoHideDuration={3000} onClose={() => setToast(null)} anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}>
        <Alert severity={toast?.severity} onClose={() => setToast(null)}>{toast?.msg}</Alert>
      </Snackbar>
    </Box>
  )
}

// ─── Shared save button ────────────────────────────────
function SaveButton({ isSubmitting, label }: { isSubmitting: boolean; label: string }) {
  return (
    <Button
      type="submit"
      variant="contained"
      disabled={isSubmitting}
      startIcon={isSubmitting ? undefined : <SaveIcon />}
      sx={{
        bgcolor: CORAL, '&:hover': { bgcolor: '#e55555' }, '&.Mui-disabled': { bgcolor: '#FFA0A0' },
        textTransform: 'none', fontWeight: 700, borderRadius: 2, alignSelf: 'flex-start', px: 4,
      }}
    >
      {isSubmitting ? <CircularProgress size={20} sx={{ color: 'white' }} /> : label}
    </Button>
  )
}

// ─── Main Page ─────────────────────────────────────────
export default function EditVolunteerProfilePage() {
  const { t } = useTranslation()
  const { volunteerId } = useParams<{ volunteerId: string }>()
  const navigate = useLangNavigate()
  const { user } = useAuthStore()
  const [tab, setTab] = useState(0)

  const isOwner = !!user && !!volunteerId && user.id === volunteerId

  const { data: volunteer, isLoading } = useGetVolunteerByIdQuery(volunteerId ?? '', { skip: !volunteerId })

  if (isLoading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '60vh' }}>
        <CircularProgress sx={{ color: CORAL }} />
      </Box>
    )
  }

  if (!isOwner || !volunteer) {
    return (
      <Container maxWidth="sm" sx={{ py: 8 }}>
        <Alert severity="error">{t('editVolunteer.notOwner')}</Alert>
        <Button onClick={() => navigate(-1)} sx={{ mt: 2, color: CORAL, textTransform: 'none' }}>
          ← {t('common.back')}
        </Button>
      </Container>
    )
  }

  return (
    <Box sx={{ bgcolor: 'background.default', minHeight: '100%', py: 4 }}>
      <PageMeta title={t('editVolunteer.title')} description={t('editVolunteer.title')} path={`/edit-profile/volunteer/${volunteerId}`} noIndex />
      <Container maxWidth="md">
        <Button
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate(`/volunteers/${volunteerId}`)}
          sx={{ mb: 3, color: '#6B7280', textTransform: 'none' }}
        >
          {t('volunteerProfile.backToHome')}
        </Button>

        <Typography variant="h4" fontWeight="bold" sx={{ mb: 3 }}>{t('editVolunteer.title')}</Typography>

        <Paper elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 3, overflow: 'hidden' }}>
          <Tabs
            value={tab}
            onChange={(_, v) => setTab(v)}
            TabIndicatorProps={{ style: { backgroundColor: CORAL } }}
            sx={{ borderBottom: '1px solid #E5E7EB', '& .MuiTab-root.Mui-selected': { color: CORAL }, px: 1 }}
          >
            <Tab label={t('editVolunteer.tabMainInfo')} />
            <Tab label={t('editVolunteer.tabSocialNetworks')} />
            <Tab label={t('editVolunteer.tabRequisites')} />
          </Tabs>

          <Box sx={{ p: 3 }}>
            {tab === 0 && (
              <MainInfoTab
                volunteerId={volunteerId!}
                initial={{
                  firstName: volunteer.firstName,
                  lastName: volunteer.lastName,
                  patronymic: volunteer.patronymic,
                  email: volunteer.email,
                  phone: volunteer.phone,
                  generalDescription: volunteer.generalDescription,
                  experienceYears: volunteer.experienceYears,
                }}
              />
            )}
            {tab === 1 && (
              <SocialNetworksTab
                volunteerId={volunteerId!}
                initial={{ socialNetworks: volunteer.socialNetworks ?? [] }}
              />
            )}
            {tab === 2 && (
              <RequisitesTab
                volunteerId={volunteerId!}
                initial={{ requisites: [] }}
              />
            )}
          </Box>
        </Paper>
      </Container>
    </Box>
  )
}
