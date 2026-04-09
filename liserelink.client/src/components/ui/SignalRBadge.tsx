import { useSignalRStore } from '@/stores/useSignalRStore';

type SignalRStatus = 'disconnected' | 'connecting' | 'connected' | 'reconnecting';

const STATUS_COLOR: Record<SignalRStatus, string> = {
  connected: 'bg-[#43a200]',
  reconnecting: 'bg-[#b28a2c]',
  connecting: 'bg-[#b28a2c]',
  disconnected: 'bg-[#e51940]',
};

export default function SignalRBadge() {
  const status = useSignalRStore((s) => s.status);

  return (
    <span
      className={`inline-block w-2 h-2 rounded-full shrink-0 ${STATUS_COLOR[status]}`}
      aria-label={`SignalR ${status}`}
    />
  );
}
