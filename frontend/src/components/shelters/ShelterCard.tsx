import { useTranslation } from 'react-i18next'
import Box from '@mui/material/Box'
import Card from '@mui/material/Card'
import CardContent from '@mui/material/CardContent'
import CardActions from '@mui/material/CardActions'
import Avatar from '@mui/material/Avatar'
import Button from '@mui/material/Button'
import Typography from '@mui/material/Typography'
import Divider from '@mui/material/Divider'
import IconButton from '@mui/material/IconButton'
import Tooltip from '@mui/material/Tooltip'
import PhoneIcon from '@mui/icons-material/Phone'
import EmailIcon from '@mui/icons-material/Email'
import LanguageIcon from '@mui/icons-material/Language'
import OpenInNewIcon from '@mui/icons-material/OpenInNew'
import LocationOnIcon from '@mui/icons-material/LocationOn'
import FacebookIcon from '@mui/icons-material/Facebook'
import InstagramIcon from '@mui/icons-material/Instagram'
import { safeHref } from '../../lib/safeHref'

const CORAL = '#FF6B6B'

export interface ShelterItem {
  url: string
  name: string
  city: string
  photo: string | null
  phone: string | null
  email: string | null
  facebook: string | null
  instagram: string | null
  website: string | null
  description: string | null
  country?: string
  country_name?: string
}

const COUNTRY_FLAGS: Record<string, string> = {
  UA: '🇺🇦', DE: '🇩🇪', PL: '🇵🇱', GB: '🇬🇧', FR: '🇫🇷',
  ES: '🇪🇸', IT: '🇮🇹', CH: '🇨🇭', AT: '🇦🇹', BE: '🇧🇪',
  NL: '🇳🇱', CZ: '🇨🇿', SK: '🇸🇰', HU: '🇭🇺', RO: '🇷🇴',
  BG: '🇧🇬', HR: '🇭🇷', SI: '🇸🇮', RS: '🇷🇸', PT: '🇵🇹',
  SE: '🇸🇪', NO: '🇳🇴', DK: '🇩🇰', FI: '🇫🇮', LT: '🇱🇹',
  LV: '🇱🇻', EE: '🇪🇪', IE: '🇮🇪', GR: '🇬🇷', MD: '🇲🇩',
}

interface Props {
  shelter: ShelterItem
  showFlag?: boolean
}

export default function ShelterCard({ shelter, showFlag = true }: Props) {
  const { t } = useTranslation()
  const initials = shelter.name
    .split(/[\s,]+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((w) => w[0])
    .join('')
    .toUpperCase()

  const flag = showFlag ? (COUNTRY_FLAGS[shelter.country ?? ''] ?? '') : ''
  const isUkrainian = shelter.country === 'UA'
  const viewLabel = isUkrainian ? t('shelters.viewOnHappyPaw') : t('shelters.viewOnOsm')

  return (
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
          boxShadow: '0 8px 24px rgba(255,107,107,0.12)',
          transform: 'translateY(-2px)',
        },
      }}
    >
      <CardContent sx={{ flex: 1, pb: 1 }}>
        <Box sx={{ display: 'flex', gap: 2, alignItems: 'center', mb: 2 }}>
          <Avatar
            src={shelter.photo ?? undefined}
            imgProps={{ referrerPolicy: 'no-referrer' }}
            sx={{ width: 56, height: 56, bgcolor: '#FFF0F0', color: CORAL, fontSize: 18, fontWeight: 700, flexShrink: 0 }}
          >
            {!shelter.photo && initials}
          </Avatar>
          <Box sx={{ minWidth: 0, flex: 1 }}>
            <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 0.5, justifyContent: 'space-between' }}>
              <Typography
                variant="subtitle1"
                fontWeight="bold"
                sx={{ lineHeight: 1.3, wordBreak: 'break-word', fontSize: '0.9rem' }}
              >
                {shelter.name}
              </Typography>
              {flag && (
                <Typography component="span" sx={{ fontSize: 16, flexShrink: 0, mt: '1px' }}>
                  {flag}
                </Typography>
              )}
            </Box>
            {shelter.city && (
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.4, mt: 0.25, color: '#6B7280' }}>
                <LocationOnIcon sx={{ fontSize: 13 }} />
                <Typography variant="caption">{shelter.city}</Typography>
              </Box>
            )}
          </Box>
        </Box>

        {shelter.description && (
          <Typography
            variant="body2"
            color="text.secondary"
            sx={{
              mb: 1.5,
              overflow: 'hidden',
              display: '-webkit-box',
              WebkitLineClamp: 2,
              WebkitBoxOrient: 'vertical',
              lineHeight: 1.55,
              fontSize: '0.8rem',
            }}
          >
            {shelter.description}
          </Typography>
        )}

        <Divider sx={{ mb: 1.5 }} />

        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.6 }}>
          {shelter.phone && (
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.75 }}>
              <PhoneIcon sx={{ fontSize: 14, color: '#9CA3AF', flexShrink: 0 }} />
              <Typography
                variant="caption"
                component="a"
                href={`tel:${shelter.phone}`}
                sx={{ color: '#374151', textDecoration: 'none', '&:hover': { color: CORAL } }}
              >
                {shelter.phone}
              </Typography>
            </Box>
          )}
          {shelter.email && (
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.75 }}>
              <EmailIcon sx={{ fontSize: 14, color: '#9CA3AF', flexShrink: 0 }} />
              <Typography
                variant="caption"
                component="a"
                href={`mailto:${shelter.email}`}
                sx={{
                  color: CORAL,
                  textDecoration: 'none',
                  '&:hover': { textDecoration: 'underline' },
                  overflow: 'hidden',
                  textOverflow: 'ellipsis',
                  whiteSpace: 'nowrap',
                }}
              >
                {shelter.email}
              </Typography>
            </Box>
          )}
        </Box>

        {(shelter.facebook || shelter.instagram || shelter.website) && (
          <Box sx={{ display: 'flex', gap: 0.5, mt: 1.5 }}>
            {safeHref(shelter.facebook) && (
              <Tooltip title={t('shelters.facebook')}>
                <IconButton
                  size="small"
                  component="a"
                  href={safeHref(shelter.facebook)}
                  target="_blank"
                  rel="noopener noreferrer"
                  sx={{ color: '#1877F2', p: 0.5 }}
                >
                  <FacebookIcon sx={{ fontSize: 20 }} />
                </IconButton>
              </Tooltip>
            )}
            {safeHref(shelter.instagram) && (
              <Tooltip title={t('shelters.instagram')}>
                <IconButton
                  size="small"
                  component="a"
                  href={safeHref(shelter.instagram)}
                  target="_blank"
                  rel="noopener noreferrer"
                  sx={{ color: '#E1306C', p: 0.5 }}
                >
                  <InstagramIcon sx={{ fontSize: 20 }} />
                </IconButton>
              </Tooltip>
            )}
            {safeHref(shelter.website) && (
              <Tooltip title={t('shelters.website')}>
                <IconButton
                  size="small"
                  component="a"
                  href={safeHref(shelter.website)}
                  target="_blank"
                  rel="noopener noreferrer"
                  sx={{ color: '#6B7280', p: 0.5 }}
                >
                  <LanguageIcon sx={{ fontSize: 20 }} />
                </IconButton>
              </Tooltip>
            )}
          </Box>
        )}
      </CardContent>

      <CardActions sx={{ px: 2, pb: 2, pt: 0 }}>
        <Button
          variant="outlined"
          size="small"
          fullWidth
          endIcon={<OpenInNewIcon sx={{ fontSize: '14px !important' }} />}
          component="a"
          href={safeHref(shelter.url)}
          target="_blank"
          rel="noopener noreferrer"
          sx={{
            borderColor: '#E5E7EB',
            color: '#6B7280',
            borderRadius: 2,
            textTransform: 'none',
            fontWeight: 500,
            fontSize: 12,
            '&:hover': { borderColor: CORAL, color: CORAL, bgcolor: '#FFF0F0' },
          }}
        >
          {viewLabel}
        </Button>
      </CardActions>
    </Card>
  )
}
