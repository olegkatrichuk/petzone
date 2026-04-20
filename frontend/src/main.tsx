import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter, useLocation } from 'react-router-dom'
import { Provider } from 'react-redux'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { ReactQueryDevtools } from '@tanstack/react-query-devtools'
import { HelmetProvider } from 'react-helmet-async'
import { store } from './store/store'
import { ErrorBoundary } from './components/ErrorBoundary'
import ThemeWrapper from './components/layout/ThemeWrapper'
import './i18n'
import './index.css'
import App from './App.tsx'

const queryClient = new QueryClient({
  defaultOptions: {
    queries: { retry: 1, staleTime: 1000 * 60 * 5 },
  },
})

function AppWithBoundary() {
  const location = useLocation()
  return (
    <ErrorBoundary resetKey={location.key}>
      <App />
    </ErrorBoundary>
  )
}

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <Provider store={store}>
      <QueryClientProvider client={queryClient}>
        <ThemeWrapper>
          <HelmetProvider>
            <BrowserRouter>
              <AppWithBoundary />
            </BrowserRouter>
          </HelmetProvider>
        </ThemeWrapper>
        <ReactQueryDevtools initialIsOpen={false} />
      </QueryClientProvider>
    </Provider>
  </StrictMode>
)