import { useState } from 'react';
import { ZoneType } from '@/constants/enums';
import { formatZone } from '@/utils/formatters';
import { useAuthStore } from '@/stores/authStore';

interface ZoneSelectorModalProps {
  /** Called after the zone is saved — the only way to close the modal */
  onClose: () => void;
  /** Pre-select the current zone when re-opening from the header */
  initialZone?: ZoneType | null;
}

const ALL_ZONES = Object.values(ZoneType);

export default function ZoneSelectorModal({ onClose, initialZone }: ZoneSelectorModalProps) {
  const [selected, setSelected] = useState<ZoneType | null>(initialZone ?? null);
  const setZone = useAuthStore((s) => s.setZone);

  function handleConfirm() {
    if (!selected) return;
    setZone(selected);
    onClose();
  }

  return (
    <div className="fixed inset-0 z-50 flex flex-col justify-end">
      {/* Backdrop */}
      <div className="absolute inset-0 bg-black/40" />

      {/* Sheet */}
      <div className="relative bg-white rounded-t-3xl px-6 pt-6 pb-10">
        {/* Handle */}
        <div className="w-10 h-1 bg-[#e1e1e1] rounded-full mx-auto mb-6" />

        <p className="font-[Oswald] text-[11px] tracking-[2px] uppercase text-[#969696] mb-6">
          Sélectionnez votre zone
        </p>

        <ul className="space-y-2 mb-8">
          {ALL_ZONES.map((zone) => {
            const isSelected = selected === zone;
            return (
              <li key={zone}>
                <button
                  type="button"
                  onClick={() => setSelected(zone)}
                  className={[
                    'w-full flex items-center justify-between px-4 py-4 text-left',
                    'border transition-colors',
                    isSelected
                      ? 'border-[#121212] bg-[#121212] text-white'
                      : 'border-[#e1e1e1] bg-white text-[#121212]',
                  ].join(' ')}
                >
                  <span className="font-['Libre_Baskerville'] text-[15px]">{formatZone(zone)}</span>
                  {isSelected && (
                    <svg width="16" height="16" viewBox="0 0 16 16" fill="none" aria-hidden="true">
                      <path
                        d="M2.5 8L6.5 12L13.5 4"
                        stroke="currentColor"
                        strokeWidth="1.5"
                        strokeLinecap="round"
                        strokeLinejoin="round"
                      />
                    </svg>
                  )}
                </button>
              </li>
            );
          })}
        </ul>

        <button
          type="button"
          onClick={handleConfirm}
          disabled={!selected}
          className="w-full py-4 bg-[#121212] text-white font-[Oswald] text-[13px] tracking-[2.5px] uppercase disabled:opacity-40 disabled:cursor-not-allowed"
        >
          Confirmer
        </button>
      </div>
    </div>
  );
}
