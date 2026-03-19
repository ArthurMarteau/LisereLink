import { create } from 'zustand'

type SignalRStatus = 'disconnected' | 'connecting' | 'connected' | 'reconnecting'

interface SignalRState {
  status: SignalRStatus
  setStatus: (status: SignalRStatus) => void
}

export const useSignalRStore = create<SignalRState>()((set) => ({
  status: 'disconnected',
  setStatus: (status) => set({ status }),
}))
