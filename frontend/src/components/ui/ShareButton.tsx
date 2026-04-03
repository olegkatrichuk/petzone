import { useState } from 'react'
import IconButton from '@mui/material/IconButton'
import Tooltip from '@mui/material/Tooltip'
import ShareIcon from '@mui/icons-material/Share'
import CheckIcon from '@mui/icons-material/Check'

interface Props {
  title: string
  text?: string
}

export default function ShareButton({ title, text }: Props) {
  const [copied, setCopied] = useState(false)

  const handleShare = async () => {
    const url = window.location.href
    if (navigator.share) {
      try {
        await navigator.share({ title, text, url })
      } catch {
        // user cancelled
      }
      return
    }
    await navigator.clipboard.writeText(url)
    setCopied(true)
    setTimeout(() => setCopied(false), 2000)
  }

  return (
    <Tooltip title={copied ? 'Посилання скопійовано!' : 'Поділитись'}>
      <IconButton onClick={handleShare} size="small" sx={{ color: '#6B7280', '&:hover': { color: '#FF6B6B' } }}>
        {copied ? <CheckIcon fontSize="small" sx={{ color: '#059669' }} /> : <ShareIcon fontSize="small" />}
      </IconButton>
    </Tooltip>
  )
}
