import { useRef, useState, useCallback } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { ArrowLeft, Zap } from 'lucide-react';
import { toast } from 'sonner';
import apiClient from '@/services/apiClient';
import { useArticleStore } from '@/stores/useArticleStore';
import { useBarcode } from '@/hooks/useBarcode';
import type { ArticleDto } from '@/types';

export default function StockistScanPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const requestId = searchParams.get('requestId') ?? '';

  const scannerRef = useRef<HTMLDivElement>(null);
  const [torchOn, setTorchOn] = useState(false);
  const [manualBarcode, setManualBarcode] = useState('');
  const isProcessing = useRef(false);

  const handleDetected = useCallback(
    async (barcode: string) => {
      if (isProcessing.current) return;
      isProcessing.current = true;
      try {
        const res = await apiClient.get<ArticleDto>(`/articles/${barcode}`);
        useArticleStore.getState().setSelectedArticle(res.data);
        navigate(`/stockist-article/${res.data.id}?requestId=${requestId}`);
      } catch {
        toast.error('Article introuvable');
        isProcessing.current = false;
      }
    },
    [navigate, requestId],
  );

  useBarcode(scannerRef, handleDetected);

  async function toggleTorch() {
    const video = scannerRef.current?.querySelector('video');
    const stream = video?.srcObject as MediaStream | null;
    const track = stream?.getVideoTracks()[0];
    if (!track) return;
    try {
      await track.applyConstraints({
        advanced: [{ torch: !torchOn } as MediaTrackConstraintSet],
      });
      setTorchOn((prev) => !prev);
    } catch {
      // Torch not supported on this device — silently ignore
    }
  }

  async function handleManualSubmit(e: React.FormEvent) {
    e.preventDefault();
    const code = manualBarcode.trim();
    if (!code) return;
    await handleDetected(code);
  }

  return (
    <div className="fixed inset-0 z-50 bg-black flex flex-col">
      {/* Minimal header */}
      <div className="flex items-center justify-between px-5 h-16 shrink-0">
        <button
          type="button"
          onClick={() => navigate(`/stockist-search?requestId=${requestId}`)}
          className="text-white min-h-[44px] min-w-[44px] flex items-center justify-center"
          aria-label="Retour à la recherche"
        >
          <ArrowLeft size={24} />
        </button>

        <span className="font-[Oswald] text-[13px] tracking-[2.5px] uppercase text-white">
          Scanner un article
        </span>

        <button
          type="button"
          onClick={() => void toggleTorch()}
          className={`min-h-[44px] min-w-[44px] flex items-center justify-center transition-colors ${
            torchOn ? 'text-[#b28a2c]' : 'text-white'
          }`}
          aria-label="Torche"
          aria-pressed={torchOn}
        >
          <Zap size={22} fill={torchOn ? 'currentColor' : 'none'} />
        </button>
      </div>

      {/* Camera / viewfinder */}
      <div className="flex-1 relative overflow-hidden">
        <div
          ref={scannerRef}
          className="absolute inset-0 [&_video]:w-full [&_video]:h-full [&_video]:object-cover [&_canvas]:hidden"
        />
        <div className="absolute inset-0 flex items-center justify-center">
          <div className="relative w-64 h-40">
            <span className="absolute top-0 left-0 w-6 h-6 border-t-2 border-l-2 border-white" />
            <span className="absolute top-0 right-0 w-6 h-6 border-t-2 border-r-2 border-white" />
            <span className="absolute bottom-0 left-0 w-6 h-6 border-b-2 border-l-2 border-white" />
            <span className="absolute bottom-0 right-0 w-6 h-6 border-b-2 border-r-2 border-white" />
            <div className="absolute inset-x-2 top-1/2 h-px bg-white/70 animate-pulse" />
          </div>
        </div>
      </div>

      {/* Bottom controls */}
      <div className="px-5 pt-4 pb-8 shrink-0 space-y-3">
        <form onSubmit={(e) => void handleManualSubmit(e)} className="flex gap-2">
          <input
            type="text"
            value={manualBarcode}
            onChange={(e) => setManualBarcode(e.target.value)}
            placeholder="Saisir un code-barre"
            inputMode="numeric"
            className="flex-1 px-3 py-2 bg-white/10 text-white placeholder:text-white/40 font-['Libre_Baskerville'] text-[14px] outline-none border border-white/20"
          />
          <button
            type="submit"
            disabled={!manualBarcode.trim()}
            className="px-4 py-2 bg-white text-[#121212] font-[Oswald] text-[11px] tracking-[2px] uppercase disabled:opacity-40"
          >
            OK
          </button>
        </form>

        <button
          type="button"
          onClick={() => navigate(`/stockist-search?requestId=${requestId}`)}
          className="w-full py-3 border border-white text-white font-[Oswald] text-[13px] tracking-[2.5px] uppercase"
        >
          Rechercher manuellement
        </button>
      </div>
    </div>
  );
}
