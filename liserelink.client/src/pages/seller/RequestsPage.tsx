import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useRequestStore } from '@/stores/useRequestStore';
import { useRequestActions } from '@/hooks/useRequestActions';
import { useCartStore } from '@/stores/useCartStore';
import RequestCard from '@/components/ui/RequestCard';
import { RequestStatus } from '@/constants/enums';
import type { RequestDto } from '@/types';

// Returns true if the request was completed today (Europe/Paris timezone)
function isCompletedToday(request: RequestDto): boolean {
  const dateStr = request.completedAt ?? request.createdAt;
  const d = new Date(dateStr);
  const now = new Date();
  const fmt = new Intl.DateTimeFormat('fr-FR', {
    timeZone: 'Europe/Paris',
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
  });
  return fmt.format(d) === fmt.format(now);
}

const ACTIVE_STATUSES = new Set<RequestStatus>([
  RequestStatus.Pending,
  RequestStatus.InProgress,
  RequestStatus.AwaitingSellerResponse,
]);

const HISTORY_STATUSES = new Set<RequestStatus>([
  RequestStatus.Delivered,
  RequestStatus.Unavailable,
  RequestStatus.Cancelled,
]);

export default function RequestsPage() {
  const navigate = useNavigate();
  const requests = useRequestStore((s) => s.requests);
  const isLoading = useRequestStore((s) => s.isLoading);
  const { fetchRequests, cancelRequest, acceptAlternative, rejectAlternative } =
    useRequestActions();

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
  const historyRequests = requests.filter(
    (r) => HISTORY_STATUSES.has(r.status) && isCompletedToday(r),
  );

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
              onAcceptAlternative={
                request.status === RequestStatus.AwaitingSellerResponse
                  ? () => void acceptAlternative(request.id)
                  : undefined
              }
              onRejectAlternative={
                request.status === RequestStatus.AwaitingSellerResponse
                  ? () => void rejectAlternative(request.id)
                  : undefined
              }
            />
          ))}
        </div>
      </section>

      {/* ── HISTORIQUE DU JOUR ────────────────────────────────────────────── */}
      {historyRequests.length > 0 && (
        <section className="px-5 pt-8">
          <h2 className="font-[Oswald] text-[11px] tracking-[2.5px] uppercase text-[#969696] mb-3">
            Historique du jour
          </h2>

          <div className="space-y-3" style={{ opacity: 0.6 }}>
            {historyRequests.map((request) => (
              <RequestCard key={request.id} request={request} />
            ))}
          </div>
        </section>
      )}
    </div>
  );
}
