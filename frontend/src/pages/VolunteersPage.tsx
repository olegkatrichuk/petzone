import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useLangNavigate } from '../hooks/useLangNavigate'
import PageMeta from '../components/meta/PageMeta'
import Container from '@mui/material/Container'
import Typography from '@mui/material/Typography'
import Box from '@mui/material/Box'
import Grid from '@mui/material/Grid'
import Card from '@mui/material/Card'
import CardContent from '@mui/material/CardContent'
import CardActions from '@mui/material/CardActions'
import Avatar from '@mui/material/Avatar'
import Button from '@mui/material/Button'
import TextField from '@mui/material/TextField'
import Chip from '@mui/material/Chip'
import Alert from '@mui/material/Alert'
import VolunteerCardSkeleton from '../components/volunteers/VolunteerCardSkeleton'
import InputAdornment from '@mui/material/InputAdornment'
import Divider from '@mui/material/Divider'
import WorkHistoryIcon from '@mui/icons-material/WorkHistory'
import EmailIcon from '@mui/icons-material/Email'
import SearchIcon from '@mui/icons-material/Search'
import { useGetVolunteersQuery } from '../services/volunteersApi'
import Pagination from '../components/ui/Pagination'

const CORAL = '#FF6B6B'
const PAGE_SIZE = 12

export default function VolunteersPage() {
  const { t } = useTranslation()
  const navigate = useLangNavigate()
  const [page, setPage] = useState(1)
  const [search, setSearch] = useState('')

  const { data, isLoading, isError, refetch } = useGetVolunteersQuery({ page, pageSize: PAGE_SIZE })

  const volunteers = data?.items ?? []

  // Client-side name filter within the loaded page
  const filtered = search.trim()
    ? volunteers.filter((v) => {
        const full = `${v.firstName} ${v.lastName} ${v.patronymic}`.toLowerCase()
        return full.includes(search.toLowerCase())
      })
    : volunteers

  const handlePageChange = (newPage: number) => {
    setPage(newPage)
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <PageMeta title={t('volunteers.pageTitle')} description={t('volunteers.pageTitle')} path="/volunteers" />
      {/* Header */}
      <Box sx={{ mb: 4 }}>
        <Typography variant="h4" fontWeight="bold" sx={{ mb: 0.5 }}>
          {t('volunteers.pageTitle')}
        </Typography>
      </Box>

      {/* Search */}
      <Box sx={{ mb: 3, maxWidth: 400 }}>
        <TextField
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder={t('volunteers.searchByName')}
          size="small"
          fullWidth
          slotProps={{
            input: {
              startAdornment: (
                <InputAdornment position="start">
                  <SearchIcon sx={{ color: '#9CA3AF', fontSize: 20 }} />
                </InputAdornment>
              ),
            },
          }}
          sx={{
            '& .MuiOutlinedInput-root.Mui-focused .MuiOutlinedInput-notchedOutline': { borderColor: CORAL },
          }}
        />
      </Box>

      {/* Loading */}
      {isLoading && (
        <Grid container spacing={3}>
          {Array.from({ length: 12 }).map((_, i) => (
            <Grid key={i} size={{ xs: 12, sm: 6, lg: 4 }}>
              <VolunteerCardSkeleton />
            </Grid>
          ))}
        </Grid>
      )}

      {/* Error */}
      {isError && (
        <Alert
          severity="error"
          action={
            <Button size="small" onClick={refetch} sx={{ color: CORAL, textTransform: 'none' }}>
              {t('errors.retry')}
            </Button>
          }
          sx={{ mb: 3 }}
        >
          {t('volunteers.loadError')}
        </Alert>
      )}

      {/* Empty */}
      {!isLoading && !isError && filtered.length === 0 && (
        <Box sx={{ textAlign: 'center', py: 10 }}>
          <Typography variant="h6" color="text.secondary">{t('volunteers.notFound')}</Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
            {t('volunteers.notFoundHint')}
          </Typography>
        </Box>
      )}

      {/* Grid */}
      {!isLoading && filtered.length > 0 && (
        <Grid container spacing={3}>
          {filtered.map((volunteer) => {
            const initials = `${volunteer.firstName?.[0] ?? ''}${volunteer.lastName?.[0] ?? ''}`
            const fullName = [volunteer.lastName, volunteer.firstName, volunteer.patronymic]
              .filter(Boolean)
              .join(' ')

            return (
              <Grid size={{ xs: 12, sm: 6, md: 4 }} key={volunteer.id}>
                <Card
                  elevation={0}
                  sx={{
                    border: '1px solid #E5E7EB',
                    borderRadius: 3,
                    height: '100%',
                    display: 'flex',
                    flexDirection: 'column',
                    transition: 'box-shadow 0.2s, transform 0.2s',
                    '&:hover': {
                      boxShadow: '0 8px 24px rgba(255,107,107,0.15)',
                      transform: 'translateY(-2px)',
                    },
                  }}
                >
                  <CardContent sx={{ flex: 1 }}>
                    {/* Avatar + name */}
                    <Box sx={{ display: 'flex', gap: 2, alignItems: 'center', mb: 2 }}>
                      <Avatar
                        src={volunteer.photoPath ?? undefined}
                        sx={{ width: 56, height: 56, bgcolor: CORAL, fontSize: 20, flexShrink: 0 }}
                      >
                        {!volunteer.photoPath && initials}
                      </Avatar>
                      <Box sx={{ minWidth: 0 }}>
                        <Typography
                          variant="subtitle1"
                          fontWeight="bold"
                          sx={{ lineHeight: 1.3, wordBreak: 'break-word' }}
                        >
                          {fullName}
                        </Typography>
                        <Chip
                          icon={<WorkHistoryIcon sx={{ fontSize: '14px !important' }} />}
                          label={t('volunteers.experience', { years: volunteer.experienceYears })}
                          size="small"
                          variant="outlined"
                          sx={{ mt: 0.5, borderColor: '#E5E7EB', color: '#6B7280', fontSize: 11 }}
                        />
                      </Box>
                    </Box>

                    {/* Description */}
                    {volunteer.generalDescription && (
                      <Typography
                        variant="body2"
                        color="text.secondary"
                        sx={{
                          mb: 2,
                          overflow: 'hidden',
                          display: '-webkit-box',
                          WebkitLineClamp: 3,
                          WebkitBoxOrient: 'vertical',
                          lineHeight: 1.6,
                        }}
                      >
                        {volunteer.generalDescription}
                      </Typography>
                    )}

                    <Divider sx={{ mb: 1.5 }} />

                    {/* Email */}
                    {volunteer.email && (
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                        <EmailIcon sx={{ fontSize: 16, color: '#9CA3AF' }} />
                        <Typography
                          variant="caption"
                          component="a"
                          href={`mailto:${volunteer.email}`}
                          sx={{
                            color: CORAL,
                            textDecoration: 'none',
                            '&:hover': { textDecoration: 'underline' },
                            overflow: 'hidden',
                            textOverflow: 'ellipsis',
                            whiteSpace: 'nowrap',
                          }}
                        >
                          {volunteer.email}
                        </Typography>
                      </Box>
                    )}
                  </CardContent>

                  <CardActions sx={{ px: 2, pb: 2 }}>
                    <Button
                      variant="contained"
                      size="small"
                      fullWidth
                      onClick={() => navigate(`/volunteers/${volunteer.id}`)}
                      sx={{
                        bgcolor: CORAL,
                        '&:hover': { bgcolor: '#e55555' },
                        borderRadius: 2,
                        textTransform: 'none',
                        fontWeight: 600,
                      }}
                    >
                      {t('volunteers.viewProfile')}
                    </Button>
                  </CardActions>
                </Card>
              </Grid>
            )
          })}
        </Grid>
      )}

      {/* Pagination */}
      {!isLoading && data && (
        <Pagination
          page={page}
          pageSize={PAGE_SIZE}
          totalCount={data.totalCount}
          onChange={handlePageChange}
          ofLabel={t('volunteers.of')}
        />
      )}
    </Container>
  )
}
