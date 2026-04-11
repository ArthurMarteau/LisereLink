import { useState, use, Suspense } from 'react';
import { useNavigate } from 'react-router-dom';
import { toast } from 'sonner';
import { ChevronRight } from 'lucide-react';
import apiClient from '@/services/apiClient';
import { useAuthStore } from '@/stores/authStore';
import { UserRole } from '@/constants/enums';
import type { StoreDto } from '@/types';

// ── Inner component: uses() suspends until stores are ready ─────────────────

interface StoreListProps {
  promise: Promise<StoreDto[]>;
  selectedId: string | null;
  onSelect: (store: StoreDto) => void;
}

function StoreList({ promise, selectedId, onSelect }: StoreListProps) {
  const stores = use(promise);
  const visibleStores = stores.filter(s => s.code.toLowerCase() !== 'online');

  if (visibleStores.length === 0) {
    return (
      <p className="font-[Oswald] text-[11px] tracking-[2px] uppercase text-[#969696] py-4 text-center">
        Aucun magasin disponible
      </p>
    );
  }

  return (
    <ul className="space-y-[10px]">
      {visibleStores.map((store) => {
        const isSelected = store.code === selectedId;
        return (
          <li key={store.id}>
            <button
              type="button"
              onClick={() => onSelect(store)}
              className={[
                'w-full flex items-center justify-between bg-white px-4 py-4 text-left transition-colors min-h-[56px]',
                'border',
                isSelected ? 'border-[#121212]' : 'border-[#e1e1e1]',
              ].join(' ')}
            >
              <span className="font-['Libre_Baskerville'] text-[15px] text-[#121212]">
                {store.name}
              </span>
              <ChevronRight
                size={16}
                className={isSelected ? 'text-[#121212]' : 'text-[#969696]'}
              />
            </button>
          </li>
        );
      })}
    </ul>
  );
}

// ── Skeleton shown while stores load ────────────────────────────────────────

function StoreSkeleton() {
  return (
    <ul className="space-y-[10px]" aria-busy="true" aria-label="Chargement des magasins">
      {[1, 2, 3].map((n) => (
        <li key={n} className="h-[56px] bg-white animate-pulse" />
      ))}
    </ul>
  );
}

// ── Page ────────────────────────────────────────────────────────────────────

export default function StoreSelectionPage() {
  const navigate = useNavigate();
  const user = useAuthStore((s) => s.user);

  const [storesPromise] = useState<Promise<StoreDto[]>>(() =>
    apiClient
      .get<StoreDto[]>('/stores')
      .then((r) => r.data)
      .catch(() => {
        toast.error('Impossible de charger la liste des magasins.');
        return [] as StoreDto[];
      })
  );

  const [selectedStore, setSelectedStore] = useState<StoreDto | null>(null);

  function handleConfirm() {
    if (!selectedStore) return;
    useAuthStore.getState().setStore(selectedStore.code, selectedStore.name);
    if (user?.role === UserRole.Seller) navigate('/search');
    else if (user?.role === UserRole.Stockist) navigate('/queue');
    else navigate('/admin');
  }

  return (
    <div className="min-h-screen bg-[#f9f4ef] flex flex-col">
      {/* Dark header */}
      <header className="bg-[#121212] px-6 py-6 shrink-0">
        <p className="font-[Oswald] text-[11px] tracking-[2px] uppercase text-[#969696] mb-1">
          Bonjour
        </p>
        <h1 className="font-['Libre_Baskerville'] text-white text-2xl">{user?.firstName}</h1>
      </header>

      {/* Content */}
      <main className="flex-1 px-5 py-6 pb-32 overflow-y-auto">
        <p className="font-[Oswald] text-[11px] tracking-[2px] uppercase text-[#969696] mb-5">
          Sélectionnez votre magasin
        </p>

        <Suspense fallback={<StoreSkeleton />}>
          <StoreList
            promise={storesPromise}
            selectedId={selectedStore?.code ?? null}
            onSelect={setSelectedStore}
          />
        </Suspense>
      </main>

      {/* Fixed bottom confirm button */}
      <div className="fixed bottom-0 left-0 right-0 bg-[#f9f4ef] px-5 py-5 border-t border-[#e1e1e1]">
        <button
          type="button"
          onClick={handleConfirm}
          disabled={!selectedStore}
          className="w-full py-4 bg-[#121212] text-white font-[Oswald] text-[13px] tracking-[2.5px] uppercase disabled:opacity-40 disabled:cursor-not-allowed transition-opacity"
        >
          Confirmer
        </button>
      </div>
    </div>
  );
}
