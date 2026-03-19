import { describe, it, expect, vi, beforeEach } from 'vitest'
import type { InternalAxiosRequestConfig } from 'axios'
import type { ProblemDetails } from '@/types'

// Hoist shared mutable state so it's available inside vi.mock factories
const captured = vi.hoisted(() => ({
  requestSuccessFn: null as null | ((config: InternalAxiosRequestConfig) => InternalAxiosRequestConfig),
  responseSuccessFn: null as null | ((r: unknown) => unknown),
  responseErrorFn: null as null | ((e: unknown) => Promise<never>),
  mockLogout: vi.fn(),
  mockToken: null as string | null,
}))

vi.mock('@/stores/authStore', () => ({
  useAuthStore: {
    getState: () => ({ token: captured.mockToken, logout: captured.mockLogout }),
  },
}))

vi.mock('axios', () => {
  const mockInstance = {
    interceptors: {
      request: {
        use: vi.fn((fn: (config: InternalAxiosRequestConfig) => InternalAxiosRequestConfig) => {
          captured.requestSuccessFn = fn
          return 0
        }),
      },
      response: {
        use: vi.fn(
          (
            onFulfilled: (r: unknown) => unknown,
            onRejected: (e: unknown) => Promise<never>,
          ) => {
            captured.responseSuccessFn = onFulfilled
            captured.responseErrorFn = onRejected
            return 0
          },
        ),
      },
    },
  }
  return {
    default: { create: vi.fn(() => mockInstance) },
  }
})

// Importing the module triggers interceptor registration
await import('../apiClient')

describe('apiClient — request interceptor', () => {
  beforeEach(() => {
    captured.mockToken = null
    captured.mockLogout.mockReset()
  })

  it('injecte Authorization: Bearer <token> si token présent', () => {
    captured.mockToken = 'test-jwt-token'
    const config = { headers: {} } as InternalAxiosRequestConfig
    const result = captured.requestSuccessFn!(config)
    expect((result.headers as Record<string, string>)['Authorization']).toBe('Bearer test-jwt-token')
  })

  it("n'injecte pas Authorization si pas de token", () => {
    captured.mockToken = null
    const config = { headers: {} } as InternalAxiosRequestConfig
    const result = captured.requestSuccessFn!(config)
    expect((result.headers as Record<string, string>)['Authorization']).toBeUndefined()
  })
})

describe('apiClient — response interceptor', () => {
  beforeEach(() => {
    captured.mockToken = null
    captured.mockLogout.mockReset()
    Object.defineProperty(window, 'location', {
      value: { href: 'http://localhost/' },
      writable: true,
      configurable: true,
    })
  })

  it('appelle logout() et redirige vers /login sur 401', async () => {
    const error = { response: { status: 401, data: {} } }
    await expect(captured.responseErrorFn!(error)).rejects.toBeDefined()
    expect(captured.mockLogout).toHaveBeenCalledOnce()
    expect(window.location.href).toBe('/login')
  })

  it('rejette avec ProblemDetails parsé sur 400', async () => {
    const problemDetails: ProblemDetails = {
      type: 'https://api.lisere.app/errors/validation',
      title: 'Requête invalide',
      status: 400,
      detail: 'Le champ est requis.',
      instance: '/api/requests',
    }
    const error = { response: { status: 400, data: problemDetails } }
    await expect(captured.responseErrorFn!(error)).rejects.toEqual(problemDetails)
  })

  it('rejette avec message générique français sur 500', async () => {
    const error = { response: { status: 500, data: {} } }
    await expect(captured.responseErrorFn!(error)).rejects.toMatchObject({
      message: expect.stringContaining('erreur') as unknown,
    })
  })

  it('laisse passer une réponse 200 sans modification', () => {
    const response = { status: 200, data: { id: '1', name: 'Veste' } }
    const result = captured.responseSuccessFn!(response)
    expect(result).toBe(response)
  })

  it('rejette avec ProblemDetails parsé sur 404', async () => {
    const problemDetails: ProblemDetails = {
      type: 'https://api.lisere.app/errors/not-found',
      title: 'Ressource introuvable',
      status: 404,
      detail: 'La demande n\'existe pas.',
      instance: '/api/requests/999',
    }
    const error = { response: { status: 404, data: problemDetails } }
    await expect(captured.responseErrorFn!(error)).rejects.toEqual(problemDetails)
  })

  it('rejette avec message générique français si le body 400 n\'est pas un ProblemDetails valide', async () => {
    const error = { response: { status: 400, data: {} } }
    await expect(captured.responseErrorFn!(error)).rejects.toMatchObject({
      message: expect.stringContaining('erreur') as unknown,
    })
  })

  it('appelle logout() et redirige vers /login sur 401 avec payload token expiré', async () => {
    const error = {
      response: {
        status: 401,
        data: { message: 'Token expired', expiredAt: '2026-03-19T10:00:00Z' },
      },
    }
    await expect(captured.responseErrorFn!(error)).rejects.toBeDefined()
    expect(captured.mockLogout).toHaveBeenCalledOnce()
    expect(window.location.href).toBe('/login')
  })
})
