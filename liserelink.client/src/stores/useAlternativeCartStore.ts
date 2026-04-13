import { create } from 'zustand';

export interface AlternativeCartLine {
  articleId: string;
  articleName: string;
  colorOrPrint: string;
  barcode: string;
  size: string;
  quantity: 1;
  stockOverride: boolean;
}

interface AlternativeCartState {
  lines: AlternativeCartLine[];
  requestId: string | null;
  addLine: (line: AlternativeCartLine) => void;
  removeLine: (index: number) => void;
  clearCart: () => void;
  setRequestId: (id: string | null) => void;
}

export const useAlternativeCartStore = create<AlternativeCartState>()((set) => ({
  lines: [],
  requestId: null,
  addLine: (line) => set((state) => ({ lines: [...state.lines, line] })),
  removeLine: (index) =>
    set((state) => ({ lines: state.lines.filter((_, i) => i !== index) })),
  clearCart: () => set({ lines: [], requestId: null }),
  setRequestId: (id) => set({ requestId: id }),
}));
