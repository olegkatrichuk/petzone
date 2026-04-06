import { useMemo } from 'react'
import { ThemeProvider, createTheme } from '@mui/material/styles'
import CssBaseline from '@mui/material/CssBaseline'
import { useThemeStore } from '../../store/themeStore'

export default function ThemeWrapper({ children }: { children: React.ReactNode }) {
  const { mode } = useThemeStore()

  const theme = useMemo(() => createTheme({
    palette: {
      mode,
      primary: { main: '#4f46e5' },
      secondary: { main: '#7c3aed' },
      ...(mode === 'dark' ? {
        background: { default: '#0d0c1d', paper: '#16133a' },
      } : {}),
    },
    components: {
      MuiCard: {
        styleOverrides: {
          root: mode === 'dark' ? {
            backgroundImage: 'none',
            backgroundColor: '#1a1740',
            borderColor: 'rgba(255,255,255,0.08)',
          } : {},
        },
      },
      MuiPaper: {
        styleOverrides: {
          root: mode === 'dark' ? { backgroundImage: 'none' } : {},
        },
      },
    },
  }), [mode])

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      {children}
    </ThemeProvider>
  )
}