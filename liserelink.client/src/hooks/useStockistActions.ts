import { useCallback } from 'react';
import { toast } from 'sonner';
import apiClient from '@/services/apiClient';
import { useRequestStore } from '@/stores/useRequestStore';
import { useAuthStore } from '@/stores/authStore';
import type { PagedResult, ProblemDetails, RequestDto } from '@/types';

export interface ProposeAlternativeLinePayload {
  articleId: string;
  articleName: string;
  articleColorOrPrint: string;
  articleBarcode: string;
  requestedSizes: string[];
  quantity: number;
  stockOverride: boolean;
}

function isProblemDetails(error: unknown): error is ProblemDetails {
  return (
    error !== null &&
    typeof error === 'object' &&
    'title' in error &&
    'status' in error
  );
}

export function useStockistActions() {
  const { addRequest, updateRequest: storeUpdate, setLoading, setRequests } =
    useRequestStore();

  const fetchStockistRequests = useCallback(async (): Promise<void> => {
    const { selectedStoreId, selectedZone } = useAuthStore.getState();
    setLoading(true);
    try {
      const response = await apiClient.get<PagedResult<RequestDto>>('/requests', {
        params: {
          ...(selectedStoreId !== null && { storeId: selectedStoreId }),
          ...(selectedZone !== null && { zone: selectedZone }),
          page: 1,
          pageSize: 50,
        },
      });
      setRequests(response.data.items);
    } catch (error) {
      const message = isProblemDetails(error)
        ? error.detail
        : 'Impossible de charger les demandes.';
      toast.error(message);
    } finally {
      setLoading(false);
    }
  }, [setLoading, setRequests]);

  const takeRequest = useCallback(
    async (requestId: string): Promise<RequestDto | null> => {
      const { user } = useAuthStore.getState();
      if (!user) return null;
      try {
        const response = await apiClient.post<RequestDto>(
          `/requests/${requestId}/take`,
          { stockistId: user.id },
        );
        storeUpdate(requestId, response.data);
        return response.data;
      } catch (error) {
        const message = isProblemDetails(error)
          ? error.detail
          : 'Impossible de prendre en charge la demande.';
        toast.error(message);
        return null;
      }
    },
    [storeUpdate],
  );

  const markLineFound = useCallback(
    async (requestId: string, lineId: string): Promise<RequestDto | null> => {
      try {
        const response = await apiClient.post<RequestDto>(
          `/requests/${requestId}/lines/${lineId}/found`,
        );
        storeUpdate(requestId, response.data);
        return response.data;
      } catch (error) {
        const message = isProblemDetails(error)
          ? error.detail
          : 'Impossible de marquer la ligne comme trouvée.';
        toast.error(message);
        return null;
      }
    },
    [storeUpdate],
  );

  const proposeAlternatives = useCallback(
    async (
      requestId: string,
      lines: ProposeAlternativeLinePayload[],
    ): Promise<RequestDto | null> => {
      const { user } = useAuthStore.getState();
      if (!user) return null;
      try {
        const response = await apiClient.post<RequestDto>(
          `/requests/${requestId}/propose-alternatives`,
          { stockistId: user.id, lines },
        );
        storeUpdate(requestId, response.data);
        return response.data;
      } catch (error) {
        const message = isProblemDetails(error)
          ? error.detail
          : 'Impossible de proposer des alternatives.';
        toast.error(message);
        return null;
      }
    },
    [storeUpdate],
  );

  return {
    fetchStockistRequests,
    takeRequest,
    markLineFound,
    proposeAlternatives,
    addRequest,
  };
}
