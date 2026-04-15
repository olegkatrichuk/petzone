import { useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { MapContainer, TileLayer, Marker, Popup } from 'react-leaflet'
import MarkerClusterGroup from 'react-leaflet-cluster'
import L from 'leaflet'
import 'leaflet/dist/leaflet.css'
import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Typography from '@mui/material/Typography'
import Chip from '@mui/material/Chip'
import ToggleButton from '@mui/material/ToggleButton'
import ToggleButtonGroup from '@mui/material/ToggleButtonGroup'
import CircularProgress from '@mui/material/CircularProgress'
import Alert from '@mui/material/Alert'
import PageMeta from '../components/meta/PageMeta'
import { useLangNavigate } from '../hooks/useLangNavigate'
import { useGetPetsQuery } from '../services/petsApi'
import { useGetListingsQuery } from '../services/listingsApi'
import { PetStatus, type Pet } from '../types/pet'

// Fix default marker icons for Leaflet + Vite
delete (L.Icon.Default.prototype as unknown as Record<string, unknown>)._getIconUrl
L.Icon.Default.mergeOptions({
  iconRetinaUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png',
  iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
  shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
})

const petIcon = new L.Icon({
  iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
  iconRetinaUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png',
  shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  popupAnchor: [1, -34],
})

const listingIcon = new L.Icon({
  iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-red.png',
  iconRetinaUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-red.png',
  shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  popupAnchor: [1, -34],
})

// Ukrainian cities coordinates (most common)
const CITY_COORDS: Record<string, [number, number]> = {
  'київ': [50.4501, 30.5234],
  'kyiv': [50.4501, 30.5234],
  'харків': [49.9935, 36.2304],
  'kharkiv': [49.9935, 36.2304],
  'одеса': [46.4825, 30.7233],
  'odesa': [46.4825, 30.7233],
  'одесса': [46.4825, 30.7233],
  'дніпро': [48.4647, 35.0462],
  'dnipro': [48.4647, 35.0462],
  'львів': [49.8397, 24.0297],
  'lviv': [49.8397, 24.0297],
  'запоріжжя': [47.8388, 35.1396],
  'запорожье': [47.8388, 35.1396],
  'вінниця': [49.2328, 28.4682],
  'полтава': [49.5883, 34.5514],
  'луцьк': [50.7472, 25.3254],
  'чернігів': [51.4982, 31.2893],
  'суми': [50.9077, 34.7981],
  'хмельницький': [49.4229, 26.9871],
  'черкаси': [49.4444, 32.0598],
  'рівне': [50.6196, 26.2513],
  'івано-франківськ': [48.9226, 24.7111],
  'тернопіль': [49.5535, 25.5948],
  'ужгород': [48.6208, 22.2879],
  'херсон': [46.6354, 32.6169],
  'миколаїв': [46.9750, 31.9946],
  'житомир': [50.2547, 28.6587],
  'кропивницький': [48.5131, 32.2623],
  'варшава': [52.2297, 21.0122],
  'warszawa': [52.2297, 21.0122],
  'берлін': [52.5200, 13.4050],
  'berlin': [52.5200, 13.4050],
  'прага': [50.0755, 14.4378],
  'prague': [50.0755, 14.4378],
  'краків': [50.0647, 19.9450],
  'krakow': [50.0647, 19.9450],
}

function getCityCoords(city: string): [number, number] | null {
  const key = city.toLowerCase().trim()
  return CITY_COORDS[key] ?? null
}

type Mode = 'all' | 'pets' | 'listings'

export default function MapPage() {
  const { t } = useTranslation()
  const navigate = useLangNavigate()
  const [mode, setMode] = useState<Mode>('all')

  const { data: petsData, isLoading: petsLoading, isError: petsError } = useGetPetsQuery({
    page: 1,
    status: PetStatus.LookingForHome,
    pageSize: 100,
  })
  const { data: listingsData, isLoading: listingsLoading, isError: listingsError } = useGetListingsQuery({ pageSize: 100 })
  const listings = listingsData?.items ?? []

  const isLoading = petsLoading || listingsLoading
  const isError = petsError || listingsError

  const petMarkers = useMemo(() => {
    if (mode === 'listings') return []
    return (petsData?.items ?? [] as Pet[])
      .map(pet => ({ pet, coords: getCityCoords(pet.city ?? '') }))
      .filter((x): x is { pet: Pet; coords: [number, number] } => x.coords !== null)
  }, [petsData, mode])

  const listingMarkers = useMemo(() => {
    if (mode === 'pets') return []
    return listings
      .filter(l => l.status === 'Active')
      .map(l => ({ listing: l, coords: getCityCoords(l.city) }))
      .filter(({ coords }) => coords !== null) as { listing: typeof listings[number]; coords: [number, number] }[]
  }, [listings, mode])

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'background.default' }}>
      <PageMeta title={t('map.pageTitle')} description={t('map.pageTitle')} path="/map" />

      <Container maxWidth="xl" sx={{ pt: 3, pb: 2 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', gap: 2, mb: 2 }}>
          <Typography variant="h5" fontWeight="bold">{t('map.pageTitle')}</Typography>

          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, flexWrap: 'wrap' }}>
            <Box sx={{ display: 'flex', gap: 1 }}>
              <Chip size="small" sx={{ bgcolor: '#2563EB', color: '#fff' }} label={t('map.legendPets')} />
              <Chip size="small" sx={{ bgcolor: '#DC2626', color: '#fff' }} label={t('map.legendListings')} />
            </Box>

            <ToggleButtonGroup
              value={mode}
              exclusive
              onChange={(_, v) => v && setMode(v)}
              size="small"
            >
              <ToggleButton value="all">{t('map.all')}</ToggleButton>
              <ToggleButton value="pets">{t('map.onlyPets')}</ToggleButton>
              <ToggleButton value="listings">{t('map.onlyListings')}</ToggleButton>
            </ToggleButtonGroup>
          </Box>
        </Box>

        {isError && (
          <Alert severity="error" sx={{ mb: 2 }}>{t('errors.unknown')}</Alert>
        )}

        {isLoading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 10 }}>
            <CircularProgress sx={{ color: '#FF6B6B' }} />
          </Box>
        ) : (
          <Box sx={{ borderRadius: 3, overflow: 'hidden', border: '1px solid #E5E7EB', height: { xs: '60vh', md: '75vh' } }}>
            <MapContainer
              center={[49.0, 31.5]}
              zoom={6}
              style={{ height: '100%', width: '100%' }}
            >
              <TileLayer
                attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
                url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
              />

              <MarkerClusterGroup chunkedLoading>
                {petMarkers.map(({ pet, coords }) => (
                  <Marker key={pet.id} position={coords} icon={petIcon}>
                    <Popup>
                      <Box sx={{ minWidth: 160 }}>
                        <Typography variant="subtitle2" fontWeight={700}>{pet.nickname}</Typography>
                        <Typography variant="caption" color="text.secondary">{pet.city}</Typography>
                        <br />
                        <Box
                          component="a"
                          onClick={() => navigate(`/pets/${pet.id}`)}
                          sx={{ color: '#2563EB', cursor: 'pointer', fontSize: 13 }}
                        >
                          {t('pets.details')}
                        </Box>
                      </Box>
                    </Popup>
                  </Marker>
                ))}

                {listingMarkers.map(({ listing, coords }) => (
                  <Marker key={listing.id} position={coords} icon={listingIcon}>
                    <Popup>
                      <Box sx={{ minWidth: 160 }}>
                        <Typography variant="subtitle2" fontWeight={700}>{listing.title}</Typography>
                        <Typography variant="caption" color="text.secondary">{listing.city}</Typography>
                        <br />
                        <Box
                          component="a"
                          onClick={() => navigate(`/listings/${listing.id}`)}
                          sx={{ color: '#DC2626', cursor: 'pointer', fontSize: 13 }}
                        >
                          {t('pets.details')}
                        </Box>
                      </Box>
                    </Popup>
                  </Marker>
                ))}
              </MarkerClusterGroup>
            </MapContainer>
          </Box>
        )}

        <Typography variant="caption" color="text.secondary" sx={{ mt: 1, display: 'block' }}>
          {t('map.hint')}
        </Typography>
      </Container>
    </Box>
  )
}