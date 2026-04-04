import { useEffect, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useLangNavigate } from '../hooks/useLangNavigate'
import PageMeta from '../components/meta/PageMeta'
import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Typography from '@mui/material/Typography'
import Button from '@mui/material/Button'
import Paper from '@mui/material/Paper'
import Grid from '@mui/material/Grid'
import Skeleton from '@mui/material/Skeleton'
import PetsIcon from '@mui/icons-material/Pets'
import FavoriteIcon from '@mui/icons-material/Favorite'
import VolunteerActivismIcon from '@mui/icons-material/VolunteerActivism'
import SearchIcon from '@mui/icons-material/Search'
import ArrowForwardIcon from '@mui/icons-material/ArrowForward'
import { useGetPetsQuery } from '../services/petsApi'
import { useGetVolunteersQuery } from '../services/volunteersApi'
import PetCard from '../components/pets/PetCard'

const CORAL = '#FF6B6B'

// ── Hooks ──────────────────────────────────────────────────

function useCountUp(target: number, duration = 1600) {
  const [count, setCount] = useState(0)
  const ref = useRef<HTMLDivElement>(null)

  useEffect(() => {
    if (!target) return
    let triggered = false
    const observer = new IntersectionObserver(
      ([entry]) => {
        if (entry.isIntersecting && !triggered) {
          triggered = true
          let startTime: number | null = null
          const step = (now: number) => {
            if (!startTime) startTime = now
            const elapsed = now - startTime
            const progress = Math.min(elapsed / duration, 1)
            const eased = 1 - Math.pow(1 - progress, 3)
            setCount(Math.floor(eased * target))
            if (progress < 1) requestAnimationFrame(step)
          }
          requestAnimationFrame(step)
        }
      },
      { threshold: 0.3 },
    )
    if (ref.current) observer.observe(ref.current)
    return () => observer.disconnect()
  }, [target, duration])

  return { count, ref }
}

function useFadeIn(delay = 0) {
  const ref = useRef<HTMLDivElement>(null)
  const [visible, setVisible] = useState(false)

  useEffect(() => {
    const observer = new IntersectionObserver(
      ([entry]) => { if (entry.isIntersecting) setVisible(true) },
      { threshold: 0.1 },
    )
    if (ref.current) observer.observe(ref.current)
    return () => observer.disconnect()
  }, [])

  return {
    ref,
    sx: {
      opacity: visible ? 1 : 0,
      transform: visible ? 'translateY(0)' : 'translateY(28px)',
      transition: `opacity 0.65s ease ${delay}ms, transform 0.65s ease ${delay}ms`,
    },
  }
}

// ── Components ─────────────────────────────────────────────

function AnimatedStatCard({ target, label, icon }: { target: number; label: string; icon: React.ReactNode }) {
  const { count, ref } = useCountUp(target)
  return (
    <Paper
      ref={ref}
      elevation={0}
      sx={{ p: { xs: 2, sm: 3 }, textAlign: 'center', border: '1px solid #E5E7EB', borderRadius: 3, flex: 1, minWidth: { xs: 100, sm: 140 } }}
    >
      <Box sx={{ color: CORAL, mb: 1 }}>{icon}</Box>
      <Typography variant="h4" fontWeight="bold" sx={{ color: '#1F2937' }}>
        {target > 0 ? count : '—'}
      </Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>{label}</Typography>
    </Paper>
  )
}

function FreeCard({ label, icon }: { label: string; icon: React.ReactNode }) {
  return (
    <Paper elevation={0} sx={{ p: { xs: 2, sm: 3 }, textAlign: 'center', border: '1px solid #E5E7EB', borderRadius: 3, flex: 1, minWidth: { xs: 100, sm: 140 } }}>
      <Box sx={{ color: CORAL, mb: 1 }}>{icon}</Box>
      <Typography variant="h4" fontWeight="bold" sx={{ color: '#1F2937' }}>100%</Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>{label}</Typography>
    </Paper>
  )
}

function StepCard({ step, titleKey, descKey, delay }: { step: number; titleKey: string; descKey: string; delay?: number }) {
  const { t } = useTranslation()
  const fade = useFadeIn(delay)
  return (
    <Box ref={fade.ref} sx={{ flex: 1, minWidth: { xs: '80%', sm: 220 }, textAlign: 'center', px: { xs: 1, sm: 2 }, ...fade.sx }}>
      <Box sx={{
        width: 56, height: 56, borderRadius: '50%',
        bgcolor: '#FFF0F0', color: CORAL,
        display: 'flex', alignItems: 'center', justifyContent: 'center',
        mx: 'auto', mb: 2, fontSize: 22, fontWeight: 700,
        boxShadow: '0 4px 12px rgba(255,107,107,0.2)',
      }}>
        {step}
      </Box>
      <Typography variant="h6" fontWeight="bold" sx={{ mb: 1 }}>{t(titleKey)}</Typography>
      <Typography variant="body2" color="text.secondary" sx={{ lineHeight: 1.7 }}>{t(descKey)}</Typography>
    </Box>
  )
}

// Floating paw decoration for hero
function FloatingDot({ size, top, left, right, delay, opacity }: {
  size: number; top?: string; left?: string; right?: string; delay: number; opacity: number
}) {
  return (
    <Box sx={{
      position: 'absolute', top, left, right,
      width: size, height: size, borderRadius: '50%',
      bgcolor: `rgba(255,107,107,${opacity})`,
      animation: 'heroFloat 6s ease-in-out infinite',
      animationDelay: `${delay}s`,
      '@keyframes heroFloat': {
        '0%, 100%': { transform: 'translateY(0px) scale(1)' },
        '50%': { transform: 'translateY(-18px) scale(1.05)' },
      },
      pointerEvents: 'none',
    }} />
  )
}

// ── Main component ─────────────────────────────────────────

export default function HomePage() {
  const { t } = useTranslation()
  const navigate = useLangNavigate()

  const { data: petsData } = useGetPetsQuery({ page: 1, pageSize: 1 })
  const { data: volunteersData } = useGetVolunteersQuery({ page: 1, pageSize: 1 })
  const { data: featuredPetsData, isLoading: featuredLoading } = useGetPetsQuery({ page: 1, pageSize: 6 })

  const featuredFade = useFadeIn()

  // ── Structured data ─────────────────────────────────────

  const websiteJsonLd = {
    '@context': 'https://schema.org',
    '@type': 'WebSite',
    name: 'PetZone',
    url: 'https://getpetzone.com',
    potentialAction: {
      '@type': 'SearchAction',
      target: {
        '@type': 'EntryPoint',
        urlTemplate: 'https://getpetzone.com/uk/pets?nickname={search_term_string}',
      },
      'query-input': 'required name=search_term_string',
    },
  }

  const organizationJsonLd = {
    '@context': 'https://schema.org',
    '@type': 'Organization',
    name: 'PetZone',
    url: 'https://getpetzone.com',
    logo: 'https://getpetzone.com/pwa-192.svg',
    description: 'Безкоштовна платформа для усиновлення домашніх тварин — з\'єднуємо волонтерів і майбутніх господарів',
    areaServed: 'UA',
    knowsLanguage: ['uk', 'en', 'ru', 'de', 'pl', 'fr'],
  }

  const faqJsonLd = {
    '@context': 'https://schema.org',
    '@type': 'FAQPage',
    mainEntity: [
      {
        '@type': 'Question',
        name: 'Що таке PetZone?',
        acceptedAnswer: { '@type': 'Answer', text: 'PetZone — це безкоштовна платформа, яка з\'єднує волонтерів, що рятують тварин, з людьми, готовими дати їм дім.' },
      },
      {
        '@type': 'Question',
        name: 'Як усиновити тварину через PetZone?',
        acceptedAnswer: { '@type': 'Answer', text: 'Знайдіть тваринку, перейдіть на профіль волонтера, напишіть йому напряму і домовтесь про знайомство та умови передачі.' },
      },
      {
        '@type': 'Question',
        name: 'Чи є плата за усиновлення?',
        acceptedAnswer: { '@type': 'Answer', text: 'Платформа PetZone безкоштовна. Умови передачі визначає волонтер індивідуально.' },
      },
      {
        '@type': 'Question',
        name: 'Як стати волонтером на PetZone?',
        acceptedAnswer: { '@type': 'Answer', text: 'Натисніть «Стати волонтером», заповніть форму і надішліть заявку. Після схвалення отримаєте доступ до кабінету волонтера.' },
      },
      {
        '@type': 'Question',
        name: 'Чи потрібна реєстрація для перегляду тварин?',
        acceptedAnswer: { '@type': 'Answer', text: 'Ні. Переглядати анкети тварин можна без реєстрації.' },
      },
    ],
  }

  return (
    <Box>
      <PageMeta title={t('home.hero.title')} description={t('home.hero.subtitle')} path="/" />
      <script type="application/ld+json" dangerouslySetInnerHTML={{ __html: JSON.stringify(websiteJsonLd) }} />
      <script type="application/ld+json" dangerouslySetInnerHTML={{ __html: JSON.stringify(organizationJsonLd) }} />
      <script type="application/ld+json" dangerouslySetInnerHTML={{ __html: JSON.stringify(faqJsonLd) }} />

      {/* ── Hero ─────────────────────────────────────────── */}
      <Box
        component="section"
        sx={{
          background: 'linear-gradient(135deg, #1e1b4b 0%, #312e81 50%, #4338ca 100%)',
          color: 'white',
          py: { xs: 8, md: 13 },
          px: 3,
          position: 'relative',
          overflow: 'hidden',
        }}
      >
        {/* Static blobs */}
        <Box sx={{ position: 'absolute', top: -60, right: -60, width: 320, height: 320, borderRadius: '50%', bgcolor: 'rgba(255,107,107,0.08)' }} />
        <Box sx={{ position: 'absolute', bottom: -60, left: -60, width: 240, height: 240, borderRadius: '50%', bgcolor: 'rgba(99,102,241,0.15)' }} />

        {/* Floating dots */}
        <FloatingDot size={14} top="15%"  left="8%"   delay={0}   opacity={0.25} />
        <FloatingDot size={20} top="60%"  left="5%"   delay={1.5} opacity={0.15} />
        <FloatingDot size={10} top="25%"  right="10%" delay={0.8} opacity={0.2}  />
        <FloatingDot size={24} top="55%"  right="7%"  delay={2.2} opacity={0.12} />
        <FloatingDot size={16} top="80%"  left="20%"  delay={1}   opacity={0.18} />
        <FloatingDot size={12} top="10%"  right="25%" delay={3}   opacity={0.22} />

        <Container maxWidth="md" sx={{ position: 'relative', textAlign: 'center' }}>
          {/* Badge */}
          <Box sx={{
            display: 'inline-flex', alignItems: 'center', gap: 1,
            bgcolor: 'rgba(255,107,107,0.15)', border: '1px solid rgba(255,107,107,0.35)',
            borderRadius: 5, px: 2, py: 0.75, mb: 3,
            animation: 'badgePulse 3s ease-in-out infinite',
            '@keyframes badgePulse': {
              '0%, 100%': { boxShadow: '0 0 0 0 rgba(255,107,107,0)' },
              '50%': { boxShadow: '0 0 0 6px rgba(255,107,107,0.1)' },
            },
          }}>
            <PetsIcon sx={{ fontSize: 16, color: CORAL }} />
            <Typography variant="caption" sx={{ color: CORAL, fontWeight: 600, letterSpacing: 0.8, textTransform: 'uppercase', fontSize: 11 }}>
              {t('home.hero.badge')}
            </Typography>
          </Box>

          <Typography
            component="h1"
            variant="h2"
            fontWeight="bold"
            sx={{ mb: 2.5, lineHeight: 1.15, fontSize: { xs: '2rem', sm: '2.6rem', md: '3.2rem' } }}
          >
            {t('home.hero.title')}
          </Typography>

          <Typography variant="h6" sx={{ mb: 1.5, opacity: 0.75, fontWeight: 400, maxWidth: 580, mx: 'auto', lineHeight: 1.65, fontSize: { xs: '1rem', md: '1.1rem' } }}>
            {t('home.hero.subtitle')}
          </Typography>

          {/* SEO-visible keyword text */}
          <Typography variant="body2" sx={{ mb: 5, opacity: 0.55, maxWidth: 480, mx: 'auto', lineHeight: 1.6 }}>
            {t('home.hero.seoText')}
          </Typography>

          <Box sx={{ display: 'flex', gap: 2, justifyContent: 'center', flexWrap: 'wrap' }}>
            <Button
              variant="contained"
              size="large"
              startIcon={<SearchIcon />}
              onClick={() => navigate('/pets')}
              sx={{
                bgcolor: CORAL,
                '&:hover': { bgcolor: '#e55555', transform: 'translateY(-2px)', boxShadow: '0 6px 20px rgba(255,107,107,0.4)' },
                textTransform: 'none', fontWeight: 700, borderRadius: 3,
                px: 4, py: 1.5, fontSize: '1rem',
                transition: 'all 0.2s ease',
              }}
            >
              {t('home.hero.browsePets')}
            </Button>
            <Button
              variant="outlined"
              size="large"
              startIcon={<VolunteerActivismIcon />}
              onClick={() => navigate('/register/volunteer')}
              sx={{
                color: 'white', borderColor: 'rgba(255,255,255,0.4)',
                textTransform: 'none', fontWeight: 600, borderRadius: 3,
                px: 4, py: 1.5, fontSize: '1rem',
                '&:hover': { borderColor: CORAL, color: CORAL, bgcolor: 'transparent', transform: 'translateY(-2px)' },
                transition: 'all 0.2s ease',
              }}
            >
              {t('home.hero.becomeVolunteer')}
            </Button>
          </Box>
        </Container>
      </Box>

      {/* ── Stats ─────────────────────────────────────────── */}
      <Box component="section" sx={{ bgcolor: 'white', py: 5, borderBottom: '1px solid #E5E7EB' }}>
        <Container maxWidth="sm">
          <Box sx={{ display: 'flex', gap: 3, flexWrap: 'wrap', justifyContent: 'center' }}>
            <AnimatedStatCard
              target={petsData?.totalCount ?? 0}
              label={t('home.stats.pets')}
              icon={<PetsIcon sx={{ fontSize: 32 }} />}
            />
            <AnimatedStatCard
              target={volunteersData?.totalCount ?? 0}
              label={t('home.stats.volunteers')}
              icon={<VolunteerActivismIcon sx={{ fontSize: 32 }} />}
            />
            <FreeCard
              label={t('home.stats.free')}
              icon={<FavoriteIcon sx={{ fontSize: 32 }} />}
            />
          </Box>
        </Container>
      </Box>

      {/* ── How it works ──────────────────────────────────── */}
      <Box component="section" sx={{ bgcolor: '#FAFAFA', py: { xs: 7, md: 10 } }}>
        <Container maxWidth="md">
          <Typography variant="h2" component="h2" fontWeight="bold" textAlign="center" sx={{ mb: 1, color: '#1F2937', fontSize: { xs: '1.7rem', md: '2.2rem' } }}>
            {t('home.how.title')}
          </Typography>
          <Typography variant="body1" textAlign="center" color="text.secondary" sx={{ mb: 6, maxWidth: 480, mx: 'auto' }}>
            {t('home.how.subtitle')}
          </Typography>
          <Box sx={{ display: 'flex', gap: 4, flexWrap: 'wrap', justifyContent: 'center' }}>
            <StepCard step={1} titleKey="home.how.step1.title" descKey="home.how.step1.desc" delay={0} />
            <StepCard step={2} titleKey="home.how.step2.title" descKey="home.how.step2.desc" delay={100} />
            <StepCard step={3} titleKey="home.how.step3.title" descKey="home.how.step3.desc" delay={200} />
          </Box>
        </Container>
      </Box>

      {/* ── Featured Pets ─────────────────────────────────── */}
      <Box component="section" ref={featuredFade.ref} sx={{ bgcolor: 'white', py: { xs: 6, md: 9 }, ...featuredFade.sx }}>
        <Container maxWidth="lg">
          <Box sx={{ textAlign: 'center', mb: 6 }}>
            <Typography variant="h2" component="h2" fontWeight="bold" sx={{ mb: 1.5, color: '#1F2937', fontSize: { xs: '1.7rem', md: '2.2rem' } }}>
              {t('home.featured.title')}
            </Typography>
            <Typography variant="body1" color="text.secondary" sx={{ maxWidth: 520, mx: 'auto' }}>
              {t('home.featured.subtitle')}
            </Typography>
          </Box>

          {featuredLoading ? (
            <Grid container spacing={3}>
              {Array.from({ length: 6 }).map((_, i) => (
                <Grid key={i} size={{ xs: 12, sm: 6, md: 4 }}>
                  <Skeleton variant="rounded" height={420} sx={{ borderRadius: 3 }} />
                </Grid>
              ))}
            </Grid>
          ) : (
            <Grid container spacing={3}>
              {(featuredPetsData?.items ?? []).map((pet) => (
                <Grid key={pet.id} size={{ xs: 12, sm: 6, md: 4 }}>
                  <PetCard pet={pet} />
                </Grid>
              ))}
            </Grid>
          )}

          <Box sx={{ textAlign: 'center', mt: 5 }}>
            <Button
              variant="outlined"
              size="large"
              endIcon={<ArrowForwardIcon />}
              onClick={() => navigate('/pets')}
              sx={{
                borderColor: CORAL, color: CORAL,
                textTransform: 'none', fontWeight: 600, borderRadius: 3,
                px: 4, py: 1.4,
                '&:hover': { bgcolor: '#FFF0F0', borderColor: CORAL, transform: 'translateX(4px)' },
                transition: 'all 0.2s ease',
              }}
            >
              {t('home.featured.viewAll')}
            </Button>
          </Box>
        </Container>
      </Box>

      {/* ── Trust strip ───────────────────────────────────── */}
      <Box sx={{ bgcolor: '#F9FAFB', borderTop: '1px solid #E5E7EB', borderBottom: '1px solid #E5E7EB', py: 3 }}>
        <Container maxWidth="md">
          <Box sx={{ display: 'flex', gap: { xs: 2, md: 5 }, justifyContent: 'center', flexWrap: 'wrap', alignItems: 'center' }}>
            {[
              { icon: '🔒', text: t('home.trust.free') },
              { icon: '✅', text: t('home.trust.verified') },
              { icon: '🐾', text: t('home.trust.volunteers') },
              { icon: '📍', text: t('home.trust.ukraine') },
            ].map((item) => (
              <Box key={item.text} sx={{ display: 'flex', alignItems: 'center', gap: 0.75 }}>
                <Typography sx={{ fontSize: 18 }}>{item.icon}</Typography>
                <Typography variant="body2" color="text.secondary" fontWeight={500}>{item.text}</Typography>
              </Box>
            ))}
          </Box>
        </Container>
      </Box>

      {/* ── CTA Banner ────────────────────────────────────── */}
      <Box
        component="section"
        sx={{
          background: 'linear-gradient(135deg, #FF6B6B 0%, #ee5a24 100%)',
          py: 8, px: 3, position: 'relative', overflow: 'hidden',
        }}
      >
        {/* Decoration */}
        <Box sx={{ position: 'absolute', top: -30, right: -30, width: 180, height: 180, borderRadius: '50%', bgcolor: 'rgba(255,255,255,0.08)' }} />
        <Box sx={{ position: 'absolute', bottom: -20, left: -20, width: 120, height: 120, borderRadius: '50%', bgcolor: 'rgba(255,255,255,0.06)' }} />

        <Container maxWidth="sm" sx={{ textAlign: 'center', position: 'relative' }}>
          <VolunteerActivismIcon sx={{ fontSize: 52, color: 'white', mb: 2, opacity: 0.95 }} />
          <Typography variant="h3" component="h2" fontWeight="bold" sx={{ color: 'white', mb: 1.5, fontSize: { xs: '1.7rem', md: '2.2rem' } }}>
            {t('home.cta.title')}
          </Typography>
          <Typography variant="body1" sx={{ color: 'rgba(255,255,255,0.88)', mb: 4, lineHeight: 1.75 }}>
            {t('home.cta.subtitle')}
          </Typography>
          <Button
            variant="contained"
            size="large"
            onClick={() => navigate('/register/volunteer')}
            sx={{
              bgcolor: 'white', color: CORAL,
              '&:hover': { bgcolor: 'rgba(255,255,255,0.92)', transform: 'translateY(-2px)', boxShadow: '0 8px 24px rgba(0,0,0,0.15)' },
              textTransform: 'none', fontWeight: 700,
              borderRadius: 3, px: 5, py: 1.5, fontSize: '1rem',
              transition: 'all 0.2s ease',
            }}
          >
            {t('home.cta.btn')}
          </Button>
        </Container>
      </Box>
    </Box>
  )
}
