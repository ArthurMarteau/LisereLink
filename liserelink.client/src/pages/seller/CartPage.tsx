import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Trash2 } from 'lucide-react';
import { toast } from 'sonner';
import apiClient from '@/services/apiClient';
import { useCartStore } from '@/stores/useCartStore';
import { useAuthStore } from '@/stores/authStore';
import { useRequestStore } from '@/stores/useRequestStore';
import type { CartLine } from '@/stores/useCartStore';
import type { ProblemDetails, RequestDto } from '@/types';

function isProblemDetails(error: unknown): error is ProblemDetails {
  return (
    error !== null &&
    typeof error === 'object' &&
    'title' in error &&
    'status' in error
  );
}

function buildLines(lines: CartLine[]) {
  return lines.map((line) => ({
    articleId: line.articleId,
    articleName: line.articleName,
    articleColorOrPrint: line.colorOrPrint,
    articleBarcode: line.barcode,
    size: line.size,
    quantity: 1,
  }));
}

export default function CartPage() {
  const navigate = useNavigate();
  const editRequestId = useCartStore((s) => s.editRequestId);
  const lines = useCartStore((s) => s.lines);
  const removeLine = useCartStore((s) => s.removeLine);
  const clearCart = useCartStore((s) => s.clearCart);
  const addRequest = useRequestStore((s) => s.addRequest);
  const removeRequest = useRequestStore((s) => s.removeRequest);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit() {
    if (isSubmitting) return;

    if (lines.length === 0) {
      toast.error('Ajoutez au moins un article à votre demande.');
      return;
    }

    const { selectedStoreId, selectedZone, user } = useAuthStore.getState();
    if (!selectedStoreId || selectedZone === null) return;

    setIsSubmitting(true);
    try {
      if (editRequestId) {
        await apiClient.delete(`/requests/${editRequestId}`);
        removeRequest(editRequestId);
      }

      const response = await apiClient.post<RequestDto>('/requests', {
        sellerId: user!.id,
        storeId: selectedStoreId,
        zone: selectedZone,
        lines: buildLines(lines),
      });
      addRequest(response.data);
      clearCart();
      navigate('/requests');
    } catch (error) {
      const message = isProblemDetails(error)
        ? error.detail
        : 'Impossible de créer la demande.';
      toast.error(message);
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="bg-[#f9f4ef] min-h-full pb-48">
      {/* Header */}
      <div className="px-5 pt-6 pb-4">
        <p className="font-[Oswald] text-[11px] tracking-[2.5px] uppercase text-[#969696] mb-1">
          {editRequestId ? 'Modifier la demande' : 'Ma demande'}
        </p>
        <p className="font-[Oswald] text-[13px] tracking-[2px] uppercase text-[#121212]">
          {lines.length} article{lines.length !== 1 ? 's' : ''}
        </p>
      </div>

      {/* Empty state */}
      {lines.length === 0 && (
        <div className="px-5 py-10 text-center">
          <p className="font-[Oswald] text-[11px] tracking-[2px] uppercase text-[#969696]">
            Ajoutez au moins un article à votre demande
          </p>
        </div>
      )}

      {/* Cart lines */}
      {lines.length > 0 && (
        <ul className="px-5 space-y-2">
          {lines.map((line, index) => (
            <li
              key={index}
              className="bg-white px-4 py-4 flex items-center justify-between gap-3"
            >
              <div className="flex-1 min-w-0">
                <p className="font-['Libre_Baskerville'] text-[14px] text-[#121212] truncate">
                  {line.articleName}
                </p>
                <p className="font-[Oswald] text-[11px] tracking-[1.5px] uppercase text-[#969696] mt-0.5">
                  {line.colorOrPrint} · {line.size}
                </p>
              </div>
              <button
                type="button"
                onClick={() => removeLine(index)}
                className="shrink-0 p-2 text-[#969696] hover:text-[#e51940] transition-colors min-h-[44px] min-w-[44px] flex items-center justify-center"
                aria-label={`Supprimer ${line.articleName} ${line.size}`}
              >
                <Trash2 size={16} />
              </button>
            </li>
          ))}
        </ul>
      )}

      {/* Fixed bottom actions */}
      <div className="fixed bottom-16 left-0 right-0 bg-[#f9f4ef] px-5 py-5 border-t border-[#e1e1e1] space-y-3 z-30">
        <button
          type="button"
          onClick={() => navigate('/search')}
          className="w-full py-4 border border-[#121212] font-[Oswald] text-[13px] tracking-[2.5px] uppercase text-[#121212] transition-opacity"
        >
          Ajouter un article
        </button>
        <button
          type="button"
          onClick={() => void handleSubmit()}
          disabled={lines.length === 0 || isSubmitting}
          className="w-full py-4 bg-[#121212] text-white font-[Oswald] text-[13px] tracking-[2.5px] uppercase disabled:opacity-40 disabled:cursor-not-allowed transition-opacity"
        >
          {isSubmitting ? 'Envoi en cours...' : 'Soumettre la demande'}
        </button>
      </div>
    </div>
  );
}
