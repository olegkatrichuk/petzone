import { useNavigate, useParams, type NavigateOptions } from 'react-router-dom'
import { DEFAULT_LANG } from '../lib/langUtils'

/**
 * Language-aware navigate. Prepends /:lang to every absolute path.
 * Relative paths and numbers (back/forward) are passed through unchanged.
 */
export function useLangNavigate() {
  const navigate = useNavigate()
  const { lang } = useParams<{ lang: string }>()
  const prefix = `/${lang ?? DEFAULT_LANG}`

  return (to: string | number, options?: NavigateOptions) => {
    if (typeof to === 'number') {
      navigate(to)
      return
    }
    const target = to.startsWith('/') ? `${prefix}${to}` : to
    navigate(target, options)
  }
}
