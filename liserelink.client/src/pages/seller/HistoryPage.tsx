import { useEffect } from 'react';
import { useRequestStore } from '@/stores/useRequestStore';
import { useRequestActions } from '@/hooks/useRequestActions';
import RequestCard from '@/components/ui/RequestCard';
import { RequestStatus } from '@/constants/enums';
import { isCompletedToday } from '@/utils/requestUtils';

const HISTORY_STATUSES = new Set<RequestStatus>([
  RequestStatus.Delivered,
  RequestStatus.Unavailable,
  RequestStatus.Cancelled,
]);

export default function SellerHistoryPage() {
  const requests = useRequestStore((s) => s.requests);
  const isLoading = useRequestStore((s) => s.isLoading);
  const { fetchRequests } = useRequestActions();

  useEffect(() => {
    void fetchRequests();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const historyRequests = requests
    .filter((r) => HISTORY_STATUSES.has(r.status) && isCompletedToday(r))
    .sort((a, b) => new Date(b.completedAt ?? b.createdAt).getTime() - new Date(a.completedAt ?? a.createdAt).getTime());

  return (
    <div className="bg-[#f9f4ef] min-h-full pb-8">
      <section className="px-5 pt-6">
        <h2 className="font-[Oswald] text-[11px] tracking-[2.5px] uppercase text-[#969696] mb-3">
          Historique du jour
        </h2>

        {isLoading && historyRequests.length === 0 && (
          <div className="space-y-3">
            {[1, 2, 3].map((n) => (
              <div key={n} className="h-24 bg-white animate-pulse" />
            ))}
          </div>
        )}

        {!isLoading && historyRequests.length === 0 && (
          <p className="font-[Oswald] text-[11px] tracking-[2px] uppercase text-[#969696] py-6 text-center">
            Aucune demande terminée aujourd'hui
          </p>
        )}

        <div className="space-y-3" style={{ opacity: 0.6 }}>
          {historyRequests.map((request) => (
            <RequestCard key={request.id} request={request} />
          ))}
        </div>
      </section>
    </div>
  );
}
