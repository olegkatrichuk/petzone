const ALLOWED_PROTOCOLS = new Set(['http:', 'https:', 'mailto:', 'tel:'])

/**
 * Returns the input URL if its scheme is in the allowlist, otherwise undefined.
 * Prevents `javascript:` / `data:` / `vbscript:` URLs from being bound to `href`
 * or `src`, which React 19 no longer sanitizes at render time.
 *
 * Use on any URL that originates from user-provided fields (social networks,
 * external links, avatars, blog cover images, etc.).
 */
export function safeHref(value: string | null | undefined): string | undefined {
  if (!value) return undefined
  const trimmed = value.trim()
  if (!trimmed) return undefined

  // Protocol-relative (`//host`) and scheme-less paths (`/path`, `path`) are OK —
  // the browser resolves them against the current origin.
  if (trimmed.startsWith('/') || trimmed.startsWith('#')) return trimmed
  if (!/^[a-z][a-z0-9+.-]*:/i.test(trimmed)) {
    // No scheme — treat as http URL.
    return `https://${trimmed}`
  }

  try {
    const url = new URL(trimmed)
    return ALLOWED_PROTOCOLS.has(url.protocol) ? trimmed : undefined
  } catch {
    return undefined
  }
}
