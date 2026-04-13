import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useRequestStore } from '@/stores/useRequestStore';
import { useStockistActions } from '@/hooks/useStockistActions';
import { useStockistSignalR } from '@/hooks/useStockistSignalR';
import { StockistQueueCard } from '@/pages/stockist/QueuePage';
import { RequestStatus } from '@/constants/enums';

const IN_PROGRESS_STATUSES = new Set<RequestStatus>([
  RequestStatus.InProgress,
  RequestStatus.AwaitingSellerResponse,
]);

export default function InProgressPage() {
  const navigate = useNavigate();
  const requests = useRequestStore((s) => s.requests);
  const isLoading = useRequestStore((s) => s.isLoading);
  const { fetchStockistRequests } = useStockistActions();

  useStockistSignalR();

  useEffect(() => {
    void fetchStockistRequests();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const inProgressRequests = requests
    .filter((r) => IN_PROGRESS_STATUSES.has(r.status))
    .sort(
      (a, b) =>
        new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(),
    );

  return (
    <div className="bg-[#f9f4ef] min-h-full pb-8">
      <section className="px-5 pt-6">
        <h2 className="font-[Oswald] text-[11px] tracking-[2.5px] uppercase text-[#969696] mb-3">
          En cours de traitement
        </h2>

        {isLoading && inProgressRequests.length === 0 && (
          <div className="space-y-3">
            {[1, 2, 3].map((n) => (
              <div key={n} className="h-24 bg-white animate-pulse" />
            ))}
          </div>
        )}

        {!isLoading && inProgressRequests.length === 0 && (
          <p className="font-[Oswald] text-[11px] tracking-[2px] uppercase text-[#969696] py-6 text-center">
            Aucune demande en cours de traitement
          </p>
        )}

        <div className="space-y-3">
          {inProgressRequests.map((request) => (
            <StockistQueueCard
              key={request.id}
              request={request}
              onClick={() => navigate(`/queue/${request.id}`)}
            />
          ))}
        </div>
      </section>
    </div>
  );
}
