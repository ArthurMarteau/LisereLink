import { useEffect, useRef } from 'react';
import type { RefObject } from 'react';
import Quagga from '@ericblade/quagga2';

interface QuaggaResult {
  codeResult: {
    code: string | null;
  };
}

const DEDUPE_WINDOW_MS = 2000;

export function useBarcode(
  containerRef: RefObject<HTMLDivElement | null>,
  onDetected: (barcode: string) => void
): void {
  // Keep callback ref fresh to avoid stale closures inside the effect
  const onDetectedRef = useRef(onDetected);
  onDetectedRef.current = onDetected;

  // Deduplicate rapid successive detections of the same code
  const lastDetected = useRef<{ code: string; time: number } | null>(null);

  useEffect(() => {
    if (!containerRef.current) return;

    let started = false;

    Quagga.init(
      {
        inputStream: {
          name: 'Live',
          type: 'LiveStream',
          target: containerRef.current,
          constraints: {
            facingMode: 'environment',
            width: { ideal: 1280 },
            height: { ideal: 720 },
          },
        },
        decoder: {
          readers: ['ean_reader'],
        },
        locate: true,
      },
      (err) => {
        if (err) {
          console.error('[useBarcode] Quagga init error:', err);
          return;
        }
        started = true;
        Quagga.start();
      }
    );

    const handleDetected = (result: QuaggaResult) => {
      const code = result.codeResult.code;
      if (!code) return;

      const now = Date.now();
      if (
        lastDetected.current?.code === code &&
        now - lastDetected.current.time < DEDUPE_WINDOW_MS
      ) {
        return;
      }
      lastDetected.current = { code, time: now };
      onDetectedRef.current(code);
    };

    Quagga.onDetected(handleDetected);

    return () => {
      Quagga.offDetected(handleDetected);
      if (started) {
        Quagga.stop();
      }
    };
    // containerRef is stable — intentionally omitted from deps
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);
}
