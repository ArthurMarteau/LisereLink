import { Temporal } from '@js-temporal/polyfill';
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

  const instant = Temporal.Instant.from(
    dateStr.endsWith('Z') ? dateStr : dateStr + 'Z'
  );
  const requestDate = instant
    .toZonedDateTimeISO('Europe/Paris')
    .toPlainDate();
  const today = Temporal.Now.plainDateISO('Europe/Paris');

  return Temporal.PlainDate.compare(requestDate, today) === 0;
}
