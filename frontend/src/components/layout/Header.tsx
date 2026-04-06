import { useState } from 'react'
import { useLocation, useParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useLangNavigate } from '../../hooks/useLangNavigate'
import { DEFAULT_LANG } from '../../lib/langUtils'
import AppBar from '@mui/material/AppBar'
import Toolbar from '@mui/material/Toolbar'
import IconButton from '@mui/material/IconButton'
import Typography from '@mui/material/Typography'
import Box from '@mui/material/Box'
import Tooltip from '@mui/material/Tooltip'
import Button from '@mui/material/Button'
import Drawer from '@mui/material/Drawer'
import List from '@mui/material/List'
import ListItem from '@mui/material/ListItem'
import ListItemButton from '@mui/material/ListItemButton'
import ListItemText from '@mui/material/ListItemText'
import Divider from '@mui/material/Divider'
import PetsIcon from '@mui/icons-material/Pets'
import FavoriteIcon from '@mui/icons-material/Favorite'
import AccountCircleIcon from '@mui/icons-material/AccountCircle'
import MenuIcon from '@mui/icons-material/Menu'
import { useAuthStore } from '../../store/authStore'
import { useThemeStore } from '../../store/themeStore'
import LanguageSwitcher from '../ui/LanguageSwitcher'
import DarkModeIcon from '@mui/icons-material/DarkMode'
import LightModeIcon from '@mui/icons-material/LightMode'

const NAV_ITEMS = [
  { labelKey: 'nav.home', path: '/' },
  { labelKey: 'nav.pets', path: '/pets' },
  { labelKey: 'nav.listings', path: '/listings' },
  { labelKey: 'nav.volunteers', path: '/volunteers' },
  { labelKey: 'nav.digest', path: '/digest' },
  { labelKey: 'nav.about', path: '/about' },
]

export default function Header() {
  const navigate = useLangNavigate()
  const location = useLocation()
  const { lang } = useParams<{ lang: string }>()
  const { t } = useTranslation()
  const { accessToken, user } = useAuthStore()
  const { mode, toggle: toggleTheme } = useThemeStore()
  const isAuthenticated = !!accessToken
  const isAdmin = user?.role === 'Admin'
  const [drawerOpen, setDrawerOpen] = useState(false)

  const prefix = `/${lang ?? DEFAULT_LANG}`
  const isActive = (path: string) =>
    path === '/'
      ? location.pathname === prefix || location.pathname === `${prefix}/`
      : location.pathname.startsWith(`${prefix}${path}`)

  return (
    <>
      <AppBar position="sticky" elevation={0} sx={{ bgcolor: '#1e1b4b', borderBottom: '1px solid rgba(255,255,255,0.08)' }}>
        <Toolbar sx={{ justifyContent: 'space-between', minHeight: { xs: 56, sm: 64 } }}>
          {/* Logo */}
          <Box
            sx={{ display: 'flex', alignItems: 'center', gap: 1, cursor: 'pointer', flexShrink: 0 }}
            onClick={() => navigate('/')}
          >
            <PetsIcon sx={{ fontSize: 28, color: '#FF6B6B' }} />
            <Typography variant="h6" fontWeight="bold" letterSpacing={1} sx={{ color: 'white' }}>
              PetZone
            </Typography>
          </Box>

          {/* Desktop nav links */}
          <Box sx={{ display: { xs: 'none', md: 'flex' }, alignItems: 'center', gap: 0.5, ml: 4, flex: 1 }}>
            {NAV_ITEMS.map((item) => (
              <Button
                key={item.path}
                onClick={() => navigate(item.path)}
                sx={{
                  color: isActive(item.path) ? '#FF6B6B' : 'rgba(255,255,255,0.75)',
                  fontWeight: isActive(item.path) ? 700 : 400,
                  textTransform: 'none',
                  fontSize: '0.95rem',
                  px: 1.5,
                  borderBottom: isActive(item.path) ? '2px solid #FF6B6B' : '2px solid transparent',
                  borderRadius: 0,
                  '&:hover': { color: 'white', bgcolor: 'transparent' },
                }}
              >
                {t(item.labelKey)}
              </Button>
            ))}
            {isAdmin && (
              <Button
                onClick={() => navigate('/admin')}
                sx={{
                  color: isActive('/admin') ? '#FF6B6B' : 'rgba(255,255,255,0.75)',
                  fontWeight: isActive('/admin') ? 700 : 400,
                  textTransform: 'none',
                  fontSize: '0.95rem',
                  px: 1.5,
                  borderBottom: isActive('/admin') ? '2px solid #FF6B6B' : '2px solid transparent',
                  borderRadius: 0,
                  '&:hover': { color: 'white', bgcolor: 'transparent' },
                }}
              >
                {t('admin.navLabel')}
              </Button>
            )}
          </Box>

          {/* Right side */}
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, ml: 'auto' }}>
            <Tooltip title={t(mode === 'dark' ? 'ui.lightMode' : 'ui.darkMode')}>
              <IconButton onClick={toggleTheme} sx={{ color: 'rgba(255,255,255,0.8)', '&:hover': { color: '#FF6B6B' } }}>
                {mode === 'dark' ? <LightModeIcon /> : <DarkModeIcon />}
              </IconButton>
            </Tooltip>
            <LanguageSwitcher />

            {isAuthenticated && (
              <Tooltip title={t('nav.favorites')}>
                <IconButton sx={{ color: 'rgba(255,255,255,0.8)', '&:hover': { color: '#FF6B6B' } }} onClick={() => navigate('/favorites')}>
                  <FavoriteIcon />
                </IconButton>
              </Tooltip>
            )}

            <Tooltip title={isAuthenticated ? t('nav.profile') : t('nav.login')}>
              <IconButton sx={{ color: 'rgba(255,255,255,0.8)', '&:hover': { color: '#FF6B6B' } }} onClick={() => navigate(isAuthenticated ? '/profile' : '/login')}>
                <AccountCircleIcon />
              </IconButton>
            </Tooltip>

            {!isAuthenticated && (
              <Button
                variant="outlined"
                size="small"
                onClick={() => navigate('/login')}
                sx={{
                  display: { xs: 'none', sm: 'flex' },
                  color: 'white',
                  borderColor: 'rgba(255,255,255,0.4)',
                  textTransform: 'none',
                  ml: 1,
                  '&:hover': { borderColor: '#FF6B6B', color: '#FF6B6B' },
                }}
              >
                {t('nav.login')}
              </Button>
            )}

            {/* Mobile burger */}
            <IconButton
              sx={{ display: { xs: 'flex', md: 'none' }, color: 'white', ml: 0.5 }}
              onClick={() => setDrawerOpen(true)}
            >
              <MenuIcon />
            </IconButton>
          </Box>
        </Toolbar>
      </AppBar>

      {/* Mobile drawer */}
      <Drawer anchor="right" open={drawerOpen} onClose={() => setDrawerOpen(false)}>
        <Box sx={{ width: 240, pt: 2 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, px: 2, pb: 2 }}>
            <PetsIcon sx={{ color: '#FF6B6B' }} />
            <Typography fontWeight="bold">PetZone</Typography>
          </Box>
          <Divider />
          <List>
            {NAV_ITEMS.map((item) => (
              <ListItem key={item.path} disablePadding>
                <ListItemButton
                  selected={isActive(item.path)}
                  onClick={() => { navigate(item.path); setDrawerOpen(false) }}
                  sx={{ '&.Mui-selected': { color: '#FF6B6B', bgcolor: '#FFF0F0' } }}
                >
                  <ListItemText primary={t(item.labelKey)} />
                </ListItemButton>
              </ListItem>
            ))}
            {isAdmin && (
              <ListItem disablePadding>
                <ListItemButton
                  selected={isActive('/admin')}
                  onClick={() => { navigate('/admin'); setDrawerOpen(false) }}
                  sx={{ '&.Mui-selected': { color: '#FF6B6B', bgcolor: '#FFF0F0' } }}
                >
                  <ListItemText primary={t('admin.navLabel')} />
                </ListItemButton>
              </ListItem>
            )}
          </List>
          <Divider />
          <List>
            {isAuthenticated ? (
              <>
                <ListItem disablePadding>
                  <ListItemButton onClick={() => { navigate('/favorites'); setDrawerOpen(false) }}>
                    <ListItemText primary={t('nav.favorites')} />
                  </ListItemButton>
                </ListItem>
                <ListItem disablePadding>
                  <ListItemButton onClick={() => { navigate('/profile'); setDrawerOpen(false) }}>
                    <ListItemText primary={t('nav.profile')} />
                  </ListItemButton>
                </ListItem>
              </>
            ) : (
              <ListItem disablePadding>
                <ListItemButton onClick={() => { navigate('/login'); setDrawerOpen(false) }}>
                  <ListItemText primary={t('nav.login')} />
                </ListItemButton>
              </ListItem>
            )}
          </List>
        </Box>
      </Drawer>
    </>
  )
}
