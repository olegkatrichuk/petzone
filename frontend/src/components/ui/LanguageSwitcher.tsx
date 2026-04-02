import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate, useParams, useLocation } from 'react-router-dom'
import Button from '@mui/material/Button'
import Menu from '@mui/material/Menu'
import MenuItem from '@mui/material/MenuItem'
import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown'
import { isValidLang, DEFAULT_LANG } from '../../lib/langUtils'

const LANGUAGES = [
  { code: 'uk', flag: '🇺🇦', label: 'UK' },
  { code: 'ru', flag: '🇷🇺', label: 'RU' },
  { code: 'en', flag: '🇬🇧', label: 'EN' },
  { code: 'pl', flag: '🇵🇱', label: 'PL' },
  { code: 'de', flag: '🇩🇪', label: 'DE' },
  { code: 'fr', flag: '🇫🇷', label: 'FR' },
]

export default function LanguageSwitcher() {
  const { i18n } = useTranslation()
  const navigate = useNavigate()
  const location = useLocation()
  const { lang } = useParams<{ lang: string }>()
  const [anchor, setAnchor] = useState<null | HTMLElement>(null)

  const currentLang = isValidLang(lang) ? lang : DEFAULT_LANG
  const current = LANGUAGES.find((l) => l.code === currentLang) ?? LANGUAGES[0]

  const handleSelect = (code: string) => {
    setAnchor(null)
    // Replace current lang segment in URL with new lang
    const newPath = location.pathname.replace(`/${currentLang}`, `/${code}`)
    i18n.changeLanguage(code)
    navigate(newPath + location.search, { replace: true })
  }

  return (
    <>
      <Button
        color="inherit"
        onClick={(e) => setAnchor(e.currentTarget)}
        endIcon={<KeyboardArrowDownIcon />}
        sx={{ textTransform: 'none', fontWeight: 600, fontSize: 14, gap: 0.5, minWidth: 80 }}
      >
        {current.flag} {current.label}
      </Button>

      <Menu
        anchorEl={anchor}
        open={Boolean(anchor)}
        onClose={() => setAnchor(null)}
        slotProps={{ paper: { sx: { mt: 1, minWidth: 130 } } }}
      >
        {LANGUAGES.map((l) => (
          <MenuItem
            key={l.code}
            selected={l.code === currentLang}
            onClick={() => handleSelect(l.code)}
            sx={{ gap: 1.5, fontWeight: l.code === currentLang ? 700 : 400 }}
          >
            <span style={{ fontSize: 20 }}>{l.flag}</span>
            {l.label}
          </MenuItem>
        ))}
      </Menu>
    </>
  )
}
