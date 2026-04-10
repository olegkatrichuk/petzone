import { useTranslation } from 'react-i18next'
import { useLangNavigate } from '../hooks/useLangNavigate'
import PageMeta from '../components/meta/PageMeta'
import { motion } from 'framer-motion'
import { useRef, useState } from 'react'
import { useCountUp } from 'react-countup'
import HeroCatAnimation from '../components/animations/HeroCatAnimation'
import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Typography from '@mui/material/Typography'
import Button from '@mui/material/Button'
import Paper from '@mui/material/Paper'
import Grid from '@mui/material/Grid'
import Skeleton from '@mui/material/Skeleton'
import Alert from '@mui/material/Alert'
import PetsIcon from '@mui/icons-material/Pets'
import FavoriteIcon from '@mui/icons-material/Favorite'
import VolunteerActivismIcon from '@mui/icons-material/VolunteerActivism'
import SearchIcon from '@mui/icons-material/Search'
import ArrowForwardIcon from '@mui/icons-material/ArrowForward'
import { useGetPetsQuery } from '../services/petsApi'
import { useGetVolunteersQuery } from '../services/volunteersApi'
import PetCard from '../components/pets/PetCard'

const CORAL = '#FF6B6B'

// ── Framer Motion variants ─────────────────────────────────

const EASE: [number, number, number, number] = [0.22, 1, 0.36, 1]

const fadeUp = {
  hidden: { opacity: 0, y: 32 },
  visible: { opacity: 1, y: 0, transition: { duration: 0.6, ease: EASE } },
}

const fadeIn = {
  hidden: { opacity: 0 },
  visible: { opacity: 1, transition: { duration: 0.7 } },
}

const staggerContainer = {
  hidden: {},
  visible: { transition: { staggerChildren: 0.1 } },
}

const cardVariant = {
  hidden: { opacity: 0, y: 40, scale: 0.97 },
  visible: { opacity: 1, y: 0, scale: 1, transition: { duration: 0.5, ease: EASE } },
}

const stepVariant = {
  hidden: { opacity: 0, y: 28 },
  visible: (i: number) => ({
    opacity: 1, y: 0,
    transition: { duration: 0.55, delay: i * 0.12, ease: EASE },
  }),
}

// ── Stat cards ─────────────────────────────────────────────

function AnimatedStatCard({ target, label, icon }: { target: number; label: string; icon: React.ReactNode }) {
  const countRef = useRef<HTMLElement>(null)
  useCountUp({ ref: countRef as React.RefObject<HTMLElement>, end: target, duration: 1.8, enableScrollSpy: true, scrollSpyOnce: true })
  return (
    <motion.div
      variants={fadeUp}
      initial="hidden"
      whileInView="visible"
      viewport={{ once: true, margin: '-40px' }}
      style={{ flex: 1, minWidth: 160, display: 'flex' }}
    >
      <Paper
        elevation={0}
        sx={{ p: { xs: 2, sm: 3 }, textAlign: 'center', border: '1px solid', borderColor: 'divider', borderRadius: 3, width: '100%', display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center' }}
      >
        <Box sx={{ color: CORAL, mb: 1 }}>{icon}</Box>
        <Typography variant="h4" fontWeight="bold" color="text.primary">
          <span ref={countRef}>0</span>
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>{label}</Typography>
      </Paper>
    </motion.div>
  )
}

function FreeCard({ label, icon }: { label: string; icon: React.ReactNode }) {
  return (
    <motion.div
      variants={fadeUp}
      initial="hidden"
      whileInView="visible"
      viewport={{ once: true, margin: '-40px' }}
      style={{ flex: 1, minWidth: 160, display: 'flex' }}
    >
      <Paper elevation={0} sx={{ p: { xs: 2, sm: 3 }, textAlign: 'center', border: '1px solid #E5E7EB', borderRadius: 3, width: '100%', display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center' }}>
        <Box sx={{ color: CORAL, mb: 1 }}>{icon}</Box>
        <Typography variant="h4" fontWeight="bold" sx={{ color: '#1F2937' }}>100%</Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>{label}</Typography>
      </Paper>
    </motion.div>
  )
}

// ── Floating dot (hero decoration) ────────────────────────

function FloatingDot({ size, top, left, right, delay, opacity }: {
  size: number; top?: string; left?: string; right?: string; delay: number; opacity: number
}) {
  return (
    <motion.div
      style={{
        position: 'absolute', top, left, right,
        width: size, height: size, borderRadius: '50%',
        background: `rgba(255,107,107,${opacity})`,
        pointerEvents: 'none',
      }}
      animate={{ y: [0, -16, 0], scale: [1, 1.06, 1] }}
      transition={{ duration: 5 + delay, repeat: Infinity, ease: 'easeInOut', delay }}
    />
  )
}

// ── Main ───────────────────────────────────────────────────

export default function HomePage() {
  const { t } = useTranslation()
  const navigate = useLangNavigate()

  // Random page so featured pets differ on every visit (all sources mixed)
  const [featuredPage] = useState(() => Math.floor(Math.random() * 20) + 1)

  const { data: petsData } = useGetPetsQuery({ page: 1, pageSize: 1 }, { refetchOnMountOrArgChange: true })
  const { data: volunteersData } = useGetVolunteersQuery({ page: 1, pageSize: 1 }, { refetchOnMountOrArgChange: true })
  const { data: featuredPetsData, isLoading: featuredLoading, isError: featuredError } = useGetPetsQuery(
    { page: featuredPage, pageSize: 6, source: 'local' },
    { refetchOnMountOrArgChange: true }
  )

  // ── Structured data ─────────────────────────────────────

  const websiteJsonLd = {
    '@context': 'https://schema.org',
    '@type': 'WebSite',
    name: 'PetZone',
    url: 'https://getpetzone.com',
    potentialAction: {
      '@type': 'SearchAction',
      target: { '@type': 'EntryPoint', urlTemplate: 'https://getpetzone.com/uk/pets?nickname={search_term_string}' },
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
      { '@type': 'Question', name: 'Що таке PetZone?', acceptedAnswer: { '@type': 'Answer', text: 'PetZone — безкоштовна платформа, яка з\'єднує волонтерів з людьми, готовими дати тваринам дім.' } },
      { '@type': 'Question', name: 'Як усиновити тварину?', acceptedAnswer: { '@type': 'Answer', text: 'Знайдіть тваринку, перейдіть на профіль волонтера і зв\'яжіться з ним напряму.' } },
      { '@type': 'Question', name: 'Чи є плата за усиновлення?', acceptedAnswer: { '@type': 'Answer', text: 'Платформа PetZone безкоштовна.' } },
      { '@type': 'Question', name: 'Як стати волонтером?', acceptedAnswer: { '@type': 'Answer', text: 'Натисніть «Стати волонтером», заповніть форму і надішліть заявку.' } },
      { '@type': 'Question', name: 'Чи потрібна реєстрація для перегляду тварин?', acceptedAnswer: { '@type': 'Answer', text: 'Ні. Переглядати анкети тварин можна без реєстрації.' } },
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
        <Box sx={{ position: 'absolute', top: -60, right: -60, width: 320, height: 320, borderRadius: '50%', bgcolor: 'rgba(255,107,107,0.08)' }} />
        <Box sx={{ position: 'absolute', bottom: -60, left: -60, width: 240, height: 240, borderRadius: '50%', bgcolor: 'rgba(99,102,241,0.15)' }} />

        <FloatingDot size={14} top="15%"  left="8%"   delay={0}   opacity={0.25} />
        <FloatingDot size={20} top="60%"  left="5%"   delay={1.5} opacity={0.15} />
        <FloatingDot size={10} top="25%"  right="10%" delay={0.8} opacity={0.2}  />
        <FloatingDot size={24} top="55%"  right="7%"  delay={2.2} opacity={0.12} />
        <FloatingDot size={16} top="80%"  left="20%"  delay={1}   opacity={0.18} />
        <FloatingDot size={12} top="10%"  right="25%" delay={3}   opacity={0.22} />

        <Container maxWidth="lg" sx={{ position: 'relative' }}>
          <Box sx={{
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            gap: { xs: 0, md: 6 },
            flexDirection: { xs: 'column', md: 'row' },
          }}>
            {/* ── Left: text ── */}
            <Box sx={{ flex: 1, textAlign: { xs: 'center', md: 'left' }, maxWidth: { md: 560 } }}>
              {/* Badge */}
              <motion.div
                initial={{ opacity: 0, scale: 0.85 }}
                animate={{ opacity: 1, scale: 1 }}
                transition={{ duration: 0.5, ease: 'backOut' }}
                style={{ display: 'inline-block', marginBottom: 24 }}
              >
                <Box sx={{
                  display: 'inline-flex', alignItems: 'center', gap: 1,
                  bgcolor: 'rgba(255,107,107,0.15)', border: '1px solid rgba(255,107,107,0.35)',
                  borderRadius: 5, px: 2, py: 0.75,
                }}>
                  <PetsIcon sx={{ fontSize: 16, color: CORAL }} />
                  <Typography variant="caption" sx={{ color: CORAL, fontWeight: 600, letterSpacing: 0.8, textTransform: 'uppercase', fontSize: 11 }}>
                    {t('home.hero.badge')}
                  </Typography>
                </Box>
              </motion.div>

              {/* Headline */}
              <motion.div
                initial={{ opacity: 0, y: 24 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.65, delay: 0.1, ease: EASE }}
              >
                <Typography
                  component="h1"
                  variant="h2"
                  fontWeight="bold"
                  sx={{ mb: 2.5, lineHeight: 1.15, fontSize: { xs: '2rem', sm: '2.6rem', md: '3rem' } }}
                >
                  {t('home.hero.title')}
                </Typography>
              </motion.div>

              {/* Subtitle */}
              <motion.div
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.65, delay: 0.22, ease: EASE }}
              >
                <Typography variant="h6" sx={{ mb: 1.5, opacity: 0.75, fontWeight: 400, lineHeight: 1.65, fontSize: { xs: '1rem', md: '1.1rem' } }}>
                  {t('home.hero.subtitle')}
                </Typography>
                <Typography variant="body2" sx={{ mb: 5, opacity: 0.55, lineHeight: 1.6 }}>
                  {t('home.hero.seoText')}
                </Typography>
              </motion.div>

              {/* Buttons */}
              <motion.div
                initial={{ opacity: 0, y: 16 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.55, delay: 0.35, ease: EASE }}
                style={{ display: 'flex', gap: 16, justifyContent: 'flex-start', flexWrap: 'wrap' }}
              >
                <motion.div whileHover={{ scale: 1.04, y: -2 }} whileTap={{ scale: 0.97 }}>
                  <Button
                    variant="contained"
                    size="large"
                    startIcon={<SearchIcon />}
                    onClick={() => navigate('/pets')}
                    sx={{
                      bgcolor: CORAL, '&:hover': { bgcolor: '#e55555', boxShadow: '0 6px 20px rgba(255,107,107,0.45)' },
                      textTransform: 'none', fontWeight: 700, borderRadius: 3, px: 4, py: 1.5, fontSize: '1rem',
                    }}
                  >
                    {t('home.hero.browsePets')}
                  </Button>
                </motion.div>
                <motion.div whileHover={{ scale: 1.04, y: -2 }} whileTap={{ scale: 0.97 }}>
                  <Button
                    variant="outlined"
                    size="large"
                    startIcon={<VolunteerActivismIcon />}
                    onClick={() => navigate('/register/volunteer')}
                    sx={{
                      color: 'white', borderColor: 'rgba(255,255,255,0.4)',
                      textTransform: 'none', fontWeight: 600, borderRadius: 3, px: 4, py: 1.5, fontSize: '1rem',
                      '&:hover': { borderColor: CORAL, color: CORAL, bgcolor: 'transparent' },
                    }}
                  >
                    {t('home.hero.becomeVolunteer')}
                  </Button>
                </motion.div>
              </motion.div>
            </Box>

            {/* ── Right: Lottie animation ── */}
            <motion.div
              initial={{ opacity: 0, x: 40, scale: 0.92 }}
              animate={{ opacity: 1, x: 0, scale: 1 }}
              transition={{ duration: 0.8, delay: 0.3, ease: EASE }}
              style={{ flexShrink: 0 }}
            >
              <Box sx={{
                width: { xs: 220, sm: 280, md: 380 },
                height: { xs: 220, sm: 280, md: 380 },
                display: { xs: 'none', sm: 'block' },
              }}>
                <HeroCatAnimation />
              </Box>
            </motion.div>
          </Box>
        </Container>
      </Box>

      {/* ── Marquee ───────────────────────────────────────── */}
      <Box
        sx={{ bgcolor: '#1e1b4b', py: 1.5, overflow: 'hidden', borderBottom: '1px solid rgba(255,107,107,0.2)' }}
        aria-hidden="true"
      >
        <Box
          sx={{
            display: 'inline-flex',
            whiteSpace: 'nowrap',
            animation: 'marqueeScroll 32s linear infinite',
            '@keyframes marqueeScroll': {
              from: { transform: 'translateX(0)' },
              to:   { transform: 'translateX(-50%)' },
            },
          }}
        >
          {[0, 1].map((setIdx) =>
            [
              t('home.marquee.item1'),
              t('home.marquee.item2'),
              t('home.marquee.item3'),
              t('home.marquee.item4'),
              t('home.marquee.item5'),
              t('home.marquee.item6'),
            ].map((text, j) => (
              <Box key={`${setIdx}-${j}`} sx={{ display: 'inline-flex', alignItems: 'center', mx: 2.5 }}>
                <Typography variant="caption" sx={{ color: 'rgba(255,255,255,0.65)', fontWeight: 500, fontSize: 12, letterSpacing: 0.3 }}>
                  {text}
                </Typography>
                <Box component="span" sx={{ ml: 2.5, color: CORAL, fontSize: 9 }}>✦</Box>
              </Box>
            ))
          )}
        </Box>
      </Box>

      {/* ── Stats ─────────────────────────────────────────── */}
      <Box component="section" sx={{ bgcolor: 'background.paper', py: 5, borderBottom: '1px solid divider' }}>
        <Container maxWidth="sm">
          <Box sx={{ display: 'flex', gap: 3, flexWrap: 'wrap', justifyContent: 'center', alignItems: 'stretch' }}>
            <AnimatedStatCard key={`pets-${petsData?.totalCount ?? 'x'}`} target={petsData?.totalCount ?? 0} label={t('home.stats.pets')} icon={<PetsIcon sx={{ fontSize: 32 }} />} />
            <AnimatedStatCard key={`vols-${volunteersData?.totalCount ?? 'x'}`} target={volunteersData?.totalCount ?? 0} label={t('home.stats.volunteers')} icon={<VolunteerActivismIcon sx={{ fontSize: 32 }} />} />
            <FreeCard label={t('home.stats.free')} icon={<FavoriteIcon sx={{ fontSize: 32 }} />} />
          </Box>
        </Container>
      </Box>

      {/* ── How it works ──────────────────────────────────── */}
      <Box component="section" sx={{ bgcolor: 'background.default', py: { xs: 7, md: 10 } }}>
        <Container maxWidth="md">
          <motion.div
            variants={fadeUp}
            initial="hidden"
            whileInView="visible"
            viewport={{ once: true, margin: '-60px' }}
          >
            <Typography variant="h2" component="h2" fontWeight="bold" textAlign="center" sx={{ mb: 1, color: '#1F2937', fontSize: { xs: '1.7rem', md: '2.2rem' } }}>
              {t('home.how.title')}
            </Typography>
            <Typography variant="body1" textAlign="center" color="text.secondary" sx={{ mb: 6, maxWidth: 480, mx: 'auto' }}>
              {t('home.how.subtitle')}
            </Typography>
          </motion.div>

          <Box sx={{ display: 'flex', gap: 4, flexWrap: 'wrap', justifyContent: 'center' }}>
            {[
              { step: 1, titleKey: 'home.how.step1.title', descKey: 'home.how.step1.desc' },
              { step: 2, titleKey: 'home.how.step2.title', descKey: 'home.how.step2.desc' },
              { step: 3, titleKey: 'home.how.step3.title', descKey: 'home.how.step3.desc' },
            ].map(({ step, titleKey, descKey }) => (
              <motion.div
                key={step}
                custom={step - 1}
                variants={stepVariant}
                initial="hidden"
                whileInView="visible"
                viewport={{ once: true, margin: '-40px' }}
                style={{ flex: 1, minWidth: 220, textAlign: 'center', padding: '0 8px' }}
              >
                <motion.div
                  whileHover={{ scale: 1.08 }}
                  transition={{ type: 'spring', stiffness: 300 }}
                  style={{
                    width: 56, height: 56, borderRadius: '50%',
                    background: '#FFF0F0', color: CORAL,
                    display: 'flex', alignItems: 'center', justifyContent: 'center',
                    margin: '0 auto 16px',
                    fontSize: 22, fontWeight: 700,
                    boxShadow: '0 4px 14px rgba(255,107,107,0.25)',
                  }}
                >
                  {step}
                </motion.div>
                <Typography variant="h6" fontWeight="bold" sx={{ mb: 1 }}>{t(titleKey)}</Typography>
                <Typography variant="body2" color="text.secondary" sx={{ lineHeight: 1.7 }}>{t(descKey)}</Typography>
              </motion.div>
            ))}
          </Box>
        </Container>
      </Box>

      {/* ── Featured Pets ─────────────────────────────────── */}
      <Box component="section" sx={{ bgcolor: 'background.paper', py: { xs: 6, md: 9 } }}>
        <Container maxWidth="lg">
          <motion.div
            variants={fadeUp}
            initial="hidden"
            whileInView="visible"
            viewport={{ once: true, margin: '-60px' }}
            style={{ textAlign: 'center', marginBottom: 48 }}
          >
            <Typography variant="h2" component="h2" fontWeight="bold" sx={{ mb: 1.5, color: '#1F2937', fontSize: { xs: '1.7rem', md: '2.2rem' } }}>
              {t('home.featured.title')}
            </Typography>
            <Typography variant="body1" color="text.secondary" sx={{ maxWidth: 520, mx: 'auto' }}>
              {t('home.featured.subtitle')}
            </Typography>
          </motion.div>

          {featuredError && (
            <Alert severity="error" sx={{ mb: 3 }}>{t('errors.unknown')}</Alert>
          )}

          {featuredLoading ? (
            <Grid container spacing={3}>
              {Array.from({ length: 6 }).map((_, i) => (
                <Grid key={i} size={{ xs: 12, sm: 6, md: 4 }}>
                  <Skeleton variant="rounded" height={420} sx={{ borderRadius: 3 }} />
                </Grid>
              ))}
            </Grid>
          ) : (
            <motion.div
              variants={staggerContainer}
              initial="hidden"
              whileInView="visible"
              viewport={{ once: true, margin: '-40px' }}
            >
              <Grid container spacing={3}>
                {(featuredPetsData?.items ?? []).map((pet) => (
                  <Grid key={pet.id} size={{ xs: 12, sm: 6, md: 4 }}>
                    <motion.div variants={cardVariant} style={{ height: '100%' }}>
                      <PetCard pet={pet} />
                    </motion.div>
                  </Grid>
                ))}
              </Grid>
            </motion.div>
          )}

          <motion.div
            variants={fadeIn}
            initial="hidden"
            whileInView="visible"
            viewport={{ once: true }}
            style={{ textAlign: 'center', marginTop: 40 }}
          >
            <motion.div whileHover={{ x: 4 }} style={{ display: 'inline-block' }}>
              <Button
                variant="outlined"
                size="large"
                endIcon={<ArrowForwardIcon />}
                onClick={() => navigate('/pets')}
                sx={{
                  borderColor: CORAL, color: CORAL,
                  textTransform: 'none', fontWeight: 600, borderRadius: 3, px: 4, py: 1.4,
                  '&:hover': { bgcolor: '#FFF0F0', borderColor: CORAL },
                }}
              >
                {t('home.featured.viewAll')}
              </Button>
            </motion.div>
          </motion.div>
        </Container>
      </Box>

      {/* ── Trust strip ───────────────────────────────────── */}
      <Box sx={{ bgcolor: 'background.default', borderTop: '1px solid', borderBottom: '1px solid', borderColor: 'divider', py: 3 }}>
        <Container maxWidth="md">
          <motion.div
            variants={staggerContainer}
            initial="hidden"
            whileInView="visible"
            viewport={{ once: true }}
            style={{ display: 'flex', gap: 32, justifyContent: 'center', flexWrap: 'wrap', alignItems: 'center' }}
          >
            {[
              { icon: '🔒', text: t('home.trust.free') },
              { icon: '✅', text: t('home.trust.verified') },
              { icon: '🐾', text: t('home.trust.volunteers') },
              { icon: '📍', text: t('home.trust.location') },
            ].map((item) => (
              <motion.div key={item.text} variants={fadeUp} style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
                <Typography sx={{ fontSize: 18 }}>{item.icon}</Typography>
                <Typography variant="body2" color="text.secondary" fontWeight={500}>{item.text}</Typography>
              </motion.div>
            ))}
          </motion.div>
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
        <Box sx={{ position: 'absolute', top: -30, right: -30, width: 180, height: 180, borderRadius: '50%', bgcolor: 'rgba(255,255,255,0.08)' }} />
        <Box sx={{ position: 'absolute', bottom: -20, left: -20, width: 120, height: 120, borderRadius: '50%', bgcolor: 'rgba(255,255,255,0.06)' }} />

        <Container maxWidth="sm" sx={{ textAlign: 'center', position: 'relative' }}>
          <motion.div
            variants={fadeUp}
            initial="hidden"
            whileInView="visible"
            viewport={{ once: true }}
          >
            <motion.div
              animate={{ scale: [1, 1.1, 1] }}
              transition={{ duration: 2.5, repeat: Infinity, ease: 'easeInOut' }}
              style={{ display: 'inline-block', marginBottom: 16 }}
            >
              <VolunteerActivismIcon sx={{ fontSize: 52, color: 'white', opacity: 0.95 }} />
            </motion.div>
            <Typography variant="h3" component="h2" fontWeight="bold" sx={{ color: 'white', mb: 1.5, fontSize: { xs: '1.7rem', md: '2.2rem' } }}>
              {t('home.cta.title')}
            </Typography>
            <Typography variant="body1" sx={{ color: 'rgba(255,255,255,0.88)', mb: 4, lineHeight: 1.75 }}>
              {t('home.cta.subtitle')}
            </Typography>
            <motion.div whileHover={{ scale: 1.05, y: -3 }} whileTap={{ scale: 0.97 }} style={{ display: 'inline-block' }}>
              <Button
                variant="contained"
                size="large"
                onClick={() => navigate('/register/volunteer')}
                sx={{
                  bgcolor: 'white', color: CORAL,
                  '&:hover': { bgcolor: 'rgba(255,255,255,0.92)', boxShadow: '0 8px 24px rgba(0,0,0,0.15)' },
                  textTransform: 'none', fontWeight: 700,
                  borderRadius: 3, px: 5, py: 1.5, fontSize: '1rem',
                }}
              >
                {t('home.cta.btn')}
              </Button>
            </motion.div>
          </motion.div>
        </Container>
      </Box>
    </Box>
  )
}
