import { useState, use, Suspense, useCallback } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { ArrowLeft } from 'lucide-react';
import { toast } from 'sonner';
import apiClient from '@/services/apiClient';
import { useArticleStore } from '@/stores/useArticleStore';
import { useAuthStore } from '@/stores/authStore';
import { useCartStore } from '@/stores/useCartStore';
import SizeChip from '@/components/ui/SizeChip';
import type { StockDto } from '@/types';
import type { Size } from '@/constants/enums';

// ── Stock section: suspends until stock data is ready ────────────────────────

interface StockSectionProps {
  promise: Promise<StockDto[]>;
  availableSizes: Size[];
  selectedSizes: Size[];
  onToggleSize: (size: Size) => void;
}

function StockSection({ promise, availableSizes, selectedSizes, onToggleSize }: StockSectionProps) {
  const stocks = use(promise);

  const getQuantity = (size: Size): number => {
    const entry = stocks.find((s) => s.size === (size as string));
    return entry?.availableQuantity ?? 0;
  };

  const totalStock = stocks.reduce((sum, s) => sum + s.availableQuantity, 0);

  return (
    <>
      {/* Availability badge */}
      <div className="mb-5">
        {totalStock > 0 ? (
          <span className="font-[Oswald] text-[11px] tracking-[2px] uppercase bg-[#43a200] text-white px-2 py-1">
            Disponible
          </span>
        ) : (
          <span className="font-[Oswald] text-[11px] tracking-[2px] uppercase bg-[#e51940] text-white px-2 py-1">
            Épuisé
          </span>
        )}
      </div>

      {/* Size picker */}
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

      {selectedSizes.length > 0 && (
        <p className="font-[Oswald] text-[11px] tracking-[2px] uppercase text-[#969696] mt-3">
          {selectedSizes.length} taille(s) sélectionnée(s) · {selectedSizes.join(', ')}
        </p>
      )}
    </>
  );
}

// ── Skeleton ─────────────────────────────────────────────────────────────────

function StockSkeleton() {
  return (
    <div className="space-y-3" aria-busy="true" aria-label="Chargement du stock">
      <div className="h-7 w-24 bg-[#e1e1e1] animate-pulse" />
      <div className="h-4 w-48 bg-[#e1e1e1] animate-pulse" />
      <div className="flex gap-2">
        {[1, 2, 3, 4].map((n) => (
          <div key={n} className="h-7 w-10 bg-[#e1e1e1] animate-pulse" />
        ))}
      </div>
    </div>
  );
}

// ── Page ─────────────────────────────────────────────────────────────────────

export default function ArticleDetailPage() {
  const navigate = useNavigate();
  const { id: articleId } = useParams<{ id: string }>();
  const selectedArticle = useArticleStore((s) => s.selectedArticle);
  const selectedStoreId = useAuthStore((s) => s.selectedStoreId);
  const selectedZone = useAuthStore((s) => s.selectedZone);
  const [selectedSizes, setSelectedSizes] = useState<Size[]>([]);
  const addLine = useCartStore((s) => s.addLine);

  // Load stock once at mount — no useEffect, Promise created in useState initializer
  const [stockPromise] = useState<Promise<StockDto[]>>(() => {
    if (!articleId || !selectedStoreId) return Promise.resolve([]);
    return apiClient
      .get<StockDto[]>(
        `/stock/${articleId}?storeId=${encodeURIComponent(selectedStoreId)}`
      )
      .then((r) => r.data)
      .catch(() => {
        toast.error('Impossible de charger le stock.');
        return [];
      });
  });

  const handleToggleSize = useCallback((size: Size) => {
    setSelectedSizes((prev) =>
      prev.includes(size) ? prev.filter((s) => s !== size) : [...prev, size]
    );
  }, []);

  function handleAddToCart() {
    if (!selectedArticle) return;
    for (const size of selectedSizes) {
      addLine({
        articleId: selectedArticle.id,
        articleName: selectedArticle.name,
        colorOrPrint: selectedArticle.colorOrPrint,
        barcode: selectedArticle.barcode,
        size,
        quantity: 1,
      });
    }
    navigate('/cart');
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

  const canSubmit = selectedSizes.length > 0 && selectedZone !== null;

  return (
    <div className="bg-[#f9f4ef] pb-8">
      {/* Back navigation */}
      <div className="px-5 py-4">
        <button
          type="button"
          onClick={() => navigate(-1)}
          className="flex items-center gap-2 text-[#121212] min-h-[44px]"
        >
          <ArrowLeft size={16} />
          <span className="font-[Oswald] text-[11px] tracking-[2px] uppercase">Retour</span>
        </button>
      </div>

      {/* Article image */}
      <div className="w-full bg-[#f0ece6] overflow-hidden" style={{ aspectRatio: '4/5', maxHeight: '288px' }}>
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

      {/* Add to cart */}
      <div className="px-5 pt-2">
        {!selectedZone && (
          <p className="font-[Oswald] text-[11px] tracking-[2px] uppercase text-[#e51940] mb-3">
            Sélectionnez une zone d&apos;abord
          </p>
        )}
        <button
          type="button"
          onClick={handleAddToCart}
          disabled={!canSubmit}
          className="w-full py-4 bg-[#121212] text-white font-[Oswald] text-[13px] tracking-[2.5px] uppercase disabled:opacity-40 disabled:cursor-not-allowed transition-opacity"
        >
          Ajouter à la demande
        </button>
      </div>
    </div>
  );
}
