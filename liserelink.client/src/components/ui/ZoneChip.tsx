import { useAuthStore } from '@/stores/authStore';
import { ZoneType } from '@/constants/enums';

const ZONE_SHORT: Record<ZoneType, string> = {
  [ZoneType.RTW]: 'PAP',
  [ZoneType.FittingRooms]: 'CAB',
  [ZoneType.Checkout]: 'CAI',
  [ZoneType.Reception]: 'ACC',
  [ZoneType.Custom]: 'PERS',
};

interface ZoneChipProps {
  onClick: () => void;
}

export default function ZoneChip({ onClick }: ZoneChipProps) {
  const zone = useAuthStore((s) => s.selectedZone);
  const label = zone ? ZONE_SHORT[zone] : '—';

  return (
    <button
      type="button"
      onClick={onClick}
      className="border border-[#555] px-3 py-1.5 font-[Oswald] text-[11px] tracking-[1px] uppercase text-white"
    >
      {label} ▼
    </button>
  );
}
