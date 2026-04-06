import { useMemo, useEffect } from 'react'
import { ThemeProvider, createTheme } from '@mui/material/styles'
import CssBaseline from '@mui/material/CssBaseline'
import GlobalStyles from '@mui/material/GlobalStyles'
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

  useEffect(() => {
    document.documentElement.setAttribute('data-theme', mode)
  }, [mode])

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <GlobalStyles styles={mode === 'dark' ? {
        // Fix remaining hardcoded dark text colors on light backgrounds
        '[data-theme="dark"] h1, [data-theme="dark"] h2, [data-theme="dark"] h3, [data-theme="dark"] h4, [data-theme="dark"] h5, [data-theme="dark"] h6': {
          color: 'inherit',
        },
        // Chip default backgrounds
        '[data-theme="dark"] .MuiChip-root:not([class*="MuiChip-color"])': {
          backgroundColor: 'rgba(255,255,255,0.08)',
          color: 'rgba(255,255,255,0.85)',
        },
      } : {}} />
      {children}
    </ThemeProvider>
  )
}