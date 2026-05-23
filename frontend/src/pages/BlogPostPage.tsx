import { useParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { Helmet } from 'react-helmet-async'
import ReactMarkdown from 'react-markdown'
import remarkGfm from 'remark-gfm'
import PageMeta from '../components/meta/PageMeta'
import { safeJsonLd } from '../lib/safeJsonLd'
import { useLangNavigate } from '../hooks/useLangNavigate'
import AppBreadcrumbs from '../components/ui/AppBreadcrumbs'
import { useGetBlogPostBySlugQuery } from '../services/blogApi'
import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Paper from '@mui/material/Paper'
import Typography from '@mui/material/Typography'
import Button from '@mui/material/Button'
import Skeleton from '@mui/material/Skeleton'
import Alert from '@mui/material/Alert'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'

const SITE_URL = 'https://getpetzone.com'

export default function BlogPostPage() {
  const { slug } = useParams<{ slug: string }>()
  const { t, i18n } = useTranslation()
  const navigate = useLangNavigate()
  const lang = i18n.language?.slice(0, 2) || 'uk'

  const { data: post, isLoading, isError } = useGetBlogPostBySlugQuery(slug ?? '', { skip: !slug })

  const formatDate = (s: string) =>
    new Date(s).toLocaleDateString(lang, { year: 'numeric', month: 'long', day: 'numeric' })

  if (isLoading) {
    return (
      <Box sx={{ bgcolor: 'background.default', minHeight: '100%', py: 4 }}>
        <Container maxWidth="md">
          <Skeleton variant="text" width={200} height={32} sx={{ mb: 2 }} />
          <Skeleton variant="rectangular" height={400} sx={{ borderRadius: 3 }} />
        </Container>
      </Box>
    )
  }

  if (isError || !post) {
    return (
      <Box sx={{ bgcolor: 'background.default', minHeight: '100%', py: 4 }}>
        <Container maxWidth="md">
          <Alert severity="error">{t('common.notFound')}</Alert>
        </Container>
      </Box>
    )
  }

  return (
    <Box sx={{ bgcolor: 'background.default', minHeight: '100%', py: 4 }}>
      <PageMeta
        title={post.title}
        description={post.summary}
        path={`/blog/${post.slug}`}
        image={post.coverImageUrl ?? undefined}
        type="article"
      />
      <script
        type="application/ld+json"
        dangerouslySetInnerHTML={{
          __html: safeJsonLd({
            '@context': 'https://schema.org',
            '@type': 'BlogPosting',
            headline: post.title,
            description: post.summary,
            datePublished: post.createdAt,
            ...(post.updatedAt && { dateModified: post.updatedAt }),
            ...(post.coverImageUrl && { image: post.coverImageUrl }),
            inLanguage: post.language,
            mainEntityOfPage: `${SITE_URL}/${post.language}/blog/${post.slug}`,
            publisher: {
              '@type': 'Organization',
              name: 'PetZone',
              logo: { '@type': 'ImageObject', url: `${SITE_URL}/pwa-512.svg` },
            },
          }),
        }}
      />
      <Helmet>
        <script type="application/ld+json">{safeJsonLd({
          '@context': 'https://schema.org',
          '@type': 'BreadcrumbList',
          itemListElement: [
            { '@type': 'ListItem', position: 1, name: t('nav.home'), item: `${SITE_URL}/${lang}` },
            { '@type': 'ListItem', position: 2, name: t('blog.pageTitle', { defaultValue: 'Блог' }), item: `${SITE_URL}/${lang}/blog` },
            { '@type': 'ListItem', position: 3, name: post.title },
          ],
        })}</script>
      </Helmet>

      <Container maxWidth="md">
        <AppBreadcrumbs items={[
          { label: t('nav.home'), path: '/' },
          { label: t('blog.pageTitle', { defaultValue: 'Блог' }), path: '/blog' },
          { label: post.title },
        ]} />

        <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/blog')}
          sx={{ mb: 3, color: '#6B7280', textTransform: 'none' }}>
          {t('common.back')}
        </Button>

        <Paper elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 4, overflow: 'hidden' }}>
          {post.coverImageUrl && (
            <Box
              component="img"
              src={post.coverImageUrl}
              alt={post.title}
              loading="lazy"
              sx={{ width: '100%', aspectRatio: '16/7', objectFit: 'cover', display: 'block' }}
            />
          )}
          <Box sx={{ p: { xs: 3, md: 5 } }}>
            <Typography variant="caption" color="text.secondary">
              {formatDate(post.createdAt)}
            </Typography>
            <Typography variant="h1" fontSize={{ xs: '1.7rem', sm: '2.2rem' }} fontWeight="bold" sx={{ color: '#1F2937', mb: 2, mt: 0.5, lineHeight: 1.25 }}>
              {post.title}
            </Typography>
            <Typography variant="subtitle1" color="text.secondary" sx={{ mb: 3, fontStyle: 'italic' }}>
              {post.summary}
            </Typography>
            <Box
              sx={{
                color: '#374151',
                lineHeight: 1.75,
                fontSize: '1.05rem',
                '& h2': { fontSize: '1.5rem', fontWeight: 700, mt: 4, mb: 1.5, color: '#1F2937' },
                '& h3': { fontSize: '1.2rem', fontWeight: 700, mt: 3, mb: 1, color: '#1F2937' },
                '& p': { mb: 2 },
                '& a': { color: '#FF6B6B', textDecoration: 'underline' },
                '& ul, & ol': { pl: 3, mb: 2 },
                '& li': { mb: 0.5 },
                '& blockquote': { borderLeft: '4px solid #FF6B6B', pl: 2, color: '#6B7280', fontStyle: 'italic', my: 2 },
                '& code': { bgcolor: '#F3F4F6', px: 0.5, borderRadius: 0.5, fontSize: '0.9em' },
                '& pre': { bgcolor: '#F3F4F6', p: 2, borderRadius: 1, overflowX: 'auto', mb: 2 },
                '& img': { maxWidth: '100%', borderRadius: 1, my: 2 },
              }}
            >
              <ReactMarkdown remarkPlugins={[remarkGfm]}>{post.contentMarkdown}</ReactMarkdown>
            </Box>
          </Box>
        </Paper>
      </Container>
    </Box>
  )
}
