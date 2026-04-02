import Paper from '@mui/material/Paper'
import Box from '@mui/material/Box'
import Skeleton from '@mui/material/Skeleton'
import Divider from '@mui/material/Divider'

export default function VolunteerCardSkeleton() {
  return (
    <Paper elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 3, p: 2.5 }}>
      {/* Avatar + name */}
      <Box sx={{ display: 'flex', gap: 2, alignItems: 'center', mb: 2 }}>
        <Skeleton variant="circular" width={56} height={56} />
        <Box sx={{ flex: 1 }}>
          <Skeleton width="60%" height={24} />
          <Skeleton width="40%" height={20} sx={{ mt: 0.5 }} />
        </Box>
        <Skeleton width={70} height={24} sx={{ borderRadius: 5 }} />
      </Box>

      {/* Description */}
      <Skeleton height={18} />
      <Skeleton width="85%" height={18} sx={{ mb: 2 }} />

      <Divider sx={{ mb: 2 }} />

      {/* Button */}
      <Skeleton height={36} sx={{ borderRadius: 2 }} />
    </Paper>
  )
}
