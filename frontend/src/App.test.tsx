import { render, screen } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import App from './App'
import { getHealth } from './features/health/api/getHealth'

vi.mock('./features/health/api/getHealth', () => ({
  getHealth: vi.fn(),
}))

const getHealthMock = vi.mocked(getHealth)

describe('App health status', () => {
  beforeEach(() => {
    getHealthMock.mockReset()
  })

  it('shows the loading state while the API request is pending', () => {
    getHealthMock.mockReturnValue(new Promise(() => undefined))

    render(<App />)

    expect(
      screen.getByRole('heading', {
        name: 'Tu preparación, pregunta a pregunta.',
      }),
    ).toBeInTheDocument()
    expect(screen.getByRole('status')).toHaveTextContent(
      'Comprobando la conexión con la API',
    )
  })

  it('shows the application name when the API is available', async () => {
    getHealthMock.mockResolvedValue({
      status: 'ok',
      application: 'OpoMatic-3000',
    })

    render(<App />)

    expect(await screen.findByRole('status')).toHaveTextContent(
      'API disponible: OpoMatic-3000',
    )
  })

  it('shows an understandable message when the API is unavailable', async () => {
    getHealthMock.mockRejectedValue(new TypeError('Failed to fetch'))

    render(<App />)

    expect(await screen.findByRole('alert')).toHaveTextContent(
      'No se puede conectar con la API',
    )
  })

  it('aborts the request when the component is unmounted', () => {
    getHealthMock.mockReturnValue(new Promise(() => undefined))
    const { unmount } = render(<App />)
    const signal = getHealthMock.mock.calls[0][0]

    unmount()

    expect(signal?.aborted).toBe(true)
  })
})
