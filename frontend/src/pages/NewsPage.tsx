import { useParams } from 'react-router-dom'
import { useLangNavigate } from '../hooks/useLangNavigate'
import PageMeta from '../components/meta/PageMeta'
import { useTranslation } from 'react-i18next'
import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Typography from '@mui/material/Typography'
import Button from '@mui/material/Button'
import Paper from '@mui/material/Paper'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import ArticleIcon from '@mui/icons-material/Article'


export default function NewsPage() {
  const { t } = useTranslation()
  const { volunteerId } = useParams<{ volunteerId: string }>()
  const navigate = useLangNavigate()

  return (
    <Box sx={{ bgcolor: '#FAFAFA', minHeight: '100%', py: 4 }}>
      <PageMeta title={t('news.title')} description={t('news.comingSoon')} path={`/news/${volunteerId}`} />
      <Container maxWidth="sm">
        <Button
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate(`/volunteers/${volunteerId}`)}
          sx={{ mb: 3, color: '#6B7280', textTransform: 'none' }}
        >
          {t('news.backToProfile')}
        </Button>

        <Typography variant="h4" fontWeight="bold" sx={{ mb: 4 }}>
          {t('news.title')}
        </Typography>

        <Paper
          elevation={0}
          sx={{
            border: '1px solid #E5E7EB',
            borderRadius: 3,
            p: 6,
            textAlign: 'center',
          }}
        >
          <ArticleIcon sx={{ fontSize: 64, color: '#E5E7EB', mb: 2 }} />
          <Typography variant="h6" color="text.secondary" sx={{ mb: 1 }}>
            {t('news.comingSoon')}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            {t('news.comingSoonHint')}
          </Typography>
        </Paper>
      </Container>
    </Box>
  )
}
