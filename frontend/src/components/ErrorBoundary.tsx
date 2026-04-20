import { Component, type ErrorInfo, type ReactNode } from 'react'
import { Box, Button, Typography } from '@mui/material'
import WarningAmberIcon from '@mui/icons-material/WarningAmber'

interface Props {
  children: ReactNode
  fallback?: ReactNode
  resetKey?: string
}

interface State {
  hasError: boolean
  error: Error | null
}

export class ErrorBoundary extends Component<Props, State> {
  state: State = { hasError: false, error: null }

  static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error }
  }

  componentDidUpdate(prevProps: Props) {
    if (prevProps.resetKey !== this.props.resetKey && this.state.hasError) {
      this.setState({ hasError: false, error: null })
    }
  }

  componentDidCatch(error: Error, info: ErrorInfo) {
    console.error('[ErrorBoundary]', error, info.componentStack)
  }

  handleReset = () => {
    this.setState({ hasError: false, error: null })
  }

  render() {
    if (this.state.hasError) {
      if (this.props.fallback) return this.props.fallback

      return (
        <Box
          sx={{
            minHeight: '60vh',
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            justifyContent: 'center',
            gap: 2,
            px: 3,
            textAlign: 'center',
          }}
        >
          <WarningAmberIcon sx={{ fontSize: 64, color: '#FF6B6B' }} />
          <Typography variant="h5" fontWeight={700} color="text.primary">
            Щось пішло не так
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ maxWidth: 400 }}>
            {this.state.error?.message ?? 'Невідома помилка'}
          </Typography>
          <Box sx={{ display: 'flex', gap: 2, mt: 1 }}>
            <Button
              variant="contained"
              onClick={this.handleReset}
              sx={{ bgcolor: '#FF6B6B', '&:hover': { bgcolor: '#e55555' } }}
            >
              Спробувати знову
            </Button>
            <Button variant="outlined" onClick={() => (window.location.href = '/')}>
              На головну
            </Button>
          </Box>
        </Box>
      )
    }

    return this.props.children
  }
}
