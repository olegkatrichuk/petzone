import { useState } from 'react'
import { useParams } from 'react-router-dom'
import { useLangNavigate } from '../hooks/useLangNavigate'
import PageMeta from '../components/meta/PageMeta'
import { useTranslation } from 'react-i18next'
import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Paper from '@mui/material/Paper'
import Typography from '@mui/material/Typography'
import Button from '@mui/material/Button'
import Avatar from '@mui/material/Avatar'
import Divider from '@mui/material/Divider'
import CircularProgress from '@mui/material/CircularProgress'
import Alert from '@mui/material/Alert'
import IconButton from '@mui/material/IconButton'
import Tooltip from '@mui/material/Tooltip'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import ContentCopyIcon from '@mui/icons-material/ContentCopy'
import CheckIcon from '@mui/icons-material/Check'
import LinkIcon from '@mui/icons-material/Link'
import FavoriteIcon from '@mui/icons-material/Favorite'
import { useGetVolunteerByIdQuery } from '../services/volunteersApi'

const CORAL = '#FF6B6B'

function CopyField({ value }: { value: string }) {
  const { t } = useTranslation()
  const [copied, setCopied] = useState(false)

  const handleCopy = () => {
    navigator.clipboard.writeText(value).then(() => {
      setCopied(true)
      setTimeout(() => setCopied(false), 2000)
    })
  }

  return (
    <Box
      sx={{
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        bgcolor: '#F9FAFB',
        border: '1px solid #E5E7EB',
        borderRadius: 2,
        px: 2,
        py: 1.2,
        gap: 1,
      }}
    >
      <Typography
        variant="body2"
        sx={{ fontFamily: 'monospace', wordBreak: 'break-all', flex: 1 }}
      >
        {value}
      </Typography>
      <Tooltip title={copied ? t('help.copied') : t('help.copy')}>
        <IconButton size="small" onClick={handleCopy} sx={{ flexShrink: 0, color: copied ? '#22C55E' : '#6B7280' }}>
          {copied ? <CheckIcon fontSize="small" /> : <ContentCopyIcon fontSize="small" />}
        </IconButton>
      </Tooltip>
    </Box>
  )
}

export default function HelpVolunteerPage() {
  const { t } = useTranslation()
  const { volunteerId } = useParams<{ volunteerId: string }>()
  const navigate = useLangNavigate()

  const { data: volunteer, isLoading, isError } = useGetVolunteerByIdQuery(
    volunteerId ?? '',
    { skip: !volunteerId },
  )

  if (isLoading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '60vh' }}>
        <CircularProgress sx={{ color: CORAL }} />
      </Box>
    )
  }

  if (isError || !volunteer) {
    return (
      <Container maxWidth="sm" sx={{ py: 8 }}>
        <Alert severity="error">{t('volunteerProfile.notFound')}</Alert>
      </Container>
    )
  }

  const initials = `${volunteer.firstName?.[0] ?? ''}${volunteer.lastName?.[0] ?? ''}`.toUpperCase()
  const fullName = [volunteer.lastName, volunteer.firstName].filter(Boolean).join(' ')

  return (
    <Box sx={{ bgcolor: '#FAFAFA', minHeight: '100%', py: 4 }}>
      <PageMeta title={t('help.title')} description={t('help.subtitle')} path={`/help/${volunteerId}`} />
      <Container maxWidth="sm">
        <Button
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate(`/volunteers/${volunteerId}`)}
          sx={{ mb: 3, color: '#6B7280', textTransform: 'none' }}
        >
          {t('help.backToProfile')}
        </Button>

        {/* Header */}
        <Paper elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 3, p: 3, mb: 3 }}>
          <Box sx={{ display: 'flex', gap: 2, alignItems: 'center', mb: 2 }}>
            <Avatar
              src={volunteer.photoPath ?? undefined}
              sx={{ width: 64, height: 64, bgcolor: CORAL, fontSize: 22, fontWeight: 700 }}
            >
              {!volunteer.photoPath && initials}
            </Avatar>
            <Box>
              <Typography variant="h6" fontWeight="bold">{fullName}</Typography>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mt: 0.5 }}>
                <FavoriteIcon sx={{ fontSize: 16, color: CORAL }} />
                <Typography variant="body2" color="text.secondary">
                  {t('help.title')}
                </Typography>
              </Box>
            </Box>
          </Box>
          <Typography variant="body2" color="text.secondary">
            {t('help.subtitle')}
          </Typography>
        </Paper>

        {/* Requisites */}
        <Paper elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 3, p: 3, mb: 3 }}>
          <Typography variant="subtitle1" fontWeight="bold" sx={{ mb: 2 }}>
            {t('help.requisitesTitle')}
          </Typography>

          {(!volunteer.socialNetworks || volunteer.socialNetworks.length === 0) ? (
            <Typography variant="body2" color="text.secondary">
              {t('help.noRequisites')}
            </Typography>
          ) : (
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1.5 }}>
              {volunteer.socialNetworks.map((sn, i) => (
                <Box key={i}>
                  <Typography variant="caption" color="text.secondary" sx={{ mb: 0.5, display: 'block' }}>
                    {sn.name}
                  </Typography>
                  <CopyField value={sn.link} />
                </Box>
              ))}
            </Box>
          )}
        </Paper>

        {/* Social networks */}
        {volunteer.socialNetworks && volunteer.socialNetworks.length > 0 && (
          <Paper elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 3, p: 3 }}>
            <Typography variant="subtitle1" fontWeight="bold" sx={{ mb: 2 }}>
              {t('help.socialTitle')}
            </Typography>
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
              {volunteer.socialNetworks.map((sn, i) => (
                <Box key={i} sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <LinkIcon sx={{ fontSize: 17, color: '#6B7280' }} />
                  <Typography
                    variant="body2"
                    component="a"
                    href={sn.link}
                    target="_blank"
                    rel="noopener noreferrer"
                    sx={{ color: CORAL, textDecoration: 'none', '&:hover': { textDecoration: 'underline' } }}
                  >
                    {sn.name}
                  </Typography>
                </Box>
              ))}
            </Box>
          </Paper>
        )}

        <Divider sx={{ my: 3 }} />

        <Button
          variant="contained"
          fullWidth
          onClick={() => navigate(`/volunteers/${volunteerId}`)}
          sx={{
            bgcolor: CORAL,
            '&:hover': { bgcolor: '#e55555' },
            textTransform: 'none',
            fontWeight: 600,
            borderRadius: 2,
          }}
        >
          {t('help.backToProfile')}
        </Button>
      </Container>
    </Box>
  )
}
