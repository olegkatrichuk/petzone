import { useState, useEffect } from 'react'
import Lottie from 'lottie-react'

export default function HeroCatAnimation() {
  const [animationData, setAnimationData] = useState<unknown>(null)

  useEffect(() => {
    fetch('/animations/cat.json')
      .then(r => r.json())
      .then(setAnimationData)
      .catch(() => {})
  }, [])

  if (!animationData) return null

  return (
    <Lottie
      animationData={animationData}
      loop
      autoplay
      style={{ width: '100%', height: '100%' }}
    />
  )
}