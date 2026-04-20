import { lazy, type ComponentType, type LazyExoticComponent } from 'react'

/**
 * Wraps React.lazy with automatic full-page reload on chunk load failure.
 * Prevents "Something went wrong" after a new deployment when the browser
 * has a stale index.html referencing old chunk filenames that no longer exist.
 * Uses sessionStorage to avoid infinite reload loops.
 */
let _retrySeq = 0

export function lazyWithRetry<T extends ComponentType<any>>(
  factory: () => Promise<{ default: T }>,
): LazyExoticComponent<T> {
  const key = `chunk-retry:${_retrySeq++}`
  return lazy(() =>
    factory().catch((error: unknown) => {
      if (!sessionStorage.getItem(key)) {
        sessionStorage.setItem(key, '1')
        window.location.reload()
      }
      throw error
    }),
  )
}