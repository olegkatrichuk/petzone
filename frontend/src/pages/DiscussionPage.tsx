import { useState, useRef, useEffect } from 'react'
import { useParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useLangNavigate } from '../hooks/useLangNavigate'
import { useAuthStore } from '../store/authStore'
import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Typography from '@mui/material/Typography'
import Paper from '@mui/material/Paper'
import TextField from '@mui/material/TextField'
import IconButton from '@mui/material/IconButton'
import Tooltip from '@mui/material/Tooltip'
import CircularProgress from '@mui/material/CircularProgress'
import Alert from '@mui/material/Alert'
import Button from '@mui/material/Button'
import Chip from '@mui/material/Chip'
import SendIcon from '@mui/icons-material/Send'
import EditIcon from '@mui/icons-material/Edit'
import DeleteIcon from '@mui/icons-material/Delete'
import CheckIcon from '@mui/icons-material/Check'
import CloseIcon from '@mui/icons-material/Close'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import LockIcon from '@mui/icons-material/Lock'
import {
  useGetDiscussionQuery,
  useAddMessageMutation,
  useEditMessageMutation,
  useDeleteMessageMutation,
} from '../services/discussionApi'
import type { MessageDto } from '../services/discussionApi'

const CORAL = '#FF6B6B'
const POLL_INTERVAL = 10_000

// ── Single message bubble ──────────────────────────────────

interface BubbleProps {
  message: MessageDto
  isMine: boolean
  discussionId: string
  isClosed: boolean
}

function MessageBubble({ message, isMine, discussionId, isClosed }: BubbleProps) {
  const { t } = useTranslation()
  const [editing, setEditing] = useState(false)
  const [editText, setEditText] = useState(message.text)
  const [editMessage, { isLoading: saving }] = useEditMessageMutation()
  const [deleteMessage, { isLoading: deleting }] = useDeleteMessageMutation()

  const time = new Date(message.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })

  const handleSaveEdit = async () => {
    if (!editText.trim() || editText.trim() === message.text) { setEditing(false); return }
    await editMessage({ discussionId, messageId: message.id, newText: editText.trim() })
    setEditing(false)
  }

  const handleDelete = () => deleteMessage({ discussionId, messageId: message.id })

  return (
    <Box
      sx={{
        display: 'flex',
        justifyContent: isMine ? 'flex-end' : 'flex-start',
        mb: 1,
      }}
    >
      <Box sx={{ maxWidth: '72%', minWidth: 80 }}>
        {/* Bubble */}
        <Box
          sx={{
            bgcolor: isMine ? CORAL : 'white',
            color: isMine ? 'white' : 'text.primary',
            border: isMine ? 'none' : '1px solid #E5E7EB',
            borderRadius: isMine ? '18px 18px 4px 18px' : '18px 18px 18px 4px',
            px: 2,
            py: 1,
            position: 'relative',
          }}
        >
          {editing ? (
            <Box sx={{ display: 'flex', alignItems: 'flex-end', gap: 0.5 }}>
              <TextField
                value={editText}
                onChange={(e) => setEditText(e.target.value)}
                multiline
                size="small"
                autoFocus
                variant="standard"
                onKeyDown={(e) => { if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); handleSaveEdit() } }}
                sx={{
                  flex: 1,
                  '& .MuiInput-root': { color: 'white' },
                  '& .MuiInput-underline:before': { borderColor: 'rgba(255,255,255,0.4)' },
                  '& .MuiInput-underline:after': { borderColor: 'white' },
                }}
              />
              <IconButton size="small" onClick={handleSaveEdit} disabled={saving} sx={{ color: 'white', p: 0.25 }}>
                {saving ? <CircularProgress size={14} color="inherit" /> : <CheckIcon fontSize="small" />}
              </IconButton>
              <IconButton size="small" onClick={() => { setEditing(false); setEditText(message.text) }} sx={{ color: 'white', p: 0.25 }}>
                <CloseIcon fontSize="small" />
              </IconButton>
            </Box>
          ) : (
            <Typography variant="body2" sx={{ whiteSpace: 'pre-wrap', wordBreak: 'break-word', lineHeight: 1.6 }}>
              {message.text}
            </Typography>
          )}
        </Box>

        {/* Meta row */}
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mt: 0.25, justifyContent: isMine ? 'flex-end' : 'flex-start' }}>
          <Typography variant="caption" sx={{ color: '#9CA3AF', fontSize: 10 }}>
            {time}
          </Typography>
          {message.isEdited && (
            <Typography variant="caption" sx={{ color: '#9CA3AF', fontSize: 10 }}>
              · {t('chat.edited')}
            </Typography>
          )}

          {/* Own message actions */}
          {isMine && !isClosed && !editing && (
            <>
              <Tooltip title={t('chat.edit')}>
                <IconButton size="small" onClick={() => setEditing(true)} sx={{ p: 0.25, color: '#9CA3AF', '&:hover': { color: CORAL } }}>
                  <EditIcon sx={{ fontSize: 13 }} />
                </IconButton>
              </Tooltip>
              <Tooltip title={t('chat.delete')}>
                <IconButton size="small" onClick={handleDelete} disabled={deleting} sx={{ p: 0.25, color: '#9CA3AF', '&:hover': { color: '#DC2626' } }}>
                  {deleting ? <CircularProgress size={11} /> : <DeleteIcon sx={{ fontSize: 13 }} />}
                </IconButton>
              </Tooltip>
            </>
          )}
        </Box>
      </Box>
    </Box>
  )
}

// ── Main page ──────────────────────────────────────────────

export default function DiscussionPage() {
  const { t } = useTranslation()
  const { discussionId } = useParams<{ discussionId: string }>()
  const navigate = useLangNavigate()
  const { user } = useAuthStore()
  const bottomRef = useRef<HTMLDivElement>(null)
  const [text, setText] = useState('')

  const { data: discussion, isLoading, isError } = useGetDiscussionQuery(
    discussionId ?? '',
    { skip: !discussionId, pollingInterval: POLL_INTERVAL },
  )

  const [addMessage, { isLoading: sending }] = useAddMessageMutation()

  // Scroll to bottom when messages change
  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [discussion?.messages.length])

  const handleSend = async () => {
    const trimmed = text.trim()
    if (!trimmed || !discussionId) return
    setText('')
    await addMessage({ discussionId, text: trimmed })
  }

  if (!user) {
    return (
      <Container maxWidth="sm" sx={{ py: 8, textAlign: 'center' }}>
        <Alert severity="error">{t('profile.notLoggedIn')}</Alert>
      </Container>
    )
  }

  if (isLoading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '60vh' }}>
        <CircularProgress sx={{ color: CORAL }} />
      </Box>
    )
  }

  if (isError || !discussion) {
    return (
      <Container maxWidth="sm" sx={{ py: 8 }}>
        <Alert severity="error" sx={{ mb: 2 }}>{t('chat.notFound')}</Alert>
        <Button startIcon={<ArrowBackIcon />} onClick={() => navigate(-1)} sx={{ color: CORAL, textTransform: 'none' }}>
          {t('common.back')}
        </Button>
      </Container>
    )
  }

  const isClosed = discussion.isClosed
  const messages = [...discussion.messages].sort(
    (a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime()
  )

  return (
    <Box sx={{ bgcolor: '#F3F4F6', minHeight: 'calc(100vh - 120px)', display: 'flex', flexDirection: 'column' }}>
      {/* Header */}
      <Paper elevation={0} sx={{ borderBottom: '1px solid #E5E7EB', borderRadius: 0, position: 'sticky', top: 0, zIndex: 10 }}>
        <Container maxWidth="md">
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5, py: 1.5 }}>
            <IconButton size="small" onClick={() => navigate(-1)} sx={{ color: '#6B7280' }}>
              <ArrowBackIcon />
            </IconButton>
            <Box sx={{ flex: 1 }}>
              <Typography variant="subtitle1" fontWeight="bold">{t('chat.title')}</Typography>
              <Typography variant="caption" color="text.secondary">
                {t('chat.pollHint')}
              </Typography>
            </Box>
            {isClosed && (
              <Chip
                icon={<LockIcon sx={{ fontSize: '14px !important' }} />}
                label={t('chat.closed')}
                size="small"
                sx={{ bgcolor: '#F3F4F6', color: '#6B7280', border: '1px solid #E5E7EB' }}
              />
            )}
          </Box>
        </Container>
      </Paper>

      {/* Messages area */}
      <Box sx={{ flex: 1, overflow: 'auto' }}>
        <Container maxWidth="md" sx={{ py: 3 }}>
          {messages.length === 0 ? (
            <Box sx={{ textAlign: 'center', py: 8 }}>
              <Typography color="text.secondary">{t('chat.empty')}</Typography>
            </Box>
          ) : (
            messages.map((msg) => (
              <MessageBubble
                key={msg.id}
                message={msg}
                isMine={msg.userId === user.id}
                discussionId={discussion.id}
                isClosed={isClosed}
              />
            ))
          )}
          <div ref={bottomRef} />
        </Container>
      </Box>

      {/* Input */}
      <Paper elevation={0} sx={{ borderTop: '1px solid #E5E7EB', borderRadius: 0, position: 'sticky', bottom: 0 }}>
        <Container maxWidth="md">
          {isClosed ? (
            <Box sx={{ py: 1.5, textAlign: 'center' }}>
              <Typography variant="body2" color="text.secondary" sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 0.5 }}>
                <LockIcon sx={{ fontSize: 15 }} /> {t('chat.closedHint')}
              </Typography>
            </Box>
          ) : (
            <Box sx={{ display: 'flex', gap: 1, py: 1.5, alignItems: 'flex-end' }}>
              <TextField
                value={text}
                onChange={(e) => setText(e.target.value)}
                onKeyDown={(e) => { if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); handleSend() } }}
                placeholder={t('chat.placeholder')}
                multiline
                maxRows={4}
                fullWidth
                size="small"
                sx={{
                  '& .MuiOutlinedInput-root': { borderRadius: 3, bgcolor: 'white' },
                  '& .MuiOutlinedInput-root.Mui-focused .MuiOutlinedInput-notchedOutline': { borderColor: CORAL },
                }}
              />
              <IconButton
                onClick={handleSend}
                disabled={!text.trim() || sending}
                sx={{
                  bgcolor: CORAL,
                  color: 'white',
                  width: 40,
                  height: 40,
                  flexShrink: 0,
                  '&:hover': { bgcolor: '#e55555' },
                  '&.Mui-disabled': { bgcolor: '#E5E7EB', color: '#9CA3AF' },
                }}
              >
                {sending ? <CircularProgress size={18} color="inherit" /> : <SendIcon sx={{ fontSize: 18 }} />}
              </IconButton>
            </Box>
          )}
        </Container>
      </Paper>
    </Box>
  )
}
