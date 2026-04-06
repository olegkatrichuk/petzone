import { useState, useEffect } from 'react'
import { motion, AnimatePresence } from 'framer-motion'
import Fab from '@mui/material/Fab'
import Tooltip from '@mui/material/Tooltip'
import KeyboardArrowUpIcon from '@mui/icons-material/KeyboardArrowUp'
import { useTranslation } from 'react-i18next'

export default function ScrollToTopFab() {
  const { t } = useTranslation()
  const [visible, setVisible] = useState(false)

  useEffect(() => {
    const onScroll = () => setVisible(window.scrollY > 320)
    window.addEventListener('scroll', onScroll, { passive: true })
    return () => window.removeEventListener('scroll', onScroll)
  }, [])

  return (
    <AnimatePresence>
      {visible && (
        <motion.div
          initial={{ opacity: 0, scale: 0.6, y: 16 }}
          animate={{ opacity: 1, scale: 1, y: 0 }}
          exit={{ opacity: 0, scale: 0.6, y: 16 }}
          transition={{ duration: 0.25, ease: 'easeOut' }}
          style={{ position: 'fixed', bottom: 28, right: 24, zIndex: 1200 }}
        >
          <Tooltip title={t('ui.scrollToTop')} placement="left">
            <Fab
              size="medium"
              onClick={() => window.scrollTo({ top: 0, behavior: 'smooth' })}
              sx={{
                bgcolor: '#FF6B6B',
                color: 'white',
                boxShadow: '0 4px 16px rgba(255,107,107,0.45)',
                '&:hover': { bgcolor: '#e55555', transform: 'translateY(-2px)' },
                transition: 'transform 0.2s, background-color 0.2s',
              }}
            >
              <KeyboardArrowUpIcon />
            </Fab>
          </Tooltip>
        </motion.div>
      )}
    </AnimatePresence>
  )
}