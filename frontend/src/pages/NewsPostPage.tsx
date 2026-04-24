import { useParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { Helmet } from 'react-helmet-async'
import PageMeta from '../components/meta/PageMeta'
import { useLangNavigate } from '../hooks/useLangNavigate'
import { useGetNewsPostByIdQuery } from '../services/newsApi'
import AppBreadcrumbs from '../components/ui/AppBreadcrumbs'
import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Paper from '@mui/material/Paper'
import Typography from '@mui/material/Typography'
import Button from '@mui/material/Button'
import Skeleton from '@mui/material/Skeleton'
import Alert from '@mui/material/Alert'
import Divider from '@mui/material/Divider'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'

export default function NewsPostPage() {
  const { t } = useTranslation()
  const { volunteerId, postId } = useParams<{ volunteerId: string; postId: string }>()
  const navigate = useLangNavigate()

  const { data: post, isLoading, isError } = useGetNewsPostByIdQuery(postId ?? '', { skip: !postId })

  const formatDate = (dateStr: string) =>
    new Date(dateStr).toLocaleDateString(undefined, { year: 'numeric', month: 'long', day: 'numeric' })

  if (isLoading) {
    return (
      <Box sx={{ bgcolor: 'background.default', minHeight: '100%', py: 4 }}>
        <Container maxWidth="md">
          <Skeleton variant="text" width={200} height={32} sx={{ mb: 2 }} />
          <Skeleton variant="rectangular" height={300} sx={{ borderRadius: 3 }} />
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

  const descriptionPreview = post.content.replace(/\s+/g, ' ').slice(0, 160)

  return (
    <Box sx={{ bgcolor: 'background.default', minHeight: '100%', py: 4 }}>
      <PageMeta
        title={post.title}
        description={descriptionPreview}
        path={`/news/${volunteerId}/${postId}`}
        type="article"
      />
      <Helmet>
        <script type="application/ld+json">{JSON.stringify({
          '@context': 'https://schema.org',
          '@type': 'BlogPosting',
          headline: post.title,
          description: descriptionPreview,
          datePublished: post.createdAt,
          dateModified: post.updatedAt ?? post.createdAt,
          url: `https://getpetzone.com/uk/news/${volunteerId}/${postId}`,
          author: {
            '@type': 'Person',
            url: `https://getpetzone.com/uk/volunteers/${volunteerId}`,
          },
        })}</script>
        <script type="application/ld+json">{JSON.stringify({
          '@context': 'https://schema.org',
          '@type': 'BreadcrumbList',
          itemListElement: [
            { '@type': 'ListItem', position: 1, name: t('nav.home'), item: 'https://getpetzone.com/uk' },
            { '@type': 'ListItem', position: 2, name: t('nav.volunteers'), item: 'https://getpetzone.com/uk/volunteers' },
            { '@type': 'ListItem', position: 3, name: t('news.title'), item: `https://getpetzone.com/uk/news/${volunteerId}` },
            { '@type': 'ListItem', position: 4, name: post.title },
          ],
        })}</script>
      </Helmet>

      <Container maxWidth="md">
        <AppBreadcrumbs items={[
          { label: t('nav.home'), path: '/' },
          { label: t('nav.volunteers'), path: '/volunteers' },
          { label: t('news.title'), path: `/news/${volunteerId}` },
          { label: post.title },
        ]} />

        <Button
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate(`/news/${volunteerId}`)}
          sx={{ color: '#6B7280', textTransform: 'none', mb: 3 }}
        >
          {t('news.backToList')}
        </Button>

        <Paper elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 3, p: { xs: 3, md: 5 } }}>
          <Typography variant="h4" component="h1" sx={{ fontWeight: 700, mb: 1, lineHeight: 1.3 }}>
            {post.title}
          </Typography>

          <Typography variant="body2" sx={{ color: '#9CA3AF', mb: 3 }}>
            {formatDate(post.createdAt)}
            {post.updatedAt && ` · ${t('news.edited')} ${formatDate(post.updatedAt)}`}
          </Typography>

          <Divider sx={{ mb: 3 }} />

          <Typography variant="body1" sx={{ whiteSpace: 'pre-wrap', lineHeight: 1.8, color: 'text.primary' }}>
            {post.content}
          </Typography>
        </Paper>
      </Container>
    </Box>
  )
}
