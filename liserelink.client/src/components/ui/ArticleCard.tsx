import { ChevronRight } from 'lucide-react';
import type { ArticleDto } from '@/types';
import SizeChip from './SizeChip';

interface ArticleCardProps {
  article: ArticleDto;
  onClick: () => void;
}

function ArticlePlaceholder() {
  return (
    <svg
      viewBox="0 0 56 70"
      className="w-full h-full"
      fill="none"
      aria-hidden="true"
    >
      <rect width="56" height="70" fill="#f0ece6" />
      <path
        d="M20 30 L28 22 L36 30 L36 48 L20 48 Z"
        stroke="#bcbcbc"
        strokeWidth="1.5"
        fill="none"
      />
    </svg>
  );
}

export default function ArticleCard({ article, onClick }: ArticleCardProps) {
  return (
    <button
      type="button"
      onClick={onClick}
      className="w-full flex items-center gap-3 bg-white p-[14px] text-left"
    >
      {/* Thumbnail */}
      <div className="shrink-0 w-14 h-[70px] bg-[#f0ece6] overflow-hidden">
        {article.imageUrl ? (
          <img
            src={article.imageUrl}
            alt={article.name}
            className="w-full h-full object-cover"
          />
        ) : (
          <ArticlePlaceholder />
        )}
      </div>

      {/* Info */}
      <div className="flex-1 min-w-0">
        <p className="font-['Libre_Baskerville'] text-[15px] text-[#121212] truncate">
          {article.name}
        </p>
        <p className="font-[Oswald] text-[11px] tracking-[2px] uppercase text-[#969696] mt-0.5">
          {article.colorOrPrint} · {article.family}
        </p>
        {article.availableSizes.length > 0 && (
          <div className="flex flex-wrap gap-1 mt-2">
            {article.availableSizes.map((size) => (
              <SizeChip key={size} size={size} available selected={false} />
            ))}
          </div>
        )}
      </div>

      <ChevronRight size={16} className="text-[#969696] shrink-0" />
    </button>
  );
}
