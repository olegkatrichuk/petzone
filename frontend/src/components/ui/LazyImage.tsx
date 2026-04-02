import { useState } from 'react'
import { Box, type SxProps, type Theme } from '@mui/material'

interface Props {
  src: string
  alt: string
  sx?: SxProps<Theme>
}

export default function LazyImage({ src, alt, sx }: Props) {
  const [loaded, setLoaded] = useState(false)

  return (
    <Box sx={{ position: 'relative', overflow: 'hidden', ...sx }}>
      {!loaded && (
        <Box
          sx={{
            position: 'absolute',
            inset: 0,
            bgcolor: '#E5E7EB',
            animation: 'pulse 1.5s ease-in-out infinite',
            '@keyframes pulse': {
              '0%, 100%': { opacity: 1 },
              '50%': { opacity: 0.5 },
            },
          }}
        />
      )}
      <Box
        component="img"
        src={src}
        alt={alt}
        loading="lazy"
        onLoad={() => setLoaded(true)}
        sx={{
          width: '100%',
          height: '100%',
          objectFit: 'cover',
          display: 'block',
          opacity: loaded ? 1 : 0,
          transition: 'opacity 0.3s ease',
        }}
      />
    </Box>
  )
}
