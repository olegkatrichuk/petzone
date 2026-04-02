import Card from '@mui/material/Card'
import CardContent from '@mui/material/CardContent'
import Skeleton from '@mui/material/Skeleton'
import Box from '@mui/material/Box'
import Divider from '@mui/material/Divider'

export default function PetCardSkeleton() {
  return (
    <Card elevation={0} sx={{ border: '1px solid #E5E7EB', borderRadius: 3 }}>
      {/* Photo */}
      <Skeleton variant="rectangular" height={220} />

      <CardContent>
        {/* Name + status */}
        <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1.5 }}>
          <Skeleton width="55%" height={28} />
          <Skeleton width="28%" height={24} sx={{ borderRadius: 5 }} />
        </Box>

        {/* Color chip */}
        <Skeleton width="30%" height={24} sx={{ borderRadius: 5, mb: 1.5 }} />

        {/* Info rows */}
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.8, mb: 1.5 }}>
          <Skeleton width="45%" height={20} />
          <Skeleton width="55%" height={20} />
          <Skeleton width="50%" height={20} />
        </Box>

        {/* Weight + height */}
        <Box sx={{ display: 'flex', gap: 2, mb: 1.5 }}>
          <Skeleton width="25%" height={20} />
          <Skeleton width="25%" height={20} />
        </Box>

        {/* Description */}
        <Skeleton height={20} />
        <Skeleton width="80%" height={20} sx={{ mb: 1.5 }} />

        <Divider sx={{ mb: 1.5 }} />
        <Skeleton width="40%" height={20} />
      </CardContent>

      {/* Actions */}
      <Box sx={{ px: 2, pb: 2, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Skeleton width={90} height={36} sx={{ borderRadius: 2 }} />
        <Skeleton variant="circular" width={36} height={36} />
      </Box>
    </Card>
  )
}
