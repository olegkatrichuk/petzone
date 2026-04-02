import { useTranslation } from 'react-i18next'
import MuiPagination from '@mui/material/Pagination'
import Box from '@mui/material/Box'
import Typography from '@mui/material/Typography'

const CORAL = '#FF6B6B'

interface Props {
  page: number
  pageSize: number
  totalCount: number
  onChange: (page: number) => void
  ofLabel?: string
}

export default function Pagination({ page, pageSize, totalCount, onChange, ofLabel }: Props) {
  const { t } = useTranslation()
  const totalPages = Math.ceil(totalCount / pageSize)
  const from = (page - 1) * pageSize + 1
  const to = Math.min(page * pageSize, totalCount)

  if (totalPages <= 1) return null

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 1, mt: 4 }}>
      <Typography variant="body2" color="text.secondary">
        {from}–{to} {ofLabel ?? t('pets.of')} {totalCount}
      </Typography>
      <MuiPagination
        count={totalPages}
        page={page}
        onChange={(_, value) => onChange(value)}
        sx={{
          '& .MuiPaginationItem-root.Mui-selected': {
            bgcolor: CORAL,
            color: 'white',
            '&:hover': { bgcolor: '#e55555' },
          },
          '& .MuiPaginationItem-root:hover': {
            bgcolor: 'rgba(255,107,107,0.1)',
          },
        }}
      />
    </Box>
  )
}
