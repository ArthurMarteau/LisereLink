import { RequestLineStatus, RequestStatus } from '@/constants/enums';
import type { RequestDto } from '@/types';
import StatusBadge from './StatusBadge';

interface RequestCardProps {
  request: RequestDto;
  onCancel?: () => void;
  onModify?: () => void;
  onAcceptAlternative?: () => void;
  onRejectAlternative?: () => void;
}

function borderColor(status: RequestStatus): string {
  switch (status) {
    case RequestStatus.Pending:
      return 'border-l-[3px] border-l-[#121212]';
    case RequestStatus.InProgress:
      return 'border-l-[3px] border-l-[#b28a2c]';
    case RequestStatus.AwaitingSellerResponse:
      return 'border-l-[3px] border-l-[#b28a2c] animate-pulse';
    case RequestStatus.Delivered:
      return 'border-l-[3px] border-l-[#43a200]';
    default:
      return 'border-l-[3px] border-l-[#e1e1e1]';
  }
}

function timeAgo(dateStr: string): string {
  const diffMs = Date.now() - new Date(dateStr).getTime();
  const diffMin = Math.floor(diffMs / 60000);
  if (diffMin < 1) return "IL Y A MOINS D'1 MIN";
  if (diffMin < 60) return `IL Y A ${String(diffMin)} MIN`;
  const diffH = Math.floor(diffMin / 60);
  return `IL Y A ${String(diffH)}H`;
}

export default function RequestCard({
  request,
  onCancel,
  onModify,
  onAcceptAlternative,
  onRejectAlternative,
}: RequestCardProps) {
  const hasAlternative = request.lines.some(
    (l) => l.status === RequestLineStatus.AlternativeProposed,
  );

  return (
    <div className={`bg-white px-4 py-4 ${borderColor(request.status)}`}>
      {/* Header row */}
      <div className="flex items-start justify-between gap-2 mb-3">
        <StatusBadge status={request.status} />
        <span className="font-[Oswald] text-[10px] tracking-[1.5px] uppercase text-[#969696]">
          {timeAgo(request.createdAt)}
        </span>
      </div>

      {/* Lines */}
      {request.lines.map((line, i) => (
        <div key={line.id ?? i} className="mb-2">
          <p className="font-['Libre_Baskerville'] text-[14px] text-[#121212] leading-snug">
            {line.colorOrPrint}
          </p>
          <p className="font-[Oswald] text-[11px] tracking-[1.5px] uppercase text-[#969696] mt-0.5">
            {line.requestedSizes.join(' · ')}
          </p>
        </div>
      ))}

      {/* Alternative proposed section */}
      {hasAlternative && (
        <div className="mt-3 pt-3 border-t border-[#e1e1e1]">
          <p className="font-[Oswald] text-[10px] tracking-[2px] uppercase text-[#b28a2c] mb-2">
            Alternative proposée
          </p>
          {request.lines
            .filter((l) => l.status === RequestLineStatus.AlternativeProposed)
            .map((line, i) => (
              <div key={`alt-${line.id ?? i}`} className="mb-1">
                {line.alternativeColorOrPrint && (
                  <p className="font-['Libre_Baskerville'] text-[13px] text-[#121212]">
                    {line.alternativeColorOrPrint}
                  </p>
                )}
                {line.alternativeSizes && line.alternativeSizes.length > 0 && (
                  <p className="font-[Oswald] text-[11px] tracking-[1.5px] uppercase text-[#969696]">
                    {line.alternativeSizes.join(' · ')}
                  </p>
                )}
              </div>
            ))}
        </div>
      )}

      {/* Actions */}
      {(onCancel || onModify || onAcceptAlternative || onRejectAlternative) && (
        <div className="flex gap-2 mt-4">
          {onModify && (
            <button
              type="button"
              onClick={onModify}
              className="flex-1 py-3 border border-[#121212] font-[Oswald] text-[12px] tracking-[2px] uppercase text-[#121212] min-h-[44px]"
            >
              Modifier
            </button>
          )}
          {onCancel && (
            <button
              type="button"
              onClick={onCancel}
              className="flex-1 py-3 border border-[#e1e1e1] font-[Oswald] text-[12px] tracking-[2px] uppercase text-[#969696] min-h-[44px]"
            >
              Annuler
            </button>
          )}
          {onAcceptAlternative && (
            <button
              type="button"
              onClick={onAcceptAlternative}
              className="flex-1 py-3 bg-[#121212] font-[Oswald] text-[12px] tracking-[2px] uppercase text-white min-h-[44px]"
            >
              Accepter
            </button>
          )}
          {onRejectAlternative && (
            <button
              type="button"
              onClick={onRejectAlternative}
              className="flex-1 py-3 border border-[#e51940] font-[Oswald] text-[12px] tracking-[2px] uppercase text-[#e51940] min-h-[44px]"
            >
              Refuser
            </button>
          )}
        </div>
      )}
    </div>
  );
}
