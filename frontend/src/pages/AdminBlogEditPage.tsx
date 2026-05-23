import { useEffect, useState } from 'react'
import { Navigate, useParams } from 'react-router-dom'
import ReactMarkdown from 'react-markdown'
import remarkGfm from 'remark-gfm'
import { useAuthStore } from '../store/authStore'
import { useLangNavigate } from '../hooks/useLangNavigate'
import PageMeta from '../components/meta/PageMeta'
import {
  useGetBlogPostBySlugQuery,
  useGetBlogPostsQuery,
  useCreateBlogPostMutation,
  useUpdateBlogPostMutation,
} from '../services/blogApi'
import { getApiError } from '../lib/getApiError'
import { useTranslation } from 'react-i18next'
import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Paper from '@mui/material/Paper'
import Typography from '@mui/material/Typography'
import TextField from '@mui/material/TextField'
import MenuItem from '@mui/material/MenuItem'
import Button from '@mui/material/Button'
import IconButton from '@mui/material/IconButton'
import CircularProgress from '@mui/material/CircularProgress'
import Snackbar from '@mui/material/Snackbar'
import Alert from '@mui/material/Alert'
import Tabs from '@mui/material/Tabs'
import Tab from '@mui/material/Tab'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'

const CORAL = '#FF6B6B'
const LANGS = ['uk', 'ru', 'en', 'pl', 'de', 'fr']

// Single page handles both /admin/blog/new and /admin/blog/:id/edit.
// In edit mode we look up by id via a paged list (no GET-by-id endpoint
// in v1) — for v1 this is fine because admin list returns ≤50 posts and
// edit is rare. Once the blog grows past ~50 posts add GET /blog/by-id/{id}.
export default function AdminBlogEditPage() {
  const { id } = useParams<{ id?: string }>()
  const isEdit = !!id
  const { user } = useAuthStore()
  const navigate = useLangNavigate()
  const { t } = useTranslation()

  // Form state
  const [slug, setSlug] = useState('')
  const [language, setLanguage] = useState('uk')
  const [title, setTitle] = useState('')
  const [summary, setSummary] = useState('')
  const [contentMarkdown, setContentMarkdown] = useState('')
  const [coverImageUrl, setCoverImageUrl] = useState('')
  const [preview, setPreview] = useState(0)
  const [toast, setToast] = useState<{ msg: string; severity: 'success' | 'error' } | null>(null)

  // Find existing post by id when editing — list endpoint is the only way today.
  const { data: list, isLoading: isLoadingList } = useGetBlogPostsQuery(
    { page: 1, pageSize: 100 },
    { skip: !isEdit },
  )
  const existing = isEdit ? list?.items.find(p => p.id === id) : undefined
  // We have list item (no full content), so we also need the post by slug to get content.
  const { data: full } = useGetBlogPostBySlugQuery(existing?.slug ?? '', { skip: !existing })

  const [createPost, { isLoading: isCreating }] = useCreateBlogPostMutation()
  const [updatePost, { isLoading: isUpdating }] = useUpdateBlogPostMutation()
  const isSaving = isCreating || isUpdating

  useEffect(() => {
    if (!full) return
    setSlug(full.slug)
    setLanguage(full.language)
    setTitle(full.title)
    setSummary(full.summary)
    setContentMarkdown(full.contentMarkdown)
    setCoverImageUrl(full.coverImageUrl ?? '')
  }, [full])

  if (!user) return <Navigate to="/login" replace />
  if (user.role !== 'Admin') return <Navigate to="/" replace />

  const handleSave = async () => {
    try {
      if (isEdit && existing) {
        await updatePost({
          id: existing.id,
          slug: existing.slug,
          data: {
            title: title.trim(),
            summary: summary.trim(),
            contentMarkdown,
            coverImageUrl: coverImageUrl.trim() || undefined,
          },
        }).unwrap()
        setToast({ msg: 'Saved', severity: 'success' })
      } else {
        const res = await createPost({
          slug: slug.trim(),
          language,
          title: title.trim(),
          summary: summary.trim(),
          contentMarkdown,
          coverImageUrl: coverImageUrl.trim() || undefined,
        }).unwrap()
        setToast({ msg: 'Created', severity: 'success' })
        navigate(`/admin/blog/${res.id}/edit`)
      }
    } catch (err) {
      setToast({ msg: getApiError(err, t), severity: 'error' })
    }
  }

  const canSave = title.trim() && summary.trim() && contentMarkdown.trim() && (isEdit || slug.trim())

  if (isEdit && isLoadingList) {
    return (
      <Box sx={{ minHeight: '60vh', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
        <CircularProgress sx={{ color: CORAL }} />
      </Box>
    )
  }

  if (isEdit && !existing && !isLoadingList) {
    return (
      <Container maxWidth="md" sx={{ py: 4 }}>
        <Alert severity="error">Post not found</Alert>
      </Container>
    )
  }

  return (
    <Box sx={{ bgcolor: 'background.default', minHeight: '100%', py: 4 }}>
      <PageMeta title="Admin · Blog · Edit" description="" path="/admin/blog/edit" noIndex />
      <Container maxWidth="md">
        <Button
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate('/admin/blog')}
          sx={{ mb: 3, color: '#6B7280', textTransform: 'none' }}
        >
          Back to blog admin
        </Button>

        <Typography variant="h5" fontWeight="bold" sx={{ mb: 3 }}>
          {isEdit ? `Edit: ${title || existing?.title}` : 'New blog post'}
        </Typography>

        <Paper elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 3, p: 3 }}>
          <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '2fr 1fr' }, gap: 2, mb: 2 }}>
            <TextField
              label="Slug (URL part, lowercase + hyphens)"
              value={slug}
              onChange={e => setSlug(e.target.value.toLowerCase().replace(/[^a-z0-9-]/g, ''))}
              disabled={isEdit}
              required
              fullWidth
              helperText={isEdit ? 'Slug is immutable after creation' : 'e.g. how-to-adopt-a-dog'}
            />
            <TextField
              label="Language"
              value={language}
              onChange={e => setLanguage(e.target.value)}
              disabled={isEdit}
              select
              fullWidth
            >
              {LANGS.map(l => <MenuItem key={l} value={l}>{l}</MenuItem>)}
            </TextField>
          </Box>

          <TextField
            label="Title"
            value={title}
            onChange={e => setTitle(e.target.value)}
            required
            fullWidth
            inputProps={{ maxLength: 200 }}
            sx={{ mb: 2 }}
          />

          <TextField
            label="Summary"
            value={summary}
            onChange={e => setSummary(e.target.value)}
            required
            multiline
            rows={2}
            fullWidth
            inputProps={{ maxLength: 500 }}
            helperText={`${summary.length}/500`}
            sx={{ mb: 2 }}
          />

          <TextField
            label="Cover image URL (optional)"
            value={coverImageUrl}
            onChange={e => setCoverImageUrl(e.target.value)}
            fullWidth
            placeholder="https://… or /api/files/…/redirect"
            sx={{ mb: 2 }}
          />

          <Tabs value={preview} onChange={(_, v) => setPreview(v)} sx={{ mb: 1, '& .MuiTab-root': { textTransform: 'none' }, '& .Mui-selected': { color: CORAL }, '& .MuiTabs-indicator': { bgcolor: CORAL } }}>
            <Tab label="Write (Markdown)" />
            <Tab label="Preview" />
          </Tabs>

          {preview === 0 ? (
            <TextField
              value={contentMarkdown}
              onChange={e => setContentMarkdown(e.target.value)}
              multiline
              minRows={18}
              fullWidth
              placeholder={'# Heading\n\nParagraph with **bold**, *italic*, [links](https://…).\n\n## Section\n\n- list item\n- list item'}
              inputProps={{ maxLength: 50000, style: { fontFamily: 'monospace', fontSize: '0.92rem' } }}
              helperText={`${contentMarkdown.length}/50 000`}
            />
          ) : (
            <Box
              sx={{
                border: '1px solid #E5E7EB',
                borderRadius: 1,
                p: 3,
                minHeight: 400,
                color: '#374151',
                lineHeight: 1.75,
                '& h1': { fontSize: '1.8rem', fontWeight: 700, mb: 2 },
                '& h2': { fontSize: '1.4rem', fontWeight: 700, mt: 3, mb: 1.5 },
                '& p': { mb: 2 },
                '& a': { color: CORAL },
                '& ul, & ol': { pl: 3 },
                '& blockquote': { borderLeft: `4px solid ${CORAL}`, pl: 2, color: '#6B7280', fontStyle: 'italic' },
                '& code': { bgcolor: '#F3F4F6', px: 0.5, borderRadius: 0.5 },
                '& pre': { bgcolor: '#F3F4F6', p: 2, borderRadius: 1, overflowX: 'auto' },
                '& img': { maxWidth: '100%' },
              }}
            >
              {contentMarkdown
                ? <ReactMarkdown remarkPlugins={[remarkGfm]}>{contentMarkdown}</ReactMarkdown>
                : <Typography color="text.secondary">Nothing to preview yet.</Typography>}
            </Box>
          )}

          <Box sx={{ display: 'flex', justifyContent: 'flex-end', mt: 3, gap: 2 }}>
            <Button onClick={() => navigate('/admin/blog')} disabled={isSaving}>Cancel</Button>
            <Button
              variant="contained"
              onClick={handleSave}
              disabled={!canSave || isSaving}
              sx={{ bgcolor: CORAL, '&:hover': { bgcolor: '#e55555' }, textTransform: 'none', borderRadius: 2, px: 3 }}
            >
              {isSaving ? <CircularProgress size={22} sx={{ color: '#fff' }} /> : (isEdit ? 'Save' : 'Create')}
            </Button>
          </Box>
        </Paper>

        <Snackbar open={!!toast} autoHideDuration={4000} onClose={() => setToast(null)}>
          <Alert severity={toast?.severity ?? 'info'} onClose={() => setToast(null)}>{toast?.msg}</Alert>
        </Snackbar>
      </Container>
    </Box>
  )
}
