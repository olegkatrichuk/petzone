import { useState, useRef, useCallback, useEffect } from 'react'
import { useParams, Link, useLocation } from 'react-router-dom'
import { useLangNavigate } from '../hooks/useLangNavigate'
import PageMeta from '../components/meta/PageMeta'
import { useTranslation } from 'react-i18next'
import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Paper from '@mui/material/Paper'
import Typography from '@mui/material/Typography'
import Avatar from '@mui/material/Avatar'
import Button from '@mui/material/Button'
import IconButton from '@mui/material/IconButton'
import List from '@mui/material/List'
import ListItemButton from '@mui/material/ListItemButton'
import ListItemIcon from '@mui/material/ListItemIcon'
import ListItemText from '@mui/material/ListItemText'
import Dialog from '@mui/material/Dialog'
import DialogTitle from '@mui/material/DialogTitle'
import DialogContent from '@mui/material/DialogContent'
import DialogActions from '@mui/material/DialogActions'
import Alert from '@mui/material/Alert'
import Skeleton from '@mui/material/Skeleton'
import VolunteerProfileSkeleton from '../components/skeletons/VolunteerProfileSkeleton'
import AppBreadcrumbs from '../components/ui/AppBreadcrumbs'
import Divider from '@mui/material/Divider'
import Chip from '@mui/material/Chip'
import Tooltip from '@mui/material/Tooltip'
import PersonIcon from '@mui/icons-material/Person'
import PetsIcon from '@mui/icons-material/Pets'
import AddCircleOutlineIcon from '@mui/icons-material/AddCircleOutline'
import ArticleIcon from '@mui/icons-material/Article'
import AssignmentIcon from '@mui/icons-material/Assignment'
import EditIcon from '@mui/icons-material/Edit'
import FavoriteIcon from '@mui/icons-material/Favorite'
import EmailIcon from '@mui/icons-material/Email'
import PhoneIcon from '@mui/icons-material/Phone'
import LinkIcon from '@mui/icons-material/Link'
import WorkHistoryIcon from '@mui/icons-material/WorkHistory'
import HomeIcon from '@mui/icons-material/Home'
import ChevronLeftIcon from '@mui/icons-material/ChevronLeft'
import ChevronRightIcon from '@mui/icons-material/ChevronRight'
import { useAuthStore } from '../store/authStore'
import { useGetVolunteerByIdQuery } from '../services/volunteersApi'
import { useGetPetsQuery } from '../services/petsApi'
import { PetStatus } from '../types/pet'
import type { Pet } from '../types/pet'
import PetCard from '../components/pets/PetCard'

const CORAL = '#FF6B6B'

// ─────────────────────────────────────────────────────────
// SIDEBAR
// ─────────────────────────────────────────────────────────

interface SidebarProps {
  volunteerId: string
  isOwner: boolean
}

function VolunteerSidebar({ volunteerId, isOwner }: SidebarProps) {
  const { t } = useTranslation()
  const { pathname } = useLocation()

  const ownerItems = [
    { label: t('volunteerProfile.myProfile'), to: `/volunteers/${volunteerId}`, icon: <PersonIcon fontSize="small" /> },
    { label: t('volunteerProfile.myAnimals'), to: `/animals/${volunteerId}`, icon: <PetsIcon fontSize="small" /> },
    { label: t('volunteerProfile.addAnimal'), to: '/add-animal', icon: <AddCircleOutlineIcon fontSize="small" /> },
    { label: t('volunteerProfile.myNews'), to: `/news/${volunteerId}`, icon: <ArticleIcon fontSize="small" /> },
    { label: t('volunteerProfile.myApplications'), to: '/volunteer-applications', icon: <AssignmentIcon fontSize="small" /> },
  ]

  const visitorItems = [
    { label: t('volunteerProfile.volunteerProfileLabel'), to: `/volunteers/${volunteerId}`, icon: <PersonIcon fontSize="small" /> },
    { label: t('volunteerProfile.volunteerAnimals'), to: `/animals/${volunteerId}`, icon: <PetsIcon fontSize="small" /> },
    { label: t('volunteerProfile.volunteerNews'), to: `/news/${volunteerId}`, icon: <ArticleIcon fontSize="small" /> },
  ]

  const items = isOwner ? ownerItems : visitorItems

  return (
    <Paper
      elevation={0}
      sx={{ border: '1px solid #E5E7EB', borderRadius: 3, overflow: 'hidden', width: 260, flexShrink: 0, display: { xs: 'none', lg: 'block' } }}
    >
      <Box sx={{ bgcolor: CORAL, px: 2.5, py: 2 }}>
        <Typography variant="subtitle1" fontWeight="bold" color="white">
          {isOwner ? t('volunteerProfile.myCabinet') : t('volunteerProfile.volunteerLabel')}
        </Typography>
      </Box>
      <List disablePadding>
        {items.map((item, i) => {
          const active = pathname === item.to
          return (
            <Box key={item.to}>
              {i > 0 && <Divider />}
              <ListItemButton
                component={Link}
                to={item.to}
                selected={active}
                sx={{
                  px: 2.5,
                  py: 1.5,
                  '&.Mui-selected': {
                    bgcolor: '#FFF0F0',
                    color: CORAL,
                    '& .MuiListItemIcon-root': { color: CORAL },
                  },
                  '&:hover': { bgcolor: '#FFF5F5' },
                }}
              >
                <ListItemIcon sx={{ minWidth: 34, color: '#6B7280' }}>{item.icon}</ListItemIcon>
                <ListItemText
                  primary={item.label}
                  primaryTypographyProps={{
                    variant: 'body2',
                    fontWeight: active ? 600 : 400,
                  }}
                />
              </ListItemButton>
            </Box>
          )
        })}
      </List>
    </Paper>
  )
}

// ─────────────────────────────────────────────────────────
// AVATAR EDIT MODAL
// ─────────────────────────────────────────────────────────

interface AvatarEditModalProps {
  open: boolean
  onClose: () => void
  onSave: (url: string) => void
  currentUrl: string
}

function AvatarEditModal({ open, onClose, onSave, currentUrl }: AvatarEditModalProps) {
  const { t } = useTranslation()
  const [preview, setPreview] = useState(currentUrl)
  const [file, setFile] = useState<File | null>(null)
  const [error, setError] = useState('')

  useEffect(() => {
    if (open) {
      setPreview(currentUrl)
      setFile(null)
      setError('')
    }
  }, [open, currentUrl])

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const selected = e.target.files?.[0]
    if (!selected) return
    if (!selected.type.startsWith('image/')) {
      setError(t('common.invalidImage'))
      setFile(null)
      return
    }
    setError('')
    setFile(selected)
    setPreview(URL.createObjectURL(selected))
  }

  const handleSave = () => {
    if (!file) {
      setError(t('common.noFileSelected'))
      return
    }
    onSave(preview)
    onClose()
  }

  return (
    <Dialog open={open} onClose={onClose} maxWidth="xs" fullWidth>
      <DialogTitle>{t('volunteerProfile.avatarModalTitle')}</DialogTitle>
      <DialogContent sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 3, pt: '16px !important' }}>
        <Avatar
          src={preview || undefined}
          sx={{ width: 120, height: 120, fontSize: 42, bgcolor: CORAL, border: `3px solid #FFD5D5` }}
        />

        <Box sx={{ width: '100%' }}>
          <Button
            variant="outlined"
            component="label"
            fullWidth
            sx={{
              borderColor: error ? 'error.main' : '#E5E7EB',
              color: '#374151',
              textTransform: 'none',
              '&:hover': { borderColor: CORAL },
            }}
          >
            {t('common.chooseFile')}
            <input type="file" accept="image/*" hidden onChange={handleFileChange} />
          </Button>

          {file && (
            <Typography variant="caption" color="text.secondary" sx={{ mt: 0.5, display: 'block' }}>
              {file.name}
            </Typography>
          )}

          {error && (
            <Typography variant="caption" color="error" sx={{ mt: 0.5, display: 'block' }}>
              {error}
            </Typography>
          )}
        </Box>
      </DialogContent>

      <DialogActions sx={{ px: 3, pb: 3, gap: 1 }}>
        <Button onClick={onClose} sx={{ color: '#6B7280', textTransform: 'none' }}>
          {t('common.cancel')}
        </Button>
        <Button
          variant="contained"
          onClick={handleSave}
          sx={{ bgcolor: CORAL, '&:hover': { bgcolor: '#e55555' }, textTransform: 'none', fontWeight: 700 }}
        >
          {t('common.save')}
        </Button>
      </DialogActions>
    </Dialog>
  )
}

// ─────────────────────────────────────────────────────────
// PET CAROUSEL
// ─────────────────────────────────────────────────────────

interface PetCarouselProps {
  pets: Pet[]
}

function PetCarousel({ pets }: PetCarouselProps) {
  const { t } = useTranslation()
  const containerRef = useRef<HTMLDivElement>(null)
  const [canLeft, setCanLeft] = useState(false)
  const [canRight, setCanRight] = useState(false)

  const syncButtons = useCallback(() => {
    const el = containerRef.current
    if (!el) return
    setCanLeft(el.scrollLeft > 1)
    setCanRight(el.scrollLeft + el.clientWidth < el.scrollWidth - 1)
  }, [])

  useEffect(() => {
    syncButtons()
    const el = containerRef.current
    if (!el) return
    el.addEventListener('scroll', syncButtons, { passive: true })
    window.addEventListener('resize', syncButtons)
    return () => {
      el.removeEventListener('scroll', syncButtons)
      window.removeEventListener('resize', syncButtons)
    }
  }, [pets, syncButtons])

  const scroll = (dir: 'left' | 'right') =>
    containerRef.current?.scrollBy({ left: dir === 'left' ? -320 : 320, behavior: 'smooth' })

  if (pets.length === 0) {
    return (
      <Box sx={{ textAlign: 'center', py: 4 }}>
        <Typography color="text.secondary" variant="body2">
          {t('volunteerProfile.noPetsInCategory')}
        </Typography>
      </Box>
    )
  }

  return (
    <Box sx={{ position: 'relative' }}>
      {canLeft && (
        <IconButton
          onClick={() => scroll('left')}
          size="small"
          sx={{
            position: 'absolute', left: -18, top: '40%', transform: 'translateY(-50%)', zIndex: 2,
            bgcolor: 'white', boxShadow: 2,
            '&:hover': { bgcolor: '#FFF0F0', color: CORAL },
          }}
        >
          <ChevronLeftIcon />
        </IconButton>
      )}

      <Box
        ref={containerRef}
        sx={{
          display: 'flex',
          gap: 2,
          pb: 1,
          overflowX: 'auto',
          scrollbarWidth: 'none',
          '&::-webkit-scrollbar': { display: 'none' },
        }}
      >
        {pets.map((pet) => (
          <Box key={pet.id} sx={{ minWidth: { xs: 220, sm: 260, md: 290 }, maxWidth: { xs: 220, sm: 260, md: 290 }, flexShrink: 0 }}>
            <PetCard pet={pet} />
          </Box>
        ))}
      </Box>

      {canRight && (
        <IconButton
          onClick={() => scroll('right')}
          size="small"
          sx={{
            position: 'absolute', right: -18, top: '40%', transform: 'translateY(-50%)', zIndex: 2,
            bgcolor: 'white', boxShadow: 2,
            '&:hover': { bgcolor: '#FFF0F0', color: CORAL },
          }}
        >
          <ChevronRightIcon />
        </IconButton>
      )}
    </Box>
  )
}

// ─────────────────────────────────────────────────────────
// CAROUSEL SECTION
// ─────────────────────────────────────────────────────────

interface CarouselSectionProps {
  title: string
  subtitle: string
  pets: Pet[]
  isLoading: boolean
}

function CarouselSection({ title, subtitle, pets, isLoading }: CarouselSectionProps) {
  return (
    <Paper elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 3, p: 3 }}>
      <Typography variant="h6" fontWeight="bold">{title}</Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>{subtitle}</Typography>
      <Divider sx={{ mb: 2.5 }} />

      {isLoading ? (
        <Box sx={{ display: 'flex', gap: 2, overflow: 'hidden' }}>
          {[0, 1, 2].map((i) => (
            <Skeleton key={i} variant="rectangular" sx={{ minWidth: { xs: 220, sm: 260 }, height: 320, borderRadius: 3, flexShrink: 0 }} />
          ))}
        </Box>
      ) : (
        <PetCarousel pets={pets} />
      )}
    </Paper>
  )
}

// ─────────────────────────────────────────────────────────
// MAIN PAGE
// ─────────────────────────────────────────────────────────

export default function VolunteerProfilePage() {
  const { t } = useTranslation()
  const { volunteerId } = useParams<{ volunteerId: string }>()
  const navigate = useLangNavigate()
  const { user } = useAuthStore()

  const isOwner = !!user && !!volunteerId && user.id === volunteerId

  const [avatarUrl, setAvatarUrl] = useState<string | null>(null)
  const [avatarModalOpen, setAvatarModalOpen] = useState(false)

  const {
    data: volunteer,
    isLoading: volunteerLoading,
    isError: volunteerError,
  } = useGetVolunteerByIdQuery(volunteerId ?? '', { skip: !volunteerId })

  const { data: lookingPets, isLoading: lookingLoading } = useGetPetsQuery(
    { page: 1, pageSize: 50, volunteerId: volunteerId ?? '', status: PetStatus.LookingForHome },
    { skip: !volunteerId },
  )

  const { data: needsHelpPets, isLoading: needsHelpLoading } = useGetPetsQuery(
    { page: 1, pageSize: 50, volunteerId: volunteerId ?? '', status: PetStatus.NeedsHelp },
    { skip: !volunteerId },
  )

  if (volunteerLoading) {
    return <VolunteerProfileSkeleton />
  }

  if (volunteerError || !volunteer) {
    return (
      <Container maxWidth="sm" sx={{ py: 8 }}>
        <Alert severity="error" sx={{ mb: 3 }}>
          {t('volunteerProfile.notFound')}
        </Alert>
        <Button onClick={() => navigate('/')} sx={{ color: CORAL, textTransform: 'none' }}>
          {t('volunteerProfile.backToHome')}
        </Button>
      </Container>
    )
  }

  const fullName = [volunteer.lastName, volunteer.firstName, volunteer.patronymic]
    .filter(Boolean)
    .join(' ')
  const initials = `${volunteer.firstName?.[0] ?? ''}${volunteer.lastName?.[0] ?? ''}`
  const displayAvatar = avatarUrl ?? volunteer.photoPath ?? null

  const personJsonLd = {
    '@context': 'https://schema.org',
    '@type': 'Person',
    name: fullName,
    description: volunteer.generalDescription ?? '',
    image: volunteer.photoPath ?? '',
    url: `https://getpetzone.com/uk/volunteers/${volunteerId}`,
    email: volunteer.email ?? undefined,
    telephone: volunteer.phone ?? undefined,
    address: volunteer.city ? { '@type': 'PostalAddress', addressLocality: volunteer.city } : undefined,
  }

  return (
    <Box sx={{ bgcolor: '#FAFAFA', minHeight: '100%', py: 4 }}>
      <PageMeta title={fullName} description={volunteer.generalDescription ?? t('volunteers.pageTitle')} path={`/volunteers/${volunteerId}`} />
      <script type="application/ld+json" dangerouslySetInnerHTML={{ __html: JSON.stringify(personJsonLd) }} />
      <Container maxWidth="xl">
        <AppBreadcrumbs items={[
          { label: t('nav.home'), path: '/' },
          { label: t('nav.volunteers'), path: '/volunteers' },
          { label: fullName },
        ]} />
        <Box sx={{ display: 'flex', gap: 3, alignItems: 'flex-start', flexDirection: { xs: 'column', lg: 'row' } }}>

          {/* ── SIDEBAR ─────────────────────────────────── */}
          <Box sx={{ position: 'sticky', top: 16 }}>
            <VolunteerSidebar volunteerId={volunteerId!} isOwner={isOwner} />
          </Box>

          {/* ── MAIN CONTENT ────────────────────────────── */}
          <Box sx={{ flex: 1, minWidth: 0, display: 'flex', flexDirection: 'column', gap: 3 }}>

            {/* ── VOLUNTEER INFO CARD ──────────────────── */}
            <Paper elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 3, p: 3 }}>
              <Box sx={{ display: 'flex', gap: 3, flexWrap: 'wrap' }}>

                {/* Avatar column */}
                <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 1 }}>
                  <Tooltip title={isOwner ? t('volunteerProfile.clickToChangePhoto') : ''} arrow>
                    <Avatar
                      src={displayAvatar ?? undefined}
                      onClick={() => { if (isOwner) setAvatarModalOpen(true) }}
                      sx={{
                        width: 120, height: 120, fontSize: 40,
                        bgcolor: CORAL,
                        cursor: isOwner ? 'pointer' : 'default',
                        transition: 'box-shadow 0.2s',
                        '&:hover': isOwner ? { boxShadow: `0 0 0 4px #FFD5D5` } : {},
                      }}
                    >
                      {!displayAvatar && initials}
                    </Avatar>
                  </Tooltip>
                  {isOwner && (
                    <Typography variant="caption" color="text.secondary" sx={{ fontSize: 11 }}>
                      {t('volunteerProfile.changePhoto')}
                    </Typography>
                  )}
                </Box>

                {/* Info column */}
                <Box sx={{ flex: 1, minWidth: 220 }}>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 1.5, flexWrap: 'wrap', gap: 1 }}>
                    <Typography variant="h5" fontWeight="bold">{fullName}</Typography>

                    {isOwner ? (
                      <Button
                        startIcon={<EditIcon />}
                        variant="outlined"
                        size="small"
                        onClick={() => navigate(`/edit-profile/volunteer/${volunteerId}`)}
                        sx={{
                          borderColor: CORAL, color: CORAL, textTransform: 'none',
                          '&:hover': { borderColor: '#e55555', bgcolor: '#FFF0F0' },
                        }}
                      >
                        {t('volunteerProfile.editProfile')}
                      </Button>
                    ) : (
                      <Button
                        startIcon={<FavoriteIcon />}
                        variant="contained"
                        size="small"
                        onClick={() => navigate(`/help/${volunteerId}`)}
                        sx={{
                          bgcolor: CORAL, '&:hover': { bgcolor: '#e55555' },
                          textTransform: 'none', fontWeight: 700,
                        }}
                      >
                        {t('volunteerProfile.helpVolunteer')}
                      </Button>
                    )}
                  </Box>

                  {volunteer.generalDescription && (
                    <Typography
                      variant="body2"
                      color="text.secondary"
                      sx={{ mb: 2, maxWidth: 640, lineHeight: 1.7 }}
                    >
                      {volunteer.generalDescription}
                    </Typography>
                  )}

                  {/* Stats chips */}
                  <Box sx={{ display: 'flex', gap: 1.5, flexWrap: 'wrap', mb: 2.5 }}>
                    <Chip
                      icon={<WorkHistoryIcon sx={{ fontSize: '16px !important' }} />}
                      label={t('volunteerProfile.experience', { years: volunteer.experienceYears })}
                      variant="outlined"
                      size="small"
                      sx={{ borderColor: '#E5E7EB', color: '#374151' }}
                    />
                    {volunteer.petsCount != null && (
                      <Chip
                        icon={<HomeIcon sx={{ fontSize: '16px !important' }} />}
                        label={t('volunteerProfile.foundHome', { count: volunteer.petsCount })}
                        variant="outlined"
                        size="small"
                        sx={{ borderColor: '#E5E7EB', color: '#374151' }}
                      />
                    )}
                  </Box>

                  <Divider sx={{ mb: 2 }} />

                  {/* Contacts */}
                  <Box sx={{ mb: volunteer.socialNetworks?.length ? 2.5 : 0 }}>
                    <Typography
                      variant="subtitle2"
                      fontWeight="bold"
                      color="text.secondary"
                      sx={{ mb: 1, textTransform: 'uppercase', fontSize: 11, letterSpacing: 0.5 }}
                    >
                      {t('volunteerProfile.contacts')}
                    </Typography>
                    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.8 }}>
                      {volunteer.email && (
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          <EmailIcon sx={{ fontSize: 17, color: '#6B7280' }} />
                          <Typography
                            variant="body2"
                            component="a"
                            href={`mailto:${volunteer.email}`}
                            sx={{ color: CORAL, textDecoration: 'none', '&:hover': { textDecoration: 'underline' } }}
                          >
                            {volunteer.email}
                          </Typography>
                        </Box>
                      )}
                      {volunteer.phone && (
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          <PhoneIcon sx={{ fontSize: 17, color: '#6B7280' }} />
                          <Typography
                            variant="body2"
                            component="a"
                            href={`tel:${volunteer.phone}`}
                            sx={{ color: '#374151', textDecoration: 'none', '&:hover': { color: CORAL } }}
                          >
                            {volunteer.phone}
                          </Typography>
                        </Box>
                      )}
                    </Box>
                  </Box>

                  {/* Social networks */}
                  {volunteer.socialNetworks && volunteer.socialNetworks.length > 0 && (
                    <Box>
                      <Typography
                        variant="subtitle2"
                        fontWeight="bold"
                        color="text.secondary"
                        sx={{ mb: 1, textTransform: 'uppercase', fontSize: 11, letterSpacing: 0.5 }}
                      >
                        {t('volunteerProfile.socialNetworks')}
                      </Typography>
                      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.8 }}>
                        {volunteer.socialNetworks.map((sn, i) => (
                          <Box key={i} sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                            <LinkIcon sx={{ fontSize: 17, color: '#6B7280' }} />
                            <Typography
                              variant="body2"
                              component="a"
                              href={sn.link}
                              target="_blank"
                              rel="noopener noreferrer"
                              sx={{
                                color: CORAL, textDecoration: 'none',
                                '&:hover': { textDecoration: 'underline' },
                              }}
                            >
                              {sn.name}
                            </Typography>
                          </Box>
                        ))}
                      </Box>
                    </Box>
                  )}
                </Box>
              </Box>
            </Paper>

            {/* ── LOOKING FOR HOME CAROUSEL ────────────── */}
            <CarouselSection
              title={t('volunteerProfile.lookingForHomeTitle')}
              subtitle={t('volunteerProfile.lookingForHomeSubtitle')}
              pets={lookingPets?.items ?? []}
              isLoading={lookingLoading}
            />

            {/* ── NEEDS HELP CAROUSEL ──────────────────── */}
            <CarouselSection
              title={t('volunteerProfile.needsHelpTitle')}
              subtitle={t('volunteerProfile.needsHelpSubtitle')}
              pets={needsHelpPets?.items ?? []}
              isLoading={needsHelpLoading}
            />
          </Box>
        </Box>
      </Container>

      <AvatarEditModal
        open={avatarModalOpen}
        onClose={() => setAvatarModalOpen(false)}
        onSave={(url) => setAvatarUrl(url)}
        currentUrl={displayAvatar ?? ''}
      />
    </Box>
  )
}
