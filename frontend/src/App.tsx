import { lazy, Suspense } from 'react'
import { Routes, Route, Navigate } from 'react-router-dom'
import { ScrollToTop } from './components/ui/ScrollToTop'
import Box from '@mui/material/Box'
import CircularProgress from '@mui/material/CircularProgress'
import LangLayout from './components/layout/LangLayout'
import { detectBrowserLang } from './lib/langUtils'

const HomePage = lazy(() => import('./pages/HomePage'))
const LoginPage = lazy(() => import('./pages/LoginPage'))
const RegisterPage = lazy(() => import('./pages/RegisterPage'))
const VolunteersPage = lazy(() => import('./pages/VolunteersPage'))
const ProfilePage = lazy(() => import('./pages/ProfilePage'))
const RegisterVolunteerPage = lazy(() => import('./pages/RegisterVolunteerPage'))
const VolunteerProfilePage = lazy(() => import('./pages/VolunteerProfilePage'))
const PetDetailPage = lazy(() => import('./pages/PetDetailPage'))
const PetsPage = lazy(() => import('./pages/PetsPage'))
const FavoritesPage = lazy(() => import('./pages/FavoritesPage'))
const VolunteerAnimalsPage = lazy(() => import('./pages/VolunteerAnimalsPage'))
const HelpVolunteerPage = lazy(() => import('./pages/HelpVolunteerPage'))
const MyApplicationsPage = lazy(() => import('./pages/MyApplicationsPage'))
const EditVolunteerProfilePage = lazy(() => import('./pages/EditVolunteerProfilePage'))
const NewsPage = lazy(() => import('./pages/NewsPage'))
const NotFoundPage = lazy(() => import('./pages/NotFoundPage'))
const AboutPage = lazy(() => import('./pages/AboutPage'))
const AdminPage = lazy(() => import('./pages/AdminPage'))
const FaqPage = lazy(() => import('./pages/FaqPage'))
const DiscussionPage = lazy(() => import('./pages/DiscussionPage'))
const ListingsPage = lazy(() => import('./pages/ListingsPage'))
const CreateListingPage = lazy(() => import('./pages/CreateListingPage'))
const ListingDetailPage = lazy(() => import('./pages/ListingDetailPage'))
const MyListingsPage = lazy(() => import('./pages/MyListingsPage'))
const MapPage = lazy(() => import('./pages/MapPage'))

function PageLoader() {
  return (
    <Box sx={{ minHeight: '60vh', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
      <CircularProgress sx={{ color: '#FF6B6B' }} />
    </Box>
  )
}

function RootRedirect() {
  return <Navigate to={`/${detectBrowserLang()}`} replace />
}

function App() {
  return (
    <>
    <ScrollToTop />
    <Routes>
      {/* Root → redirect to browser language */}
      <Route path="/" element={<RootRedirect />} />

      {/* All pages under /:lang */}
      <Route path="/:lang" element={<LangLayout />}>
        <Route index element={<Suspense fallback={<PageLoader />}><HomePage /></Suspense>} />
        <Route path="login" element={<Suspense fallback={<PageLoader />}><LoginPage /></Suspense>} />
        <Route path="register" element={<Suspense fallback={<PageLoader />}><RegisterPage /></Suspense>} />
        <Route path="register/volunteer" element={<Suspense fallback={<PageLoader />}><RegisterVolunteerPage /></Suspense>} />
        <Route path="pets" element={<Suspense fallback={<PageLoader />}><PetsPage /></Suspense>} />
        <Route path="pets/:petId" element={<Suspense fallback={<PageLoader />}><PetDetailPage /></Suspense>} />
        <Route path="volunteers" element={<Suspense fallback={<PageLoader />}><VolunteersPage /></Suspense>} />
        <Route path="volunteers/:volunteerId" element={<Suspense fallback={<PageLoader />}><VolunteerProfilePage /></Suspense>} />
        <Route path="favorites" element={<Suspense fallback={<PageLoader />}><FavoritesPage /></Suspense>} />
        <Route path="profile" element={<Suspense fallback={<PageLoader />}><ProfilePage /></Suspense>} />
        <Route path="animals/:volunteerId" element={<Suspense fallback={<PageLoader />}><VolunteerAnimalsPage /></Suspense>} />
        <Route path="help/:volunteerId" element={<Suspense fallback={<PageLoader />}><HelpVolunteerPage /></Suspense>} />
        <Route path="news/:volunteerId" element={<Suspense fallback={<PageLoader />}><NewsPage /></Suspense>} />
        <Route path="edit-profile/volunteer/:volunteerId" element={<Suspense fallback={<PageLoader />}><EditVolunteerProfilePage /></Suspense>} />
        <Route path="volunteer-applications" element={<Suspense fallback={<PageLoader />}><MyApplicationsPage /></Suspense>} />
        <Route path="about" element={<Suspense fallback={<PageLoader />}><AboutPage /></Suspense>} />
        <Route path="admin" element={<Suspense fallback={<PageLoader />}><AdminPage /></Suspense>} />
        <Route path="faq" element={<Suspense fallback={<PageLoader />}><FaqPage /></Suspense>} />
        <Route path="discussion/:discussionId" element={<Suspense fallback={<PageLoader />}><DiscussionPage /></Suspense>} />
        <Route path="listings" element={<Suspense fallback={<PageLoader />}><ListingsPage /></Suspense>} />
        <Route path="listings/create" element={<Suspense fallback={<PageLoader />}><CreateListingPage /></Suspense>} />
        <Route path="listings/:listingId" element={<Suspense fallback={<PageLoader />}><ListingDetailPage /></Suspense>} />
        <Route path="my-listings" element={<Suspense fallback={<PageLoader />}><MyListingsPage /></Suspense>} />
        <Route path="map" element={<Suspense fallback={<PageLoader />}><MapPage /></Suspense>} />
        <Route path="*" element={<Suspense fallback={<PageLoader />}><NotFoundPage /></Suspense>} />
      </Route>

      {/* Fallback — redirect unknown paths to default lang root */}
      <Route path="*" element={<RootRedirect />} />
    </Routes>
    </>
  )
}

export default App
