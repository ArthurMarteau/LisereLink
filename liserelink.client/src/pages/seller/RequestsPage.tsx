import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useRequestStore } from '@/stores/useRequestStore';
import { useRequestActions } from '@/hooks/useRequestActions';
import { useCartStore } from '@/stores/useCartStore';
import RequestCard from '@/components/ui/RequestCard';
import { RequestStatus } from '@/constants/enums';
import type { RequestDto } from '@/types';

const ACTIVE_STATUSES = new Set<RequestStatus>([
  RequestStatus.Pending,
  RequestStatus.InProgress,
  RequestStatus.AwaitingSellerResponse,
]);

export default function RequestsPage() {
  const navigate = useNavigate();
  const requests = useRequestStore((s) => s.requests);
  const isLoading = useRequestStore((s) => s.isLoading);
  const { fetchRequests, cancelRequest, respondToAlternatives } = useRequestActions();

  function handleModify(request: RequestDto) {
    const { clearCart, addLine, setEditRequestId } = useCartStore.getState();
    clearCart();
    setEditRequestId(request.id);
    for (const line of request.lines) {
      for (const size of line.requestedSizes) {
        addLine({
          articleId: line.articleId,
          articleName: line.articleName,
          colorOrPrint: line.colorOrPrint,
          barcode: line.articleBarcode,
          size,
          quantity: 1,
        });
      }
    }
    navigate('/cart');
  }

  // Legitimate initial sync side effect — fetchRequests reads storeId + zone from authStore
  useEffect(() => {
    void fetchRequests();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const activeRequests = requests.filter((r) => ACTIVE_STATUSES.has(r.status));

  return (
    <div className="bg-[#f9f4ef] min-h-full pb-8">
      {/* ── EN COURS ──────────────────────────────────────────────────────── */}
      <section className="px-5 pt-6">
        <h2 className="font-[Oswald] text-[11px] tracking-[2.5px] uppercase text-[#969696] mb-3">
          En cours
        </h2>

        {isLoading && activeRequests.length === 0 && (
          <div className="space-y-3">
            {[1, 2].map((n) => (
              <div key={n} className="h-24 bg-white animate-pulse" />
            ))}
          </div>
        )}

        {!isLoading && activeRequests.length === 0 && (
          <p className="font-[Oswald] text-[11px] tracking-[2px] uppercase text-[#969696] py-6 text-center">
            Aucune demande en cours
          </p>
        )}

        <div className="space-y-3">
          {activeRequests.map((request) => (
            <RequestCard
              key={request.id}
              request={request}
              onCancel={
                request.status === RequestStatus.Pending
                  ? () => void cancelRequest(request.id)
                  : undefined
              }
              onModify={
                request.status === RequestStatus.Pending
                  ? () => handleModify(request)
                  : undefined
              }
              onRespondAlternative={
                request.status === RequestStatus.AwaitingSellerResponse
                  ? ({ alternativeLineId, accepted }) =>
                      void respondToAlternatives(request.id, [{ alternativeLineId, accepted }])
                  : undefined
              }
            />
          ))}
        </div>
      </section>
    </div>
  );
}
