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
import CircularProgress from '@mui/material/CircularProgress'
import AutoStoriesIcon from '@mui/icons-material/AutoStories'
import CalendarTodayIcon from '@mui/icons-material/CalendarToday'
import SearchIcon from '@mui/icons-material/Search'
import LocalHospitalIcon from '@mui/icons-material/LocalHospital'
import HomeIcon from '@mui/icons-material/Home'
import GroupIcon from '@mui/icons-material/Group'
import LightbulbIcon from '@mui/icons-material/Lightbulb'
import { useGetSystemNewsQuery } from '../services/newsApi'
import { useLangNavigate } from '../hooks/useLangNavigate'
import { useState, useEffect } from 'react'
import type { SystemNewsPost } from '../types/systemNews.types'

const CORAL = '#FF6B6B'

async function translateText(text: string, targetLang: string): Promise<string> {
  if (!text || targetLang === 'en') return text
  try {
    const url = `https://api.mymemory.translated.net/get?q=${encodeURIComponent(text)}&langpair=en|${targetLang}`
    const res = await fetch(url)
    const data = await res.json()
    const translated: string = data?.responseData?.translatedText
    // MyMemory returns the original text if it can't translate
    if (translated && translated !== text && !translated.includes('MYMEMORY WARNING')) {
      return translated
    }
  } catch { /* fall through */ }
  return text
}

function formatDate(iso: string, lang: string): string {
  return new Date(iso).toLocaleDateString(lang === 'uk' ? 'uk-UA' : lang, {
    day: 'numeric', month: 'long', year: 'numeric',
  })
}

function StatRow({ icon, label, value, color }: {
  icon: React.ReactNode; label: string; value: number; color?: string
}) {
  return (
    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5, py: 0.75 }}>
      <Box sx={{ color: color ?? '#6B7280', display: 'flex', alignItems: 'center' }}>{icon}</Box>
      <Typography variant="body2" color="text.secondary" sx={{ flex: 1 }}>{label}</Typography>
      <Typography variant="body2" fontWeight={700} sx={{ color: color ?? '#1F2937', fontSize: '1rem' }}>
        {value}
      </Typography>
    </Box>
  )
}

function DigestCard({ post, lang }: { post: SystemNewsPost; lang: string }) {
  const { t } = useTranslation()
  const [translatedFact, setTranslatedFact] = useState<string | null>(null)
  const [translating, setTranslating] = useState(false)

  useEffect(() => {
    if (lang === 'en') {
      setTranslatedFact(post.factEn)
      return
    }
    setTranslating(true)
    translateText(post.factEn, lang).then((result) => {
      setTranslatedFact(result)
      setTranslating(false)
    })
  }, [post.factEn, lang])

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
      {/* Header */}
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 2, mb: 2 }}>
        <Typography variant="h6" fontWeight={700}>
          📊 {t('digest.cardTitle')}
        </Typography>
        <Chip
          icon={<CalendarTodayIcon sx={{ fontSize: '13px !important' }} />}
          label={formatDate(post.publishedAt, lang)}
          size="small"
          sx={{ bgcolor: 'action.hover', flexShrink: 0, fontSize: 11 }}
        />
      </Box>

      {/* Stats */}
      <Box sx={{ mb: 2 }}>
        <StatRow
          icon={<SearchIcon sx={{ fontSize: 18 }} />}
          label={t('digest.lookingForHome')}
          value={post.lookingForHome}
          color="#2563EB"
        />
        <StatRow
          icon={<LocalHospitalIcon sx={{ fontSize: 18 }} />}
          label={t('digest.needsHelp')}
          value={post.needsHelp}
          color="#D97706"
        />
        <StatRow
          icon={<HomeIcon sx={{ fontSize: 18 }} />}
          label={t('digest.foundHomeThisWeek')}
          value={post.foundHomeThisWeek}
          color="#059669"
        />
        <StatRow
          icon={<GroupIcon sx={{ fontSize: 18 }} />}
          label={t('digest.totalVolunteers')}
          value={post.totalVolunteers}
          color="#7C3AED"
        />
      </Box>

      <Divider sx={{ my: 2 }} />

      {/* Animal fact */}
      <Box sx={{ display: 'flex', gap: 1.5, alignItems: 'flex-start' }}>
        <LightbulbIcon sx={{ fontSize: 20, color: CORAL, flexShrink: 0, mt: 0.3 }} />
        <Box>
          <Typography variant="caption" fontWeight={700} color="text.secondary" sx={{ display: 'block', mb: 0.5 }}>
            {t('digest.factDay')}
          </Typography>
          {translating ? (
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
              <CircularProgress size={14} sx={{ color: CORAL }} />
              <Typography variant="body2" color="text.secondary">{t('digest.translating')}</Typography>
            </Box>
          ) : (
            <Typography variant="body2" color="text.secondary" sx={{ fontStyle: 'italic', lineHeight: 1.6 }}>
              {translatedFact ?? post.factEn}
            </Typography>
          )}
        </Box>
      </Box>
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
    <Box sx={{ bgcolor: 'background.default', minHeight: '100%', py: 4 }}>
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
              <Skeleton key={i} variant="rounded" height={220} sx={{ borderRadius: 3 }} />
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
              <DigestCard key={post.id} post={post} lang={i18n.language} />
            ))}
          </Box>
        )}

        {hasMore && !isLoading && (
          <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}>
            <Button
              variant="outlined"
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