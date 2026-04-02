import { Outlet } from 'react-router-dom'
import Box from '@mui/material/Box'
import Header from './Header'
import Footer from './Footer'
import ToastContainer from '../ui/ToastContainer'
import BackToTop from '../ui/BackToTop'
import CompareBar from '../pets/CompareBar'

export default function Layout() {
  return (
    <Box sx={{ minHeight: '100vh', display: 'flex', flexDirection: 'column' }}>
      <Header />
      <Box component="main" sx={{ flex: 1 }}>
        <Outlet />
      </Box>
      <Footer />
      <ToastContainer />
      <CompareBar />
      <BackToTop />
    </Box>
  )
}
