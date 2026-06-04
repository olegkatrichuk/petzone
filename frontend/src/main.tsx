import { StrictMode, lazy, Suspense } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter, useLocation } from 'react-router-dom'
import { Provider } from 'react-redux'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
// Dev-only: the import sits in a branch that is statically false in prod,
// so Rollup drops the devtools package from the production bundle entirely.
const ReactQueryDevtools = import.meta.env.DEV
  ? lazy(() => import('@tanstack/react-query-devtools').then((m) => ({ default: m.ReactQueryDevtools })))
  : null
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
        {ReactQueryDevtools && (
          <Suspense>
            <ReactQueryDevtools initialIsOpen={false} />
          </Suspense>
        )}
      </QueryClientProvider>
    </Provider>
  </StrictMode>
)