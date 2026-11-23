import { useEffect, useState } from 'react'
import { Gamepad2, Monitor, Moon, QrCode, ScrollText, Smartphone, Trophy, WifiOff } from 'lucide-react'
import { useThemeEffect } from '../hooks/useThemeEffect'
import { NavBar } from '../components/landing/NavBar'
import { Hero } from '../components/landing/Hero'
import { Reveal } from '../components/landing/Reveal'
import { InstallButton } from '../components/landing/InstallButton'
import { InstallBanner } from '../components/landing/InstallBanner'
import { Gamepad } from '../components/Gamepad'
import { HonorBoard } from '../components/HonorBoard'

const CHARACTERS = [
  {
    id: 'falin',
    name: 'Falin',
    breed: 'Border Collie',
    tagline: 'La fuerza del bosque.',
    text: 'Ágil y firme. Empuja rocas y troncos para abrir camino, y con su salto impulsado llega a donde nadie más alcanza.',
    skills: ['Empujar objetos', 'Salto impulsado'],
    idle: '/dogs/falin-idle.webp',
    run: '/dogs/falin-walk.webp',
    frame: 252,
    nudgeX: 15,
    nudgeY: 15,
    color: 'text-amber-800 dark:text-amber-400',
    glow: 'from-amber-400/25',
  },
  {
    id: 'mia',
    name: 'Mia',
    breed: 'Schnauzer',
    tagline: 'El olfato del bosque.',
    text: 'Pequeña y lista. Se mete por huecos donde nadie más cabe y su olfato descubre los secretos que el bosque esconde.',
    skills: ['Espacios estrechos', 'Olfato que revela secretos'],
    idle: '/dogs/mia-idle.webp',
    run: '/dogs/mia-walk.webp',
    frame: 264,
    nudgeX: 8,
    nudgeY: 12,
    color: 'text-pink-700 dark:text-pink-400',
    glow: 'from-pink-400/25',
  },
]

const FEATURES = [
  { Icon: Gamepad2, title: 'Control en tiempo real', text: 'Los botones viajan por WebSocket directo a la PC, sin pasar por internet.' },
  { Icon: ScrollText, title: 'Lore del bosque', text: 'Lee las notas que vas encontrando en cada nivel.' },
  { Icon: Trophy, title: 'Cuadro de honor', text: 'Puntaje, tiempo, saltos y caídas de cada partida guardada.' },
  { Icon: WifiOff, title: 'Funciona sin internet', text: 'Lo que ya viste se queda guardado en el celular.' },
  { Icon: Moon, title: 'Modo claro y oscuro', text: 'Para jugar de día o en la noche del bosque.' },
  { Icon: Smartphone, title: 'Se instala como app', text: 'Sin tienda de apps. Se agrega a tu pantalla de inicio.' },
]

const STEPS = [
  { Icon: Monitor, title: 'Abre Kimaya en tu PC', text: 'En el menú aparece un código QR.' },
  { Icon: QrCode, title: 'Escanéalo', text: 'Con la cámara del celular o desde la app.' },
  { Icon: Gamepad2, title: 'A jugar', text: 'Tu celular ya es el control.' },
]

export function LandingPage() {
  useThemeEffect()

  useEffect(() => {
    document.documentElement.classList.add('landing')
    return () => document.documentElement.classList.remove('landing')
  }, [])

  return (
    <div className="bg-surface text-ink">
      <NavBar />
      <Hero />
      <Characters />
      <ControllerShowcase />
      <Features />
      <HowItWorks />
      <HonorSection />
      <InstallSection />
      <footer className="text-muted px-6 py-10 pb-28 text-center text-xs sm:pb-10">
        Kimaya: El Bosque del Eco · Proyecto integrador
      </footer>
      <InstallBanner className="sm:hidden" />
    </div>
  )
}

function SectionTitle({ eyebrow, title, text }) {
  return (
    <Reveal className="mx-auto max-w-3xl text-center">
      <p className="text-accent text-sm font-extrabold uppercase tracking-[0.3em]">{eyebrow}</p>
      <h2 className="mt-3 text-[clamp(36px,6vw,72px)] font-black leading-[1.02] tracking-tight">{title}</h2>
      {text && <p className="text-muted mx-auto mt-5 max-w-xl text-lg">{text}</p>}
    </Reveal>
  )
}

function Characters() {
  return (
    <section id="personajes" className="scroll-mt-14 px-5 py-16 sm:px-6 sm:py-32">
      <SectionTitle eyebrow="Personajes" title="Dos formas de cruzar el bosque." text="Cambia entre Falin y Mia en cualquier momento. Cada uno resuelve el camino a su manera." />
      <div className="mx-auto mt-16 grid max-w-5xl gap-8 md:grid-cols-2">
        {CHARACTERS.map((c) => (
          <Reveal key={c.id} className="min-w-0">
            <CharacterCard character={c} />
          </Reveal>
        ))}
      </div>
    </section>
  )
}

function CharacterCard({ character: c }) {
  const [hover, setHover] = useState(false)
  return (
    <article
      onMouseEnter={() => setHover(true)}
      onMouseLeave={() => setHover(false)}
      className="neu relative w-full min-w-0 overflow-hidden rounded-[36px] p-6 sm:p-8"
    >
      <div className={`absolute inset-x-0 top-0 h-48 bg-gradient-to-b ${c.glow} to-transparent`} />
      <div className="neu-inset relative mx-auto h-48 overflow-hidden rounded-[28px] sm:h-52">
        <img
          src={hover ? c.run : c.idle}
          alt=""
          className="pixel absolute left-1/2 top-1/2 max-w-none"
          style={{
            height: c.frame,
            transform: `translate(calc(-50% + ${c.nudgeX}px), calc(-50% + ${c.nudgeY}px))`,
          }}
        />
      </div>
      <p className={`mt-7 text-sm font-extrabold uppercase tracking-[0.25em] ${c.color}`}>{c.breed}</p>
      <h3 className="mt-1 text-5xl font-black tracking-tight">{c.name}</h3>
      <p className="mt-2 text-xl font-bold">{c.tagline}</p>
      <p className="text-muted mt-3 leading-relaxed">{c.text}</p>
      <ul className="mt-5 flex flex-wrap gap-2">
        {c.skills.map((s) => (
          <li key={s} className="neu-sm rounded-full px-3 py-1.5 text-xs font-bold">
            {s}
          </li>
        ))}
      </ul>
    </article>
  )
}

function ControllerShowcase() {
  return (
    <section id="control" className="scroll-mt-14 px-6 py-24 sm:py-32">
      <SectionTitle
        eyebrow="Companion app"
        title="Tu celular es el control."
        text="Mueve el Joystick 360° para explorar el Santuario o apuntar el salto. Mantén A o B para cargar la barra y suéltalo para saltar. Con X cambias de perrito y con PAUSA abres el menú."
      />

      <Reveal className="mx-auto mt-14 max-w-4xl">
        <div className="rounded-[48px] bg-neutral-900 p-3 shadow-2xl sm:p-4">
          <div className="bg-[#1c1c1c] relative aspect-[4/3] overflow-hidden rounded-[36px] [container-type:size] sm:aspect-[19.5/9]">
            <Gamepad embedded />
          </div>
        </div>
      </Reveal>
    </section>
  )
}

function Features() {
  return (
    <section className="px-6 py-24 sm:py-32">
      <SectionTitle eyebrow="Todo en uno" title="Más que un control." />
      <div className="mx-auto mt-16 grid min-w-0 max-w-5xl gap-6 sm:grid-cols-2 lg:grid-cols-3">
        {FEATURES.map(({ Icon, title, text }, i) => (
          <Reveal key={title} className="min-w-0">
            <div className="neu h-full rounded-[28px] p-7">
              <span className="neu-inset text-accent flex h-12 w-12 items-center justify-center rounded-2xl">
                <Icon size={22} />
              </span>
              <h3 className="mt-5 text-lg font-extrabold">{title}</h3>
              <p className="text-muted mt-2 text-sm leading-relaxed">{text}</p>
            </div>
          </Reveal>
        ))}
      </div>
    </section>
  )
}

function HowItWorks() {
  return (
    <section id="como-funciona" className="scroll-mt-14 px-6 py-24 sm:py-32">
      <SectionTitle eyebrow="Cómo funciona" title="Tres pasos y listo." text="El celular y la PC tienen que estar en la misma red wifi." />
      <div className="mx-auto mt-16 grid max-w-4xl gap-10 sm:grid-cols-3">
        {STEPS.map(({ Icon, title, text }, i) => (
          <Reveal key={title} delay={i * 150} className="text-center">
            <div className="neu text-accent relative mx-auto flex h-24 w-24 items-center justify-center rounded-full">
              <Icon size={34} />
              <span className="bg-accent absolute -right-1 -top-1 flex h-8 w-8 items-center justify-center rounded-full text-sm font-black text-white dark:text-[#1c1c1c]">
                {i + 1}
              </span>
            </div>
            <h3 className="mt-6 text-xl font-extrabold">{title}</h3>
            <p className="text-muted mt-2">{text}</p>
          </Reveal>
        ))}
      </div>
    </section>
  )
}

function HonorSection() {
  return (
    <section id="honor" className="scroll-mt-14 px-5 py-16 sm:px-6 sm:py-32">
      <SectionTitle
        eyebrow="Ranking"
        title="Cuadro de honor."
        text="Todas las partidas que el juego ha guardado: puntaje, tiempo, monedas, vidas, saltos y caídas."
      />
      <div className="mx-auto mt-12 max-w-3xl">
        <HonorBoard />
      </div>
    </section>
  )
}

function InstallSection() {
  return (
    <section id="instalar" className="scroll-mt-14 px-6 py-24 sm:py-32">
      <Reveal className="neu mx-auto max-w-4xl rounded-[48px] px-8 py-16 text-center sm:px-16">
        <img src="/icon-192.png" alt="" className="pixel neu mx-auto h-24 w-24 rounded-[26px] p-1.5" />
        <h2 className="mt-8 text-[clamp(36px,6vw,64px)] font-black leading-[1.02] tracking-tight">Tu control, a un toque.</h2>
        <p className="text-muted mx-auto mt-4 max-w-md text-lg">Instálalo en tu celular. Sin tiendas, sin cuentas, gratis.</p>
        <div className="mt-10 flex flex-col items-center gap-4">
          <InstallButton size="lg" />
          <a href="/?app=1" className="text-accent text-sm font-bold">
            O ábrelo en el navegador →
          </a>
        </div>
      </Reveal>
    </section>
  )
}
