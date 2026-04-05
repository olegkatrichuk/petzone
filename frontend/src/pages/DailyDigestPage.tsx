import { useTranslation } from 'react-i18next'
import PageMeta from '../components/meta/PageMeta'
import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Typography from '@mui/material/Typography'
import Paper from '@mui/material/Paper'
import Chip from '@mui/material/Chip'
import Skeleton from '@mui/material/Skeleton'
import Alert from '@mui/material/Alert'
import Button from '@mui/material/Button'
import Divider from '@mui/material/Divider'
import AutoStoriesIcon from '@mui/icons-material/AutoStories'
import CalendarTodayIcon from '@mui/icons-material/CalendarToday'
import ArrowForwardIcon from '@mui/icons-material/ArrowForward'
import { useGetSystemNewsQuery } from '../services/newsApi'
import { useLangNavigate } from '../hooks/useLangNavigate'
import { useState } from 'react'
import type { SystemNewsPost } from '../types/systemNews.types'

const CORAL = '#FF6B6B'

function formatDate(iso: string, lang: string): string {
  return new Date(iso).toLocaleDateString(lang === 'uk' ? 'uk-UA' : lang, {
    day: 'numeric', month: 'long', year: 'numeric',
  })
}

function NewsCard({ post, lang }: { post: SystemNewsPost; lang: string }) {
  const [expanded, setExpanded] = useState(false)
  const lines = post.content.split('\n')
  const preview = lines.slice(0, 6).join('\n')
  const hasMore = lines.length > 6

  return (
    <Paper
      elevation={0}
      sx={{
        border: '1px solid #E5E7EB',
        borderRadius: 3,
        p: 3,
        transition: 'box-shadow 0.2s',
        '&:hover': { boxShadow: '0 4px 16px rgba(0,0,0,0.08)' },
      }}
    >
      <Box sx={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', gap: 2, mb: 1.5 }}>
        <Typography variant="h6" fontWeight={700} sx={{ lineHeight: 1.3 }}>
          {post.title}
        </Typography>
        <Chip
          icon={<CalendarTodayIcon sx={{ fontSize: '13px !important' }} />}
          label={formatDate(post.publishedAt, lang)}
          size="small"
          sx={{ bgcolor: '#F3F4F6', flexShrink: 0, fontSize: 11 }}
        />
      </Box>

      <Typography
        variant="body2"
        color="text.secondary"
        sx={{ whiteSpace: 'pre-line', lineHeight: 1.8 }}
      >
        {expanded ? post.content : preview}
      </Typography>

      {hasMore && (
        <Button
          size="small"
          onClick={() => setExpanded(!expanded)}
          sx={{ mt: 1, textTransform: 'none', color: CORAL, fontWeight: 600, p: 0 }}
        >
          {expanded ? '↑ Згорнути' : '↓ Читати далі'}
        </Button>
      )}
    </Paper>
  )
}

const PAGE_SIZE = 10

export default function DailyDigestPage() {
  const { t, i18n } = useTranslation()
  const navigate = useLangNavigate()
  const [page, setPage] = useState(1)
  const { data, isLoading, isError } = useGetSystemNewsQuery({ page, pageSize: PAGE_SIZE })

  const posts = data?.items ?? []
  const hasMore = posts.length + (page - 1) * PAGE_SIZE < (data?.totalCount ?? 0)

  return (
    <Box sx={{ bgcolor: '#FAFAFA', minHeight: '100%', py: 4 }}>
      <PageMeta
        title={t('digest.pageTitle')}
        description={t('digest.pageSubtitle')}
        path="/digest"
      />
      <Container maxWidth="md">
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 1 }}>
          <AutoStoriesIcon sx={{ fontSize: 32, color: CORAL }} />
          <Typography variant="h4" fontWeight="bold" sx={{ color: '#1F2937' }}>
            {t('digest.pageTitle')}
          </Typography>
        </Box>
        <Typography variant="body1" color="text.secondary" sx={{ mb: 4 }}>
          {t('digest.pageSubtitle')}
        </Typography>

        <Divider sx={{ mb: 4 }} />

        {isError && (
          <Alert severity="error" sx={{ mb: 3 }}>{t('errors.unknown')}</Alert>
        )}

        {isLoading ? (
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
            {Array.from({ length: 4 }).map((_, i) => (
              <Skeleton key={i} variant="rounded" height={160} sx={{ borderRadius: 3 }} />
            ))}
          </Box>
        ) : posts.length === 0 ? (
          <Box sx={{ textAlign: 'center', py: 10 }}>
            <Typography variant="h6" color="text.secondary">{t('digest.empty')}</Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
              {t('digest.emptyHint')}
            </Typography>
          </Box>
        ) : (
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
            {posts.map((post) => (
              <NewsCard key={post.id} post={post} lang={i18n.language} />
            ))}
          </Box>
        )}

        {hasMore && !isLoading && (
          <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}>
            <Button
              variant="outlined"
              endIcon={<ArrowForwardIcon />}
              onClick={() => setPage((p) => p + 1)}
              sx={{
                borderColor: '#1e1b4b', color: '#1e1b4b',
                textTransform: 'none', fontWeight: 600, borderRadius: 2, px: 4,
                '&:hover': { bgcolor: '#f0f0ff' },
              }}
            >
              {t('pets.loadMore')}
            </Button>
          </Box>
        )}

        <Box sx={{ mt: 6, textAlign: 'center' }}>
          <Button
            variant="text"
            onClick={() => navigate('/pets')}
            sx={{ color: CORAL, textTransform: 'none', fontWeight: 600 }}
          >
            {t('home.hero.browsePets')} →
          </Button>
        </Box>
      </Container>
    </Box>
  )
}