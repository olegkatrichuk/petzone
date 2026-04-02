import MuiBreadcrumbs from '@mui/material/Breadcrumbs'
import Typography from '@mui/material/Typography'
import Link from '@mui/material/Link'
import NavigateNextIcon from '@mui/icons-material/NavigateNext'
import { useParams } from 'react-router-dom'
import { useLangNavigate } from '../../hooks/useLangNavigate'
import { DEFAULT_LANG } from '../../lib/langUtils'

const SITE_URL = 'https://getpetzone.com'

export interface BreadcrumbItem {
  label: string
  path?: string // if omitted — current (last) item
}

interface Props {
  items: BreadcrumbItem[]
}

export default function AppBreadcrumbs({ items }: Props) {
  const navigate = useLangNavigate()
  const { lang } = useParams<{ lang: string }>()
  const currentLang = lang ?? DEFAULT_LANG

  const jsonLd = {
    '@context': 'https://schema.org',
    '@type': 'BreadcrumbList',
    itemListElement: items.map((item, i) => ({
      '@type': 'ListItem',
      position: i + 1,
      name: item.label,
      ...(item.path ? { item: `${SITE_URL}/${currentLang}${item.path}` } : {}),
    })),
  }

  return (
    <>
      <script type="application/ld+json" dangerouslySetInnerHTML={{ __html: JSON.stringify(jsonLd) }} />
      <MuiBreadcrumbs
        separator={<NavigateNextIcon fontSize="small" sx={{ color: '#9CA3AF' }} />}
        sx={{ mb: 2.5 }}
      >
        {items.map((item, i) => {
          const isLast = i === items.length - 1

          if (isLast || !item.path) {
            return (
              <Typography
                key={i}
                variant="body2"
                color="text.primary"
                sx={{ fontWeight: 500 }}
              >
                {item.label}
              </Typography>
            )
          }

          return (
            <Link
              key={i}
              component="button"
              variant="body2"
              underline="hover"
              onClick={() => navigate(item.path!)}
              sx={{ color: '#6B7280', '&:hover': { color: '#FF6B6B' } }}
            >
              {item.label}
            </Link>
          )
        })}
      </MuiBreadcrumbs>
    </>
  )
}
