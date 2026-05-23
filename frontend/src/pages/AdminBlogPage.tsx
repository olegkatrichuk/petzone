import { useState } from 'react'
import { Navigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useAuthStore } from '../store/authStore'
import { useLangNavigate } from '../hooks/useLangNavigate'
import PageMeta from '../components/meta/PageMeta'
import { LangLink as Link } from '../components/ui/LangLink'
import {
  useGetBlogPostsQuery,
  useDeleteBlogPostMutation,
} from '../services/blogApi'
import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Typography from '@mui/material/Typography'
import Paper from '@mui/material/Paper'
import Button from '@mui/material/Button'
import IconButton from '@mui/material/IconButton'
import Chip from '@mui/material/Chip'
import Table from '@mui/material/Table'
import TableBody from '@mui/material/TableBody'
import TableCell from '@mui/material/TableCell'
import TableHead from '@mui/material/TableHead'
import TableRow from '@mui/material/TableRow'
import Dialog from '@mui/material/Dialog'
import DialogTitle from '@mui/material/DialogTitle'
import DialogActions from '@mui/material/DialogActions'
import Skeleton from '@mui/material/Skeleton'
import Alert from '@mui/material/Alert'
import Snackbar from '@mui/material/Snackbar'
import AddIcon from '@mui/icons-material/Add'
import EditIcon from '@mui/icons-material/Edit'
import DeleteIcon from '@mui/icons-material/Delete'
import OpenInNewIcon from '@mui/icons-material/OpenInNew'

const CORAL = '#FF6B6B'

export default function AdminBlogPage() {
  const { t } = useTranslation()
  const { user } = useAuthStore()
  const navigate = useLangNavigate()
  const [toast, setToast] = useState<string | null>(null)
  const [confirmDelete, setConfirmDelete] = useState<{ id: string; slug: string; title: string } | null>(null)

  // Admin sees ALL posts across languages; lang filter is intentionally omitted.
  const { data, isLoading } = useGetBlogPostsQuery({ page: 1, pageSize: 50 })
  const [deletePost, { isLoading: isDeleting }] = useDeleteBlogPostMutation()

  if (!user) return <Navigate to="/login" replace />
  if (user.role !== 'Admin') return <Navigate to="/" replace />

  const handleDelete = async () => {
    if (!confirmDelete) return
    try {
      await deletePost({ id: confirmDelete.id, slug: confirmDelete.slug }).unwrap()
      setToast(`"${confirmDelete.title}" видалено`)
      setConfirmDelete(null)
    } catch {
      setToast('Не вдалося видалити')
    }
  }

  return (
    <Box sx={{ bgcolor: 'background.default', minHeight: '100%', py: 4 }}>
      <PageMeta title="Admin · Blog" description="Admin · Blog" path="/admin/blog" noIndex />
      <Container maxWidth="lg">
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 3 }}>
          <Typography variant="h5" fontWeight="bold">Blog (admin)</Typography>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => navigate('/admin/blog/new')}
            sx={{ bgcolor: CORAL, '&:hover': { bgcolor: '#e55555' }, textTransform: 'none', borderRadius: 2 }}
          >
            New post
          </Button>
        </Box>

        <Paper elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 3, overflow: 'hidden' }}>
          {isLoading ? (
            <Box sx={{ p: 2 }}>
              {Array.from({ length: 5 }).map((_, i) => (
                <Skeleton key={i} height={48} sx={{ mb: 1 }} />
              ))}
            </Box>
          ) : !data || data.items.length === 0 ? (
            <Box sx={{ p: 4, textAlign: 'center' }}>
              <Typography color="text.secondary" sx={{ mb: 2 }}>{t('blog.empty', { defaultValue: 'Поки немає публікацій.' })}</Typography>
            </Box>
          ) : (
            <Table>
              <TableHead>
                <TableRow sx={{ bgcolor: '#F9FAFB' }}>
                  <TableCell sx={{ fontWeight: 600 }}>Title</TableCell>
                  <TableCell sx={{ fontWeight: 600, width: 80 }}>Lang</TableCell>
                  <TableCell sx={{ fontWeight: 600 }}>Slug</TableCell>
                  <TableCell sx={{ fontWeight: 600, width: 120 }}>Created</TableCell>
                  <TableCell sx={{ fontWeight: 600, width: 140, textAlign: 'right' }}>Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {data.items.map(post => (
                  <TableRow key={post.id} hover>
                    <TableCell>{post.title}</TableCell>
                    <TableCell><Chip size="small" label={post.language} /></TableCell>
                    <TableCell>
                      <Typography component="code" sx={{ fontFamily: 'monospace', fontSize: '0.85rem', color: '#6B7280' }}>
                        {post.slug}
                      </Typography>
                    </TableCell>
                    <TableCell>{new Date(post.createdAt).toLocaleDateString()}</TableCell>
                    <TableCell sx={{ textAlign: 'right' }}>
                      <IconButton
                        size="small"
                        component={Link}
                        to={`/blog/${post.slug}`}
                        title="View public page"
                      >
                        <OpenInNewIcon fontSize="small" />
                      </IconButton>
                      <IconButton
                        size="small"
                        onClick={() => navigate(`/admin/blog/${post.id}/edit`)}
                        title="Edit"
                      >
                        <EditIcon fontSize="small" />
                      </IconButton>
                      <IconButton
                        size="small"
                        onClick={() => setConfirmDelete({ id: post.id, slug: post.slug, title: post.title })}
                        title="Delete"
                        sx={{ color: '#DC2626' }}
                      >
                        <DeleteIcon fontSize="small" />
                      </IconButton>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </Paper>

        {data && data.total > data.items.length && (
          <Alert severity="info" sx={{ mt: 2 }}>
            Показано {data.items.length} з {data.total}. Пагінація додається пізніше.
          </Alert>
        )}

        <Dialog open={!!confirmDelete} onClose={() => !isDeleting && setConfirmDelete(null)}>
          <DialogTitle>Видалити "{confirmDelete?.title}"?</DialogTitle>
          <DialogActions>
            <Button onClick={() => setConfirmDelete(null)} disabled={isDeleting}>Скасувати</Button>
            <Button onClick={handleDelete} disabled={isDeleting} color="error">Видалити</Button>
          </DialogActions>
        </Dialog>

        <Snackbar open={!!toast} autoHideDuration={4000} onClose={() => setToast(null)}>
          <Alert severity="success" onClose={() => setToast(null)}>{toast}</Alert>
        </Snackbar>
      </Container>
    </Box>
  )
}
