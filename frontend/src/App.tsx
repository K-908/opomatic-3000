import { useEffect, useState } from 'react'
import { getHealth, type HealthResponse } from './features/health/api/getHealth'
import { TopicsPage } from './features/topics/TopicsPage'
import { QuestionsPage } from './features/questions/QuestionsPage'
import { QuestionFormPage } from './features/questions/QuestionFormPage'
import './App.css'

type HealthState =
  | { status: 'loading' }
  | { status: 'available'; data: HealthResponse }
  | { status: 'unavailable' }

function App() {
  if (window.location.pathname === '/questions') {
    return <QuestionsPage />
  }

  if (window.location.pathname === '/questions/new') {
    return <QuestionFormPage />
  }

  const editMatch = window.location.pathname.match(/^\/questions\/(\d+)\/edit$/)
  if (editMatch) {
    return <QuestionFormPage questionId={Number(editMatch[1])} />
  }

  if (window.location.pathname === '/topics') {
    return <TopicsPage />
  }

  return <HomePage />
}

function HomePage() {
  const [health, setHealth] = useState<HealthState>({ status: 'loading' })

  useEffect(() => {
    const controller = new AbortController()

    getHealth(controller.signal)
      .then((data) => {
        if (!controller.signal.aborted) {
          setHealth({ status: 'available', data })
        }
      })
      .catch(() => {
        if (!controller.signal.aborted) {
          setHealth({ status: 'unavailable' })
        }
      })

    return () => controller.abort()
  }, [])

  return (
    <main className="app-shell">
      <section className="welcome-card" aria-labelledby="welcome-title">
        <p className="eyebrow">OpoMatic-3000</p>
        <h1 id="welcome-title">Tu preparación, pregunta a pregunta.</h1>
        <p className="welcome-copy">
          Organiza tus materias y construye un banco de preguntas para preparar
          cada test con intención.
        </p>
        <HealthStatus health={health} />
        <a className="primary-link" href="/topics">
          Administrar temas
        </a>
        <a className="primary-link" href="/questions">
          Administrar preguntas
        </a>
      </section>
    </main>
  )
}

interface HealthStatusProps {
  health: HealthState
}

function HealthStatus({ health }: HealthStatusProps) {
  if (health.status === 'unavailable') {
    return (
      <p className="project-status project-status--error" role="alert">
        <span className="status-dot" aria-hidden="true" />
        No se puede conectar con la API. Comprueba que el backend esté
        ejecutándose.
      </p>
    )
  }

  if (health.status === 'available') {
    return (
      <p className="project-status project-status--available" role="status">
        <span className="status-dot" aria-hidden="true" />
        API disponible: {health.data.application}
      </p>
    )
  }

  return (
    <p className="project-status project-status--loading" role="status">
      <span className="status-dot" aria-hidden="true" />
      Comprobando la conexión con la API…
    </p>
  )
}

export default App
