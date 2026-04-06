import { motion } from 'framer-motion'

const CORAL = '#FF6B6B'
const CORAL_LIGHT = 'rgba(255,107,107,0.18)'
const WHITE = 'rgba(255,255,255,0.92)'
const WHITE_DIM = 'rgba(255,255,255,0.35)'

export default function HeroCatAnimation() {
  return (
    <svg
      viewBox="0 0 320 320"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
      style={{ width: '100%', height: '100%', overflow: 'visible' }}
      aria-hidden="true"
    >
      {/* ── Floating paw prints ───────────────────────────── */}
      {[
        { x: 30,  y: 60,  delay: 0,   size: 18 },
        { x: 270, y: 80,  delay: 1.2, size: 14 },
        { x: 50,  y: 240, delay: 0.6, size: 12 },
        { x: 260, y: 260, delay: 1.8, size: 16 },
        { x: 155, y: 20,  delay: 2.4, size: 10 },
      ].map(({ x, y, delay, size }) => (
        <motion.g
          key={`${x}-${y}`}
          animate={{ y: [0, -10, 0], opacity: [0.4, 0.9, 0.4] }}
          transition={{ duration: 3.5, repeat: Infinity, ease: 'easeInOut', delay }}
        >
          <PawPrint cx={x} cy={y} size={size} color={CORAL_LIGHT} />
        </motion.g>
      ))}

      {/* ── Glow circle behind cat ────────────────────────── */}
      <motion.circle
        cx="160" cy="185" r="105"
        fill={CORAL_LIGHT}
        animate={{ scale: [1, 1.04, 1], opacity: [0.6, 0.9, 0.6] }}
        transition={{ duration: 4, repeat: Infinity, ease: 'easeInOut' }}
      />

      {/* ── Tail ──────────────────────────────────────────── */}
      <motion.path
        d="M 108 255 Q 55 270 48 230 Q 42 195 80 200"
        stroke={WHITE}
        strokeWidth="13"
        strokeLinecap="round"
        fill="none"
        animate={{ d: [
          'M 108 255 Q 55 270 48 230 Q 42 195 80 200',
          'M 108 255 Q 60 285 52 245 Q 45 210 82 208',
          'M 108 255 Q 55 270 48 230 Q 42 195 80 200',
        ]}}
        transition={{ duration: 2.2, repeat: Infinity, ease: 'easeInOut' }}
      />

      {/* ── Body ──────────────────────────────────────────── */}
      <motion.ellipse
        cx="160" cy="220" rx="72" ry="68"
        fill={WHITE}
        animate={{ scaleY: [1, 1.015, 1] }}
        transition={{ duration: 2.5, repeat: Infinity, ease: 'easeInOut' }}
      />

      {/* ── Belly spot ────────────────────────────────────── */}
      <ellipse cx="160" cy="228" rx="34" ry="30" fill={CORAL_LIGHT} />

      {/* ── Head ──────────────────────────────────────────── */}
      <motion.circle
        cx="160" cy="145" r="62"
        fill={WHITE}
        animate={{ y: [0, -4, 0] }}
        transition={{ duration: 2.5, repeat: Infinity, ease: 'easeInOut' }}
      >
      </motion.circle>

      {/* ── Left ear ──────────────────────────────────────── */}
      <motion.polygon
        points="108,110 92,68 130,95"
        fill={WHITE}
        animate={{ y: [0, -4, 0] }}
        transition={{ duration: 2.5, repeat: Infinity, ease: 'easeInOut' }}
      />
      <motion.polygon
        points="111,107 100,78 126,97"
        fill={CORAL}
        opacity="0.6"
        animate={{ y: [0, -4, 0] }}
        transition={{ duration: 2.5, repeat: Infinity, ease: 'easeInOut' }}
      />

      {/* ── Right ear ─────────────────────────────────────── */}
      <motion.polygon
        points="212,110 228,68 190,95"
        fill={WHITE}
        animate={{ y: [0, -4, 0] }}
        transition={{ duration: 2.5, repeat: Infinity, ease: 'easeInOut' }}
      />
      <motion.polygon
        points="209,107 220,78 194,97"
        fill={CORAL}
        opacity="0.6"
        animate={{ y: [0, -4, 0] }}
        transition={{ duration: 2.5, repeat: Infinity, ease: 'easeInOut' }}
      />

      {/* ── Eyes ──────────────────────────────────────────── */}
      <motion.g
        animate={{ y: [0, -4, 0] }}
        transition={{ duration: 2.5, repeat: Infinity, ease: 'easeInOut' }}
      >
        {/* Left eye */}
        <BlinkingEye cx={140} cy={143} />
        {/* Right eye */}
        <BlinkingEye cx={180} cy={143} />
      </motion.g>

      {/* ── Nose ──────────────────────────────────────────── */}
      <motion.path
        d="M 155 162 L 160 157 L 165 162 Z"
        fill={CORAL}
        animate={{ y: [0, -4, 0] }}
        transition={{ duration: 2.5, repeat: Infinity, ease: 'easeInOut' }}
      />

      {/* ── Mouth ─────────────────────────────────────────── */}
      <motion.path
        d="M 150 166 Q 155 173 160 168 Q 165 173 170 166"
        stroke={WHITE_DIM}
        strokeWidth="2.5"
        strokeLinecap="round"
        fill="none"
        animate={{ y: [0, -4, 0] }}
        transition={{ duration: 2.5, repeat: Infinity, ease: 'easeInOut' }}
      />

      {/* ── Whiskers left ─────────────────────────────────── */}
      <motion.g
        animate={{ y: [0, -4, 0] }}
        transition={{ duration: 2.5, repeat: Infinity, ease: 'easeInOut' }}
      >
        <line x1="100" y1="162" x2="148" y2="165" stroke={WHITE_DIM} strokeWidth="1.8" strokeLinecap="round" />
        <line x1="100" y1="170" x2="148" y2="168" stroke={WHITE_DIM} strokeWidth="1.8" strokeLinecap="round" />
        <line x1="172" y1="165" x2="220" y2="162" stroke={WHITE_DIM} strokeWidth="1.8" strokeLinecap="round" />
        <line x1="172" y1="168" x2="220" y2="170" stroke={WHITE_DIM} strokeWidth="1.8" strokeLinecap="round" />
      </motion.g>

      {/* ── Front paws ────────────────────────────────────── */}
      <motion.g
        animate={{ y: [0, -4, 0] }}
        transition={{ duration: 2.5, repeat: Infinity, ease: 'easeInOut', delay: 0.1 }}
      >
        <ellipse cx="130" cy="278" rx="24" ry="14" fill={WHITE} />
        <ellipse cx="190" cy="278" rx="24" ry="14" fill={WHITE} />
        {/* toe lines */}
        <line x1="118" y1="276" x2="118" y2="282" stroke={CORAL_LIGHT} strokeWidth="2" strokeLinecap="round" />
        <line x1="130" y1="275" x2="130" y2="283" stroke={CORAL_LIGHT} strokeWidth="2" strokeLinecap="round" />
        <line x1="142" y1="276" x2="142" y2="282" stroke={CORAL_LIGHT} strokeWidth="2" strokeLinecap="round" />
        <line x1="178" y1="276" x2="178" y2="282" stroke={CORAL_LIGHT} strokeWidth="2" strokeLinecap="round" />
        <line x1="190" y1="275" x2="190" y2="283" stroke={CORAL_LIGHT} strokeWidth="2" strokeLinecap="round" />
        <line x1="202" y1="276" x2="202" y2="282" stroke={CORAL_LIGHT} strokeWidth="2" strokeLinecap="round" />
      </motion.g>
    </svg>
  )
}

// ── Blinking eye ───────────────────────────────────────────

function BlinkingEye({ cx, cy }: { cx: number; cy: number }) {
  return (
    <motion.g>
      <circle cx={cx} cy={cy} r="10" fill="#1e1b4b" />
      <circle cx={cx + 3} cy={cy - 3} r="3" fill="white" opacity="0.8" />
      {/* Eyelid that blinks */}
      <motion.rect
        x={cx - 10} y={cy - 10}
        width={20} height={20}
        rx={10}
        fill={WHITE}
        animate={{ scaleY: [0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0] }}
        transition={{ duration: 5, repeat: Infinity, ease: 'easeInOut', times: [0, 0.35, 0.38, 0.40, 0.42, 0.45, 0.5, 0.6, 0.7, 0.8, 0.9, 0.95, 1] }}
        style={{ originX: `${cx}px`, originY: `${cy}px` }}
      />
    </motion.g>
  )
}

// ── Paw print ─────────────────────────────────────────────

function PawPrint({ cx, cy, size, color }: { cx: number; cy: number; size: number; color: string }) {
  const s = size / 18
  return (
    <g transform={`translate(${cx}, ${cy}) scale(${s})`}>
      <ellipse cx="0" cy="5" rx="7" ry="8" fill={color} />
      <ellipse cx="-9" cy="-4" rx="4" ry="5" fill={color} />
      <ellipse cx="9" cy="-4" rx="4" ry="5" fill={color} />
      <ellipse cx="-2" cy="-10" rx="3.5" ry="4.5" fill={color} />
      <ellipse cx="2" cy="-10" rx="3.5" ry="4.5" fill={color} transform="translate(5, 0)" />
    </g>
  )
}