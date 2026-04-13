import { useTranslation } from 'react-i18next'
import { useParams } from 'react-router-dom'
import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Typography from '@mui/material/Typography'
import Paper from '@mui/material/Paper'
import Chip from '@mui/material/Chip'
import Avatar from '@mui/material/Avatar'
import Button from '@mui/material/Button'
import Divider from '@mui/material/Divider'
import Skeleton from '@mui/material/Skeleton'
import PhoneIcon from '@mui/icons-material/Phone'
import PetsIcon from '@mui/icons-material/Pets'
import CheckIcon from '@mui/icons-material/Check'
import CloseIcon from '@mui/icons-material/Close'
import { useGetVolunteerApplicationsQuery, useUpdateApplicationStatusMutation } from '../services/adoptionApi'
import { useLangNavigate } from '../hooks/useLangNavigate'
import { toast } from '../store/toastStore'
import PageMeta from '../components/meta/PageMeta'

const STATUS_CHIP: Record<string, { color: 'default' | 'success' | 'error'; labelKey: string }> = {
  Pending:  { color: 'default', labelKey: 'adoption.myApplications.pending'  },
  Approved: { color: 'success', labelKey: 'adoption.myApplications.approved' },
  Rejected: { color: 'error',   labelKey: 'adoption.myApplications.rejected' },
}

export default function VolunteerAdoptionApplicationsPage() {
  const { t } = useTranslation()
  const { volunteerId } = useParams<{ volunteerId: string }>()
  const navigate = useLangNavigate()
  const { data: applications = [], isLoading } = useGetVolunteerApplicationsQuery(volunteerId ?? '', { skip: !volunteerId })
  const [updateStatus] = useUpdateApplicationStatusMutation()

  const handleAction = async (applicationId: string, action: 'approve' | 'reject') => {
    try {
      await updateStatus({ applicationId, action }).unwrap()
      toast.success(action === 'approve' ? t('adoption.myApplications.approved') : t('adoption.myApplications.rejected'))
    } catch {
      toast.error(t('common.error'))
    }
  }

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'background.default', py: 4 }}>
      <PageMeta title={t('adoption.volunteerApplications.title')} description="" path={`/adoption-applications/${volunteerId}`} noIndex />
      <Container maxWidth="md">
        <Typography variant="h5" fontWeight="bold" sx={{ mb: 3 }}>
          {t('adoption.volunteerApplications.title')}
        </Typography>

        {isLoading ? (
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
            {[0, 1, 2].map((i) => (
              <Skeleton key={i} variant="rectangular" height={120} sx={{ borderRadius: 3 }} />
            ))}
          </Box>
        ) : applications.length === 0 ? (
          <Paper elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 4, p: 4, textAlign: 'center' }}>
            <PetsIcon sx={{ fontSize: 48, color: '#D1D5DB', mb: 1 }} />
            <Typography color="text.secondary">{t('adoption.volunteerApplications.empty')}</Typography>
          </Paper>
        ) : (
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
            {applications.map((app) => {
              const status = STATUS_CHIP[app.status] ?? STATUS_CHIP.Pending
              return (
                <Paper key={app.id} elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 3, p: 2.5 }}>
                  <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 2 }}>
                    <Avatar
                      src={app.petMainPhoto ?? undefined}
                      variant="rounded"
                      sx={{ width: 64, height: 64, bgcolor: '#F3F4F6', cursor: 'pointer', flexShrink: 0 }}
                      onClick={() => navigate(`/pets/${app.petId}`)}
                    >
                      <PetsIcon sx={{ color: '#9CA3AF' }} />
                    </Avatar>

                    <Box sx={{ flex: 1 }}>
                      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', flexWrap: 'wrap', gap: 1 }}>
                        <Box>
                          <Typography fontWeight={600}>{app.petNickname}</Typography>
                          <Typography variant="body2" color="text.secondary" sx={{ mb: 0.5 }}>
                            {app.applicantName} · {new Date(app.createdAt).toLocaleDateString()}
                          </Typography>
                          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                            <PhoneIcon sx={{ fontSize: 14, color: '#6B7280' }} />
                            <Typography variant="body2">
                              <a href={`tel:${app.applicantPhone}`} style={{ color: '#374151', textDecoration: 'none' }}>
                                {app.applicantPhone}
                              </a>
                            </Typography>
                          </Box>
                        </Box>
                        <Chip label={t(status.labelKey)} color={status.color} size="small" />
                      </Box>

                      {app.message && (
                        <>
                          <Divider sx={{ my: 1.5 }} />
                          <Typography variant="body2" color="text.secondary">{app.message}</Typography>
                        </>
                      )}

                      {app.status === 'Pending' && (
                        <Box sx={{ display: 'flex', gap: 1, mt: 2 }}>
                          <Button
                            size="small"
                            variant="contained"
                            startIcon={<CheckIcon />}
                            onClick={() => handleAction(app.id, 'approve')}
                            sx={{ bgcolor: '#10B981', textTransform: 'none', '&:hover': { bgcolor: '#059669' } }}
                          >
                            {t('adoption.volunteerApplications.approve')}
                          </Button>
                          <Button
                            size="small"
                            variant="outlined"
                            startIcon={<CloseIcon />}
                            onClick={() => handleAction(app.id, 'reject')}
                            sx={{ borderColor: '#EF4444', color: '#EF4444', textTransform: 'none', '&:hover': { borderColor: '#DC2626', bgcolor: '#FEF2F2' } }}
                          >
                            {t('adoption.volunteerApplications.reject')}
                          </Button>
                        </Box>
                      )}
                    </Box>
                  </Box>
                </Paper>
              )
            })}
          </Box>
        )}
      </Container>
    </Box>
  )
}
