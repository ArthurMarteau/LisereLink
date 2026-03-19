import { describe, it, expect, vi, beforeEach } from 'vitest'

const signalRMocks = vi.hoisted(() => {
  const mockStart = vi.fn().mockResolvedValue(undefined)
  const mockStop = vi.fn().mockResolvedValue(undefined)

  let connectionState = 'Disconnected'

  const mockConnection = {
    start: mockStart,
    stop: mockStop,
    get state() {
      return connectionState
    },
  }

  const mockBuild = vi.fn().mockReturnValue(mockConnection)
  const mockWithAutomaticReconnect = vi.fn().mockReturnValue({ build: mockBuild })
  const mockWithUrl = vi.fn().mockReturnValue({
    withAutomaticReconnect: mockWithAutomaticReconnect,
    build: mockBuild,
  })

  const MockHubConnectionBuilder = vi.fn(function () {
    return { withUrl: mockWithUrl }
  })

  return {
    mockStart,
    mockStop,
    mockConnection,
    MockHubConnectionBuilder,
    mockWithUrl,
    mockWithAutomaticReconnect,
    mockBuild,
    setConnectionState: (s: string) => {
      connectionState = s
    },
  }
})

vi.mock('@/stores/authStore', () => ({
  useAuthStore: {
    getState: () => ({ token: 'mock-token', logout: vi.fn() }),
  },
}))

vi.mock('@microsoft/signalr', () => ({
  HubConnectionBuilder: signalRMocks.MockHubConnectionBuilder,
  HubConnectionState: { Disconnected: 'Disconnected', Connected: 'Connected' },
}))

const { startConnection, stopConnection, getConnection } = await import('../signalRClient')

describe('signalRClient', () => {
  beforeEach(async () => {
    // Reset module-level connection state between tests
    await stopConnection()
    vi.clearAllMocks()
    signalRMocks.setConnectionState('Disconnected')
    // Restore implementations cleared by clearAllMocks
    signalRMocks.mockStart.mockResolvedValue(undefined)
    signalRMocks.mockStop.mockResolvedValue(undefined)
    signalRMocks.mockBuild.mockReturnValue(signalRMocks.mockConnection)
    signalRMocks.mockWithAutomaticReconnect.mockReturnValue({ build: signalRMocks.mockBuild })
    signalRMocks.mockWithUrl.mockReturnValue({
      withAutomaticReconnect: signalRMocks.mockWithAutomaticReconnect,
      build: signalRMocks.mockBuild,
    })
    signalRMocks.MockHubConnectionBuilder.mockImplementation(function () {
      return { withUrl: signalRMocks.mockWithUrl }
    })
  })

  it('startConnection() starts the HubConnection', async () => {
    await startConnection()
    expect(signalRMocks.mockStart).toHaveBeenCalledOnce()
  })

  it('stopConnection() gracefully stops the connection', async () => {
    await startConnection()
    await stopConnection()
    expect(signalRMocks.mockStop).toHaveBeenCalledOnce()
  })

  it('calling startConnection() twice does not create two connections', async () => {
    await startConnection()
    signalRMocks.setConnectionState('Connected')

    await startConnection()

    expect(signalRMocks.MockHubConnectionBuilder).toHaveBeenCalledTimes(1)
    expect(signalRMocks.mockStart).toHaveBeenCalledTimes(1)
  })

  it('reconnection is configured with backoff [0, 2000, 5000, 10000]', async () => {
    await startConnection()
    expect(signalRMocks.mockWithAutomaticReconnect).toHaveBeenCalledWith([0, 2000, 5000, 10000])
  })

  it('accessTokenFactory returns the token from the auth store', async () => {
    await startConnection()
    const options = signalRMocks.mockWithUrl.mock.calls[0][1] as {
      accessTokenFactory: () => string
    }
    expect(options.accessTokenFactory()).toBe('mock-token')
  })

  it('stopConnection() on an already stopped connection does not throw', async () => {
    await expect(stopConnection()).resolves.toBeUndefined()
  })

  it('getConnection() returns null before startConnection() is called', () => {
    expect(getConnection()).toBeNull()
  })
})
