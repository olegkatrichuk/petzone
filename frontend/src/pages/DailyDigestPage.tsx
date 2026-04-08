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
import PetsIcon from '@mui/icons-material/Pets'
import LocationOnIcon from '@mui/icons-material/LocationOn'
import EmojiNatureIcon from '@mui/icons-material/EmojiNature'
import { useGetTodayDigestQuery } from '../services/newsApi'
import { useLangNavigate } from '../hooks/useLangNavigate'
import { useState, useEffect } from 'react'
import type { SystemNewsPost, TopBreed } from '../types/systemNews.types'

const CORAL = '#FF6B6B'
const PURPLE = '#7C3AED'
const GREEN = '#059669'
const BLUE = '#2563EB'

async function translateText(text: string, targetLang: string): Promise<string> {
  if (!text || targetLang === 'en') return text
  try {
    const url = `https://api.mymemory.translated.net/get?q=${encodeURIComponent(text)}&langpair=en|${targetLang}`
    const res = await fetch(url)
    const data = await res.json()
    const translated: string = data?.responseData?.translatedText
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
      <Box sx={{ color: color ?? '#6B7280', display: 'flex' }}>{icon}</Box>
      <Typography variant="body2" color="text.secondary" sx={{ flex: 1 }}>{label}</Typography>
      <Typography variant="body2" fontWeight={700} sx={{ color: color ?? '#1F2937', fontSize: '1rem' }}>
        {value}
      </Typography>
    </Box>
  )
}

function DigestCard({ post, lang }: { post: SystemNewsPost; lang: string }) {
  const { t } = useTranslation()
  const navigate = useLangNavigate()
  const [translatedFact, setTranslatedFact] = useState<string | null>(null)
  const [translating, setTranslating] = useState(false)

  const topBreeds: TopBreed[] = (() => {
    try { return JSON.parse(post.topBreedsJson) } catch { return [] }
  })()

  useEffect(() => {
    if (lang === 'en') { setTranslatedFact(post.factEn); return }
    setTranslating(true)
    translateText(post.factEn, lang).then((r) => { setTranslatedFact(r); setTranslating(false) })
  }, [post.factEn, lang])

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>

      {/* Header chip */}
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
        <Typography variant="h5" fontWeight={800} sx={{ color: '#1F2937' }}>
          📊 {t('digest.cardTitle')}
        </Typography>
        <Chip
          icon={<CalendarTodayIcon sx={{ fontSize: '13px !important' }} />}
          label={formatDate(post.publishedAt, lang)}
          size="small"
          sx={{ bgcolor: 'action.hover', fontSize: 11 }}
        />
      </Box>

      {/* Stats */}
      <Paper elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 3, p: 3 }}>
        <Typography variant="subtitle2" fontWeight={700} color="text.secondary" sx={{ mb: 1.5, textTransform: 'uppercase', fontSize: 11, letterSpacing: 0.5 }}>
          {t('digest.statsTitle', 'Статистика платформи')}
        </Typography>
        <StatRow icon={<SearchIcon sx={{ fontSize: 18 }} />} label={t('digest.lookingForHome')} value={post.lookingForHome} color={BLUE} />
        <StatRow icon={<LocalHospitalIcon sx={{ fontSize: 18 }} />} label={t('digest.needsHelp')} value={post.needsHelp} color="#D97706" />
        <StatRow icon={<HomeIcon sx={{ fontSize: 18 }} />} label={t('digest.foundHomeThisWeek')} value={post.foundHomeThisWeek} color={GREEN} />
        <StatRow icon={<GroupIcon sx={{ fontSize: 18 }} />} label={t('digest.totalVolunteers')} value={post.totalVolunteers} color={PURPLE} />
      </Paper>

      {/* Top breeds + Top city */}
      <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr' }, gap: 2 }}>

        {/* Top breeds */}
        {topBreeds.length > 0 && (
          <Paper elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 3, p: 3 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
              <EmojiNatureIcon sx={{ fontSize: 20, color: CORAL }} />
              <Typography variant="subtitle2" fontWeight={700}>
                {t('digest.topBreeds', 'Топ породи')}
              </Typography>
            </Box>
            {topBreeds.map((b, i) => (
              <Box key={b.name} sx={{ display: 'flex', alignItems: 'center', gap: 1.5, py: 0.5 }}>
                <Typography variant="body2" fontWeight={700} sx={{ color: CORAL, width: 20 }}>
                  {i + 1}.
                </Typography>
                <Typography variant="body2" sx={{ flex: 1 }}>{b.name}</Typography>
                <Chip label={b.count} size="small" sx={{ fontSize: 11, height: 20, bgcolor: '#FEF3F2', color: CORAL }} />
              </Box>
            ))}
          </Paper>
        )}

        {/* Top city */}
        {post.topCity && (
          <Paper elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 3, p: 3, display: 'flex', flexDirection: 'column', justifyContent: 'center' }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1.5 }}>
              <LocationOnIcon sx={{ fontSize: 20, color: GREEN }} />
              <Typography variant="subtitle2" fontWeight={700}>
                {t('digest.topCity', 'Найактивніше місто')}
              </Typography>
            </Box>
            <Typography variant="h5" fontWeight={800} sx={{ color: '#1F2937' }}>
              {post.topCity}
            </Typography>
            <Typography variant="caption" color="text.secondary">
              {t('digest.topCityHint', 'найбільше тварин шукають дім')}
            </Typography>
          </Paper>
        )}
      </Box>

      {/* Featured pet */}
      {post.featuredPetNickname && (
        <Paper elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 3, overflow: 'hidden' }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, px: 3, pt: 3, pb: 1.5 }}>
            <PetsIcon sx={{ fontSize: 20, color: BLUE }} />
            <Typography variant="subtitle2" fontWeight={700}>
              {t('digest.featuredPet', 'Тварина дня')}
            </Typography>
          </Box>
          <Box sx={{ display: 'flex', flexDirection: { xs: 'column', sm: 'row' }, gap: 0 }}>
            {post.featuredPetPhotoUrl && (
              <Box
                component="img"
                src={post.featuredPetPhotoUrl}
                alt={post.featuredPetNickname}
                sx={{
                  width: { xs: '100%', sm: 200 },
                  height: { xs: 200, sm: 'auto' },
                  objectFit: 'cover',
                  flexShrink: 0,
                }}
              />
            )}
            <Box sx={{ p: 3 }}>
              <Typography variant="h6" fontWeight={800}>{post.featuredPetNickname}</Typography>
              {post.featuredPetBreed && (
                <Typography variant="body2" color="text.secondary" sx={{ mb: 0.5 }}>
                  {post.featuredPetBreed}
                  {post.featuredPetCity && ` · ${post.featuredPetCity}`}
                </Typography>
              )}
              {post.featuredPetDescription && (
                <Typography variant="body2" sx={{ mt: 1, lineHeight: 1.6 }}>
                  {post.featuredPetDescription}
                </Typography>
              )}
              <Button
                size="small"
                onClick={() => navigate('/pets')}
                sx={{ mt: 2, color: BLUE, textTransform: 'none', fontWeight: 600, px: 0 }}
              >
                {t('digest.findMorePets', 'Знайти більше тварин')} →
              </Button>
            </Box>
          </Box>
        </Paper>
      )}

      {/* Animal fact */}
      <Paper elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 3, p: 3 }}>
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

    </Box>
  )
}

export default function DailyDigestPage() {
  const { t, i18n } = useTranslation()
  const { data: post, isLoading, isError, error } = useGetTodayDigestQuery()
  const isNotFound = isError && (error as { status?: number })?.status === 404
  const isRealError = isError && !isNotFound

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

        {isRealError && <Alert severity="error">{t('errors.unknown')}</Alert>}

        {isLoading ? (
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
            <Skeleton variant="rounded" height={180} sx={{ borderRadius: 3 }} />
            <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 2 }}>
              <Skeleton variant="rounded" height={140} sx={{ borderRadius: 3 }} />
              <Skeleton variant="rounded" height={140} sx={{ borderRadius: 3 }} />
            </Box>
            <Skeleton variant="rounded" height={200} sx={{ borderRadius: 3 }} />
            <Skeleton variant="rounded" height={100} sx={{ borderRadius: 3 }} />
          </Box>
        ) : post ? (
          <DigestCard post={post} lang={i18n.language} />
        ) : !isRealError ? (
          <Box sx={{ textAlign: 'center', py: 10 }}>
            <Typography variant="h6" color="text.secondary">{t('digest.empty')}</Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
              {t('digest.emptyHint')}
            </Typography>
          </Box>
        ) : null}
      </Container>
    </Box>
  )
}
