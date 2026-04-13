import { useState, use, Suspense, useMemo } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { ArrowLeft, Search, ScanLine } from 'lucide-react';
import { toast } from 'sonner';
import apiClient from '@/services/apiClient';
import { useArticleStore } from '@/stores/useArticleStore';
import { useDebounce } from '@/hooks/useDebounce';
import ArticleCard from '@/components/ui/ArticleCard';
import type { ArticleDto, PagedResult } from '@/types';

const EMPTY_RESULT: PagedResult<ArticleDto> = {
  items: [],
  totalCount: 0,
  page: 1,
  pageSize: 50,
};

interface ResultsListProps {
  promise: Promise<PagedResult<ArticleDto>>;
  onSelect: (article: ArticleDto) => void;
}

function ResultsList({ promise, onSelect }: ResultsListProps) {
  const result = use(promise);

  if (result.items.length === 0) {
    return (
      <p className="font-[Oswald] text-[11px] tracking-[2px] uppercase text-[#969696] py-8 text-center">
        Aucun article trouvé
      </p>
    );
  }

  return (
    <>
      <p className="font-[Oswald] text-[11px] tracking-[2px] uppercase text-[#969696] mb-3">
        Résultats — {result.totalCount} articles
      </p>
      <div className="space-y-[10px]">
        {result.items.map((article) => (
          <ArticleCard
            key={article.id}
            article={article}
            onClick={() => onSelect(article)}
          />
        ))}
      </div>
    </>
  );
}

function ResultsSkeleton() {
  return (
    <div className="space-y-[10px]" aria-busy="true" aria-label="Recherche en cours">
      {[1, 2, 3].map((n) => (
        <div key={n} className="h-[90px] bg-white animate-pulse" />
      ))}
    </div>
  );
}

export default function StockistSearchPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const requestId = searchParams.get('requestId') ?? '';

  const setSearchResults = useArticleStore((s) => s.setSearchResults);
  const setSelectedArticle = useArticleStore((s) => s.setSelectedArticle);

  const [searchTerm, setSearchTerm] = useState('');
  const debouncedTerm = useDebounce(searchTerm, 300);

  const searchPromise = useMemo((): Promise<PagedResult<ArticleDto>> => {
    if (debouncedTerm.length < 2) {
      return Promise.resolve(EMPTY_RESULT);
    }
    return apiClient
      .get<PagedResult<ArticleDto>>(
        `/articles?query=${encodeURIComponent(debouncedTerm)}&page=1&pageSize=50`,
      )
      .then((r) => {
        setSearchResults(r.data.items);
        return r.data;
      })
      .catch(() => {
        toast.error('Erreur lors de la recherche.');
        return EMPTY_RESULT;
      });
  }, [debouncedTerm, setSearchResults]);

  function handleSelect(article: ArticleDto) {
    setSelectedArticle(article);
    navigate(`/stockist-article/${article.id}?requestId=${requestId}`);
  }

  return (
    <div className="bg-[#f9f4ef] min-h-full">
      {/* Header */}
      <div className="px-5 py-4 flex items-center gap-3">
        <button
          type="button"
          onClick={() => navigate(`/queue/${requestId}`)}
          className="min-h-[44px] min-w-[44px] flex items-center justify-center"
          aria-label="Retour à la demande"
        >
          <ArrowLeft size={16} />
        </button>
        <p className="font-[Oswald] text-[13px] tracking-[2.5px] uppercase text-[#121212]">
          Rechercher une alternative
        </p>
      </div>

      <div className="px-5 pb-5">
        {/* Search bar */}
        <div className="relative flex items-center bg-white border border-[#e1e1e1]">
          <Search size={16} className="absolute left-3 text-[#969696] pointer-events-none" />
          <input
            type="search"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            placeholder="Rechercher un article..."
            className="flex-1 pl-9 pr-12 py-3 font-['Libre_Baskerville'] text-[14px] text-[#121212] placeholder:text-[#969696] outline-none bg-transparent"
            autoCapitalize="off"
            autoCorrect="off"
            spellCheck={false}
          />
          <button
            type="button"
            onClick={() => navigate(`/stockist-scan?requestId=${requestId}`)}
            className="absolute right-0 text-[#969696] h-full px-3 flex items-center justify-center min-h-[44px] min-w-[44px]"
            aria-label="Scanner un code-barre"
          >
            <ScanLine size={20} />
          </button>
        </div>

        {searchTerm.length > 0 && searchTerm.length < 2 && (
          <p className="font-[Oswald] text-[11px] tracking-[2px] uppercase text-[#969696] py-8 text-center">
            Saisissez au moins 2 caractères
          </p>
        )}

        {debouncedTerm.length >= 2 && (
          <div className="mt-5">
            <Suspense fallback={<ResultsSkeleton />}>
              <ResultsList promise={searchPromise} onSelect={handleSelect} />
            </Suspense>
          </div>
        )}
      </div>
    </div>
  );
}
