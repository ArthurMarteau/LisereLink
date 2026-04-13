import { useState, use, Suspense, useCallback } from 'react';
import { useNavigate, useParams, useSearchParams } from 'react-router-dom';
import { ArrowLeft } from 'lucide-react';
import { toast } from 'sonner';
import apiClient from '@/services/apiClient';
import { useArticleStore } from '@/stores/useArticleStore';
import { useAuthStore } from '@/stores/authStore';
import { useAlternativeCartStore } from '@/stores/useAlternativeCartStore';
import SizeChip from '@/components/ui/SizeChip';
import type { StockDto } from '@/types';
import type { Size } from '@/constants/enums';

// ── Stock section ─────────────────────────────────────────────────────────────

interface StockSectionProps {
  promise: Promise<StockDto[]>;
  availableSizes: Size[];
  selectedSizes: Size[];
  onToggleSize: (size: Size) => void;
}

function StockSection({
  promise,
  availableSizes,
  selectedSizes,
  onToggleSize,
}: StockSectionProps) {
  const stocks = use(promise);

  const getQuantity = (size: Size): number => {
    const entry = stocks.find((s) => s.size === (size as string));
    return entry?.availableQuantity ?? 0;
  };

  const hasOutOfStockSelected = selectedSizes.some(
    (size) => getQuantity(size) === 0,
  );

  return (
    <>
      <p className="font-[Oswald] text-[11px] tracking-[2px] uppercase text-[#969696] mb-3">
        Sélectionner une ou plusieurs tailles
      </p>
      <div className="flex flex-wrap gap-2">
        {availableSizes.map((size) => (
          <SizeChip
            key={size}
            size={size}
            available={getQuantity(size) > 0}
            selected={selectedSizes.includes(size)}
            onClick={() => onToggleSize(size)}
          />
        ))}
      </div>

      {hasOutOfStockSelected && (
        <p className="font-[Oswald] text-[9px] tracking-[1.5px] uppercase text-[#e51940] mt-2">
          ⚠ Article(s) hors stock sélectionné(s) — le vendeur sera informé
        </p>
      )}

      {selectedSizes.length > 0 && (
        <p className="font-[Oswald] text-[11px] tracking-[2px] uppercase text-[#969696] mt-3">
          {selectedSizes.length} taille(s) sélectionnée(s) · {selectedSizes.join(', ')}
        </p>
      )}
    </>
  );
}

// ── Skeleton ──────────────────────────────────────────────────────────────────

function StockSkeleton() {
  return (
    <div className="space-y-3" aria-busy="true" aria-label="Chargement du stock">
      <div className="h-4 w-48 bg-[#e1e1e1] animate-pulse" />
      <div className="flex gap-2">
        {[1, 2, 3, 4].map((n) => (
          <div key={n} className="h-7 w-10 bg-[#e1e1e1] animate-pulse" />
        ))}
      </div>
    </div>
  );
}

// ── Page ──────────────────────────────────────────────────────────────────────

export default function StockistArticleDetailPage() {
  const navigate = useNavigate();
  const { id: articleId } = useParams<{ id: string }>();
  const [searchParams] = useSearchParams();
  const requestId = searchParams.get('requestId') ?? '';

  const selectedArticle = useArticleStore((s) => s.selectedArticle);
  const selectedStoreId = useAuthStore((s) => s.selectedStoreId);
  const addLine = useAlternativeCartStore((s) => s.addLine);

  const [selectedSizes, setSelectedSizes] = useState<Size[]>([]);

  const [stockPromise] = useState<Promise<StockDto[]>>(() => {
    if (!articleId || !selectedStoreId) return Promise.resolve([]);
    return apiClient
      .get<StockDto[]>(
        `/stock/${articleId}?storeId=${encodeURIComponent(selectedStoreId)}`,
      )
      .then((r) => r.data)
      .catch(() => {
        toast.error('Impossible de charger le stock.');
        return [];
      });
  });

  const handleToggleSize = useCallback((size: Size) => {
    setSelectedSizes((prev) =>
      prev.includes(size) ? prev.filter((s) => s !== size) : [...prev, size],
    );
  }, []);

  async function handleAddToAlternative() {
    if (!selectedArticle || selectedSizes.length === 0) return;

    // Resolve stock to determine stockOverride
    let stocks: StockDto[] = [];
    try {
      stocks = await stockPromise;
    } catch {
      // If promise already failed, stocks stays empty — all sizes treated as override
    }

    const getQuantity = (size: Size): number => {
      const entry = stocks.find((s) => s.size === (size as string));
      return entry?.availableQuantity ?? 0;
    };

    for (const size of selectedSizes) {
      addLine({
        articleId: selectedArticle.id,
        articleName: selectedArticle.name,
        colorOrPrint: selectedArticle.colorOrPrint,
        barcode: selectedArticle.barcode,
        size,
        quantity: 1,
        stockOverride: getQuantity(size) === 0,
      });
    }

    navigate(`/queue/${requestId}`);
  }

  if (!selectedArticle) {
    return (
      <div className="flex items-center justify-center py-20">
        <p className="font-[Oswald] text-[11px] tracking-[2px] uppercase text-[#969696]">
          Article introuvable
        </p>
      </div>
    );
  }

  return (
    <div className="bg-[#f9f4ef] pb-8">
      {/* Back navigation */}
      <div className="px-5 py-4">
        <button
          type="button"
          onClick={() => navigate(`/stockist-search?requestId=${requestId}`)}
          className="flex items-center gap-2 text-[#121212] min-h-[44px]"
        >
          <ArrowLeft size={16} />
          <span className="font-[Oswald] text-[11px] tracking-[2px] uppercase">Retour</span>
        </button>
      </div>

      {/* Article image */}
      <div
        className="w-full bg-[#f0ece6] overflow-hidden"
        style={{ aspectRatio: '4/5', maxHeight: '288px' }}
      >
        {selectedArticle.imageUrl ? (
          <img
            src={selectedArticle.imageUrl}
            alt={selectedArticle.name}
            className="w-full h-full object-cover"
          />
        ) : (
          <div className="w-full h-full flex items-center justify-center">
            <svg viewBox="0 0 120 150" className="w-24 h-24" fill="none" aria-hidden="true">
              <rect width="120" height="150" fill="#f0ece6" />
              <path
                d="M40 60 L60 40 L80 60 L80 100 L40 100 Z"
                stroke="#bcbcbc"
                strokeWidth="2"
                fill="none"
              />
            </svg>
          </div>
        )}
      </div>

      {/* Article info */}
      <div className="px-5 py-5 bg-white">
        <h1 className="font-['Libre_Baskerville'] text-[20px] text-[#121212] mb-1">
          {selectedArticle.name}
        </h1>
        <p className="font-[Oswald] text-[11px] tracking-[2px] uppercase text-[#969696]">
          {selectedArticle.colorOrPrint} · {selectedArticle.family}
        </p>
      </div>

      {/* Stock & size selection */}
      <div className="px-5 py-5">
        <Suspense fallback={<StockSkeleton />}>
          <StockSection
            promise={stockPromise}
            availableSizes={selectedArticle.availableSizes}
            selectedSizes={selectedSizes}
            onToggleSize={handleToggleSize}
          />
        </Suspense>
      </div>

      {/* Add to alternative */}
      <div className="px-5 pt-2">
        <button
          type="button"
          onClick={() => void handleAddToAlternative()}
          disabled={selectedSizes.length === 0}
          className="w-full py-4 bg-[#121212] text-white font-[Oswald] text-[13px] tracking-[2.5px] uppercase disabled:opacity-40 disabled:cursor-not-allowed transition-opacity"
        >
          Ajouter à l&apos;alternative
        </button>
      </div>
    </div>
  );
}
