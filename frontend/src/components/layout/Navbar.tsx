import { useLocation, useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import Paper from '@mui/material/Paper'
import Tabs from '@mui/material/Tabs'
import Tab from '@mui/material/Tab'

const NAV_ITEMS = [
  { labelKey: 'nav.home', path: '/' },
  { labelKey: 'nav.volunteers', path: '/volunteers' },
  { labelKey: 'nav.pets', path: '/pets' },
  { labelKey: 'nav.map', path: '/map' },
  { labelKey: 'nav.digest', path: '/digest' },
]

export default function Navbar() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const location = useLocation()

  const currentIndex = NAV_ITEMS.findIndex((item) =>
    item.path === '/'
      ? location.pathname === '/'
      : location.pathname.startsWith(item.path)
  )

  return (
    <Paper elevation={2} square>
      <Tabs
        value={currentIndex === -1 ? false : currentIndex}
        onChange={(_, index: number) => navigate(NAV_ITEMS[index].path)}
        indicatorColor="primary"
        textColor="primary"
        variant="scrollable"
        scrollButtons="auto"
      >
        {NAV_ITEMS.map((item) => (
          <Tab key={item.path} label={t(item.labelKey)} />
        ))}
      </Tabs>
    </Paper>
  )
}
