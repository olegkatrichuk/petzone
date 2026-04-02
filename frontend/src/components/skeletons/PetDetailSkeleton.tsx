import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Grid from '@mui/material/Grid'
import Paper from '@mui/material/Paper'
import Skeleton from '@mui/material/Skeleton'

export default function PetDetailSkeleton() {
  return (
    <Box sx={{ bgcolor: '#FAFAFA', minHeight: '100%', py: 4 }}>
      <Container maxWidth="lg">
        <Skeleton width={160} height={32} sx={{ mb: 3 }} />
        <Grid container spacing={4}>
          {/* Left: photo */}
          <Grid size={{ xs: 12, md: 6 }}>
            <Paper elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 3, overflow: 'hidden' }}>
              <Skeleton variant="rectangular" height={{ xs: 240, sm: 340, md: 420 } as never} />
              <Box sx={{ display: 'flex', gap: 1, p: 1.5 }}>
                {[0, 1, 2].map((i) => <Skeleton key={i} variant="rectangular" width={72} height={56} sx={{ borderRadius: 1.5 }} />)}
              </Box>
            </Paper>
          </Grid>

          {/* Right: info */}
          <Grid size={{ xs: 12, md: 6 }}>
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2.5 }}>
              <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                <Box>
                  <Skeleton width={180} height={40} />
                  <Skeleton width={100} height={24} sx={{ mt: 0.5 }} />
                </Box>
                <Skeleton variant="circular" width={40} height={40} />
              </Box>

              <Paper elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 2, p: 2 }}>
                <Skeleton width={120} height={18} sx={{ mb: 1.5 }} />
                <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 1.2 }}>
                  {[0, 1, 2, 3].map((i) => <Skeleton key={i} height={24} />)}
                </Box>
              </Paper>

              <Paper elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 2, p: 2 }}>
                <Skeleton width={80} height={18} sx={{ mb: 1.5 }} />
                <Box sx={{ display: 'flex', gap: 2 }}>
                  <Skeleton width="45%" height={32} sx={{ borderRadius: 2 }} />
                  <Skeleton width="45%" height={32} sx={{ borderRadius: 2 }} />
                </Box>
              </Paper>

              <Paper elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 2, p: 2 }}>
                <Skeleton width={100} height={18} sx={{ mb: 1 }} />
                <Skeleton height={18} />
                <Skeleton width="80%" height={18} />
                <Skeleton width="60%" height={18} />
              </Paper>
            </Box>
          </Grid>
        </Grid>
      </Container>
    </Box>
  )
}
