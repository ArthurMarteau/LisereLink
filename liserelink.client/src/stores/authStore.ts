import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { UserDto } from '@/types';
import type { UserRole, ZoneType } from '@/constants/enums';

// State shape — données uniquement, pas de logique
interface AuthState {
  token: string | null;
  user: UserDto | null;
  selectedStoreId: string | null;
  selectedStoreName: string | null;
  selectedZone: ZoneType | null;
  setToken: (token: string | null) => void;
  setUser: (user: UserDto) => void;
  setStore: (storeId: string, storeName: string) => void;
  setZone: (zone: ZoneType | null) => void;
  logout: () => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      token: null,
      user: null,
      selectedStoreId: null,
      selectedStoreName: null,
      selectedZone: null,
      setToken: (token) => set({ token }),
      setUser: (user) => set({ user }),
      setStore: (storeId, storeName) =>
        set({ selectedStoreId: storeId, selectedStoreName: storeName }),
      setZone: (zone) => set({ selectedZone: zone }),
      logout: () =>
        set({
          token: null,
          user: null,
          selectedStoreId: null,
          selectedStoreName: null,
          selectedZone: null,
        }),
    }),
    { name: 'lisere-auth' }
  )
);

// Selectors — fonctions externes, usage simple dans les composants :
// const isAuth = useAuthStore(selectIsAuthenticated)
export const selectIsAuthenticated = (state: AuthState): boolean =>
  state.token !== null && state.token !== '';

export const selectHasRole =
  (role: UserRole) =>
  (state: AuthState): boolean =>
    state.user?.role === role;

export const selectHasZone = (state: AuthState): boolean => state.selectedZone !== null;
