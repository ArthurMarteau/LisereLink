import type { Size } from '@/constants/enums';

interface SizeChipProps {
  size: Size;
  available: boolean;
  selected: boolean;
  onClick?: () => void;
}

export default function SizeChip({ size, available, selected, onClick }: SizeChipProps) {
  const base = 'font-[Oswald] text-[10px] px-2 py-0.5 min-h-[28px] min-w-[32px] tracking-[1px] uppercase transition-colors';

  if (!available) {
    return (
      <button
        type="button"
        onClick={onClick}
        className={`${base} border border-[#e1e1e1] text-[#bcbcbc] line-through`}
      >
        {size}
      </button>
    );
  }

  if (selected) {
    return (
      <button
        type="button"
        onClick={onClick}
        className={`${base} border border-[#121212] bg-[#121212] text-white`}
      >
        {size}
      </button>
    );
  }

  return (
    <button
      type="button"
      onClick={onClick}
      className={`${base} border border-[#121212] text-[#121212]`}
    >
      {size}
    </button>
  );
}
