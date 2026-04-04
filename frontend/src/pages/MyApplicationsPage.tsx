import { useState } from 'react'
import { LangLink as Link } from '../components/ui/LangLink'
import { useLangNavigate } from '../hooks/useLangNavigate'
import PageMeta from '../components/meta/PageMeta'
import { useTranslation } from 'react-i18next'
import { useForm, useFieldArray } from 'react-hook-form'
import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Typography from '@mui/material/Typography'
import Button from '@mui/material/Button'
import Paper from '@mui/material/Paper'
import Chip from '@mui/material/Chip'
import Divider from '@mui/material/Divider'
import CircularProgress from '@mui/material/CircularProgress'
import Alert from '@mui/material/Alert'
import TextField from '@mui/material/TextField'
import Collapse from '@mui/material/Collapse'
import IconButton from '@mui/material/IconButton'
import AssignmentIcon from '@mui/icons-material/Assignment'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import ExpandLessIcon from '@mui/icons-material/ExpandLess'
import AddIcon from '@mui/icons-material/Add'
import DeleteIcon from '@mui/icons-material/Delete'
import ChatIcon from '@mui/icons-material/Chat'
import { useAuthStore } from '../store/authStore'
import { useGetMyRequestsQuery, useUpdateRequestMutation } from '../services/volunteerRequestsApi'
import { VolunteerRequestStatus } from '../types/volunteerRequest'
import type { VolunteerRequestDto } from '../types/volunteerRequest'
import { getApiError } from '../lib/getApiError'

const CORAL = '#FF6B6B'

const STATUS_CONFIG: Record<VolunteerRequestStatus, { bg: string; text: string; labelKey: string }> = {
  [VolunteerRequestStatus.Submitted]: { bg: '#DBEAFE', text: '#2563EB', labelKey: 'applications.status.Submitted' },
  [VolunteerRequestStatus.OnReview]: { bg: '#FEF3C7', text: '#D97706', labelKey: 'applications.status.OnReview' },
  [VolunteerRequestStatus.RevisionRequired]: { bg: '#FEE2E2', text: '#DC2626', labelKey: 'applications.status.RevisionRequired' },
  [VolunteerRequestStatus.Rejected]: { bg: '#F3F4F6', text: '#6B7280', labelKey: 'applications.status.Rejected' },
  [VolunteerRequestStatus.Approved]: { bg: '#D1FAE5', text: '#059669', labelKey: 'applications.status.Approved' },
}

const fieldSx = {
  '& .MuiOutlinedInput-root.Mui-focused .MuiOutlinedInput-notchedOutline': { borderColor: CORAL },
  '& .MuiInputLabel-root.Mui-focused': { color: CORAL },
}

// Inline update form for RevisionRequired status
interface UpdateFormProps {
  request: VolunteerRequestDto
  onClose: () => void
}

function UpdateForm({ request, onClose }: UpdateFormProps) {
  const { t } = useTranslation()
  const [updateRequest, { isLoading }] = useUpdateRequestMutation()
  const [error, setError] = useState<string | null>(null)

  const { register, handleSubmit, control } = useForm({
    defaultValues: {
      experience: request.volunteerInfo.experience,
      motivation: request.volunteerInfo.motivation ?? '',
      certificates: request.volunteerInfo.certificates.map((v: string) => ({ value: v })),
      requisites: request.volunteerInfo.requisites.map((v: string) => ({ value: v })),
    },
  })

  const { fields: certFields, append: appendCert, remove: removeCert } = useFieldArray({ control, name: 'certificates' })
  const { fields: reqFields, append: appendReq, remove: removeReq } = useFieldArray({ control, name: 'requisites' })

  const onSubmit = async (values: { experience: number; motivation: string; certificates: { value: string }[]; requisites: { value: string }[] }) => {
    try {
      await updateRequest({
        requestId: request.id,
        data: {
          experience: values.experience,
          motivation: values.motivation,
          certificates: values.certificates.map((c) => c.value),
          requisites: values.requisites.map((r) => r.value),
        },
      }).unwrap()
      onClose()
    } catch (err) {
      setError(getApiError(err, t))
    }
  }

  return (
    <Box component="form" onSubmit={handleSubmit(onSubmit)} sx={{ mt: 2, display: 'flex', flexDirection: 'column', gap: 2 }}>
      {error && <Alert severity="error">{error}</Alert>}

      <TextField
        {...register('experience', { valueAsNumber: true })}
        label={t('volunteerRequest.experience')}
        type="number"
        size="small"
        inputProps={{ min: 0, max: 100 }}
        sx={fieldSx}
      />

      <TextField
        {...register('motivation')}
        label={t('volunteerRequest.motivation')}
        multiline
        minRows={3}
        size="small"
        fullWidth
        sx={fieldSx}
      />

      <Box>
        <Typography variant="caption" color="text.secondary">{t('applications.certificates')}</Typography>
        {certFields.map((f, i) => (
          <Box key={f.id} sx={{ display: 'flex', gap: 1, mt: 0.5 }}>
            <TextField {...register(`certificates.${i}.value`)} size="small" fullWidth sx={fieldSx} />
            <IconButton size="small" onClick={() => removeCert(i)}><DeleteIcon fontSize="small" /></IconButton>
          </Box>
        ))}
        <Button size="small" startIcon={<AddIcon />} onClick={() => appendCert({ value: '' })} sx={{ mt: 0.5, color: '#6B7280', textTransform: 'none' }}>
          {t('volunteerRequest.addCertificate')}
        </Button>
      </Box>

      <Box>
        <Typography variant="caption" color="text.secondary">{t('applications.requisites')}</Typography>
        {reqFields.map((f, i) => (
          <Box key={f.id} sx={{ display: 'flex', gap: 1, mt: 0.5 }}>
            <TextField {...register(`requisites.${i}.value`)} size="small" fullWidth sx={fieldSx} />
            <IconButton size="small" onClick={() => removeReq(i)}><DeleteIcon fontSize="small" /></IconButton>
          </Box>
        ))}
        <Button size="small" startIcon={<AddIcon />} onClick={() => appendReq({ value: '' })} sx={{ mt: 0.5, color: '#6B7280', textTransform: 'none' }}>
          {t('volunteerRequest.addRequisite')}
        </Button>
      </Box>

      <Box sx={{ display: 'flex', gap: 1 }}>
        <Button
          type="submit"
          variant="contained"
          disabled={isLoading}
          sx={{ bgcolor: CORAL, '&:hover': { bgcolor: '#e55555' }, textTransform: 'none', fontWeight: 600, borderRadius: 2 }}
        >
          {isLoading ? <CircularProgress size={20} sx={{ color: 'white' }} /> : t('applications.updateBtn')}
        </Button>
        <Button onClick={onClose} sx={{ color: '#6B7280', textTransform: 'none' }}>
          {t('common.cancel')}
        </Button>
      </Box>
    </Box>
  )
}

// Single application card
function ApplicationCard({ request }: { request: VolunteerRequestDto }) {
  const { t } = useTranslation()
  const navigate = useLangNavigate()
  const [expanded, setExpanded] = useState(false)
  const [editing, setEditing] = useState(false)

  const cfg = STATUS_CONFIG[request.status]
  const date = new Date(request.createdAt).toLocaleDateString()
  const canUpdate = request.status === VolunteerRequestStatus.RevisionRequired

  return (
    <Paper elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 3, overflow: 'hidden' }}>
      {/* Header */}
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', p: 2.5, cursor: 'pointer' }} onClick={() => setExpanded((v) => !v)}>
        <Box>
          <Chip label={t(cfg.labelKey)} size="small" sx={{ bgcolor: cfg.bg, color: cfg.text, fontWeight: 600, mb: 0.5 }} />
          <Typography variant="body2" color="text.secondary">{t('applications.createdAt', { date })}</Typography>
          <Typography variant="body2">{t('applications.experience', { years: request.volunteerInfo.experience })}</Typography>
        </Box>
        <IconButton size="small">{expanded ? <ExpandLessIcon /> : <ExpandMoreIcon />}</IconButton>
      </Box>

      <Collapse in={expanded}>
        <Divider />
        <Box sx={{ p: 2.5 }}>
          {/* Rejection/revision comment */}
          {request.rejectionComment && (
            <Alert severity={canUpdate ? 'warning' : 'error'} sx={{ mb: 2 }}>
              <Typography variant="caption" fontWeight="bold">{t('applications.comment')}</Typography>
              <Typography variant="body2">{request.rejectionComment}</Typography>
            </Alert>
          )}

          {/* Certificates */}
          {request.volunteerInfo.certificates.length > 0 && (
            <Box sx={{ mb: 2 }}>
              <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', fontSize: 11, letterSpacing: 0.5 }}>
                {t('applications.certificates')}
              </Typography>
              {request.volunteerInfo.certificates.map((c, i) => (
                <Typography key={i} variant="body2" sx={{ mt: 0.5 }}>• {c}</Typography>
              ))}
            </Box>
          )}

          {/* Requisites */}
          {request.volunteerInfo.requisites.length > 0 && (
            <Box sx={{ mb: 2 }}>
              <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', fontSize: 11, letterSpacing: 0.5 }}>
                {t('applications.requisites')}
              </Typography>
              {request.volunteerInfo.requisites.map((r, i) => (
                <Typography key={i} variant="body2" sx={{ mt: 0.5 }}>• {r}</Typography>
              ))}
            </Box>
          )}

          {/* Actions row */}
          <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
            {canUpdate && !editing && (
              <Button
                variant="outlined"
                size="small"
                onClick={() => setEditing(true)}
                sx={{ borderColor: CORAL, color: CORAL, textTransform: 'none', '&:hover': { bgcolor: '#FFF0F0' } }}
              >
                {t('applications.updateBtn')}
              </Button>
            )}
            {request.discussionId && (
              <Button
                variant="outlined"
                size="small"
                startIcon={<ChatIcon />}
                onClick={() => navigate(`/discussion/${request.discussionId}`)}
                sx={{ borderColor: '#6B7280', color: '#6B7280', textTransform: 'none', '&:hover': { borderColor: CORAL, color: CORAL, bgcolor: '#FFF0F0' } }}
              >
                {t('chat.openChat')}
              </Button>
            )}
          </Box>
          {editing && <UpdateForm request={request} onClose={() => setEditing(false)} />}
        </Box>
      </Collapse>
    </Paper>
  )
}

export default function MyApplicationsPage() {
  const { t } = useTranslation()
  const navigate = useLangNavigate()
  const { user } = useAuthStore()

  const { data, isLoading, isError } = useGetMyRequestsQuery(
    { page: 1, pageSize: 20 },
    { skip: !user },
  )

  if (!user) {
    return (
      <Box sx={{ minHeight: '60vh', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
        <Box sx={{ textAlign: 'center' }}>
          <Typography variant="h6" sx={{ mb: 2 }}>{t('profile.notLoggedIn')}</Typography>
          <Button component={Link} to="/login" variant="contained"
            sx={{ bgcolor: CORAL, '&:hover': { bgcolor: '#e55555' }, textTransform: 'none', fontWeight: 700, borderRadius: 2, px: 4 }}>
            {t('profile.loginBtn')}
          </Button>
        </Box>
      </Box>
    )
  }

  return (
    <Container maxWidth="md" sx={{ py: 4 }}>
      <PageMeta title={t('applications.title')} description={t('applications.title')} path="/volunteer-applications" noIndex />
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5, mb: 4 }}>
        <AssignmentIcon sx={{ color: CORAL, fontSize: 28 }} />
        <Typography variant="h4" fontWeight="bold">{t('applications.title')}</Typography>
      </Box>

      {isLoading && (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 10 }}>
          <CircularProgress sx={{ color: CORAL }} />
        </Box>
      )}

      {isError && <Alert severity="error">{t('errors.unknown')}</Alert>}

      {!isLoading && !isError && (data?.items.length ?? 0) === 0 && (
        <Box sx={{ textAlign: 'center', py: 10 }}>
          <AssignmentIcon sx={{ fontSize: 72, color: '#E5E7EB', mb: 2 }} />
          <Typography variant="h6" color="text.secondary">{t('applications.empty')}</Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mt: 1, mb: 3 }}>
            {t('applications.emptyHint')}
          </Typography>
          <Button
            onClick={() => navigate('/register/volunteer')}
            variant="contained"
            sx={{ bgcolor: CORAL, '&:hover': { bgcolor: '#e55555' }, textTransform: 'none', fontWeight: 600, borderRadius: 2, px: 4 }}
          >
            {t('applications.submitBtn')}
          </Button>
        </Box>
      )}

      {!isLoading && (data?.items.length ?? 0) > 0 && (
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
          {data!.items.map((req) => (
            <ApplicationCard key={req.id} request={req} />
          ))}
        </Box>
      )}
    </Container>
  )
}
