import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import { Provider } from 'react-redux'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { ReactQueryDevtools } from '@tanstack/react-query-devtools'
import { ThemeProvider, createTheme } from '@mui/material/styles'
import CssBaseline from '@mui/material/CssBaseline'
import { HelmetProvider } from 'react-helmet-async'
import { store } from './store/store'
import { ErrorBoundary } from './components/ErrorBoundary'
import './i18n'
import './index.css'
import App from './App.tsx'

// Kept for any remaining non-pets React Query usage (e.g. auth forms)
const queryClient = new QueryClient({
  defaultOptions: {
    queries: { retry: 1, staleTime: 1000 * 60 * 5 },
  },
})

const theme = createTheme({
  palette: {
    primary: { main: '#4f46e5' },
    secondary: { main: '#7c3aed' },
  },
})

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    {/* Redux store — powers RTK Query caches for pets and species */}
    <Provider store={store}>
      <QueryClientProvider client={queryClient}>
        <ThemeProvider theme={theme}>
          <CssBaseline />
          <HelmetProvider>
            <BrowserRouter>
              <ErrorBoundary>
                <App />
              </ErrorBoundary>
            </BrowserRouter>
          </HelmetProvider>
        </ThemeProvider>
        <ReactQueryDevtools initialIsOpen={false} />
      </QueryClientProvider>
    </Provider>
  </StrictMode>
)