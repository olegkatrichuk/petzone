import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Helmet } from 'react-helmet-async'
import PageMeta from '../components/meta/PageMeta'
import { safeJsonLd } from '../lib/safeJsonLd'
import { LangLink as Link } from '../components/ui/LangLink'
import AppBreadcrumbs from '../components/ui/AppBreadcrumbs'
import { useGetBlogPostsQuery } from '../services/blogApi'
import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Typography from '@mui/material/Typography'
import Card from '@mui/material/Card'
import CardActionArea from '@mui/material/CardActionArea'
import CardContent from '@mui/material/CardContent'
import CardMedia from '@mui/material/CardMedia'
import Skeleton from '@mui/material/Skeleton'
import Alert from '@mui/material/Alert'
import Button from '@mui/material/Button'

const PAGE_SIZE = 12
const CORAL = '#FF6B6B'
const SITE_URL = 'https://getpetzone.com'

export default function BlogListPage() {
  const { t, i18n } = useTranslation()
  const lang = i18n.language?.slice(0, 2) || 'uk'
  const [page, setPage] = useState(1)

  const { data, isLoading, isError } = useGetBlogPostsQuery({ lang, page, pageSize: PAGE_SIZE })

  const formatDate = (s: string) =>
    new Date(s).toLocaleDateString(lang, { year: 'numeric', month: 'long', day: 'numeric' })

  const hasMore = data ? page * PAGE_SIZE < data.total : false

  return (
    <Box sx={{ bgcolor: 'background.default', minHeight: '100%', py: 4 }}>
      <PageMeta
        title={t('blog.pageTitle', { defaultValue: 'Блог' })}
        description={t('blog.metaDesc', { defaultValue: 'Поради з усиновлення, догляду та виховання тварин — від команди PetZone.' })}
        path="/blog"
      />
      {data && data.items.length > 0 && (
        <script
          type="application/ld+json"
          dangerouslySetInnerHTML={{
            __html: safeJsonLd({
              '@context': 'https://schema.org',
              '@type': 'Blog',
              name: 'PetZone Blog',
              url: `${SITE_URL}/${lang}/blog`,
              blogPost: data.items.slice(0, 10).map(p => ({
                '@type': 'BlogPosting',
                headline: p.title,
                description: p.summary,
                url: `${SITE_URL}/${lang}/blog/${p.slug}`,
                datePublished: p.createdAt,
                ...(p.coverImageUrl && { image: p.coverImageUrl }),
              })),
            }),
          }}
        />
      )}
      <Helmet>
        <script type="application/ld+json">{safeJsonLd({
          '@context': 'https://schema.org',
          '@type': 'BreadcrumbList',
          itemListElement: [
            { '@type': 'ListItem', position: 1, name: t('nav.home'), item: `${SITE_URL}/${lang}` },
            { '@type': 'ListItem', position: 2, name: t('blog.pageTitle', { defaultValue: 'Блог' }) },
          ],
        })}</script>
      </Helmet>

      <Container maxWidth="lg">
        <AppBreadcrumbs items={[
          { label: t('nav.home'), path: '/' },
          { label: t('blog.pageTitle', { defaultValue: 'Блог' }) },
        ]} />

        <Typography variant="h1" fontSize={{ xs: '1.7rem', sm: '2.2rem' }} fontWeight="bold" sx={{ color: '#1F2937', mb: 1 }}>
          {t('blog.pageTitle', { defaultValue: 'Блог PetZone' })}
        </Typography>
        <Typography variant="body1" color="text.secondary" sx={{ maxWidth: 720, mb: 4 }}>
          {t('blog.intro', { defaultValue: 'Поради з усиновлення, догляду та виховання тварин. Розповіді волонтерів і команди PetZone.' })}
        </Typography>

        {isError && <Alert severity="error">{t('errors.unknown')}</Alert>}

        {isLoading ? (
          <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr', md: '1fr 1fr 1fr' }, gap: 3 }}>
            {Array.from({ length: 6 }).map((_, i) => (
              <Skeleton key={i} variant="rectangular" height={300} sx={{ borderRadius: 3 }} />
            ))}
          </Box>
        ) : data && data.items.length === 0 ? (
          <Typography color="text.secondary">{t('blog.empty', { defaultValue: 'Поки немає публікацій.' })}</Typography>
        ) : (
          <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr', md: '1fr 1fr 1fr' }, gap: 3 }}>
            {data?.items.map(post => (
              <Card key={post.id} elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 3, overflow: 'hidden' }}>
                <CardActionArea component={Link} to={`/blog/${post.slug}`}>
                  {post.coverImageUrl && (
                    <CardMedia
                      component="img"
                      image={post.coverImageUrl}
                      alt={post.title}
                      loading="lazy"
                      sx={{ aspectRatio: '16/9', objectFit: 'cover' }}
                    />
                  )}
                  <CardContent>
                    <Typography variant="caption" color="text.secondary">
                      {formatDate(post.createdAt)}
                    </Typography>
                    <Typography variant="h6" fontWeight={700} sx={{ mt: 0.5, mb: 1, lineHeight: 1.3 }}>
                      {post.title}
                    </Typography>
                    <Typography variant="body2" color="text.secondary" sx={{ display: '-webkit-box', WebkitLineClamp: 3, WebkitBoxOrient: 'vertical', overflow: 'hidden' }}>
                      {post.summary}
                    </Typography>
                  </CardContent>
                </CardActionArea>
              </Card>
            ))}
          </Box>
        )}

        {hasMore && (
          <Box sx={{ textAlign: 'center', mt: 4 }}>
            <Button
              onClick={() => setPage(p => p + 1)}
              variant="contained"
              sx={{ bgcolor: CORAL, '&:hover': { bgcolor: '#e55555' }, textTransform: 'none', borderRadius: 2, px: 4 }}
            >
              {t('pets.loadMore')}
            </Button>
          </Box>
        )}
      </Container>
    </Box>
  )
}
