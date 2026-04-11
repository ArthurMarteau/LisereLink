import { useCallback } from 'react';
import { toast } from 'sonner';
import apiClient from '@/services/apiClient';
import { useRequestStore } from '@/stores/useRequestStore';
import { useAuthStore } from '@/stores/authStore';
import { RequestStatus } from '@/constants/enums';
import type { ArticleDto, PagedResult, ProblemDetails, RequestDto } from '@/types';
import type { Size } from '@/constants/enums';

interface UpdateRequestPayload {
  status: RequestStatus;
  stockistId?: string;
}

function isProblemDetails(error: unknown): error is ProblemDetails {
  return (
    error !== null &&
    typeof error === 'object' &&
    'title' in error &&
    'status' in error
  );
}

export function useRequestActions() {
  const { addRequest, removeRequest, updateRequest: storeUpdate, setLoading, setRequests } =
    useRequestStore();

  // ── createRequest ──────────────────────────────────────────────────────────

  const createRequest = useCallback(
    async (article: ArticleDto, sizes: Size[]): Promise<void> => {
      const { selectedStoreId, selectedZone, user } = useAuthStore.getState();

      if (selectedZone === null) return;
      if (selectedStoreId === null) return;

      setLoading(true);
      try {
        const response = await apiClient.post<RequestDto>('/requests', {
          sellerId: user!.id,
          storeId: selectedStoreId,
          zone: selectedZone,
          lines: [
            {
              articleId: article.id,
              articleName: article.name,
              articleColorOrPrint: article.colorOrPrint,
              articleBarcode: article.barcode,
              requestedSizes: sizes,
              quantity: 1,
            },
          ],
        });
        addRequest(response.data);
      } catch (error) {
        const message = isProblemDetails(error)
          ? error.detail
          : 'Impossible de créer la demande.';
        toast.error(message);
        throw error;
      } finally {
        setLoading(false);
      }
    },
    [addRequest, setLoading],
  );

  // ── cancelRequest ──────────────────────────────────────────────────────────

  const cancelRequest = useCallback(
    async (id: string): Promise<void> => {
      const request = useRequestStore.getState().requests.find((r) => r.id === id);

      if (!request || request.status !== RequestStatus.Pending) {
        toast.error('Seules les demandes en attente peuvent être annulées.');
        return;
      }

      try {
        await apiClient.delete(`/requests/${id}`);
        removeRequest(id);
      } catch (error) {
        const message = isProblemDetails(error)
          ? error.detail
          : "Impossible d'annuler la demande.";
        toast.error(message);
      }
    },
    [removeRequest],
  );

  // ── updateRequest ──────────────────────────────────────────────────────────

  const updateRequest = useCallback(
    async (id: string, dto: UpdateRequestPayload): Promise<void> => {
      const request = useRequestStore.getState().requests.find((r) => r.id === id);

      if (!request || request.status !== RequestStatus.Pending) {
        toast.error('Seules les demandes en attente peuvent être modifiées.');
        return;
      }

      try {
        const response = await apiClient.put<RequestDto>(`/requests/${id}`, dto);
        storeUpdate(id, response.data);
      } catch (error) {
        const message = isProblemDetails(error)
          ? error.detail
          : 'Impossible de modifier la demande.';
        toast.error(message);
      }
    },
    [storeUpdate],
  );

  // ── fetchRequests ──────────────────────────────────────────────────────────

  const fetchRequests = useCallback(async (): Promise<void> => {
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

  // ── acceptAlternative ──────────────────────────────────────────────────────

  const acceptAlternative = useCallback(
    async (id: string): Promise<void> => {
      try {
        const response = await apiClient.post<RequestDto>(
          `/requests/${id}/accept-alternative`,
        );
        storeUpdate(id, response.data);
        toast.success('Alternative acceptée.');
      } catch (error) {
        const message = isProblemDetails(error)
          ? error.detail
          : "Impossible d'accepter l'alternative.";
        toast.error(message);
      }
    },
    [storeUpdate],
  );

  // ── rejectAlternative ──────────────────────────────────────────────────────

  const rejectAlternative = useCallback(
    async (id: string): Promise<void> => {
      try {
        const response = await apiClient.post<RequestDto>(
          `/requests/${id}/reject-alternative`,
        );
        storeUpdate(id, response.data);
        toast.error('Alternative refusée. La demande est marquée indisponible.');
      } catch (error) {
        const message = isProblemDetails(error)
          ? error.detail
          : "Impossible de refuser l'alternative.";
        toast.error(message);
      }
    },
    [storeUpdate],
  );

  return {
    createRequest,
    cancelRequest,
    updateRequest,
    fetchRequests,
    acceptAlternative,
    rejectAlternative,
  };
}
