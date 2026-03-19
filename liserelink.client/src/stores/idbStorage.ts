import { get as idbGet, set as idbSet, del as idbDel } from 'idb-keyval'
import { createJSONStorage } from 'zustand/middleware'

const idbAdapter = {
  getItem: (name: string) => idbGet<string>(name).then((v) => v ?? null),
  setItem: (name: string, value: string) => idbSet(name, value),
  removeItem: (name: string) => idbDel(name),
}

export const idbStorage = createJSONStorage(() => idbAdapter)
