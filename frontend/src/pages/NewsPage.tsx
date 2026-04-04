import { useState } from 'react'
import { useParams } from 'react-router-dom'
import { useLangNavigate } from '../hooks/useLangNavigate'
import PageMeta from '../components/meta/PageMeta'
import { useTranslation } from 'react-i18next'
import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Typography from '@mui/material/Typography'
import Button from '@mui/material/Button'
import Paper from '@mui/material/Paper'
import IconButton from '@mui/material/IconButton'
import TextField from '@mui/material/TextField'
import Dialog from '@mui/material/Dialog'
import DialogTitle from '@mui/material/DialogTitle'
import DialogContent from '@mui/material/DialogContent'
import DialogActions from '@mui/material/DialogActions'
import CircularProgress from '@mui/material/CircularProgress'
import Divider from '@mui/material/Divider'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import AddIcon from '@mui/icons-material/Add'
import EditIcon from '@mui/icons-material/Edit'
import DeleteIcon from '@mui/icons-material/Delete'
import ArticleIcon from '@mui/icons-material/Article'
import {
  useGetNewsByVolunteerQuery,
  useCreateNewsPostMutation,
  useUpdateNewsPostMutation,
  useDeleteNewsPostMutation,
  type NewsPostDto,
} from '../services/newsApi'
import { useAuthStore } from '../store/authStore'

export default function NewsPage() {
  const { t } = useTranslation()
  const { volunteerId } = useParams<{ volunteerId: string }>()
  const navigate = useLangNavigate()
  const user = useAuthStore((s) => s.user)
  const isOwner = user?.id === volunteerId

  const { data: posts = [], isLoading } = useGetNewsByVolunteerQuery(volunteerId ?? '', {
    skip: !volunteerId,
  })
  const [createPost] = useCreateNewsPostMutation()
  const [updatePost] = useUpdateNewsPostMutation()
  const [deletePost] = useDeleteNewsPostMutation()

  const [createOpen, setCreateOpen] = useState(false)
  const [editPost, setEditPost] = useState<NewsPostDto | null>(null)
  const [deleteId, setDeleteId] = useState<string | null>(null)
  const [title, setTitle] = useState('')
  const [content, setContent] = useState('')
  const [saving, setSaving] = useState(false)

  const openCreate = () => {
    setTitle('')
    setContent('')
    setCreateOpen(true)
  }

  const openEdit = (post: NewsPostDto) => {
    setTitle(post.title)
    setContent(post.content)
    setEditPost(post)
  }

  const handleSaveCreate = async () => {
    if (!title.trim() || !content.trim()) return
    setSaving(true)
    try {
      await createPost({ title: title.trim(), content: content.trim() }).unwrap()
      setCreateOpen(false)
    } finally {
      setSaving(false)
    }
  }

  const handleSaveEdit = async () => {
    if (!editPost || !title.trim() || !content.trim()) return
    setSaving(true)
    try {
      await updatePost({ id: editPost.id, body: { title: title.trim(), content: content.trim() } }).unwrap()
      setEditPost(null)
    } finally {
      setSaving(false)
    }
  }

  const handleDelete = async () => {
    if (!deleteId) return
    await deletePost(deleteId).unwrap()
    setDeleteId(null)
  }

  const formatDate = (dateStr: string) =>
    new Date(dateStr).toLocaleDateString(undefined, { year: 'numeric', month: 'long', day: 'numeric' })

  return (
    <Box sx={{ bgcolor: '#FAFAFA', minHeight: '100%', py: 4 }}>
      <PageMeta title={t('news.title')} description={t('news.title')} path={`/news/${volunteerId}`} />
      <Container maxWidth="md">
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Button
            startIcon={<ArrowBackIcon />}
            onClick={() => navigate(`/volunteers/${volunteerId}`)}
            sx={{ color: '#6B7280', textTransform: 'none' }}
          >
            {t('news.backToProfile')}
          </Button>
          {isOwner && (
            <Button
              variant="contained"
              startIcon={<AddIcon />}
              onClick={openCreate}
              sx={{ textTransform: 'none', borderRadius: 2 }}
            >
              {t('news.create')}
            </Button>
          )}
        </Box>

        <Typography variant="h4" fontWeight="bold" sx={{ mb: 4 }}>
          {t('news.title')}
        </Typography>

        {isLoading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
            <CircularProgress />
          </Box>
        ) : posts.length === 0 ? (
          <Paper
            elevation={0}
            sx={{ border: '1px solid #E5E7EB', borderRadius: 3, p: 6, textAlign: 'center' }}
          >
            <ArticleIcon sx={{ fontSize: 64, color: '#E5E7EB', mb: 2 }} />
            <Typography variant="h6" color="text.secondary" sx={{ mb: 1 }}>
              {t('news.empty')}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              {isOwner ? t('news.emptyOwnerHint') : t('news.emptyHint')}
            </Typography>
          </Paper>
        ) : (
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
            {posts.map((post) => (
              <Paper
                key={post.id}
                elevation={0}
                sx={{ border: '1px solid #E5E7EB', borderRadius: 3, p: 3 }}
              >
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                  <Box sx={{ flex: 1, mr: 2 }}>
                    <Typography variant="h6" fontWeight="600" sx={{ mb: 0.5 }}>
                      {post.title}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      {formatDate(post.createdAt)}
                      {post.updatedAt && ` · ${t('news.edited')} ${formatDate(post.updatedAt)}`}
                    </Typography>
                  </Box>
                  {isOwner && (
                    <Box sx={{ display: 'flex', gap: 0.5 }}>
                      <IconButton size="small" onClick={() => openEdit(post)}>
                        <EditIcon fontSize="small" />
                      </IconButton>
                      <IconButton size="small" color="error" onClick={() => setDeleteId(post.id)}>
                        <DeleteIcon fontSize="small" />
                      </IconButton>
                    </Box>
                  )}
                </Box>
                <Divider sx={{ my: 2 }} />
                <Typography variant="body1" sx={{ whiteSpace: 'pre-wrap' }}>
                  {post.content}
                </Typography>
              </Paper>
            ))}
          </Box>
        )}
      </Container>

      {/* Create dialog */}
      <Dialog open={createOpen} onClose={() => setCreateOpen(false)} fullWidth maxWidth="sm">
        <DialogTitle>{t('news.createTitle')}</DialogTitle>
        <DialogContent sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 2 }}>
          <TextField
            label={t('news.fieldTitle')}
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            inputProps={{ maxLength: 200 }}
            fullWidth
          />
          <TextField
            label={t('news.fieldContent')}
            value={content}
            onChange={(e) => setContent(e.target.value)}
            inputProps={{ maxLength: 5000 }}
            multiline
            rows={6}
            fullWidth
          />
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={() => setCreateOpen(false)} sx={{ textTransform: 'none' }}>
            {t('common.cancel')}
          </Button>
          <Button
            variant="contained"
            onClick={handleSaveCreate}
            disabled={saving || !title.trim() || !content.trim()}
            sx={{ textTransform: 'none' }}
          >
            {saving ? <CircularProgress size={20} /> : t('news.publish')}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Edit dialog */}
      <Dialog open={!!editPost} onClose={() => setEditPost(null)} fullWidth maxWidth="sm">
        <DialogTitle>{t('news.editTitle')}</DialogTitle>
        <DialogContent sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 2 }}>
          <TextField
            label={t('news.fieldTitle')}
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            inputProps={{ maxLength: 200 }}
            fullWidth
          />
          <TextField
            label={t('news.fieldContent')}
            value={content}
            onChange={(e) => setContent(e.target.value)}
            inputProps={{ maxLength: 5000 }}
            multiline
            rows={6}
            fullWidth
          />
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={() => setEditPost(null)} sx={{ textTransform: 'none' }}>
            {t('common.cancel')}
          </Button>
          <Button
            variant="contained"
            onClick={handleSaveEdit}
            disabled={saving || !title.trim() || !content.trim()}
            sx={{ textTransform: 'none' }}
          >
            {saving ? <CircularProgress size={20} /> : t('common.save')}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Delete confirm dialog */}
      <Dialog open={!!deleteId} onClose={() => setDeleteId(null)}>
        <DialogTitle>{t('news.deleteConfirmTitle')}</DialogTitle>
        <DialogContent>
          <Typography>{t('news.deleteConfirmText')}</Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteId(null)} sx={{ textTransform: 'none' }}>
            {t('common.cancel')}
          </Button>
          <Button color="error" onClick={handleDelete} sx={{ textTransform: 'none' }}>
            {t('news.delete')}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}
