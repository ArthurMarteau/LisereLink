import { useState } from 'react';
import { Temporal } from '@js-temporal/polyfill';
import { RequestLineStatus, RequestStatus } from '@/constants/enums';
import type { RequestDto } from '@/types';
import StatusBadge from './StatusBadge';

interface AlternativeResponsePayload {
  alternativeLineId: string;
  accepted: boolean;
}

interface RequestCardProps {
  request: RequestDto;
  onCancel?: () => void;
  onModify?: () => void;
  onRespondAlternative?: (response: AlternativeResponsePayload) => void;
}

function borderColor(status: RequestStatus): string {
  switch (status) {
    case RequestStatus.Pending:
      return 'border-l-[3px] border-l-[#121212]';
    case RequestStatus.InProgress:
      return 'border-l-[3px] border-l-[#b28a2c]';
    case RequestStatus.AwaitingSellerResponse:
      return 'border-l-[3px] border-l-[#b28a2c] animate-pulse';
    case RequestStatus.Processed:
      return 'border-l-[3px] border-l-[#43a200]';
    case RequestStatus.PartiallyProcessed:
      return 'border-l-[3px] border-l-[#b28a2c]';
    default:
      return 'border-l-[3px] border-l-[#e1e1e1]';
  }
}

function timeAgo(dateStr: string): string {
  const past = Temporal.Instant.from(
    dateStr.endsWith('Z') ? dateStr : dateStr + 'Z'
  );
  const now = Temporal.Now.instant();
  const diffSeconds = Math.floor((now.epochMilliseconds - past.epochMilliseconds) / 1000);
  const diffMin = Math.floor(diffSeconds / 60);
  if (diffMin < 1) return "IL Y A MOINS D'1 MIN";
  if (diffMin < 60) return `IL Y A ${String(diffMin)} MIN`;
  const diffH = Math.floor(diffMin / 60);
  return `IL Y A ${String(diffH)}H`;
}

export default function RequestCard({
  request,
  onCancel,
  onModify,
  onRespondAlternative,
}: RequestCardProps) {
  const [confirmCancel, setConfirmCancel] = useState(false);

  const showAlternatives = request.alternativeLines.length > 0;

  return (
    <div className={`bg-white px-4 py-4 ${borderColor(request.status)}`}>
      {/* Header row */}
      <div className="flex items-start justify-between gap-2 mb-3">
        <StatusBadge status={request.status} />
        <span className="font-[Oswald] text-[10px] tracking-[1.5px] uppercase text-[#969696]">
          {timeAgo(request.createdAt)}
        </span>
      </div>

      {/* Original lines */}
      {request.lines.map((line, i) => (
        <div key={line.id ?? i} className="mb-2">
          <p className="font-['Libre_Baskerville'] text-[14px] text-[#121212] leading-snug">
            {line.articleName}
          </p>
          <p className="font-[Oswald] text-[11px] tracking-[1.5px] uppercase text-[#969696] mt-0.5">
            {line.colorOrPrint}
          </p>
          <p className="font-[Oswald] text-[11px] tracking-[1.5px] uppercase text-[#969696] mt-0.5">
            {line.size}
          </p>
        </div>
      ))}

      {/* Seller / stockist */}
      <p className="font-[Oswald] text-[10px] tracking-[1.5px] uppercase text-[#969696] mt-2">
        Par {request.sellerFirstName} {request.sellerLastName}
      </p>
      {request.stockistFirstName && (
        <p className="font-[Oswald] text-[10px] tracking-[1.5px] uppercase text-[#969696]">
          Pris en charge par {request.stockistFirstName} {request.stockistLastName}
        </p>
      )}

      {/* Alternative lines */}
      {showAlternatives && (
        <div className="mt-3 pt-3 border-t border-[#e1e1e1]">
          <p className="font-[Oswald] text-[10px] tracking-[2px] uppercase text-[#b28a2c] mb-3">
            Alternatives proposées
          </p>

          {request.alternativeLines.map((alt) => {
            const isAccepted = alt.status === RequestLineStatus.Found;
            const isDenied = alt.status === RequestLineStatus.AlternativeDenied;

            const containerClass = isAccepted
              ? 'bg-[#43a200]/5 border-2 border-[#43a200]'
              : isDenied
                ? 'bg-[#e51940]/5 border-2 border-[#e51940]'
                : 'bg-[#b28a2c]/5 border-2 border-[#b28a2c]';

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
              <div key={alt.id} className={`px-3 py-3 mb-3 ${containerClass}`}>
                <p className={`font-[Oswald] text-[9px] tracking-[2px] uppercase mb-1 ${badgeColor}`}>
                  {badgeLabel}
                </p>
                <p className="font-['Libre_Baskerville'] text-[13px] text-[#121212]">
                  {alt.articleName}
                </p>
                <p className="font-[Oswald] text-[11px] tracking-[1.5px] uppercase text-[#969696] mt-0.5">
                  {alt.articleColorOrPrint}
                </p>
                <p className="font-[Oswald] text-[11px] tracking-[1.5px] uppercase text-[#969696] mt-0.5">
                  {alt.requestedSizes.join(' · ')}
                </p>
                {alt.stockOverride && (
                  <p className="font-[Oswald] text-[9px] tracking-[1.5px] uppercase text-[#e51940] mt-1">
                    ⚠ Article hors stock
                  </p>
                )}
                {alt.status === RequestLineStatus.AlternativeProposed && onRespondAlternative && (
                  <div className="flex gap-2 mt-3">
                    <button
                      type="button"
                      onClick={() => onRespondAlternative({ alternativeLineId: alt.id, accepted: true })}
                      className="flex-1 py-2 bg-[#121212] font-[Oswald] text-[11px] tracking-[2px] uppercase text-white min-h-[44px]"
                    >
                      Accepter
                    </button>
                    <button
                      type="button"
                      onClick={() => onRespondAlternative({ alternativeLineId: alt.id, accepted: false })}
                      className="flex-1 py-2 border border-[#e51940] font-[Oswald] text-[11px] tracking-[2px] uppercase text-[#e51940] min-h-[44px]"
                    >
                      Refuser
                    </button>
                  </div>
                )}
              </div>
            );
          })}
        </div>
      )}

      {/* Actions */}
      {(onCancel || onModify) && (
        <div className="flex gap-2 mt-4">
          {onModify && (
            <button
              type="button"
              onClick={onModify}
              className="flex-1 py-3 border border-[#121212] font-[Oswald] text-[12px] tracking-[2px] uppercase text-[#121212] min-h-11"
            >
              Modifier
            </button>
          )}
          {onCancel && !confirmCancel && (
            <button
              type="button"
              onClick={() => setConfirmCancel(true)}
              className="flex-1 py-3 border border-[#e1e1e1] font-[Oswald] text-[12px] tracking-[2px] uppercase text-[#969696] min-h-11"
            >
              Annuler
            </button>
          )}
          {onCancel && confirmCancel && (
            <>
              <button
                type="button"
                onClick={() => { onCancel(); setConfirmCancel(false); }}
                className="flex-1 py-3 border border-[#e51940] font-[Oswald] text-[12px] tracking-[2px] uppercase text-[#e51940] min-h-11"
              >
                Confirmer l'annulation
              </button>
              <button
                type="button"
                onClick={() => setConfirmCancel(false)}
                className="flex-1 py-3 border border-[#e1e1e1] font-[Oswald] text-[12px] tracking-[2px] uppercase text-[#969696] min-h-11"
              >
                Ne pas annuler
              </button>
            </>
          )}
        </div>
      )}
    </div>
  );
}
