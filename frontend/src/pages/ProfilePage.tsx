import { LangLink as Link } from '../components/ui/LangLink'
import { useTranslation } from 'react-i18next'
import { useLangNavigate } from '../hooks/useLangNavigate'
import PageMeta from '../components/meta/PageMeta'
import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Paper from '@mui/material/Paper'
import Typography from '@mui/material/Typography'
import Avatar from '@mui/material/Avatar'
import Button from '@mui/material/Button'
import Chip from '@mui/material/Chip'
import Divider from '@mui/material/Divider'
import List from '@mui/material/List'
import ListItem from '@mui/material/ListItem'
import ListItemIcon from '@mui/material/ListItemIcon'
import ListItemText from '@mui/material/ListItemText'
import EmailIcon from '@mui/icons-material/Email'
import BadgeIcon from '@mui/icons-material/Badge'
import VolunteerActivismIcon from '@mui/icons-material/VolunteerActivism'
import AssignmentIcon from '@mui/icons-material/Assignment'
import CampaignIcon from '@mui/icons-material/Campaign'
import StarIcon from '@mui/icons-material/Star'
import LogoutIcon from '@mui/icons-material/Logout'
import Alert from '@mui/material/Alert'
import CircularProgress from '@mui/material/CircularProgress'
import { useAuthStore } from '../store/authStore'
import { useGetUserByIdQuery } from '../services/accountsApi'
import { useFavoritesStore } from '../store/favoritesStore'

const CORAL = '#FF6B6B'

const ROLE_COLORS: Record<string, { bg: string; text: string }> = {
  Volunteer: { bg: '#DBEAFE', text: '#2563EB' },
  Admin: { bg: '#FEE2E2', text: '#DC2626' },
  Participant: { bg: '#D1FAE5', text: '#059669' },
}

export default function ProfilePage() {
  const { t } = useTranslation()
  const navigate = useLangNavigate()
  const { user, logout } = useAuthStore()
  const { data: userDto, isLoading: userLoading, isError: userError } = useGetUserByIdQuery(user?.id ?? '', { skip: !user })
  const { ids: favoriteIds } = useFavoritesStore()

  // Not logged in
  if (!user) {
    return (
      <Box sx={{ minHeight: '60vh', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
        <Box sx={{ textAlign: 'center' }}>
          <Typography variant="h6" sx={{ mb: 2 }}>{t('profile.notLoggedIn')}</Typography>
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
            {t('profile.loginBtn')}
          </Button>
        </Box>
      </Box>
    )
  }

  const initials = `${user.firstName?.[0] ?? ''}${user.lastName?.[0] ?? ''}`.toUpperCase()
  const fullName = [user.firstName, user.lastName].filter(Boolean).join(' ')
  const roleColor = ROLE_COLORS[user.role] ?? { bg: '#F3F4F6', text: '#6B7280' }
  const roleLabel = t(`profile.roles.${user.role}`, { defaultValue: user.role })

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  if (userLoading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '40vh' }}>
        <CircularProgress sx={{ color: CORAL }} />
      </Box>
    )
  }

  return (
    <Box sx={{ bgcolor: '#FAFAFA', minHeight: '100%', py: 4 }}>
      <PageMeta title={t('profile.title')} description={t('profile.title')} path="/profile" noIndex />
      <Container maxWidth="sm">
        {userError && (
          <Alert severity="warning" sx={{ mb: 3 }}>{t('errors.unknown')}</Alert>
        )}
        <Typography variant="h5" fontWeight="bold" sx={{ mb: 3 }}>
          {t('profile.title')}
        </Typography>

        <Paper elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 3, overflow: 'hidden' }}>
          {/* Header band */}
          <Box sx={{ bgcolor: CORAL, height: 80 }} />

          {/* Avatar + name */}
          <Box sx={{ px: 3, pb: 3 }}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-end', mt: -5, mb: 2 }}>
              <Avatar
                sx={{
                  width: 80, height: 80, fontSize: 28,
                  bgcolor: '#FFF0F0', color: CORAL,
                  border: '3px solid white',
                  fontWeight: 700,
                }}
              >
                {initials}
              </Avatar>

              <Chip
                label={roleLabel}
                size="small"
                sx={{ bgcolor: roleColor.bg, color: roleColor.text, fontWeight: 600 }}
              />
            </Box>

            <Typography variant="h6" fontWeight="bold" sx={{ mb: 0.5 }}>
              {fullName}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              {t('profile.memberSince')}
            </Typography>

            <Divider sx={{ my: 2.5 }} />

            {/* Info list */}
            <List disablePadding sx={{ mb: 2 }}>
              <ListItem disablePadding sx={{ py: 0.8 }}>
                <ListItemIcon sx={{ minWidth: 36 }}>
                  <EmailIcon sx={{ fontSize: 20, color: '#6B7280' }} />
                </ListItemIcon>
                <ListItemText
                  primary={t('profile.email')}
                  secondary={user.email}
                  primaryTypographyProps={{ variant: 'caption', color: 'text.secondary', textTransform: 'uppercase', letterSpacing: 0.5 }}
                  secondaryTypographyProps={{ variant: 'body2', color: 'text.primary' }}
                />
              </ListItem>

              <ListItem disablePadding sx={{ py: 0.8 }}>
                <ListItemIcon sx={{ minWidth: 36 }}>
                  <BadgeIcon sx={{ fontSize: 20, color: '#6B7280' }} />
                </ListItemIcon>
                <ListItemText
                  primary={t('profile.role')}
                  secondary={roleLabel}
                  primaryTypographyProps={{ variant: 'caption', color: 'text.secondary', textTransform: 'uppercase', letterSpacing: 0.5 }}
                  secondaryTypographyProps={{ variant: 'body2', color: 'text.primary' }}
                />
              </ListItem>
            </List>

            <Divider sx={{ mb: 2.5 }} />

            {/* Actions */}
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1.5 }}>
              {/* Volunteer profile shortcut */}
              {/* Favorites count */}
              <Button
                startIcon={<StarIcon />}
                variant="outlined"
                onClick={() => navigate('/favorites')}
                sx={{
                  borderColor: '#E5E7EB',
                  color: '#6B7280',
                  textTransform: 'none',
                  borderRadius: 2,
                  justifyContent: 'flex-start',
                  '&:hover': { borderColor: CORAL, color: CORAL },
                }}
              >
                {t('nav.favorites')} ({favoriteIds.length})
              </Button>

              {/* Applications */}
              {userDto?.participantAccount && (
                <Button
                  startIcon={<AssignmentIcon />}
                  variant="outlined"
                  onClick={() => navigate('/volunteer-applications')}
                  sx={{
                    borderColor: '#E5E7EB',
                    color: '#6B7280',
                    textTransform: 'none',
                    borderRadius: 2,
                    justifyContent: 'flex-start',
                    '&:hover': { borderColor: CORAL, color: CORAL },
                  }}
                >
                  {t('applications.title')}
                </Button>
              )}

              {/* My listings */}
              <Button
                startIcon={<CampaignIcon />}
                variant="outlined"
                onClick={() => navigate('/my-listings')}
                sx={{
                  borderColor: '#E5E7EB',
                  color: '#6B7280',
                  textTransform: 'none',
                  borderRadius: 2,
                  justifyContent: 'flex-start',
                  '&:hover': { borderColor: CORAL, color: CORAL },
                }}
              >
                {t('listings.myListings')}
              </Button>

              {/* Volunteer profile shortcut */}
              {user.role === 'Volunteer' && (
                <Button
                  startIcon={<VolunteerActivismIcon />}
                  variant="contained"
                  onClick={() => navigate(`/volunteers/${user.id}`)}
                  sx={{
                    bgcolor: CORAL,
                    '&:hover': { bgcolor: '#e55555' },
                    textTransform: 'none',
                    fontWeight: 600,
                    borderRadius: 2,
                  }}
                >
                  {t('profile.volunteerProfile')}
                </Button>
              )}

              <Button
                startIcon={<LogoutIcon />}
                variant="outlined"
                onClick={handleLogout}
                sx={{
                  borderColor: '#E5E7EB',
                  color: '#6B7280',
                  textTransform: 'none',
                  borderRadius: 2,
                  '&:hover': { borderColor: '#DC2626', color: '#DC2626', bgcolor: '#FEF2F2' },
                }}
              >
                {t('profile.logoutBtn')}
              </Button>
            </Box>
          </Box>
        </Paper>
      </Container>
    </Box>
  )
}
