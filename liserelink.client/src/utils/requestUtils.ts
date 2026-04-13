import { RequestStatus } from '@/constants/enums';
import type { RequestDto } from '@/types';

export function isCompletedToday(request: RequestDto): boolean {
  let dateStr: string | undefined;

  if (request.status === RequestStatus.Cancelled) {
    dateStr = request.cancelledAt;
  } else {
    dateStr = request.completedAt;
  }

  if (!dateStr) return false;

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
