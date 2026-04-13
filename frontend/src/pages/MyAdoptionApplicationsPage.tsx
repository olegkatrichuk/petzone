import { useTranslation } from 'react-i18next'
import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Typography from '@mui/material/Typography'
import Paper from '@mui/material/Paper'
import Chip from '@mui/material/Chip'
import Avatar from '@mui/material/Avatar'
import Divider from '@mui/material/Divider'
import Skeleton from '@mui/material/Skeleton'
import PetsIcon from '@mui/icons-material/Pets'
import { useGetMyApplicationsQuery } from '../services/adoptionApi'
import { useLangNavigate } from '../hooks/useLangNavigate'
import PageMeta from '../components/meta/PageMeta'

const STATUS_CHIP: Record<string, { color: 'default' | 'success' | 'error'; labelKey: string }> = {
  Pending:  { color: 'default',  labelKey: 'adoption.myApplications.pending'  },
  Approved: { color: 'success',  labelKey: 'adoption.myApplications.approved' },
  Rejected: { color: 'error',    labelKey: 'adoption.myApplications.rejected' },
}

export default function MyAdoptionApplicationsPage() {
  const { t } = useTranslation()
  const navigate = useLangNavigate()
  const { data: applications = [], isLoading } = useGetMyApplicationsQuery()

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'background.default', py: 4 }}>
      <PageMeta title={t('adoption.myApplications.title')} description="" path="/my-adoptions" noIndex />
      <Container maxWidth="md">
        <Typography variant="h5" fontWeight="bold" sx={{ mb: 3 }}>
          {t('adoption.myApplications.title')}
        </Typography>

        {isLoading ? (
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
            {[0, 1, 2].map((i) => (
              <Skeleton key={i} variant="rectangular" height={100} sx={{ borderRadius: 3 }} />
            ))}
          </Box>
        ) : applications.length === 0 ? (
          <Paper elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 4, p: 4, textAlign: 'center' }}>
            <PetsIcon sx={{ fontSize: 48, color: '#D1D5DB', mb: 1 }} />
            <Typography color="text.secondary">{t('adoption.myApplications.empty')}</Typography>
          </Paper>
        ) : (
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
            {applications.map((app) => {
              const status = STATUS_CHIP[app.status] ?? STATUS_CHIP.Pending
              return (
                <Paper
                  key={app.id}
                  elevation={0}
                  sx={{ border: '1px solid #E5E7EB', borderRadius: 3, p: 2.5, cursor: 'pointer', '&:hover': { borderColor: '#FF6B6B' } }}
                  onClick={() => navigate(`/pets/${app.petId}`)}
                >
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
                    <Avatar
                      src={app.petMainPhoto ?? undefined}
                      variant="rounded"
                      sx={{ width: 64, height: 64, bgcolor: '#F3F4F6' }}
                    >
                      <PetsIcon sx={{ color: '#9CA3AF' }} />
                    </Avatar>
                    <Box sx={{ flex: 1 }}>
                      <Typography fontWeight={600}>{app.petNickname}</Typography>
                      <Typography variant="body2" color="text.secondary">
                        {new Date(app.createdAt).toLocaleDateString()}
                      </Typography>
                    </Box>
                    <Chip label={t(status.labelKey)} color={status.color} size="small" />
                  </Box>
                  {app.message && (
                    <>
                      <Divider sx={{ my: 1.5 }} />
                      <Typography variant="body2" color="text.secondary">{app.message}</Typography>
                    </>
                  )}
                </Paper>
              )
            })}
          </Box>
        )}
      </Container>
    </Box>
  )
}
