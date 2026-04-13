import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { ArrowLeft } from 'lucide-react';
import { toast } from 'sonner';
import apiClient from '@/services/apiClient';
import StatusBadge from '@/components/ui/StatusBadge';
import { RequestLineStatus } from '@/constants/enums';
import { formatZone, formatDate } from '@/utils/formatters';
import type { RequestDto } from '@/types';

export default function StockistHistoryDetailPage() {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();

  const [request, setRequest] = useState<RequestDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    if (!id) return;
    const load = async () => {
      setIsLoading(true);
      try {
        const res = await apiClient.get<RequestDto>(`/requests/${id}`);
        setRequest(res.data);
      } catch {
        toast.error('Impossible de charger la demande.');
        navigate('/stockist-history');
      } finally {
        setIsLoading(false);
      }
    };
    void load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id]);

  if (isLoading) {
    return (
      <div className="bg-[#f9f4ef] min-h-full pb-8 px-5 pt-6 space-y-3">
        {[1, 2, 3].map((n) => (
          <div key={n} className="h-24 bg-white animate-pulse" />
        ))}
      </div>
    );
  }

  if (!request) return null;

  return (
    <div className="bg-[#f9f4ef] min-h-full pb-8">
      {/* Header */}
      <div className="px-5 py-4 flex items-center gap-3">
        <button
          type="button"
          onClick={() => navigate('/stockist-history')}
          className="min-h-[44px] min-w-[44px] flex items-center justify-center"
          aria-label="Retour à l'historique"
        >
          <ArrowLeft size={16} />
        </button>
        <div>
          <p className="font-['Libre_Baskerville'] text-[16px] text-[#121212]">
            {request.sellerFirstName} {request.sellerLastName}
          </p>
          <p className="font-[Oswald] text-[10px] tracking-[2px] uppercase text-[#969696]">
            {formatZone(request.zone)} · {formatDate(request.createdAt)}
          </p>
        </div>
        <div className="ml-auto">
          <StatusBadge status={request.status} />
        </div>
      </div>

      {/* Articles demandés */}
      <section className="px-5 pt-2">
        <h2 className="font-[Oswald] text-[11px] tracking-[2.5px] uppercase text-[#969696] mb-3">
          Articles demandés
        </h2>

        {request.lines.map((line) => {
          const isFound = line.status === RequestLineStatus.Found;
          const isNotFound = line.status === RequestLineStatus.NotFound;
          const containerClass = isFound
            ? 'bg-[#43a200]/5 border-l-[3px] border-l-[#43a200]'
            : isNotFound
              ? 'bg-[#e51940]/5 border-l-[3px] border-l-[#e51940]'
              : 'bg-white border-l-[3px] border-l-[#121212]';

          return (
            <div key={line.id} className={`px-4 py-4 mb-3 ${containerClass}`}>
              <div className="flex items-start justify-between gap-2">
                <div>
                  <p className="font-['Libre_Baskerville'] text-[14px] text-[#121212]">
                    {line.articleName}
                  </p>
                  <p className="font-[Oswald] text-[11px] tracking-[1.5px] uppercase text-[#969696] mt-0.5">
                    {line.colorOrPrint} · {line.size}
                  </p>
                </div>
                <StatusBadge status={line.status} />
              </div>
            </div>
          );
        })}
      </section>

      {/* Alternatives */}
      {request.alternativeLines.length > 0 && (
        <section className="px-5 pt-4">
          <h2 className="font-[Oswald] text-[11px] tracking-[2.5px] uppercase text-[#969696] mb-3">
            Alternatives proposées
          </h2>

          {request.alternativeLines.map((alt) => {
            const isAccepted = alt.status === RequestLineStatus.Found;
            const isDenied = alt.status === RequestLineStatus.AlternativeDenied;

            const containerClass = isAccepted
              ? 'bg-[#43a200]/5 border-l-[3px] border-l-[#43a200]'
              : isDenied
                ? 'bg-[#e51940]/5 border-l-[3px] border-l-[#e51940]'
                : 'bg-[#b28a2c]/5 border-l-[3px] border-l-[#b28a2c]';

            const badgeLabel = isAccepted
              ? 'Alternative acceptée'
              : isDenied
                ? 'Alternative refusée'
                : 'Alternative proposée';

            const badgeColor = isAccepted
              ? 'text-[#43a200]'
              : isDenied
                ? 'text-[#e51940]'
                : 'text-[#b28a2c]';

            return (
              <div key={alt.id} className={`px-4 py-4 mb-3 ${containerClass}`}>
                <p className={`font-[Oswald] text-[9px] tracking-[2px] uppercase mb-1 ${badgeColor}`}>
                  {badgeLabel}
                </p>
                <p className="font-['Libre_Baskerville'] text-[14px] text-[#121212]">
                  {alt.articleName}
                </p>
                <p className="font-[Oswald] text-[11px] tracking-[1.5px] uppercase text-[#969696] mt-0.5">
                  {alt.articleColorOrPrint} · {alt.requestedSizes.join(' · ')}
                </p>
                {alt.stockOverride && (
                  <p className="font-[Oswald] text-[9px] tracking-[1.5px] uppercase text-[#e51940] mt-1">
                    ⚠ Hors stock
                  </p>
                )}
              </div>
            );
          })}
        </section>
      )}
    </div>
  );
}
