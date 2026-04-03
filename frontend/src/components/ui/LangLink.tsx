import { Link, useParams } from 'react-router-dom'
import { DEFAULT_LANG } from '../../lib/langUtils'

type Props = React.ComponentProps<typeof Link> & { to: string }

export function LangLink({ to, ...props }: Props) {
  const { lang } = useParams<{ lang: string }>()
  const prefix = `/${lang ?? DEFAULT_LANG}`
  const href = to.startsWith('/') ? `${prefix}${to}` : to
  return <Link to={href} {...props} />
}
