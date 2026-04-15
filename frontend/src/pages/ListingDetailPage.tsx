import { useRef, useState, useEffect } from 'react'
import { useParams } from 'react-router-dom'
import { Helmet } from 'react-helmet-async'
import { useTranslation } from 'react-i18next'
import { useLangNavigate } from '../hooks/useLangNavigate'
import PageMeta from '../components/meta/PageMeta'
import ShareButton from '../components/ui/ShareButton'
import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Paper from '@mui/material/Paper'
import Typography from '@mui/material/Typography'
import Button from '@mui/material/Button'
import IconButton from '@mui/material/IconButton'
import Chip from '@mui/material/Chip'
import Divider from '@mui/material/Divider'
import CircularProgress from '@mui/material/CircularProgress'
import Dialog from '@mui/material/Dialog'
import DialogTitle from '@mui/material/DialogTitle'
import DialogActions from '@mui/material/DialogActions'
import Snackbar from '@mui/material/Snackbar'
import Alert from '@mui/material/Alert'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import EditIcon from '@mui/icons-material/Edit'
import LocationOnIcon from '@mui/icons-material/LocationOn'
import PhoneIcon from '@mui/icons-material/Phone'
import EmailIcon from '@mui/icons-material/Email'
import AddPhotoAlternateIcon from '@mui/icons-material/AddPhotoAlternate'
import DeleteIcon from '@mui/icons-material/Delete'
import {
  useGetListingByIdQuery,
  useMarkAdoptedMutation,
  useDeleteListingMutation,
  useAddListingPhotoMutation,
  useRemoveListingPhotoMutation,
} from '../services/listingsApi'
import { useAuthStore } from '../store/authStore'
import { getApiError } from '../lib/getApiError'
import { api } from '../lib/axios'

const CORAL = '#FF6B6B'
const MINIO_BASE = '/files'

function getPhotoUrl(fileName: string) {
  return `${MINIO_BASE}/${encodeURIComponent(fileName)}/url`
}

function ListingPhoto({ fileName, canDelete, onDelete, alt }: {
  fileName: string
  canDelete: boolean
  onDelete: (name: string) => void
  alt?: string
}) {
  const [url, setUrl] = useState<string | null>(null)

  useEffect(() => {
    api.get(getPhotoUrl(fileName))
      .then(res => {
        const data = res.data
        setUrl(typeof data === 'string' ? data : data?.result ?? null)
      })
      .catch(() => setUrl(null))
  }, [fileName])

  if (!url) return (
    <Box sx={{
      width: '100%', aspectRatio: '4/3', bgcolor: 'action.hover',
      borderRadius: 2, display: 'flex', alignItems: 'center', justifyContent: 'center',
    }}>
      <CircularProgress size={24} sx={{ color: CORAL }} />
    </Box>
  )

  return (
    <Box sx={{ position: 'relative', borderRadius: 2, overflow: 'hidden', aspectRatio: '4/3' }}>
      <img src={url} alt={alt ?? fileName} style={{ width: '100%', height: '100%', objectFit: 'cover', display: 'block' }} />
      {canDelete && (
        <IconButton
          size="small"
          onClick={() => onDelete(fileName)}
          sx={{
            position: 'absolute', top: 4, right: 4,
            bgcolor: 'rgba(0,0,0,0.5)', color: '#fff',
            '&:hover': { bgcolor: 'rgba(220,38,38,0.85)' },
          }}
        >
          <DeleteIcon fontSize="small" />
        </IconButton>
      )}
    </Box>
  )
}

export default function ListingDetailPage() {
  const { listingId } = useParams<{ listingId: string }>()
  const { t } = useTranslation()
  const navigate = useLangNavigate()
  const { user } = useAuthStore()
  const fileInputRef = useRef<HTMLInputElement>(null)
  const [toast, setToast] = useState<string | null>(null)
  const [confirmDelete, setConfirmDelete] = useState(false)
  const [uploading, setUploading] = useState(false)

  const { data: listing, isLoading, isError } = useGetListingByIdQuery(listingId ?? '')
  const [markAdopted] = useMarkAdoptedMutation()
  const [deleteListing] = useDeleteListingMutation()
  const [addPhoto] = useAddListingPhotoMutation()
  const [removePhoto] = useRemoveListingPhotoMutation()

  const isOwner = !!(user && listing && user.id === listing.userId)

  const ageLabel = !listing ? '' : listing.ageMonths < 1
    ? t('pets.ageLessThanMonth')
    : listing.ageMonths < 12
      ? `${listing.ageMonths} ${t('pets.ageMonths')}`
      : `${Math.floor(listing.ageMonths / 12)} ${t('pets.ageYears')}`

  const handleMarkAdopted = async () => {
    if (!listingId) return
    try {
      await markAdopted(listingId).unwrap()
    } catch (err) {
      setToast(getApiError(err, t))
    }
  }

  const handleDelete = async () => {
    if (!listingId) return
    try {
      await deleteListing(listingId).unwrap()
      navigate('/pets')
    } catch (err) {
      setConfirmDelete(false)
      setToast(getApiError(err, t))
    }
  }

  const handlePhotoUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    if (!file || !listingId) return
    if (!file.type.startsWith('image/')) {
      setToast(t('common.invalidImage'))
      return
    }
    setUploading(true)
    try {
      const formData = new FormData()
      formData.append('file', file)
      const uploadRes = await api.post('/files/upload', formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
      const fileName: string = uploadRes.data?.result ?? uploadRes.data
      await addPhoto({ id: listingId, fileName }).unwrap()
    } catch (err) {
      setToast(getApiError(err, t))
    } finally {
      setUploading(false)
      if (fileInputRef.current) fileInputRef.current.value = ''
    }
  }

  const handleRemovePhoto = async (fileName: string) => {
    if (!listingId) return
    try {
      await removePhoto({ id: listingId, fileName }).unwrap()
    } catch (err) {
      setToast(getApiError(err, t))
    }
  }

  if (isLoading) {
    return (
      <Box sx={{ minHeight: '60vh', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
        <CircularProgress sx={{ color: CORAL }} />
      </Box>
    )
  }

  if (isError || !listing) {
    return (
      <Box sx={{ minHeight: '60vh', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
        <Typography color="text.secondary">{t('errors.unknown')}</Typography>
      </Box>
    )
  }

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'background.default', py: 4 }}>
      <PageMeta
        title={listing.title}
        description={listing.description}
        path={`/listings/${listing.id}`}
        image={listing.photos[0] ? `/api/files/${encodeURIComponent(listing.photos[0])}/redirect` : undefined}
        type="article"
        noIndex={listing.status !== 'Active'}
      />
      <script type="application/ld+json" dangerouslySetInnerHTML={{ __html: JSON.stringify({
        '@context': 'https://schema.org',
        '@type': 'ItemPage',
        name: listing.title,
        description: listing.description,
        url: `https://getpetzone.com/uk/listings/${listing.id}`,
        author: { '@type': 'Person', name: listing.userName, email: listing.userEmail },
        locationCreated: { '@type': 'Place', name: listing.city },
        datePublished: listing.createdAt,
      }) }} />
      <Helmet>
        <script type="application/ld+json">{JSON.stringify({
          '@context': 'https://schema.org',
          '@type': 'BreadcrumbList',
          itemListElement: [
            { '@type': 'ListItem', position: 1, name: t('nav.home'), item: 'https://getpetzone.com/uk' },
            { '@type': 'ListItem', position: 2, name: t('nav.listings'), item: 'https://getpetzone.com/uk/listings' },
            { '@type': 'ListItem', position: 3, name: listing.title },
          ],
        })}</script>
      </Helmet>
      <Container maxWidth="sm">
        <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/pets')}
          sx={{ mb: 3, color: '#6B7280', textTransform: 'none' }}>
          {t('common.back')}
        </Button>

        <Paper elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 4, p: 4 }}>
          {/* Photos grid */}
          {(listing.photos.length > 0 || isOwner) && (
            <Box sx={{ mb: 3 }}>
              {listing.photos.length > 0 && (
                <Box sx={{
                  display: 'grid',
                  gridTemplateColumns: listing.photos.length === 1 ? '1fr' : 'repeat(2, 1fr)',
                  gap: 1, mb: isOwner ? 1.5 : 0,
                }}>
                  {listing.photos.map((photo, i) => (
                    <ListingPhoto
                      key={photo}
                      fileName={photo}
                      alt={`${listing.title} — фото ${i + 1}`}
                      canDelete={isOwner && listing.status === 'Active'}
                      onDelete={handleRemovePhoto}
                    />
                  ))}
                </Box>
              )}

              {isOwner && listing.status === 'Active' && listing.photos.length < 5 && (
                <>
                  <input
                    ref={fileInputRef}
                    type="file"
                    accept="image/*"
                    style={{ display: 'none' }}
                    onChange={handlePhotoUpload}
                  />
                  <Button
                    variant="outlined"
                    startIcon={uploading ? <CircularProgress size={16} /> : <AddPhotoAlternateIcon />}
                    disabled={uploading}
                    onClick={() => fileInputRef.current?.click()}
                    sx={{ borderColor: CORAL, color: CORAL, textTransform: 'none', borderRadius: 2, fontSize: 13 }}
                  >
                    {uploading ? t('listings.photoUploading') : t('listings.addPhotoBtn')}
                  </Button>
                  <Typography variant="caption" color="text.secondary" sx={{ ml: 1 }}>
                    {t('listings.photoHint')}
                  </Typography>
                </>
              )}
            </Box>
          )}

          <Box sx={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', gap: 1, mb: 2 }}>
            <Typography variant="h1" fontSize="1.5rem" fontWeight="bold">{listing.title}</Typography>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, flexShrink: 0 }}>
              <ShareButton title={listing.title} text={listing.description} />
              <Chip
              label={
                listing.status === 'Active' ? t('listings.statusActive')
                  : listing.status === 'Adopted' ? t('listings.statusAdopted')
                  : t('listings.statusRemoved')
              }
              size="small"
              sx={{
                bgcolor: listing.status === 'Active' ? '#D1FAE5' : listing.status === 'Adopted' ? '#DBEAFE' : '#F3F4F6',
                color: listing.status === 'Active' ? '#059669' : listing.status === 'Adopted' ? '#2563EB' : '#6B7280',
                fontWeight: 600,
              }}
            />
            </Box>
          </Box>

          <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', mb: 2 }}>
            <Chip label={ageLabel} size="small" sx={{ bgcolor: 'action.hover' }} />
            <Chip label={listing.color} size="small" sx={{ bgcolor: 'action.hover' }} />
            {listing.vaccinated && <Chip label={t('pets.vaccinated')} size="small" sx={{ bgcolor: '#D1FAE5', color: '#059669' }} />}
            {listing.castrated && <Chip label={t('pets.castrated')} size="small" sx={{ bgcolor: '#DBEAFE', color: '#2563EB' }} />}
          </Box>

          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, color: '#6B7280', mb: 2 }}>
            <LocationOnIcon sx={{ fontSize: 16 }} />
            <Typography variant="body2">{listing.city}</Typography>
          </Box>

          <Typography variant="body1" sx={{ mb: 3, lineHeight: 1.7 }}>{listing.description}</Typography>

          <Divider sx={{ mb: 2 }} />

          <Typography variant="subtitle2" fontWeight={700} sx={{ mb: 1 }}>{t('listings.contactTitle')}</Typography>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.75 }}>
            <Typography variant="body2" fontWeight={600}>{listing.userName}</Typography>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.75 }}>
              <EmailIcon sx={{ fontSize: 16, color: '#6B7280' }} />
              <Typography variant="body2">{listing.userEmail}</Typography>
            </Box>
            {listing.userPhone && (
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.75 }}>
                <PhoneIcon sx={{ fontSize: 16, color: '#6B7280' }} />
                <Typography variant="body2">{listing.userPhone}</Typography>
              </Box>
            )}
          </Box>

          {isOwner && listing.status === 'Active' && (
            <>
              <Divider sx={{ my: 3 }} />
              <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
                <Button
                  variant="outlined"
                  startIcon={<EditIcon />}
                  onClick={() => navigate(`/listings/${listingId}/edit`)}
                  sx={{ borderColor: CORAL, color: CORAL, '&:hover': { borderColor: '#e55555', bgcolor: 'rgba(255,107,107,0.04)' }, textTransform: 'none', fontWeight: 700, borderRadius: 2 }}
                >
                  {t('listings.editBtn') || 'Редагувати'}
                </Button>
                <Button
                  variant="contained"
                  onClick={handleMarkAdopted}
                  sx={{ bgcolor: '#059669', '&:hover': { bgcolor: '#047857' }, textTransform: 'none', fontWeight: 700, borderRadius: 2 }}
                >
                  {t('listings.markAdoptedBtn')}
                </Button>
                <Button
                  variant="outlined"
                  color="error"
                  onClick={() => setConfirmDelete(true)}
                  sx={{ textTransform: 'none', fontWeight: 700, borderRadius: 2 }}
                >
                  {t('listings.deleteBtn')}
                </Button>
              </Box>
            </>
          )}
        </Paper>
      </Container>

      <Dialog open={confirmDelete} onClose={() => setConfirmDelete(false)}>
        <DialogTitle>{t('listings.deleteConfirm')}</DialogTitle>
        <DialogActions>
          <Button onClick={() => setConfirmDelete(false)}>{t('common.cancel')}</Button>
          <Button color="error" onClick={handleDelete}>{t('listings.deleteBtn')}</Button>
        </DialogActions>
      </Dialog>

      <Snackbar open={!!toast} autoHideDuration={5000} onClose={() => setToast(null)}
        anchorOrigin={{ vertical: 'top', horizontal: 'center' }}>
        <Alert severity="error" onClose={() => setToast(null)}>{toast}</Alert>
      </Snackbar>
    </Box>
  )
}
