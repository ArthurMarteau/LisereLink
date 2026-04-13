import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { ArrowLeft, Trash2 } from 'lucide-react';
import { Temporal } from '@js-temporal/polyfill';
import { toast } from 'sonner';
import apiClient from '@/services/apiClient';
import { useAuthStore } from '@/stores/authStore';
import {
  useAlternativeCartStore,
  type AlternativeCartLine,
} from '@/stores/useAlternativeCartStore';
import { useStockistActions } from '@/hooks/useStockistActions';
import StatusBadge from '@/components/ui/StatusBadge';
import { RequestLineStatus, RequestStatus } from '@/constants/enums';
import { formatZone } from '@/utils/formatters';
import type { RequestDto } from '@/types';

function isRequestClosed(r: RequestDto): boolean {
  return (
    r.status === RequestStatus.Processed ||
    r.status === RequestStatus.PartiallyProcessed ||
    r.status === RequestStatus.Unavailable
  );
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

export default function RequestDetailPage() {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const user = useAuthStore((s) => s.user);

  const [request, setRequest] = useState<RequestDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const alternativeCart = useAlternativeCartStore();
  const { takeRequest, markLineFound, markLineNotFound, proposeAlternatives } = useStockistActions();

  useEffect(() => {
    if (!id) return;

    const init = async () => {
      setIsLoading(true);
      try {
        const res = await apiClient.get<RequestDto>(`/requests/${id}`);
        const fetched = res.data;

        // Initialiser le cart uniquement si on change de demande
        const cartState = useAlternativeCartStore.getState();
        if (cartState.requestId !== id) {
          cartState.setRequestId(id);
          cartState.clearCart();
        }

        let resolved: RequestDto;
        if (fetched.status === RequestStatus.Pending) {
          const taken = await takeRequest(id);
          resolved = taken ?? fetched;
        } else {
          resolved = fetched;
        }
        setRequest(resolved);
      } catch {
        toast.error('Impossible de charger la demande.');
        navigate('/queue');
      } finally {
        setIsLoading(false);
      }
    };

    void init();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id]);

  async function handleProposeAlternatives() {
    if (isSubmitting || !request) return;
    setIsSubmitting(true);

    const map = new Map<string, AlternativeCartLine & { requestedSizes: string[] }>();
    for (const line of alternativeCart.lines) {
      const key = `${line.articleId}__${line.colorOrPrint}`;
      const existing = map.get(key);
      if (existing) {
        existing.requestedSizes.push(line.size);
      } else {
        map.set(key, { ...line, requestedSizes: [line.size] });
      }
    }

    const payload = Array.from(map.values()).map((g) => ({
      articleId: g.articleId,
      articleName: g.articleName,
      articleColorOrPrint: g.colorOrPrint,
      articleBarcode: g.barcode,
      requestedSizes: g.requestedSizes,
      quantity: 1,
      stockOverride: g.stockOverride,
    }));

    console.log('[propose-alternatives] payload:', JSON.stringify(payload, null, 2));
    const result = await proposeAlternatives(request.id, payload);
    console.log('[propose-alternatives] result:', result);
    if (result) {
      setRequest(result);
      useAlternativeCartStore.getState().clearCart();
      toast.success('Alternatives envoyées au vendeur.');
    }
    setIsSubmitting(false);
  }

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

  const isTakenByOther =
    request.status === RequestStatus.InProgress &&
    request.stockistId !== undefined &&
    request.stockistId !== user?.id;

  return (
    <div className="bg-[#f9f4ef] min-h-full pb-32">
      {/* ── Header ────────────────────────────────────────────────────────── */}
      <div className="px-5 py-4 flex items-center gap-3">
        <button
          type="button"
          onClick={() => navigate('/queue')}
          className="min-h-[44px] min-w-[44px] flex items-center justify-center"
          aria-label="Retour à la file"
        >
          <ArrowLeft size={16} />
        </button>
        <div>
          <p className="font-['Libre_Baskerville'] text-[16px] text-[#121212]">
            {request.sellerFirstName} {request.sellerLastName}
          </p>
          <p className="font-[Oswald] text-[10px] tracking-[2px] uppercase text-[#969696]">
            {formatZone(request.zone)} · {timeAgo(request.createdAt)}
          </p>
        </div>
      </div>

      {/* ── Prise en charge par un autre stockiste ────────────────────────── */}
      {isTakenByOther && (
        <div className="mx-5 mb-4 px-4 py-3 border border-[#e51940]">
          <p className="font-[Oswald] text-[11px] tracking-[2px] uppercase text-[#e51940]">
            Demande prise en charge par {request.stockistFirstName}{' '}
            {request.stockistLastName}
          </p>
        </div>
      )}

      {/* ── Articles demandés ─────────────────────────────────────────────── */}
      <section className="px-5 pt-2">
        <h2 className="font-[Oswald] text-[11px] tracking-[2.5px] uppercase text-[#969696] mb-3">
          Articles demandés
        </h2>

        {request.lines.map((line) => {
          const isProcessed = line.status !== RequestLineStatus.Pending;

          return (
            <div
              key={line.id}
              className={`bg-white px-4 py-4 mb-3 border-l-[3px] ${
                line.status === RequestLineStatus.Found
                  ? 'border-l-[#43a200]'
                  : line.status === RequestLineStatus.NotFound
                  ? 'border-l-[#e51940]'
                  : 'border-l-[#121212]'
              }`}
            >
              <div className="flex items-start justify-between gap-2 mb-2">
                <div>
                  <p className="font-['Libre_Baskerville'] text-[14px] text-[#121212]">
                    {line.articleName}
                  </p>
                  <p className="font-[Oswald] text-[11px] tracking-[1.5px] uppercase text-[#969696] mt-0.5">
                    {line.colorOrPrint} · {line.size}
                  </p>
                </div>
                {isProcessed && <StatusBadge status={line.status} />}
              </div>

              {!isProcessed &&
                request.status === RequestStatus.InProgress &&
                !isTakenByOther && (
                  <div className="flex gap-2 mt-3">
                    <button
                      type="button"
                      onClick={() => void markLineFound(request.id, line.id).then((r) => {
                        if (!r) return;
                        setRequest(r);
                        if (isRequestClosed(r)) {
                          toast.success(r.status === RequestStatus.Processed
                            ? 'Demande traitée !'
                            : 'Demande traitée partiellement.');
                          navigate('/queue');
                        }
                      })}
                      className="flex-1 py-3 bg-[#121212] text-white font-[Oswald] text-[11px] tracking-[1.5px] uppercase min-h-[44px]"
                    >
                      Trouvé
                    </button>
                    <button
                      type="button"
                      onClick={() => navigate(`/stockist-search?requestId=${request.id}`)}
                      className="flex-1 py-3 border border-[#b28a2c] text-[#b28a2c] font-[Oswald] text-[11px] tracking-[1.5px] uppercase min-h-[44px]"
                    >
                      Alternative
                    </button>
                    <button
                      type="button"
                      onClick={() => void markLineNotFound(request.id, line.id).then((r) => {
                        if (!r) return;
                        setRequest(r);
                        if (isRequestClosed(r)) {
                          toast.success(r.status === RequestStatus.Processed
                            ? 'Demande traitée !'
                            : 'Demande traitée partiellement.');
                          navigate('/queue');
                        }
                      })}
                      className="flex-1 py-3 border border-[#e51940] text-[#e51940] font-[Oswald] text-[11px] tracking-[1.5px] uppercase min-h-[44px]"
                    >
                      Non trouvé
                    </button>
                  </div>
                )}
            </div>
          );
        })}
      </section>

      {/* ── Alternatives à proposer (panier local) ────────────────────────── */}
      {alternativeCart.lines.length > 0 && (
        <section className="px-5 pt-4">
          <h2 className="font-[Oswald] text-[11px] tracking-[2.5px] uppercase text-[#969696] mb-3">
            Alternatives à proposer
          </h2>
          {alternativeCart.lines.map((alt, i) => (
            <div
              key={i}
              className="bg-[#b28a2c]/5 border-l-[3px] border-l-[#b28a2c] px-4 py-4 mb-3"
            >
              <div className="flex items-start justify-between">
                <div>
                  <p className="font-[Oswald] text-[9px] tracking-[2px] uppercase text-[#b28a2c] mb-1">
                    Alternative — En attente d'envoi
                  </p>
                  <p className="font-['Libre_Baskerville'] text-[14px] text-[#121212]">
                    {alt.articleName}
                  </p>
                  <p className="font-[Oswald] text-[11px] tracking-[1.5px] uppercase text-[#969696] mt-0.5">
                    {alt.colorOrPrint} · {alt.size}
                  </p>
                  {alt.stockOverride && (
                    <p className="font-[Oswald] text-[9px] tracking-[1.5px] uppercase text-[#e51940] mt-1">
                      ⚠ Hors stock
                    </p>
                  )}
                </div>
                <button
                  type="button"
                  onClick={() => alternativeCart.removeLine(i)}
                  className="min-h-[44px] min-w-[44px] flex items-center justify-center text-[#969696]"
                  aria-label="Supprimer"
                >
                  <Trash2 size={16} />
                </button>
              </div>
            </div>
          ))}
        </section>
      )}

      {/* ── Alternatives déjà envoyées ────────────────────────────────────── */}
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
                : 'Alternative — En attente du vendeur';

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

      {/* ── Bouton ajouter une alternative ────────────────────────────────── */}
      {request.status === RequestStatus.InProgress && !isTakenByOther && (
        <div className="px-5 pt-4">
          <button
            type="button"
            onClick={() =>
              navigate(`/stockist-search?requestId=${request.id}`)
            }
            className="w-full py-4 border border-[#b28a2c] text-[#b28a2c] font-[Oswald] text-[13px] tracking-[2.5px] uppercase min-h-[44px]"
          >
            + Ajouter une alternative
          </button>
        </div>
      )}

      {/* ── Sticky bottom : envoyer alternatives ──────────────────────────── */}
      {alternativeCart.lines.length > 0 &&
        request.status === RequestStatus.InProgress &&
        !isTakenByOther && (
          <div className="fixed bottom-16 left-0 right-0 bg-[#f9f4ef] px-5 py-5 border-t border-[#e1e1e1] z-30">
            <button
              type="button"
              onClick={() => void handleProposeAlternatives()}
              disabled={isSubmitting}
              className="w-full py-4 bg-[#121212] text-white font-[Oswald] text-[13px] tracking-[2.5px] uppercase disabled:opacity-40 min-h-[44px]"
            >
              {isSubmitting
                ? 'Envoi...'
                : `Envoyer ${String(alternativeCart.lines.length)} alternative(s)`}
            </button>
          </div>
        )}
    </div>
  );
}
