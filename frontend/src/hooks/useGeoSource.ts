import { useEffect, useState } from 'react'

type PetSource = 'local' | 'imported'

const CACHE_KEY = 'petzone_geo_source'
const CACHE_TTL_MS = 24 * 60 * 60 * 1000 // 24 hours

interface CacheEntry {
  source: PetSource
  timestamp: number
}

function getCached(): PetSource | null {
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

function setCache(source: PetSource) {
  try {
    const entry: CacheEntry = { source, timestamp: Date.now() }
    localStorage.setItem(CACHE_KEY, JSON.stringify(entry))
  } catch {
    // ignore
  }
}

export function useGeoSource(): PetSource {
  const [source, setSource] = useState<PetSource>(() => getCached() ?? 'local')

  useEffect(() => {
    if (getCached() !== null) return

    fetch('https://ipapi.co/json/')
      .then((res) => res.json())
      .then((data) => {
        const detected: PetSource = data?.country_code === 'UA' ? 'local' : 'imported'
        setCache(detected)
        setSource(detected)
      })
      .catch(() => {
        // fallback to local on error
        setCache('local')
      })
  }, [])

  return source
}