import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Temporal } from '@js-temporal/polyfill';
import { useRequestStore } from '@/stores/useRequestStore';
import { useStockistActions } from '@/hooks/useStockistActions';
import { useStockistSignalR } from '@/hooks/useStockistSignalR';
import StatusBadge from '@/components/ui/StatusBadge';
import { RequestStatus } from '@/constants/enums';
import { formatZone } from '@/utils/formatters';
import type { RequestDto } from '@/types';

function borderColor(status: RequestStatus): string {
  switch (status) {
    case RequestStatus.Pending:
      return 'border-l-[3px] border-l-[#121212]';
    case RequestStatus.InProgress:
      return 'border-l-[3px] border-l-[#b28a2c]';
    case RequestStatus.AwaitingSellerResponse:
      return 'border-l-[3px] border-l-[#b28a2c] animate-pulse';
    default:
      return 'border-l-[3px] border-l-[#e1e1e1]';
  }
}

function timeAgo(dateStr: string): string {
  const past = Temporal.Instant.from(
    dateStr.endsWith('Z') ? dateStr : dateStr + 'Z',
  );
  const now = Temporal.Now.instant();
  const diffSeconds = Math.floor(
    (now.epochMilliseconds - past.epochMilliseconds) / 1000,
  );
  const diffMin = Math.floor(diffSeconds / 60);
  if (diffMin < 1) return "IL Y A MOINS D'1 MIN";
  if (diffMin < 60) return `IL Y A ${String(diffMin)} MIN`;
  const diffH = Math.floor(diffMin / 60);
  return `IL Y A ${String(diffH)}H`;
}

interface StockistQueueCardProps {
  request: RequestDto;
  onClick: () => void;
}

export function StockistQueueCard({ request, onClick }: StockistQueueCardProps) {
  return (
    <button
      type="button"
      onClick={onClick}
      className={`w-full text-left bg-white px-4 py-4 min-h-11 ${borderColor(request.status)}`}
    >
      <div className="flex items-start justify-between gap-2 mb-3">
        <StatusBadge status={request.status} />
        <span className="font-[Oswald] text-[10px] tracking-[1.5px] uppercase text-[#969696]">
          {timeAgo(request.createdAt)}
        </span>
      </div>

      {request.lines.map((line, i) => (
        <div key={line.id ?? i} className="mb-2">
          <p className="font-['Libre_Baskerville'] text-[14px] text-[#121212] leading-snug">
            {line.articleName}
          </p>
          <p className="font-[Oswald] text-[11px] tracking-[1.5px] uppercase text-[#969696] mt-0.5">
            {line.colorOrPrint} · {line.requestedSizes.join(' · ')}
          </p>
        </div>
      ))}

      <p className="font-[Oswald] text-[10px] tracking-[1.5px] uppercase text-[#969696] mt-2">
        {request.sellerFirstName} {request.sellerLastName} · {formatZone(request.zone)}
      </p>

      {request.alternativeLines.length > 0 && (
        <p className="font-[Oswald] text-[9px] tracking-[1.5px] uppercase text-[#b28a2c] mt-1">
          {request.alternativeLines.length} alternative(s) proposée(s)
        </p>
      )}
    </button>
  );
}

const QUEUE_STATUSES = new Set<RequestStatus>([RequestStatus.Pending]);
const IN_PROGRESS_STATUSES = new Set<RequestStatus>([
  RequestStatus.InProgress,
  RequestStatus.AwaitingSellerResponse,
]);

export default function QueuePage() {
  const navigate = useNavigate();
  const requests = useRequestStore((s) => s.requests);
  const isLoading = useRequestStore((s) => s.isLoading);
  const { fetchStockistRequests } = useStockistActions();

  useStockistSignalR();

  useEffect(() => {
    void fetchStockistRequests();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const pendingRequests = requests
    .filter((r) => QUEUE_STATUSES.has(r.status))
    .sort(
      (a, b) =>
        new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime(),
    );

  const inProgressRequests = requests
    .filter((r) => IN_PROGRESS_STATUSES.has(r.status))
    .sort(
      (a, b) =>
        new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(),
    );

  return (
    <div className="bg-[#f9f4ef] min-h-full pb-8">
      {/* ── FILE D'ATTENTE ────────────────────────────────────────────────── */}
      <section className="px-5 pt-6">
        <h2 className="font-[Oswald] text-[11px] tracking-[2.5px] uppercase text-[#969696] mb-3">
          File d&apos;attente
        </h2>

        {isLoading && pendingRequests.length === 0 && (
          <div className="space-y-3">
            {[1, 2, 3].map((n) => (
              <div key={n} className="h-24 bg-white animate-pulse" />
            ))}
          </div>
        )}

        {!isLoading && pendingRequests.length === 0 && (
          <p className="font-[Oswald] text-[11px] tracking-[2px] uppercase text-[#969696] py-6 text-center">
            Aucune demande en attente
          </p>
        )}

        <div className="space-y-3">
          {pendingRequests.map((request) => (
            <StockistQueueCard
              key={request.id}
              request={request}
              onClick={() => navigate(`/queue/${request.id}`)}
            />
          ))}
        </div>
      </section>

      {/* ── EN COURS ──────────────────────────────────────────────────────── */}
      {inProgressRequests.length > 0 && (
        <section className="px-5 pt-6">
          <h2 className="font-[Oswald] text-[11px] tracking-[2.5px] uppercase text-[#969696] mb-3">
            En cours
          </h2>
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
      )}
    </div>
  );
}
