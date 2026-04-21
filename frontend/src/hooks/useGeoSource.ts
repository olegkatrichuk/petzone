import { useEffect, useState } from 'react'

export type CountrySource = 'ua' | 'pl' | 'de' | 'fr' | 'other'

const COUNTRY_MAP: Record<string, CountrySource> = {
  UA: 'ua',
  PL: 'pl',
  DE: 'de',
  FR: 'fr',
}

const CACHE_KEY = 'petzone_geo_source'
const CACHE_TTL_MS = 24 * 60 * 60 * 1000

interface CacheEntry {
  source: CountrySource
  timestamp: number
}

function getCached(): CountrySource | null {
  try {
    const raw = localStorage.getItem(CACHE_KEY)
    if (!raw) return null
    const entry: CacheEntry = JSON.parse(raw)
    if (Date.now() - entry.timestamp > CACHE_TTL_MS) return null
    return entry.source
  } catch {
    return null
  }
}

function setCache(source: CountrySource) {
  try {
    localStorage.setItem(CACHE_KEY, JSON.stringify({ source, timestamp: Date.now() }))
  } catch {
    // ignore
  }
}

export function useGeoSource(): CountrySource {
  const [source, setSource] = useState<CountrySource>(() => getCached() ?? 'ua')

  useEffect(() => {
    if (getCached() !== null) return

    fetch('https://ipapi.co/json/')
      .then((res) => res.json())
      .then((data) => {
        const detected: CountrySource = COUNTRY_MAP[data?.country_code as string] ?? 'other'
        setCache(detected)
        setSource(detected)
      })
      .catch(() => {
        setCache('ua')
      })
  }, [])

  return source
}