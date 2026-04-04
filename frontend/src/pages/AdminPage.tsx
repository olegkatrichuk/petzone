import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useParams } from 'react-router-dom'
import { useLangNavigate } from '../hooks/useLangNavigate'
import { DEFAULT_LANG } from '../lib/langUtils'
import { useAuthStore } from '../store/authStore'
import PageMeta from '../components/meta/PageMeta'
import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Typography from '@mui/material/Typography'
import Tabs from '@mui/material/Tabs'
import Tab from '@mui/material/Tab'
import Paper from '@mui/material/Paper'
import Chip from '@mui/material/Chip'
import Button from '@mui/material/Button'
import IconButton from '@mui/material/IconButton'
import Alert from '@mui/material/Alert'
import CircularProgress from '@mui/material/CircularProgress'
import Dialog from '@mui/material/Dialog'
import DialogTitle from '@mui/material/DialogTitle'
import DialogContent from '@mui/material/DialogContent'
import DialogActions from '@mui/material/DialogActions'
import TextField from '@mui/material/TextField'
import Divider from '@mui/material/Divider'
import Tooltip from '@mui/material/Tooltip'
import Select from '@mui/material/Select'
import MenuItem from '@mui/material/MenuItem'
import FormControl from '@mui/material/FormControl'
import InputLabel from '@mui/material/InputLabel'
import AssignmentIcon from '@mui/icons-material/Assignment'
import CheckCircleIcon from '@mui/icons-material/CheckCircle'
import EditNoteIcon from '@mui/icons-material/EditNote'
import CancelIcon from '@mui/icons-material/Cancel'
import ChatIcon from '@mui/icons-material/Chat'
import PersonIcon from '@mui/icons-material/Person'
import WorkHistoryIcon from '@mui/icons-material/WorkHistory'
import CardMembershipIcon from '@mui/icons-material/CardMembership'
import AccountBalanceWalletIcon from '@mui/icons-material/AccountBalanceWallet'
import { VolunteerRequestStatus } from '../types/volunteerRequest'
import type { VolunteerRequestDto } from '../types/volunteerRequest'
import Table from '@mui/material/Table'
import TableBody from '@mui/material/TableBody'
import TableCell from '@mui/material/TableCell'
import TableHead from '@mui/material/TableHead'
import TableRow from '@mui/material/TableRow'
import InputAdornment from '@mui/material/InputAdornment'
import SearchIcon from '@mui/icons-material/Search'
import BlockIcon from '@mui/icons-material/Block'
import CheckCircleOutlineIcon from '@mui/icons-material/CheckCircleOutline'
import {
  useGetStatsQuery,
  useGetUnreviewedRequestsQuery,
  useGetAdminRequestsQuery,
  useTakeOnReviewMutation,
  useApproveRequestMutation,
  useSendForRevisionMutation,
  useRejectRequestMutation,
  useGetUsersQuery,
  useBanUserMutation,
  useUnbanUserMutation,
} from '../services/adminApi'
import type { VolunteerRequestStats } from '../services/adminApi'
import { useGetUserByIdQuery } from '../services/accountsApi'
import Pagination from '../components/ui/Pagination'
import Collapse from '@mui/material/Collapse'
import List from '@mui/material/List'
import ListItem from '@mui/material/ListItem'
import AddIcon from '@mui/icons-material/Add'
import DeleteIcon from '@mui/icons-material/Delete'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import ExpandLessIcon from '@mui/icons-material/ExpandLess'
import PetsIcon from '@mui/icons-material/Pets'
import {
  useGetSpeciesQuery,
  useGetBreedsQuery,
  useCreateSpeciesMutation,
  useDeleteSpeciesMutation,
  useCreateBreedMutation,
  useDeleteBreedMutation,
} from '../services/speciesApi'
import { SPECIES_LOCALES } from '../types/species'

const CORAL = '#FF6B6B'
const PAGE_SIZE = 10

// ── Status chip ────────────────────────────────────────────

const STATUS_CFG: Record<VolunteerRequestStatus, { label: string; bg: string; color: string }> = {
  [VolunteerRequestStatus.Submitted]:       { label: 'Надіслано',           bg: '#EFF6FF', color: '#2563EB' },
  [VolunteerRequestStatus.OnReview]:        { label: 'На розгляді',          bg: '#FEF9C3', color: '#CA8A04' },
  [VolunteerRequestStatus.RevisionRequired]:{ label: 'Потрібні зміни',       bg: '#FFF7ED', color: '#EA580C' },
  [VolunteerRequestStatus.Rejected]:        { label: 'Відхилено',            bg: '#FEF2F2', color: '#DC2626' },
  [VolunteerRequestStatus.Approved]:        { label: 'Схвалено',             bg: '#F0FDF4', color: '#16A34A' },
}

// ── Stats panel ────────────────────────────────────────────

interface StatCardProps { label: string; value: number; color: string; bg: string }

function StatCard({ label, value, color, bg }: StatCardProps) {
  return (
    <Paper elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 3, p: 2.5, textAlign: 'center', flex: 1, minWidth: 100 }}>
      <Typography variant="h4" fontWeight="bold" sx={{ color }}>{value}</Typography>
      <Typography variant="caption" color="text.secondary" sx={{ mt: 0.5, display: 'block', lineHeight: 1.3 }}>{label}</Typography>
      <Box sx={{ mt: 1, height: 3, borderRadius: 2, bgcolor: bg }} />
    </Paper>
  )
}

function StatsPanel({ stats }: { stats: VolunteerRequestStats }) {
  const { t } = useTranslation()
  return (
    <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap', mb: 4 }}>
      <StatCard label={t('admin.stats.total')} value={stats.total} color="#1F2937" bg="#E5E7EB" />
      <StatCard label={t('admin.stats.submitted')} value={stats.submitted} color="#2563EB" bg="#BFDBFE" />
      <StatCard label={t('admin.stats.onReview')} value={stats.onReview} color="#CA8A04" bg="#FEF08A" />
      <StatCard label={t('admin.stats.revisionRequired')} value={stats.revisionRequired} color="#EA580C" bg="#FED7AA" />
      <StatCard label={t('admin.stats.approved')} value={stats.approved} color="#16A34A" bg="#BBF7D0" />
      <StatCard label={t('admin.stats.rejected')} value={stats.rejected} color="#DC2626" bg="#FECACA" />
    </Box>
  )
}

function StatusChip({ status }: { status: VolunteerRequestStatus }) {
  const cfg = STATUS_CFG[status]
  return (
    <Chip
      label={cfg.label}
      size="small"
      sx={{ bgcolor: cfg.bg, color: cfg.color, fontWeight: 600, fontSize: 12 }}
    />
  )
}

// ── Comment dialog ─────────────────────────────────────────

interface CommentDialogProps {
  open: boolean
  title: string
  confirmLabel: string
  confirmColor: 'error' | 'warning'
  onClose: () => void
  onConfirm: (comment: string) => void
  loading: boolean
}

function CommentDialog({ open, title, confirmLabel, confirmColor, onClose, onConfirm, loading }: CommentDialogProps) {
  const { t } = useTranslation()
  const [comment, setComment] = useState('')

  const handleConfirm = () => {
    if (!comment.trim()) return
    onConfirm(comment.trim())
  }

  const handleClose = () => {
    setComment('')
    onClose()
  }

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
      <DialogTitle>{title}</DialogTitle>
      <DialogContent sx={{ pt: '16px !important' }}>
        <TextField
          autoFocus
          multiline
          rows={4}
          fullWidth
          label={t('admin.commentLabel')}
          value={comment}
          onChange={(e) => setComment(e.target.value)}
          error={!comment.trim() && comment.length > 0}
          helperText={!comment.trim() && comment.length > 0 ? t('admin.commentRequired') : ''}
          sx={{ '& .MuiOutlinedInput-root.Mui-focused .MuiOutlinedInput-notchedOutline': { borderColor: CORAL } }}
        />
      </DialogContent>
      <DialogActions sx={{ px: 3, pb: 3, gap: 1 }}>
        <Button onClick={handleClose} sx={{ color: '#6B7280', textTransform: 'none' }}>
          {t('common.cancel')}
        </Button>
        <Button
          variant="contained"
          disabled={!comment.trim() || loading}
          onClick={handleConfirm}
          color={confirmColor}
          sx={{ textTransform: 'none', fontWeight: 700, minWidth: 100 }}
        >
          {loading ? <CircularProgress size={18} color="inherit" /> : confirmLabel}
        </Button>
      </DialogActions>
    </Dialog>
  )
}

// ── Request card ───────────────────────────────────────────

function ApplicantInfo({ userId, createdDate }: { userId: string; createdDate: string }) {
  const { t } = useTranslation()
  const { data: userDto } = useGetUserByIdQuery(userId)
  const fullName = userDto ? [userDto.firstName, userDto.lastName].filter(Boolean).join(' ') : null

  return (
    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
      <Box sx={{ width: 40, height: 40, borderRadius: 2, bgcolor: '#F3F4F6', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
        <PersonIcon sx={{ color: '#6B7280' }} />
      </Box>
      <Box>
        {fullName
          ? <Typography variant="body2" fontWeight={600}>{fullName}</Typography>
          : <Typography variant="body2" sx={{ fontFamily: 'monospace', color: '#6B7280', fontSize: 11 }}>ID: {userId.slice(0, 8)}…</Typography>
        }
        {userDto?.email && (
          <Typography variant="caption" color="text.secondary">{userDto.email}</Typography>
        )}
        <Typography variant="caption" color="text.secondary" sx={{ display: 'block' }}>
          {t('admin.submitted')}: {createdDate}
        </Typography>
      </Box>
    </Box>
  )
}

interface RequestCardProps {
  request: VolunteerRequestDto
  showTakeOnReview?: boolean
}

function RequestCard({ request, showTakeOnReview = false }: RequestCardProps) {
  const { t } = useTranslation()
  const navigate = useLangNavigate()
  const [revisionOpen, setRevisionOpen] = useState(false)
  const [rejectOpen, setRejectOpen] = useState(false)

  const [takeOnReview, { isLoading: takingReview }] = useTakeOnReviewMutation()
  const [approve, { isLoading: approving }] = useApproveRequestMutation()
  const [sendForRevision, { isLoading: sendingRevision }] = useSendForRevisionMutation()
  const [rejectRequest, { isLoading: rejecting }] = useRejectRequestMutation()

  const isOnReview = request.status === VolunteerRequestStatus.OnReview
  const createdDate = new Date(request.createdAt).toLocaleDateString('uk-UA', { day: '2-digit', month: '2-digit', year: 'numeric' })

  return (
    <Paper elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 3, p: 3 }}>
      {/* Header row */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 2, gap: 1, flexWrap: 'wrap' }}>
        <ApplicantInfo userId={request.userId} createdDate={createdDate} />
        <StatusChip status={request.status} />
      </Box>

      {/* Volunteer info */}
      <Box sx={{ display: 'flex', gap: 3, flexWrap: 'wrap', mb: 2 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.75 }}>
          <WorkHistoryIcon sx={{ fontSize: 16, color: '#9CA3AF' }} />
          <Typography variant="body2" color="text.secondary">
            {t('admin.experience')}: <strong>{request.volunteerInfo.experience} {t('admin.years')}</strong>
          </Typography>
        </Box>
      </Box>

      {/* Motivation */}
      {request.volunteerInfo.motivation && (
        <Box sx={{ mb: 2, bgcolor: '#F9FAFB', borderRadius: 2, p: 2 }}>
          <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', letterSpacing: 0.5, display: 'block', mb: 0.75 }}>
            {t('admin.motivation')}
          </Typography>
          <Typography variant="body2" sx={{ whiteSpace: 'pre-wrap' }}>
            {request.volunteerInfo.motivation}
          </Typography>
        </Box>
      )}

      {/* Certificates */}
      {request.volunteerInfo.certificates.length > 0 && (
        <Box sx={{ mb: 1.5 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.75, mb: 0.75 }}>
            <CardMembershipIcon sx={{ fontSize: 15, color: '#9CA3AF' }} />
            <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', letterSpacing: 0.5 }}>
              {t('applications.certificates')}
            </Typography>
          </Box>
          <Box sx={{ display: 'flex', gap: 0.75, flexWrap: 'wrap' }}>
            {request.volunteerInfo.certificates.map((cert, i) => (
              <Chip key={i} label={cert} size="small" variant="outlined" sx={{ borderColor: '#E5E7EB', fontSize: 12 }} />
            ))}
          </Box>
        </Box>
      )}

      {/* Requisites */}
      {request.volunteerInfo.requisites.length > 0 && (
        <Box sx={{ mb: 1.5 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.75, mb: 0.75 }}>
            <AccountBalanceWalletIcon sx={{ fontSize: 15, color: '#9CA3AF' }} />
            <Typography variant="caption" color="text.secondary" sx={{ textTransform: 'uppercase', letterSpacing: 0.5 }}>
              {t('applications.requisites')}
            </Typography>
          </Box>
          <Box sx={{ display: 'flex', gap: 0.75, flexWrap: 'wrap' }}>
            {request.volunteerInfo.requisites.map((req, i) => (
              <Chip key={i} label={req} size="small" variant="outlined" sx={{ borderColor: '#E5E7EB', fontSize: 12 }} />
            ))}
          </Box>
        </Box>
      )}

      {/* Rejection comment */}
      {request.rejectionComment && (
        <Alert severity="warning" sx={{ mb: 1.5, fontSize: 13 }}>
          <strong>{t('applications.comment')}:</strong> {request.rejectionComment}
        </Alert>
      )}

      <Divider sx={{ mb: 2 }} />

      {/* Actions */}
      <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
        {showTakeOnReview && (
          <Button
            variant="contained"
            size="small"
            startIcon={takingReview ? <CircularProgress size={14} color="inherit" /> : <AssignmentIcon />}
            disabled={takingReview}
            onClick={() => takeOnReview(request.id)}
            sx={{ bgcolor: CORAL, '&:hover': { bgcolor: '#e55555' }, textTransform: 'none', fontWeight: 600 }}
          >
            {t('admin.takeOnReview')}
          </Button>
        )}

        {isOnReview && (
          <>
            <Tooltip title={t('admin.approve')}>
              <Button
                variant="contained"
                size="small"
                color="success"
                startIcon={approving ? <CircularProgress size={14} color="inherit" /> : <CheckCircleIcon />}
                disabled={approving}
                onClick={() => approve(request.id)}
                sx={{ textTransform: 'none', fontWeight: 600 }}
              >
                {t('admin.approve')}
              </Button>
            </Tooltip>

            <Tooltip title={t('admin.sendForRevision')}>
              <IconButton
                size="small"
                color="warning"
                disabled={sendingRevision}
                onClick={() => setRevisionOpen(true)}
                sx={{ border: '1px solid', borderColor: 'warning.main' }}
              >
                <EditNoteIcon fontSize="small" />
              </IconButton>
            </Tooltip>

            <Tooltip title={t('admin.reject')}>
              <IconButton
                size="small"
                color="error"
                disabled={rejecting}
                onClick={() => setRejectOpen(true)}
                sx={{ border: '1px solid', borderColor: 'error.main' }}
              >
                <CancelIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          </>
        )}

        {/* Chat button — visible when discussion exists */}
        {request.discussionId && (
          <Button
            variant="outlined"
            size="small"
            startIcon={<ChatIcon />}
            onClick={() => navigate(`/discussion/${request.discussionId}`)}
            sx={{ borderColor: '#6B7280', color: '#6B7280', textTransform: 'none', '&:hover': { borderColor: CORAL, color: CORAL, bgcolor: '#FFF0F0' } }}
          >
            {t('chat.openChat')}
          </Button>
        )}
      </Box>

      {/* Dialogs */}
      <CommentDialog
        open={revisionOpen}
        title={t('admin.revisionDialogTitle')}
        confirmLabel={t('admin.sendForRevision')}
        confirmColor="warning"
        loading={sendingRevision}
        onClose={() => setRevisionOpen(false)}
        onConfirm={(comment) => {
          sendForRevision({ requestId: request.id, comment }).then(() => setRevisionOpen(false))
        }}
      />
      <CommentDialog
        open={rejectOpen}
        title={t('admin.rejectDialogTitle')}
        confirmLabel={t('admin.reject')}
        confirmColor="error"
        loading={rejecting}
        onClose={() => setRejectOpen(false)}
        onConfirm={(comment) => {
          rejectRequest({ requestId: request.id, comment }).then(() => setRejectOpen(false))
        }}
      />
    </Paper>
  )
}

// ── Unreviewed tab ──────────────────────────────────────────

function UnreviewedTab() {
  const { t } = useTranslation()
  const [page, setPage] = useState(1)

  const { data, isLoading, isError, refetch } = useGetUnreviewedRequestsQuery({ page, pageSize: PAGE_SIZE })

  if (isLoading) {
    return <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}><CircularProgress sx={{ color: CORAL }} /></Box>
  }

  if (isError) {
    return (
      <Alert severity="error" action={<Button size="small" onClick={refetch} sx={{ color: CORAL, textTransform: 'none' }}>{t('errors.retry')}</Button>}>
        {t('admin.loadError')}
      </Alert>
    )
  }

  const items = data?.items ?? []

  if (items.length === 0) {
    return (
      <Box sx={{ textAlign: 'center', py: 10 }}>
        <CheckCircleIcon sx={{ fontSize: 56, color: '#22C55E', mb: 2 }} />
        <Typography variant="h6" color="text.secondary">{t('admin.noUnreviewed')}</Typography>
      </Box>
    )
  }

  return (
    <>
      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
        {items.map((req) => (
          <RequestCard key={req.id} request={req} showTakeOnReview />
        ))}
      </Box>
      {data && (
        <Pagination page={page} pageSize={PAGE_SIZE} totalCount={data.totalCount} onChange={setPage} ofLabel={t('volunteers.of')} />
      )}
    </>
  )
}

// ── Admin's requests tab ───────────────────────────────────

const ADMIN_STATUS_OPTIONS = [
  VolunteerRequestStatus.OnReview,
  VolunteerRequestStatus.RevisionRequired,
  VolunteerRequestStatus.Approved,
  VolunteerRequestStatus.Rejected,
]

function MyRequestsTab() {
  const { t } = useTranslation()
  const [page, setPage] = useState(1)
  const [statusFilter, setStatusFilter] = useState<VolunteerRequestStatus>(VolunteerRequestStatus.OnReview)

  const { data, isLoading, isError, refetch } = useGetAdminRequestsQuery({ page, pageSize: PAGE_SIZE, status: statusFilter })

  const handleStatusChange = (newStatus: VolunteerRequestStatus) => {
    setStatusFilter(newStatus)
    setPage(1)
  }

  if (isLoading) {
    return <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}><CircularProgress sx={{ color: CORAL }} /></Box>
  }

  if (isError) {
    return (
      <Alert severity="error" action={<Button size="small" onClick={refetch} sx={{ color: CORAL, textTransform: 'none' }}>{t('errors.retry')}</Button>}>
        {t('admin.loadError')}
      </Alert>
    )
  }

  const items = data?.items ?? []

  return (
    <>
      {/* Status filter */}
      <FormControl size="small" sx={{ mb: 3, minWidth: 200 }}>
        <InputLabel>{t('admin.filterByStatus')}</InputLabel>
        <Select
          value={statusFilter}
          label={t('admin.filterByStatus')}
          onChange={(e) => handleStatusChange(e.target.value as VolunteerRequestStatus)}
        >
          {ADMIN_STATUS_OPTIONS.map((s) => (
            <MenuItem key={s} value={s}>{STATUS_CFG[s].label}</MenuItem>
          ))}
        </Select>
      </FormControl>

      {items.length === 0 ? (
        <Box sx={{ textAlign: 'center', py: 8 }}>
          <Typography color="text.secondary">{t('admin.noRequests')}</Typography>
        </Box>
      ) : (
        <>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
            {items.map((req) => (
              <RequestCard key={req.id} request={req} />
            ))}
          </Box>
          {data && (
            <Pagination page={page} pageSize={PAGE_SIZE} totalCount={data.totalCount} onChange={setPage} ofLabel={t('volunteers.of')} />
          )}
        </>
      )}
    </>
  )
}

// ── Users tab ──────────────────────────────────────────────

const ROLE_COLORS: Record<string, { bg: string; color: string }> = {
  Admin:       { bg: '#EDE9FE', color: '#7C3AED' },
  Volunteer:   { bg: '#DBEAFE', color: '#2563EB' },
  Participant: { bg: '#F3F4F6', color: '#4B5563' },
}

const PAGE_SIZE_USERS = 15

function UsersTab() {
  const { t } = useTranslation()
  const [page, setPage] = useState(1)
  const [search, setSearch] = useState('')
  const [debouncedSearch, setDebouncedSearch] = useState('')

  // simple debounce via state
  const handleSearch = (value: string) => {
    setSearch(value)
    setPage(1)
    clearTimeout((handleSearch as any)._t)
    ;(handleSearch as any)._t = setTimeout(() => setDebouncedSearch(value), 400)
  }

  const { data, isLoading, isError } = useGetUsersQuery({ page, pageSize: PAGE_SIZE_USERS, search: debouncedSearch || undefined })
  const [ban, { isLoading: banning }] = useBanUserMutation()
  const [unban, { isLoading: unbanning }] = useUnbanUserMutation()

  if (isLoading) {
    return <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}><CircularProgress sx={{ color: CORAL }} /></Box>
  }

  if (isError) {
    return <Alert severity="error">{t('admin.users.loadError')}</Alert>
  }

  const users = data?.items ?? []

  return (
    <>
      <TextField
        size="small"
        placeholder={t('admin.users.search')}
        value={search}
        onChange={(e) => handleSearch(e.target.value)}
        sx={{ mb: 3, maxWidth: 360 }}
        InputProps={{
          startAdornment: (
            <InputAdornment position="start">
              <SearchIcon sx={{ color: '#9CA3AF', fontSize: 20 }} />
            </InputAdornment>
          ),
        }}
      />

      {users.length === 0 ? (
        <Box sx={{ textAlign: 'center', py: 8 }}>
          <Typography color="text.secondary">{t('admin.users.noResults')}</Typography>
        </Box>
      ) : (
        <Box sx={{ overflowX: 'auto' }}>
          <Table size="small" sx={{ minWidth: 600 }}>
            <TableHead>
              <TableRow sx={{ '& th': { fontWeight: 700, color: '#374151', bgcolor: '#F9FAFB', borderBottom: '2px solid #E5E7EB' } }}>
                <TableCell>{t('admin.users.name')}</TableCell>
                <TableCell>{t('admin.users.email')}</TableCell>
                <TableCell>{t('admin.users.role')}</TableCell>
                <TableCell>{t('admin.users.status')}</TableCell>
                <TableCell align="right"></TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {users.map((user) => {
                const roleCfg = ROLE_COLORS[user.role] ?? ROLE_COLORS.Participant
                return (
                  <TableRow key={user.id} hover sx={{ '& td': { borderColor: '#F3F4F6' } }}>
                    <TableCell>
                      <Typography variant="body2" fontWeight={500}>
                        {[user.firstName, user.lastName].filter(Boolean).join(' ') || '—'}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2" color="text.secondary">{user.email}</Typography>
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={user.role}
                        size="small"
                        sx={{ bgcolor: roleCfg.bg, color: roleCfg.color, fontWeight: 600, fontSize: 11 }}
                      />
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={user.isLocked ? t('admin.users.banned') : t('admin.users.active')}
                        size="small"
                        sx={{
                          bgcolor: user.isLocked ? '#FEE2E2' : '#D1FAE5',
                          color: user.isLocked ? '#DC2626' : '#059669',
                          fontWeight: 600, fontSize: 11,
                        }}
                      />
                    </TableCell>
                    <TableCell align="right">
                      {user.role !== 'Admin' && (
                        user.isLocked ? (
                          <Tooltip title={t('admin.users.unban')}>
                            <IconButton
                              size="small"
                              color="success"
                              disabled={unbanning}
                              onClick={() => unban(user.id)}
                              sx={{ border: '1px solid', borderColor: 'success.main' }}
                            >
                              <CheckCircleOutlineIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                        ) : (
                          <Tooltip title={t('admin.users.ban')}>
                            <IconButton
                              size="small"
                              color="error"
                              disabled={banning}
                              onClick={() => ban(user.id)}
                              sx={{ border: '1px solid', borderColor: 'error.main' }}
                            >
                              <BlockIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                        )
                      )}
                    </TableCell>
                  </TableRow>
                )
              })}
            </TableBody>
          </Table>
        </Box>
      )}

      {data && data.totalCount > PAGE_SIZE_USERS && (
        <Pagination page={page} pageSize={PAGE_SIZE_USERS} totalCount={data.totalCount} onChange={setPage} ofLabel={t('volunteers.of')} />
      )}
    </>
  )
}

// ── Species tab ────────────────────────────────────────────

interface TranslationDialogProps {
  open: boolean
  titleKey: string
  onClose: () => void
  onConfirm: (translations: Record<string, string>) => void
  loading: boolean
}

function TranslationDialog({ open, titleKey, onClose, onConfirm, loading }: TranslationDialogProps) {
  const { t } = useTranslation()
  const [vals, setVals] = useState<Record<string, string>>({})

  const handleClose = () => {
    setVals({})
    onClose()
  }

  const handleConfirm = () => {
    if (!vals.uk?.trim() && !vals.en?.trim()) return
    onConfirm(Object.fromEntries(Object.entries(vals).filter(([, v]) => v.trim())))
    setVals({})
  }

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
      <DialogTitle>{t(titleKey)}</DialogTitle>
      <DialogContent sx={{ pt: '16px !important', display: 'flex', flexDirection: 'column', gap: 2 }}>
        {SPECIES_LOCALES.map((loc) => (
          <TextField
            key={loc}
            label={loc.toUpperCase()}
            size="small"
            fullWidth
            required={loc === 'uk' || loc === 'en'}
            value={vals[loc] ?? ''}
            onChange={(e) => setVals((prev) => ({ ...prev, [loc]: e.target.value }))}
          />
        ))}
      </DialogContent>
      <DialogActions sx={{ px: 3, pb: 3, gap: 1 }}>
        <Button onClick={handleClose} sx={{ color: '#6B7280', textTransform: 'none' }}>
          {t('common.cancel')}
        </Button>
        <Button
          variant="contained"
          disabled={(!vals.uk?.trim() && !vals.en?.trim()) || loading}
          onClick={handleConfirm}
          sx={{ bgcolor: CORAL, '&:hover': { bgcolor: '#e55555' }, textTransform: 'none', fontWeight: 700 }}
        >
          {loading ? <CircularProgress size={18} color="inherit" /> : t('common.save')}
        </Button>
      </DialogActions>
    </Dialog>
  )
}

function BreedsList({ speciesId, onAdd }: { speciesId: string; onAdd: () => void }) {
  const { t } = useTranslation()
  const { data: breeds = [], isLoading } = useGetBreedsQuery({ speciesId, locale: 'uk' })
  const [deleteBreed, { isLoading: deleting }] = useDeleteBreedMutation()

  if (isLoading) return <Box sx={{ p: 2 }}><CircularProgress size={18} sx={{ color: CORAL }} /></Box>

  return (
    <Box sx={{ pl: 4, borderLeft: '2px solid #F3F4F6', ml: 2 }}>
      <List dense disablePadding>
        {breeds.map((breed) => (
          <ListItem
            key={breed.id}
            disableGutters
            sx={{ py: 0.5, display: 'flex', justifyContent: 'space-between' }}
          >
            <Typography variant="body2" color="text.secondary">• {breed.name}</Typography>
            <IconButton
              size="small"
              disabled={deleting}
              onClick={() => deleteBreed({ speciesId, breedId: breed.id })}
              sx={{ color: '#9CA3AF', '&:hover': { color: '#EF4444' } }}
            >
              <DeleteIcon fontSize="small" />
            </IconButton>
          </ListItem>
        ))}
        {breeds.length === 0 && (
          <Typography variant="caption" color="text.secondary" sx={{ pl: 1 }}>
            {t('admin.species.noBreeds')}
          </Typography>
        )}
      </List>
      <Button
        size="small"
        startIcon={<AddIcon />}
        onClick={onAdd}
        sx={{ textTransform: 'none', color: CORAL, mt: 0.5 }}
      >
        {t('admin.species.addBreed')}
      </Button>
    </Box>
  )
}

function SpeciesTab() {
  const { t } = useTranslation()
  const [expanded, setExpanded] = useState<string | null>(null)
  const [addSpeciesOpen, setAddSpeciesOpen] = useState(false)
  const [addBreedFor, setAddBreedFor] = useState<string | null>(null)

  const { data: species = [], isLoading, isError } = useGetSpeciesQuery('uk')
  const [createSpecies, { isLoading: creatingSpecies }] = useCreateSpeciesMutation()
  const [deleteSpecies, { isLoading: deletingSpecies }] = useDeleteSpeciesMutation()
  const [createBreed, { isLoading: creatingBreed }] = useCreateBreedMutation()

  if (isLoading) return <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}><CircularProgress sx={{ color: CORAL }} /></Box>
  if (isError) return <Alert severity="error">{t('admin.species.loadError')}</Alert>

  return (
    <>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="body2" color="text.secondary">
          {t('admin.species.total', { count: species.length })}
        </Typography>
        <Button
          variant="contained"
          size="small"
          startIcon={<AddIcon />}
          onClick={() => setAddSpeciesOpen(true)}
          sx={{ bgcolor: CORAL, '&:hover': { bgcolor: '#e55555' }, textTransform: 'none', fontWeight: 600 }}
        >
          {t('admin.species.addSpecies')}
        </Button>
      </Box>

      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
        {species.map((sp) => (
          <Paper key={sp.id} elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 2, overflow: 'hidden' }}>
            <Box
              sx={{ display: 'flex', alignItems: 'center', px: 2, py: 1.5, cursor: 'pointer', '&:hover': { bgcolor: '#FAFAFA' } }}
              onClick={() => setExpanded(expanded === sp.id ? null : sp.id)}
            >
              <PetsIcon sx={{ fontSize: 18, color: CORAL, mr: 1.5 }} />
              <Typography fontWeight={600} sx={{ flex: 1 }}>{sp.name}</Typography>
              <Chip
                label={t('admin.species.breedsCount', { count: sp.breedsCount })}
                size="small"
                sx={{ bgcolor: '#F3F4F6', color: '#6B7280', fontSize: 11, mr: 1.5 }}
              />
              <IconButton
                size="small"
                disabled={deletingSpecies}
                onClick={(e) => { e.stopPropagation(); deleteSpecies(sp.id) }}
                sx={{ color: '#9CA3AF', '&:hover': { color: '#EF4444' }, mr: 0.5 }}
              >
                <DeleteIcon fontSize="small" />
              </IconButton>
              {expanded === sp.id ? <ExpandLessIcon sx={{ color: '#9CA3AF' }} /> : <ExpandMoreIcon sx={{ color: '#9CA3AF' }} />}
            </Box>
            <Collapse in={expanded === sp.id}>
              <Box sx={{ px: 2, pb: 2 }}>
                <BreedsList speciesId={sp.id} onAdd={() => setAddBreedFor(sp.id)} />
              </Box>
            </Collapse>
          </Paper>
        ))}
      </Box>

      {/* Add species dialog */}
      <TranslationDialog
        open={addSpeciesOpen}
        titleKey="admin.species.addSpeciesTitle"
        loading={creatingSpecies}
        onClose={() => setAddSpeciesOpen(false)}
        onConfirm={(translations) => createSpecies({ translations }).then(() => setAddSpeciesOpen(false))}
      />

      {/* Add breed dialog */}
      <TranslationDialog
        open={!!addBreedFor}
        titleKey="admin.species.addBreedTitle"
        loading={creatingBreed}
        onClose={() => setAddBreedFor(null)}
        onConfirm={(translations) =>
          createBreed({ speciesId: addBreedFor!, translations }).then(() => setAddBreedFor(null))
        }
      />
    </>
  )
}

// ── Main page ──────────────────────────────────────────────

export default function AdminPage() {
  const { t } = useTranslation()
  const { user } = useAuthStore()
  const navigate = useNavigate()
  const { lang } = useParams<{ lang: string }>()
  const [tab, setTab] = useState(0)
  const { data: stats } = useGetStatsQuery()

  // Guard: only Admin role
  if (!user || user.role !== 'Admin') {
    return (
      <Container maxWidth="sm" sx={{ py: 8, textAlign: 'center' }}>
        <Alert severity="error" sx={{ mb: 3 }}>{t('admin.accessDenied')}</Alert>
        <Button onClick={() => navigate(`/${lang ?? DEFAULT_LANG}`)} sx={{ color: CORAL, textTransform: 'none' }}>
          {t('notFound.goHome')}
        </Button>
      </Container>
    )
  }

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <PageMeta title={t('admin.pageTitle')} description={t('admin.pageTitle')} path="/admin" noIndex />

      <Box sx={{ mb: 3 }}>
        <Typography variant="h4" fontWeight="bold" sx={{ mb: 0.5 }}>
          {t('admin.pageTitle')}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          {t('admin.pageSubtitle')}
        </Typography>
      </Box>

      {stats && <StatsPanel stats={stats} />}

      <Tabs
        value={tab}
        onChange={(_, v) => setTab(v)}
        sx={{
          mb: 3,
          borderBottom: '1px solid #E5E7EB',
          '& .MuiTab-root': { textTransform: 'none', fontWeight: 500 },
          '& .Mui-selected': { color: `${CORAL} !important` },
          '& .MuiTabs-indicator': { bgcolor: CORAL },
        }}
      >
        <Tab label={t('admin.tabUnreviewed')} />
        <Tab label={t('admin.tabMine')} />
        <Tab label={t('admin.tabUsers')} />
        <Tab label={t('admin.tabSpecies')} />
      </Tabs>

      {tab === 0 && <UnreviewedTab />}
      {tab === 1 && <MyRequestsTab />}
      {tab === 2 && <UsersTab />}
      {tab === 3 && <SpeciesTab />}
    </Container>
  )
}
