import { describe, it, expect, beforeEach, expectTypeOf } from 'vitest'
import { useSignalRStore } from '../useSignalRStore'

beforeEach(() => {
  useSignalRStore.setState({ status: 'disconnected' })
})

describe('useSignalRStore — initial state', () => {
  it('status is "disconnected"', () => {
    expect(useSignalRStore.getState().status).toBe('disconnected')
  })
})

describe('useSignalRStore — setStatus', () => {
  it('setStatus("connecting") updates the status', () => {
    useSignalRStore.getState().setStatus('connecting')
    expect(useSignalRStore.getState().status).toBe('connecting')
  })

  it('setStatus("connected") updates the status', () => {
    useSignalRStore.getState().setStatus('connected')
    expect(useSignalRStore.getState().status).toBe('connected')
  })

  it('setStatus("reconnecting") updates the status', () => {
    useSignalRStore.getState().setStatus('reconnecting')
    expect(useSignalRStore.getState().status).toBe('reconnecting')
  })

  it('setStatus("disconnected") reverts to disconnected', () => {
    useSignalRStore.getState().setStatus('connected')
    useSignalRStore.getState().setStatus('disconnected')
    expect(useSignalRStore.getState().status).toBe('disconnected')
  })

  it('setStatus parameter is restricted to valid values — type check', () => {
    const { setStatus } = useSignalRStore.getState()
    expectTypeOf(setStatus).parameter(0).toEqualTypeOf<
      'disconnected' | 'connecting' | 'connected' | 'reconnecting'
    >()
  })
})
