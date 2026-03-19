import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import { idbStorage } from './idbStorage'
import type { RequestDto } from '@/types'
import { RequestStatus } from '@/constants/enums'

interface RequestState {
  requests: RequestDto[]
  isLoading: boolean
  setRequests: (requests: RequestDto[]) => void
  addRequest: (request: RequestDto) => void
  updateRequest: (id: string, updates: Partial<RequestDto>) => void
  removeRequest: (id: string) => void
  setLoading: (loading: boolean) => void
  selectPendingRequests: () => RequestDto[]
  selectInProgressRequests: () => RequestDto[]
  selectAwaitingResponseRequests: () => RequestDto[]
}

export const useRequestStore = create<RequestState>()(
  persist(
    (set, get) => ({
      requests: [],
      isLoading: false,
      setRequests: (requests) => set({ requests }),
      addRequest: (request) =>
        set((state) => ({ requests: [request, ...state.requests] })),
      updateRequest: (id, updates) =>
        set((state) => ({
          requests: state.requests.map((r) => (r.id === id ? { ...r, ...updates } : r)),
        })),
      removeRequest: (id) =>
        set((state) => ({ requests: state.requests.filter((r) => r.id !== id) })),
      setLoading: (isLoading) => set({ isLoading }),
      selectPendingRequests: () =>
        get().requests.filter((r) => r.status === RequestStatus.Pending),
      selectInProgressRequests: () =>
        get().requests.filter((r) => r.status === RequestStatus.InProgress),
      selectAwaitingResponseRequests: () =>
        get().requests.filter((r) => r.status === RequestStatus.AwaitingSellerResponse),
    }),
    { name: 'lisere-requests', storage: idbStorage },
  ),
)
