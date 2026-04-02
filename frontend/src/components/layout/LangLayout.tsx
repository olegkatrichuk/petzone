import { useEffect } from 'react'
import { useParams, useNavigate, useLocation } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { isValidLang, DEFAULT_LANG } from '../../lib/langUtils'
import Layout from './Layout'

export default function LangLayout() {
  const { lang } = useParams<{ lang: string }>()
  const navigate = useNavigate()
  const location = useLocation()
  const { i18n } = useTranslation()

  useEffect(() => {
    if (!isValidLang(lang)) {
      // Replace invalid lang segment with default
      const rest = location.pathname.replace(`/${lang}`, '')
      navigate(`/${DEFAULT_LANG}${rest}`, { replace: true })
      return
    }
    if (i18n.language !== lang) {
      i18n.changeLanguage(lang)
    }
  }, [lang])

  if (!isValidLang(lang)) return null

  return <Layout />
}
