import { useEffect, useRef, useState } from 'react'
import { useGameStore } from '../store/gameStore'
import { postCommand } from '../services/commandsApi'

// Mando con Joystick Analógico 360° Multi-Touch + Barra de Carga de Salto + Botones A, B, X, SELECT y START activos.
export function Gamepad({ disabled, embedded = false }) {
  const [chargePct, setChargePct] = useState(0)
  const chargeTimerRef = useRef(null)
  const chargeStartRef = useRef(0)

  const startCharge = () => {
    clearInterval(chargeTimerRef.current)
    chargeStartRef.current = performance.now()
    chargeTimerRef.current = setInterval(() => {
      const elapsed = (performance.now() - chargeStartRef.current) / 1000
      const pct = Math.min(100, Math.round((elapsed / 0.58) * 100))
      setChargePct(pct)
    }, 25)
  }

  const stopCharge = () => {
    clearInterval(chargeTimerRef.current)
    setChargePct(0)
  }

  useEffect(() => {
    return () => clearInterval(chargeTimerRef.current)
  }, [])

  if (disabled) return null

  return (
    <div
      className={`absolute inset-0 flex items-center justify-center [container-type:size] ${embedded ? 'p-[4%]' : 'p-[2.5vh] px-[3.5vw]'}`}
    >
      <div
        className="grid h-full w-full grid-cols-[auto_minmax(0,1fr)_auto] items-center gap-[2cqw] rounded-[28px] px-[3cqw] sm:rounded-[40px]"
        style={{
          '--dpad': 'min(72cqh, 24cqw)',
          '--face': 'min(34cqh, 11.5cqw)',
          background: 'linear-gradient(180deg, #1b172e 0%, #120f20 65%, #0b0914 100%)',
          border: '2px solid rgba(245, 200, 76, 0.45)',
          boxShadow:
            'inset 0 2px 0 rgba(255,255,255,0.12), inset 0 -8px 16px rgba(0,0,0,0.65), 0 12px 28px rgba(0,0,0,0.45)',
        }}
      >
        <AnalogJoystick />
        <CenterCluster embedded={embedded} chargePct={chargePct} />
        <FaceButtons onJumpPress={startCharge} onJumpRelease={stopCharge} />
      </div>
    </div>
  )
}

function AnalogJoystick() {
  const send = useGameStore((s) => s.send)
  const baseRef = useRef(null)
  const touchIdRef = useRef(null)
  const mouseDownRef = useRef(false)
  const lastVecRef = useRef({ jx: 0, jy: 0 })
  const heldDirsRef = useRef({ LEFT: false, RIGHT: false, UP: false, DOWN: false })
  const [knob, setKnob] = useState({ x: 0, y: 0, active: false, label: 'Joystick 360°' })

  const syncDigitalDirs = (jx, jy) => {
    const nextDirs = {
      RIGHT: jx > 0.24,
      LEFT: jx < -0.24,
      UP: jy > 0.24,
      DOWN: jy < -0.24,
    }
    for (const dir of ['LEFT', 'RIGHT', 'UP', 'DOWN']) {
      if (nextDirs[dir] !== heldDirsRef.current[dir]) {
        heldDirsRef.current[dir] = nextDirs[dir]
        const s = nextDirs[dir] ? 1 : 0
        send({ b: dir, s })
        postCommand(dir, { b: dir, s }).catch(() => {})
      }
    }
  }

  const updateFromClientXY = (clientX, clientY) => {
    const el = baseRef.current
    if (!el) return
    const rect = el.getBoundingClientRect()
    const cx = rect.left + rect.width * 0.5
    const cy = rect.top + rect.height * 0.5
    const maxR = Math.max(18, rect.width * 0.34)

    let dx = clientX - cx
    let dy = clientY - cy
    const dist = Math.hypot(dx, dy)
    if (dist > maxR && dist > 0) {
      dx = (dx / dist) * maxR
      dy = (dy / dist) * maxR
    }

    const jx = Math.round((dx / maxR) * 100) / 100
    const jy = Math.round((-dy / maxR) * 100) / 100

    let label = 'Centro'
    if (Math.hypot(jx, jy) > 0.22) {
      const parts = []
      if (jy > 0.24) parts.push('↑')
      if (jy < -0.24) parts.push('↓')
      if (jx < -0.24) parts.push('←')
      if (jx > 0.24) parts.push('→')
      label = parts.join(' ') || 'Moviendo'
    }

    setKnob({ x: dx, y: dy, active: true, label })

    if (Math.abs(jx - lastVecRef.current.jx) > 0.04 || Math.abs(jy - lastVecRef.current.jy) > 0.04) {
      lastVecRef.current = { jx, jy }
      send({ jx, jy })
      syncDigitalDirs(jx, jy)
    }
  }

  const resetStick = () => {
    touchIdRef.current = null
    mouseDownRef.current = false
    lastVecRef.current = { jx: 0, jy: 0 }
    setKnob({ x: 0, y: 0, active: false, label: 'Joystick 360°' })
    send({ jx: 0, jy: 0 })
    syncDigitalDirs(0, 0)
  }

  useEffect(() => {
    const onMove = (e) => {
      if (mouseDownRef.current) updateFromClientXY(e.clientX, e.clientY)
    }
    const onUp = () => {
      if (mouseDownRef.current) resetStick()
    }
    window.addEventListener('mousemove', onMove)
    window.addEventListener('mouseup', onUp)
    return () => {
      window.removeEventListener('mousemove', onMove)
      window.removeEventListener('mouseup', onUp)
    }
  })

  return (
    <div className="flex shrink-0 flex-col items-center gap-[1cqh]">
      <div
        ref={baseRef}
        onTouchStart={(e) => {
          e.preventDefault()
          if (touchIdRef.current === null && e.changedTouches.length > 0) {
            const t = e.changedTouches[0]
            touchIdRef.current = t.identifier
            updateFromClientXY(t.clientX, t.clientY)
          }
        }}
        onTouchMove={(e) => {
          e.preventDefault()
          for (let i = 0; i < e.changedTouches.length; i++) {
            const t = e.changedTouches[i]
            if (t.identifier === touchIdRef.current) {
              updateFromClientXY(t.clientX, t.clientY)
              break
            }
          }
        }}
        onTouchEnd={(e) => {
          e.preventDefault()
          for (let i = 0; i < e.changedTouches.length; i++) {
            if (e.changedTouches[i].identifier === touchIdRef.current) {
              resetStick()
              break
            }
          }
        }}
        onTouchCancel={(e) => {
          e.preventDefault()
          resetStick()
        }}
        onMouseDown={(e) => {
          e.preventDefault()
          mouseDownRef.current = true
          updateFromClientXY(e.clientX, e.clientY)
        }}
        className="relative flex items-center justify-center rounded-full select-none"
        style={{
          width: 'var(--dpad)',
          height: 'var(--dpad)',
          background: 'radial-gradient(circle, #282240 0%, #131020 78%)',
          border: '3px solid #f5c84c',
          boxShadow: 'inset 0 0 20px rgba(0,0,0,0.85), 0 6px 18px rgba(0,0,0,0.6)',
          touchAction: 'none',
        }}
      >
        <span className="pointer-events-none absolute top-[6%] left-1/2 -translate-x-1/2 text-[calc(var(--dpad)*0.09)] font-black text-amber-300/45">
          ▲
        </span>
        <span className="pointer-events-none absolute bottom-[6%] left-1/2 -translate-x-1/2 text-[calc(var(--dpad)*0.09)] font-black text-amber-300/45">
          ▼
        </span>
        <span className="pointer-events-none absolute top-1/2 left-[7%] -translate-y-1/2 text-[calc(var(--dpad)*0.09)] font-black text-amber-300/45">
          ◀
        </span>
        <span className="pointer-events-none absolute top-1/2 right-[7%] -translate-y-1/2 text-[calc(var(--dpad)*0.09)] font-black text-amber-300/45">
          ▶
        </span>

        <div
          className="pointer-events-none absolute rounded-full"
          style={{
            width: '42%',
            height: '42%',
            transform: `translate(${knob.x.toFixed(1)}px, ${knob.y.toFixed(1)}px)`,
            background: 'radial-gradient(circle at 35% 30%, #ffe680 0%, #d99b1e 75%, #8c5d08 100%)',
            border: '2px solid #fff5cc',
            boxShadow: knob.active
              ? '0 0 22px rgba(245, 200, 76, 0.9)'
              : '0 6px 14px rgba(0, 0, 0, 0.75)',
          }}
        />
      </div>
      <span className="text-[clamp(8px,2.5cqh,11px)] font-extrabold tracking-wider text-amber-200/70 uppercase">
        {knob.label}
      </span>
    </div>
  )
}

function CenterCluster({ embedded, chargePct }) {
  const activeCharacter = useGameStore((s) => s.activeCharacter)
  return (
    <div className="flex min-w-0 flex-col items-center gap-[1.8cqh]">
      {!embedded && (
        <div className="rounded-full border border-emerald-400/40 bg-emerald-400/15 px-3 py-0.5 text-[clamp(8px,2.8cqh,12px)] font-extrabold tracking-wider text-emerald-300 uppercase">
          Líder: {activeCharacter === 'mia' ? 'Mía' : 'Falin'}
        </div>
      )}

      <div className="w-full max-w-[180px] rounded-xl border border-amber-400/35 bg-black/50 p-1.5 text-center">
        <p className="mb-1 text-[clamp(7px,2.3cqh,10px)] font-extrabold tracking-wider text-amber-300">
          CARGA SALTO: {chargePct}%
        </p>
        <div className="h-2.5 w-full overflow-hidden rounded-full bg-white/10">
          <div
            className="h-full rounded-full transition-[width] duration-75"
            style={{
              width: `${chargePct}%`,
              background: 'linear-gradient(90deg, #38e07b 0%, #f5c84c 60%, #f43f5e 100%)',
            }}
          />
        </div>
      </div>

      <div className="flex items-center gap-[1.5cqw]">
        <PadButton btn="SELECT" className="plastic-pill">
          CAMBIAR
        </PadButton>
        <PadButton btn="START" className="plastic-pill">
          PAUSA
        </PadButton>
      </div>
    </div>
  )
}

function FaceButtons({ onJumpPress, onJumpRelease }) {
  return (
    <div className="flex shrink-0 items-center gap-[2cqw]">
      <div className="flex flex-col items-center gap-[2cqh]">
        <PadButton
          btn="X"
          className="plastic-face"
          style={{
            width: 'calc(var(--face) * 0.72)',
            height: 'calc(var(--face) * 0.72)',
            background: 'radial-gradient(circle at 35% 30%, #60a5fa 0%, #2563eb 80%)',
            border: '2px solid #dbeafe',
            fontSize: 'calc(var(--face) * 0.25)',
          }}
        >
          X
        </PadButton>

        <PadButton
          btn="B"
          onDown={onJumpPress}
          onUp={onJumpRelease}
          className="plastic-face"
          style={{
            width: 'calc(var(--face) * 0.84)',
            height: 'calc(var(--face) * 0.84)',
            background: 'radial-gradient(circle at 35% 30%, #fbbf24 0%, #d97706 80%)',
            border: '2px solid #fef3c7',
            color: '#1f1103',
            fontSize: 'calc(var(--face) * 0.30)',
          }}
        >
          B
        </PadButton>
      </div>

      <PadButton
        btn="A"
        onDown={onJumpPress}
        onUp={onJumpRelease}
        className="plastic-face flex flex-col items-center justify-center"
        style={{
          width: 'calc(var(--face) * 1.18)',
          height: 'calc(var(--face) * 1.18)',
          background: 'radial-gradient(circle at 35% 30%, #4df08f 0%, #1ea856 75%, #116b35 100%)',
          border: '3px solid #dcfce7',
          color: '#062111',
          fontSize: 'calc(var(--face) * 0.44)',
        }}
      >
        <span>A</span>
        <span className="text-[calc(var(--face)*0.12)] font-extrabold tracking-wider">SALTAR</span>
      </PadButton>
    </div>
  )
}

function PadButton({ btn, className, style, onDown, onUp, children }) {
  const pad = usePadPress(btn, onDown, onUp)
  return (
    <button
      type="button"
      {...pad.events}
      style={{ touchAction: 'none', ...style }}
      className={`${className} select-none ${pad.pressed ? 'is-down scale-95 brightness-110' : ''}`}
    >
      {children}
    </button>
  )
}

function usePadPress(btn, onDown, onUp) {
  const send = useGameStore((s) => s.send)
  const toggleCharacter = useGameStore((s) => s.toggleCharacter)
  const pressedRef = useRef(false)
  const [pressed, setPressed] = useState(false)

  const press = (event) => {
    if (event?.cancelable) event.preventDefault()
    if (pressedRef.current) return
    pressedRef.current = true
    setPressed(true)
    send({ b: btn, s: 1 })
    postCommand(btn, { b: btn, s: 1 }).catch(() => {})
    if (btn === 'X' || btn === 'SELECT') toggleCharacter?.()
    navigator.vibrate?.(18)
    onDown?.()
  }

  const release = (event) => {
    if (event?.cancelable) event.preventDefault()
    if (!pressedRef.current) return
    pressedRef.current = false
    setPressed(false)
    send({ b: btn, s: 0 })
    postCommand(btn, { b: btn, s: 0 }).catch(() => {})
    onUp?.()
  }

  return {
    pressed,
    events: {
      onTouchStart: press,
      onTouchEnd: release,
      onTouchCancel: release,
      onMouseDown: press,
      onMouseUp: release,
      onMouseLeave: release,
    },
  }
}
