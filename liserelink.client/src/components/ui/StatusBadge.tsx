import { RequestLineStatus, RequestStatus } from '@/constants/enums';
import { formatRequestLineStatus, formatRequestStatus } from '@/utils/formatters';

type Status = RequestStatus | RequestLineStatus;

function getStyle(status: Status): string {
  switch (status) {
    case RequestStatus.Pending:
    case RequestLineStatus.Pending:
      return 'bg-[#121212] text-white';
    case RequestStatus.InProgress:
    case RequestStatus.AwaitingSellerResponse:
    case RequestLineStatus.AlternativeProposed:
      return 'bg-[#b28a2c] text-white';
    case RequestStatus.Delivered:
      return 'bg-[#43a200] text-white';
    case RequestStatus.Unavailable:
      return 'bg-[#e51940] text-white';
    case RequestStatus.Cancelled:
      return 'bg-[#e1e1e1] text-[#555]';
    case RequestLineStatus.Found:
      return 'border border-[#43a200] text-[#43a200]';
    case RequestLineStatus.NotFound:
      return 'border border-[#e51940] text-[#e51940]';
    default:
      return 'bg-[#e1e1e1] text-[#555]';
  }
}

function formatStatus(status: Status): string {
  if (Object.values(RequestStatus).includes(status as RequestStatus)) {
    return formatRequestStatus(status as RequestStatus);
  }
  return formatRequestLineStatus(status as RequestLineStatus);
}

interface StatusBadgeProps {
  status: Status;
}

export default function StatusBadge({ status }: StatusBadgeProps) {
  return (
    <span
      className={`inline-block font-[Oswald] text-[10px] tracking-[1.5px] uppercase px-2 py-0.5 ${getStyle(status)}`}
    >
      {formatStatus(status)}
    </span>
  );
}
