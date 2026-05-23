import { useTranslation } from 'react-i18next'
import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Typography from '@mui/material/Typography'

/**
 * SEO content block — paragraphs of indexable copy at the bottom of
 * landing pages. Loaded from i18n via `t(`${i18nKey}.title`)` and
 * `t(`${i18nKey}.paragraphs`, { returnObjects: true })`.
 *
 * Why: list/detail pages render dynamic API content that crawlers see
 * empty until JS executes. A static text block in the document body
 * gives Google something substantial to index and rank for long-tail
 * queries on the first crawl pass.
 */
interface SeoBlockProps {
  /** i18n key prefix, e.g. "seo.pets". Expects sub-keys: title, paragraphs[]. */
  i18nKey: string
}

export default function SeoBlock({ i18nKey }: SeoBlockProps) {
  const { t } = useTranslation()
  const title = t(`${i18nKey}.title`, { defaultValue: '' })
  const paragraphs = t(`${i18nKey}.paragraphs`, { returnObjects: true, defaultValue: [] }) as string[] | string

  const paragraphList = Array.isArray(paragraphs) ? paragraphs : []
  if (!title && paragraphList.length === 0) return null

  return (
    <Box sx={{ bgcolor: '#F9FAFB', borderTop: '1px solid #E5E7EB' }}>
      <Container maxWidth="lg" sx={{ py: 5 }}>
        {title && (
          <Typography
            variant="h2"
            component="h2"
            sx={{ fontSize: { xs: '1.4rem', sm: '1.6rem' }, fontWeight: 700, color: '#1F2937', mb: 2 }}
          >
            {title}
          </Typography>
        )}
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1.75 }}>
          {paragraphList.map((p, i) => (
            <Typography
              key={i}
              variant="body2"
              sx={{ color: '#4B5563', lineHeight: 1.85, fontSize: '0.97rem' }}
            >
              {p}
            </Typography>
          ))}
        </Box>
      </Container>
    </Box>
  )
}
