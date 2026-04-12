import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Helmet } from 'react-helmet-async'
import { useLangNavigate } from '../hooks/useLangNavigate'
import PageMeta from '../components/meta/PageMeta'
import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Typography from '@mui/material/Typography'
import Accordion from '@mui/material/Accordion'
import AccordionSummary from '@mui/material/AccordionSummary'
import AccordionDetails from '@mui/material/AccordionDetails'
import Button from '@mui/material/Button'
import Chip from '@mui/material/Chip'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import PetsIcon from '@mui/icons-material/Pets'

const CORAL = '#FF6B6B'
const DARK = '#1e1b4b'

const CATEGORIES = ['general', 'adoption', 'volunteers', 'technical'] as const
type Category = typeof CATEGORIES[number]

export default function FaqPage() {
  const { t } = useTranslation()
  const navigate = useLangNavigate()
  const [expanded, setExpanded] = useState<string | false>(false)
  const [activeCategory, setActiveCategory] = useState<Category | 'all'>('all')

  const questions: { id: string; category: Category; q: string; a: string }[] = [
    { id: 'q1',  category: 'general',    q: t('faq.q1.q'),  a: t('faq.q1.a') },
    { id: 'q2',  category: 'general',    q: t('faq.q2.q'),  a: t('faq.q2.a') },
    { id: 'q3',  category: 'adoption',   q: t('faq.q3.q'),  a: t('faq.q3.a') },
    { id: 'q4',  category: 'adoption',   q: t('faq.q4.q'),  a: t('faq.q4.a') },
    { id: 'q5',  category: 'adoption',   q: t('faq.q5.q'),  a: t('faq.q5.a') },
    { id: 'q6',  category: 'volunteers', q: t('faq.q6.q'),  a: t('faq.q6.a') },
    { id: 'q7',  category: 'volunteers', q: t('faq.q7.q'),  a: t('faq.q7.a') },
    { id: 'q8',  category: 'volunteers', q: t('faq.q8.q'),  a: t('faq.q8.a') },
    { id: 'q9',  category: 'technical',  q: t('faq.q9.q'),  a: t('faq.q9.a') },
    { id: 'q10', category: 'technical',  q: t('faq.q10.q'), a: t('faq.q10.a') },
  ]

  const filtered = activeCategory === 'all' ? questions : questions.filter((q) => q.category === activeCategory)

  const faqSchema = {
    '@context': 'https://schema.org',
    '@type': 'FAQPage',
    mainEntity: questions.map((item) => ({
      '@type': 'Question',
      name: item.q,
      acceptedAnswer: {
        '@type': 'Answer',
        text: item.a,
      },
    })),
  }

  const handleChange = (panel: string) => (_: React.SyntheticEvent, isExpanded: boolean) => {
    setExpanded(isExpanded ? panel : false)
  }

  return (
    <Box sx={{ bgcolor: 'background.default' }}>
      <PageMeta title={t('faq.pageTitle')} description={t('faq.metaDesc')} path="/faq" />
      <Helmet>
        <script type="application/ld+json">{JSON.stringify(faqSchema)}</script>
      </Helmet>

      {/* Hero */}
      <Box
        sx={{
          background: `linear-gradient(135deg, ${DARK} 0%, #312e81 60%, #4c1d95 100%)`,
          color: 'white',
          py: { xs: 7, md: 10 },
          textAlign: 'center',
        }}
      >
        <Container maxWidth="md">
          <Box sx={{ display: 'inline-flex', alignItems: 'center', gap: 1, bgcolor: 'rgba(255,107,107,0.15)', border: '1px solid rgba(255,107,107,0.35)', borderRadius: 5, px: 2, py: 0.75, mb: 3 }}>
            <PetsIcon sx={{ fontSize: 16, color: CORAL }} />
            <Typography variant="caption" sx={{ color: CORAL, fontWeight: 600, letterSpacing: 1, textTransform: 'uppercase' }}>
              {t('faq.badge')}
            </Typography>
          </Box>
          <Typography variant="h3" fontWeight="bold" sx={{ mb: 2, fontSize: { xs: '1.8rem', md: '2.8rem' } }}>
            {t('faq.pageTitle')}
          </Typography>
          <Typography variant="h6" sx={{ opacity: 0.8, fontWeight: 400, fontSize: { xs: '1rem', md: '1.1rem' } }}>
            {t('faq.subtitle')}
          </Typography>
        </Container>
      </Box>

      <Container maxWidth="md" sx={{ py: { xs: 5, md: 8 } }}>
        {/* Category filter */}
        <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', mb: 4, justifyContent: 'center' }}>
          {(['all', ...CATEGORIES] as const).map((cat) => (
            <Chip
              key={cat}
              label={t(`faq.category.${cat}`)}
              onClick={() => setActiveCategory(cat)}
              variant={activeCategory === cat ? 'filled' : 'outlined'}
              sx={{
                cursor: 'pointer',
                bgcolor: activeCategory === cat ? CORAL : 'transparent',
                color: activeCategory === cat ? 'white' : 'text.secondary',
                borderColor: activeCategory === cat ? CORAL : '#E5E7EB',
                fontWeight: activeCategory === cat ? 600 : 400,
                '&:hover': { bgcolor: activeCategory === cat ? '#e55555' : '#FFF0F0', borderColor: CORAL, color: activeCategory === cat ? 'white' : CORAL },
              }}
            />
          ))}
        </Box>

        {/* Accordion list */}
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1.5 }}>
          {filtered.map((item) => (
            <Accordion
              key={item.id}
              expanded={expanded === item.id}
              onChange={handleChange(item.id)}
              elevation={0}
              sx={{
                border: '1px solid',
                borderColor: expanded === item.id ? CORAL : '#E5E7EB',
                borderRadius: '12px !important',
                '&:before': { display: 'none' },
                transition: 'border-color 0.2s',
              }}
            >
              <AccordionSummary
                expandIcon={<ExpandMoreIcon sx={{ color: expanded === item.id ? CORAL : '#9CA3AF' }} />}
                sx={{ px: 3, py: 0.5, '& .MuiAccordionSummary-content': { my: 1.5 } }}
              >
                <Typography fontWeight={expanded === item.id ? 700 : 500} sx={{ color: expanded === item.id ? CORAL : 'text.primary', lineHeight: 1.5 }}>
                  {item.q}
                </Typography>
              </AccordionSummary>
              <AccordionDetails sx={{ px: 3, pb: 2.5, pt: 0 }}>
                <Typography variant="body2" color="text.secondary" sx={{ lineHeight: 1.85, whiteSpace: 'pre-line' }}>
                  {item.a}
                </Typography>
              </AccordionDetails>
            </Accordion>
          ))}
        </Box>

        {/* CTA */}
        <Box
          sx={{
            mt: 7,
            textAlign: 'center',
            bgcolor: 'background.paper',
            border: '1px solid #E5E7EB',
            borderRadius: 4,
            p: { xs: 3, md: 5 },
          }}
        >
          <Typography variant="h6" fontWeight="bold" sx={{ mb: 1 }}>
            {t('faq.ctaTitle')}
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
            {t('faq.ctaSubtitle')}
          </Typography>
          <Box sx={{ display: 'flex', gap: 1.5, justifyContent: 'center', flexWrap: 'wrap' }}>
            <Button
              variant="contained"
              onClick={() => navigate('/pets')}
              sx={{ bgcolor: CORAL, '&:hover': { bgcolor: '#e55555' }, textTransform: 'none', fontWeight: 600, borderRadius: 2 }}
            >
              {t('home.hero.browsePets')}
            </Button>
            <Button
              variant="outlined"
              onClick={() => navigate('/register/volunteer')}
              sx={{ borderColor: CORAL, color: CORAL, textTransform: 'none', fontWeight: 600, borderRadius: 2, '&:hover': { bgcolor: '#FFF0F0', borderColor: CORAL } }}
            >
              {t('home.cta.btn')}
            </Button>
          </Box>
        </Box>
      </Container>
    </Box>
  )
}
