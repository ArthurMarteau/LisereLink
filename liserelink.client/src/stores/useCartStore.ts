import { create } from 'zustand';

export interface CartLine {
  articleId: string;
  articleName: string;
  colorOrPrint: string;
  barcode: string;
  size: string;
  quantity: 1;
}

interface CartState {
  lines: CartLine[];
  editRequestId: string | null;
  addLine: (line: CartLine) => void;
  removeLine: (index: number) => void;
  clearCart: () => void;
  setEditRequestId: (id: string | null) => void;
}

export const useCartStore = create<CartState>()((set) => ({
  lines: [],
  editRequestId: null,
  addLine: (line) => set((state) => ({ lines: [...state.lines, line] })),
  removeLine: (index) =>
    set((state) => ({ lines: state.lines.filter((_, i) => i !== index) })),
  clearCart: () => set({ lines: [], editRequestId: null }),
  setEditRequestId: (id) => set({ editRequestId: id }),
}));
