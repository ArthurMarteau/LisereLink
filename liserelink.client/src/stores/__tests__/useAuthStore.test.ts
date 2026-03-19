import { describe, it, expect, beforeEach } from 'vitest'
import { useAuthStore, selectIsAuthenticated, selectHasRole, selectHasZone } from '../authStore'
import { UserRole, ZoneType } from '@/constants/enums'
import type { UserDto } from '@/types'

const initialState = {
  token: null,
  user: null,
  selectedStoreId: null,
  selectedStoreName: null,
  selectedZone: null,
}

const mockUser: UserDto = {
  id: 'user-1',
  firstName: 'Alice',
  lastName: 'Martin',
  role: UserRole.Seller,
  assignedZone: ZoneType.RTW,
}

beforeEach(() => {
  useAuthStore.setState(initialState)
  localStorage.clear()
})

describe('useAuthStore — initial state', () => {
  it('token is null', () => {
    expect(useAuthStore.getState().token).toBeNull()
  })

  it('user is null', () => {
    expect(useAuthStore.getState().user).toBeNull()
  })

  it('selectedStoreId is null', () => {
    expect(useAuthStore.getState().selectedStoreId).toBeNull()
  })

  it('selectedStoreName is null', () => {
    expect(useAuthStore.getState().selectedStoreName).toBeNull()
  })

  it('selectedZone is null', () => {
    expect(useAuthStore.getState().selectedZone).toBeNull()
  })
})

describe('useAuthStore — setToken', () => {
  it('stores the token', () => {
    useAuthStore.getState().setToken('my-jwt')
    expect(useAuthStore.getState().token).toBe('my-jwt')
  })

  it('clears the token when null is passed', () => {
    useAuthStore.getState().setToken('my-jwt')
    useAuthStore.getState().setToken(null)
    expect(useAuthStore.getState().token).toBeNull()
  })
})

describe('useAuthStore — setUser', () => {
  it('stores the user with role and assignedZone', () => {
    useAuthStore.getState().setUser(mockUser)
    expect(useAuthStore.getState().user).toEqual(mockUser)
  })

  it('does not modify selectedStoreId or selectedZone', () => {
    useAuthStore.getState().setStore('store-99', 'Boutique Paris')
    useAuthStore.getState().setZone(ZoneType.Checkout)
    useAuthStore.getState().setUser(mockUser)
    expect(useAuthStore.getState().selectedStoreId).toBe('store-99')
    expect(useAuthStore.getState().selectedZone).toBe(ZoneType.Checkout)
  })
})

describe('useAuthStore — setStore', () => {
  it('stores selectedStoreId and selectedStoreName together', () => {
    useAuthStore.getState().setStore('store-1', 'Boutique Lyon')
    expect(useAuthStore.getState().selectedStoreId).toBe('store-1')
    expect(useAuthStore.getState().selectedStoreName).toBe('Boutique Lyon')
  })

  it('does not modify the existing token or user', () => {
    useAuthStore.getState().setToken('tok-abc')
    useAuthStore.getState().setUser(mockUser)
    useAuthStore.getState().setStore('store-1', 'Boutique Lyon')
    expect(useAuthStore.getState().token).toBe('tok-abc')
    expect(useAuthStore.getState().user).toEqual(mockUser)
  })
})

describe('useAuthStore — setZone', () => {
  it('stores the zone', () => {
    useAuthStore.getState().setZone(ZoneType.FittingRooms)
    expect(useAuthStore.getState().selectedZone).toBe(ZoneType.FittingRooms)
  })

  it('clears the zone when null is passed', () => {
    useAuthStore.getState().setZone(ZoneType.FittingRooms)
    useAuthStore.getState().setZone(null)
    expect(useAuthStore.getState().selectedZone).toBeNull()
  })
})

describe('useAuthStore — logout', () => {
  it('resets all state fields to null', () => {
    useAuthStore.getState().setToken('tok-xyz')
    useAuthStore.getState().setUser(mockUser)
    useAuthStore.getState().setStore('store-1', 'Boutique Bordeaux')
    useAuthStore.getState().setZone(ZoneType.Reception)
    useAuthStore.getState().logout()
    const { token, user, selectedStoreId, selectedStoreName, selectedZone } =
      useAuthStore.getState()
    expect(token).toBeNull()
    expect(user).toBeNull()
    expect(selectedStoreId).toBeNull()
    expect(selectedStoreName).toBeNull()
    expect(selectedZone).toBeNull()
  })

  it('works from a partially filled state (token set, no store)', () => {
    useAuthStore.getState().setToken('tok-partial')
    useAuthStore.getState().logout()
    expect(useAuthStore.getState().token).toBeNull()
    expect(useAuthStore.getState().selectedStoreId).toBeNull()
  })
})

describe('useAuthStore — selectIsAuthenticated', () => {
  it('returns true when token is present and non-empty', () => {
    useAuthStore.getState().setToken('valid-token')
    expect(selectIsAuthenticated(useAuthStore.getState())).toBe(true)
  })

  it('returns false when token is null', () => {
    expect(selectIsAuthenticated(useAuthStore.getState())).toBe(false)
  })

  it('returns false when token is an empty string', () => {
    useAuthStore.getState().setToken('')
    expect(selectIsAuthenticated(useAuthStore.getState())).toBe(false)
  })
})

describe('useAuthStore — selectHasRole', () => {
  it('returns true when user role matches', () => {
    useAuthStore.getState().setUser(mockUser)
    expect(selectHasRole(UserRole.Seller)(useAuthStore.getState())).toBe(true)
  })

  it('returns false when user role does not match', () => {
    useAuthStore.getState().setUser(mockUser)
    expect(selectHasRole(UserRole.Admin)(useAuthStore.getState())).toBe(false)
  })

  it('returns false when user is null', () => {
    expect(selectHasRole(UserRole.Seller)(useAuthStore.getState())).toBe(false)
  })
})

describe('useAuthStore — selectHasZone', () => {
  it('returns true when a zone is selected', () => {
    useAuthStore.getState().setZone(ZoneType.RTW)
    expect(selectHasZone(useAuthStore.getState())).toBe(true)
  })

  it('returns false when no zone is selected', () => {
    expect(selectHasZone(useAuthStore.getState())).toBe(false)
  })
})
