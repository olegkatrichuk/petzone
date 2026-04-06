import { useEffect, useRef } from 'react'

export default function HeroCatAnimation() {
  const containerRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    if (!containerRef.current) return
    const container = containerRef.current
    let anim: { destroy: () => void } | null = null

    Promise.all([
      import('lottie-web').then(m => m.default ?? m),
      fetch('/animations/cat.json').then(r => r.json()),
    ]).then(([lottie, animationData]) => {
      if (!container) return
      anim = (lottie as typeof import('lottie-web').default).loadAnimation({
        container,
        animationData,
        renderer: 'svg',
        loop: true,
        autoplay: true,
      })
    }).catch(() => {})

    return () => { anim?.destroy() }
  }, [])

  return <div ref={containerRef} style={{ width: '100%', height: '100%' }} />
}