/**
 * Escape `<` in a JSON-LD payload so `</script>` substrings inside
 * user-supplied fields can't break out of an inline <script> block.
 * The browser parses `<` back to `<` at runtime — the value is
 * identical, only the byte sequence is safe.
 */
export function safeJsonLd(data: unknown): string {
  return JSON.stringify(data).replace(/</g, '\\u003c')
}
