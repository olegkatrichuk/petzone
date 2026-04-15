import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useLangNavigate } from '../hooks/useLangNavigate'
import PageMeta from '../components/meta/PageMeta'
import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Typography from '@mui/material/Typography'
import Button from '@mui/material/Button'
import Chip from '@mui/material/Chip'
import Divider from '@mui/material/Divider'
import Skeleton from '@mui/material/Skeleton'
import Dialog from '@mui/material/Dialog'
import DialogTitle from '@mui/material/DialogTitle'
import DialogActions from '@mui/material/DialogActions'
import Snackbar from '@mui/material/Snackbar'
import Alert from '@mui/material/Alert'
import AddIcon from '@mui/icons-material/Add'
import LocationOnIcon from '@mui/icons-material/LocationOn'
import {
  useGetMyListingsQuery,
  useMarkAdoptedMutation,
  useDeleteListingMutation,
} from '../services/listingsApi'
import type { AdoptionListing } from '../types/listing'
import { useAuthStore } from '../store/authStore'
import { getApiError } from '../lib/getApiError'

const CORAL = '#FF6B6B'

function MyListingCard({ listing, onDeleted }: { listing: AdoptionListing; onDeleted: () => void }) {
  const { t } = useTranslation()
  const navigate = useLangNavigate()
  const [confirmDelete, setConfirmDelete] = useState(false)
  const [markAdopted] = useMarkAdoptedMutation()
  const [deleteListing] = useDeleteListingMutation()
  const [toast, setToast] = useState<string | null>(null)

  const ageLabel = listing.ageMonths < 1
    ? t('pets.ageLessThanMonth')
    : listing.ageMonths < 12
      ? `${listing.ageMonths} ${t('pets.ageMonths')}`
      : `${Math.floor(listing.ageMonths / 12)} ${t('pets.ageYears')}`

  const handleMarkAdopted = async () => {
    try {
      await markAdopted(listing.id).unwrap()
    } catch (err) {
      setToast(getApiError(err, t))
    }
  }

  const handleDelete = async () => {
    try {
      await deleteListing(listing.id).unwrap()
      setConfirmDelete(false)
      onDeleted()
    } catch (err) {
      setConfirmDelete(false)
      setToast(getApiError(err, t))
    }
  }

  return (
    <Box sx={{ border: '1px solid #E5E7EB', borderRadius: 3, p: 3, bgcolor: 'background.paper' }}>
      <Box sx={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between', gap: 1, mb: 1.5 }}>
        <Typography
          variant="subtitle1" fontWeight={700} sx={{ cursor: 'pointer', '&:hover': { color: CORAL } }}
          onClick={() => navigate(`/listings/${listing.id}`)}
        >
          {listing.title}
        </Typography>
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
            fontWeight: 600, flexShrink: 0,
          }}
        />
      </Box>

      <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', mb: 1.5 }}>
        <Chip label={ageLabel} size="small" sx={{ bgcolor: 'action.hover' }} />
        <Chip label={listing.color} size="small" sx={{ bgcolor: 'action.hover' }} />
        {listing.vaccinated && <Chip label={t('pets.vaccinated')} size="small" sx={{ bgcolor: '#D1FAE5', color: '#059669' }} />}
        {listing.castrated && <Chip label={t('pets.castrated')} size="small" sx={{ bgcolor: '#DBEAFE', color: '#2563EB' }} />}
      </Box>

      <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, color: '#6B7280', mb: 2 }}>
        <LocationOnIcon sx={{ fontSize: 16 }} />
        <Typography variant="caption">{listing.city}</Typography>
      </Box>

      {listing.status === 'Active' && (
        <Box sx={{ display: 'flex', gap: 1.5, flexWrap: 'wrap' }}>
          <Button
            size="small"
            variant="outlined"
            onClick={() => navigate(`/listings/${listing.id}`)}
            sx={{ borderColor: CORAL, color: CORAL, textTransform: 'none', borderRadius: 2 }}
          >
            {t('listings.editBtn')}
          </Button>
          <Button
            size="small"
            variant="contained"
            onClick={handleMarkAdopted}
            sx={{ bgcolor: '#059669', '&:hover': { bgcolor: '#047857' }, textTransform: 'none', borderRadius: 2 }}
          >
            {t('listings.markAdoptedBtn')}
          </Button>
          <Button
            size="small"
            variant="outlined"
            color="error"
            onClick={() => setConfirmDelete(true)}
            sx={{ textTransform: 'none', borderRadius: 2 }}
          >
            {t('listings.deleteBtn')}
          </Button>
        </Box>
      )}

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

export default function MyListingsPage() {
  const { t } = useTranslation()
  const navigate = useLangNavigate()
  const { user } = useAuthStore()
  const { data: myListingsData, isLoading, refetch } = useGetMyListingsQuery({})
  const listings = myListingsData?.items ?? []

  if (!user) {
    return (
      <Box sx={{ minHeight: '60vh', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
        <Box sx={{ textAlign: 'center' }}>
          <Typography variant="h6" sx={{ mb: 2 }}>{t('listings.loginRequired')}</Typography>
          <Button variant="contained" onClick={() => navigate('/login')}
            sx={{ bgcolor: CORAL, '&:hover': { bgcolor: '#e55555' }, textTransform: 'none', fontWeight: 700, borderRadius: 2 }}>
            {t('auth.login')}
          </Button>
        </Box>
      </Box>
    )
  }

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'background.default', py: 4 }}>
      <PageMeta title={t('listings.myListings')} description={t('listings.myListings')} path="/my-listings" noIndex />
      <Container maxWidth="sm">
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 3 }}>
          <Typography variant="h5" fontWeight="bold">{t('listings.myListings')}</Typography>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => navigate('/listings/create')}
            sx={{ bgcolor: CORAL, '&:hover': { bgcolor: '#e55555' }, textTransform: 'none', fontWeight: 700, borderRadius: 2 }}
          >
            {t('listings.addBtn')}
          </Button>
        </Box>

        {isLoading ? (
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
            <Divider sx={{ mb: 1 }} />
            {Array.from({ length: 3 }).map((_, i) => (
              <Box key={i} sx={{ border: '1px solid #E5E7EB', borderRadius: 3, p: 3 }}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1.5 }}>
                  <Skeleton width="55%" height={22} />
                  <Skeleton width={70} height={24} sx={{ borderRadius: 8 }} />
                </Box>
                <Box sx={{ display: 'flex', gap: 1, mb: 1.5 }}>
                  <Skeleton width={60} height={24} sx={{ borderRadius: 8 }} />
                  <Skeleton width={70} height={24} sx={{ borderRadius: 8 }} />
                </Box>
                <Skeleton width="30%" height={16} sx={{ mb: 2 }} />
                <Box sx={{ display: 'flex', gap: 1.5 }}>
                  <Skeleton width={90} height={32} sx={{ borderRadius: 2 }} />
                  <Skeleton width={110} height={32} sx={{ borderRadius: 2 }} />
                  <Skeleton width={80} height={32} sx={{ borderRadius: 2 }} />
                </Box>
              </Box>
            ))}
          </Box>
        ) : listings.length === 0 ? (
          <Box sx={{ textAlign: 'center', py: 8 }}>
            <Typography color="text.secondary" sx={{ mb: 2 }}>{t('listings.empty')}</Typography>
            <Button
              variant="outlined"
              startIcon={<AddIcon />}
              onClick={() => navigate('/listings/create')}
              sx={{ borderColor: CORAL, color: CORAL, textTransform: 'none', borderRadius: 2 }}
            >
              {t('listings.addBtn')}
            </Button>
          </Box>
        ) : (
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
            <Divider sx={{ mb: 1 }} />
            {listings.map((l) => (
              <MyListingCard key={l.id} listing={l} onDeleted={refetch} />
            ))}
          </Box>
        )}
      </Container>
    </Box>
  )
}
