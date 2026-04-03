import { useLangNavigate } from '../../hooks/useLangNavigate'
import { useTranslation } from 'react-i18next'
import Box from '@mui/material/Box'
import Typography from '@mui/material/Typography'
import IconButton from '@mui/material/IconButton'
import Tooltip from '@mui/material/Tooltip'
import Divider from '@mui/material/Divider'
import Link from '@mui/material/Link'
import PetsIcon from '@mui/icons-material/Pets'
import GitHubIcon from '@mui/icons-material/GitHub'
import TelegramIcon from '@mui/icons-material/Telegram'
import EmailIcon from '@mui/icons-material/Email'

const CONTACTS = [
  { label: 'GitHub', icon: <GitHubIcon />, href: 'https://github.com/olegkatrichuk' },
  { label: 'Telegram', icon: <TelegramIcon />, href: 'https://t.me/Olegnewlife' },
  { label: 'Email', icon: <EmailIcon />, href: 'mailto:katrichukoleg@gmail.com' },
]

export default function Footer() {
  const { t } = useTranslation()
  const navigate = useLangNavigate()

  const NAV_LINKS = [
    { labelKey: 'nav.home', path: '/' },
    { labelKey: 'nav.pets', path: '/pets' },
    { labelKey: 'nav.listings', path: '/listings' },
    { labelKey: 'nav.volunteers', path: '/volunteers' },
    { labelKey: 'nav.about', path: '/about' },
    { labelKey: 'nav.faq', path: '/faq' },
    { labelKey: 'nav.profile', path: '/profile' },
  ]

  return (
    <Box component="footer" sx={{ bgcolor: '#1e1b4b', color: 'white', pt: 6, pb: 3, mt: 'auto' }}>
      <Box
        sx={{
          maxWidth: 1100,
          mx: 'auto',
          px: { xs: 3, md: 6 },
          display: 'grid',
          gridTemplateColumns: { xs: '1fr', sm: '2fr 1fr 1fr' },
          gap: 4,
          mb: 4,
        }}
      >
        {/* Column 1: Brand */}
        <Box>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1.5 }}>
            <PetsIcon sx={{ color: '#FF6B6B', fontSize: 28 }} />
            <Typography variant="h6" fontWeight="bold" letterSpacing={1}>
              PetZone
            </Typography>
          </Box>
          <Typography variant="body2" sx={{ opacity: 0.7, maxWidth: 300, lineHeight: 1.7 }}>
            {t('footer.description')}
          </Typography>
          <Box sx={{ display: 'flex', gap: 0.5, mt: 2 }}>
            {CONTACTS.map(({ label, icon, href }) => (
              <Tooltip key={label} title={label}>
                <IconButton
                  color="inherit"
                  href={href}
                  target={href.startsWith('mailto') ? undefined : '_blank'}
                  rel="noopener noreferrer"
                  size="small"
                  sx={{ opacity: 0.7, '&:hover': { opacity: 1, color: '#FF6B6B' } }}
                >
                  {icon}
                </IconButton>
              </Tooltip>
            ))}
          </Box>
        </Box>

        {/* Column 2: Navigation */}
        <Box>
          <Typography variant="subtitle2" fontWeight="bold" sx={{ mb: 2, textTransform: 'uppercase', letterSpacing: 1, opacity: 0.5, fontSize: 11 }}>
            {t('footer.nav')}
          </Typography>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
            {NAV_LINKS.map((link) => (
              <Link
                key={link.path}
                component="button"
                onClick={() => navigate(link.path)}
                underline="none"
                sx={{ color: 'rgba(255,255,255,0.7)', textAlign: 'left', fontSize: '0.9rem', '&:hover': { color: '#FF6B6B' } }}
              >
                {t(link.labelKey)}
              </Link>
            ))}
          </Box>
        </Box>

        {/* Column 3: For volunteers */}
        <Box>
          <Typography variant="subtitle2" fontWeight="bold" sx={{ mb: 2, textTransform: 'uppercase', letterSpacing: 1, opacity: 0.5, fontSize: 11 }}>
            {t('footer.forVolunteers')}
          </Typography>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
            <Link component="button" onClick={() => navigate('/register/volunteer')} underline="none"
              sx={{ color: 'rgba(255,255,255,0.7)', textAlign: 'left', fontSize: '0.9rem', '&:hover': { color: '#FF6B6B' } }}>
              {t('footer.becomeVolunteer')}
            </Link>
            <Link component="button" onClick={() => navigate('/volunteer-applications')} underline="none"
              sx={{ color: 'rgba(255,255,255,0.7)', textAlign: 'left', fontSize: '0.9rem', '&:hover': { color: '#FF6B6B' } }}>
              {t('applications.title')}
            </Link>
          </Box>
        </Box>
      </Box>

      <Divider sx={{ bgcolor: 'rgba(255,255,255,0.1)', mx: { xs: 3, md: 6 } }} />

      <Box sx={{ px: { xs: 3, md: 6 }, pt: 2 }}>
        <Typography variant="caption" sx={{ opacity: 0.4 }}>
          © {new Date().getFullYear()} PetZone — {t('footer.rights')}
        </Typography>
      </Box>
    </Box>
  )
}
