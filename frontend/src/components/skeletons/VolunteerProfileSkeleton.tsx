import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Paper from '@mui/material/Paper'
import Skeleton from '@mui/material/Skeleton'
import Divider from '@mui/material/Divider'

export default function VolunteerProfileSkeleton() {
  return (
    <Box sx={{ bgcolor: '#FAFAFA', minHeight: '100%', py: 4 }}>
      <Container maxWidth="xl">
        <Box sx={{ display: 'flex', gap: 3, alignItems: 'flex-start' }}>

          {/* Sidebar placeholder — hidden on mobile, matches real sidebar */}
          <Box sx={{ display: { xs: 'none', lg: 'block' }, width: 260, flexShrink: 0 }}>
            <Paper elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 3, overflow: 'hidden' }}>
              <Skeleton variant="rectangular" height={52} />
              {[0, 1, 2].map((i) => (
                <Box key={i}>
                  {i > 0 && <Divider />}
                  <Box sx={{ px: 2.5, py: 1.5, display: 'flex', alignItems: 'center', gap: 1.5 }}>
                    <Skeleton variant="circular" width={20} height={20} />
                    <Skeleton width="65%" height={20} />
                  </Box>
                </Box>
              ))}
            </Paper>
          </Box>

          {/* Main content */}
          <Box sx={{ flex: 1, minWidth: 0, display: 'flex', flexDirection: 'column', gap: 3 }}>

            {/* Volunteer info card */}
            <Paper elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 3, p: 3 }}>
              <Box sx={{ display: 'flex', gap: 3, flexWrap: 'wrap' }}>
                {/* Avatar */}
                <Skeleton variant="circular" width={120} height={120} sx={{ flexShrink: 0 }} />

                {/* Info */}
                <Box sx={{ flex: 1, minWidth: 220 }}>
                  <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1.5 }}>
                    <Skeleton width={200} height={32} />
                    <Skeleton width={110} height={32} sx={{ borderRadius: 2 }} />
                  </Box>
                  <Skeleton height={18} />
                  <Skeleton width="80%" height={18} sx={{ mb: 2 }} />

                  <Box sx={{ display: 'flex', gap: 1.5, mb: 2.5 }}>
                    <Skeleton width={120} height={28} sx={{ borderRadius: 5 }} />
                    <Skeleton width={140} height={28} sx={{ borderRadius: 5 }} />
                  </Box>

                  <Divider sx={{ mb: 2 }} />

                  <Skeleton width={80} height={14} sx={{ mb: 1 }} />
                  <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.8 }}>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <Skeleton variant="circular" width={17} height={17} />
                      <Skeleton width={180} height={18} />
                    </Box>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <Skeleton variant="circular" width={17} height={17} />
                      <Skeleton width={130} height={18} />
                    </Box>
                  </Box>
                </Box>
              </Box>
            </Paper>

            {/* Carousel section 1 */}
            <Paper elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 3, p: 3 }}>
              <Skeleton width={200} height={28} />
              <Skeleton width={260} height={18} sx={{ mb: 2 }} />
              <Divider sx={{ mb: 2.5 }} />
              <Box sx={{ display: 'flex', gap: 2 }}>
                {[0, 1, 2].map((i) => (
                  <Skeleton key={i} variant="rectangular" sx={{ minWidth: { xs: 220, sm: 260 }, height: 320, borderRadius: 3, flexShrink: 0 }} />
                ))}
              </Box>
            </Paper>

            {/* Carousel section 2 */}
            <Paper elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 3, p: 3 }}>
              <Skeleton width={180} height={28} />
              <Skeleton width={240} height={18} sx={{ mb: 2 }} />
              <Divider sx={{ mb: 2.5 }} />
              <Box sx={{ display: 'flex', gap: 2 }}>
                {[0, 1, 2].map((i) => (
                  <Skeleton key={i} variant="rectangular" sx={{ minWidth: { xs: 220, sm: 260 }, height: 320, borderRadius: 3, flexShrink: 0 }} />
                ))}
              </Box>
            </Paper>

          </Box>
        </Box>
      </Container>
    </Box>
  )
}
