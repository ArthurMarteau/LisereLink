import { useCallback } from 'react';
import { toast } from 'sonner';
import apiClient from '@/services/apiClient';
import { useRequestStore } from '@/stores/useRequestStore';
import { useAuthStore } from '@/stores/authStore';
import { RequestStatus } from '@/constants/enums';
import type { PagedResult, ProblemDetails, RequestDto } from '@/types';

interface UpdateRequestPayload {
  status: RequestStatus;
  stockistId?: string;
}

interface AlternativeResponse {
  alternativeLineId: string;
  accepted: boolean;
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
  const { updateRequest: storeUpdate, setLoading, setRequests } = useRequestStore();

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
        storeUpdate(id, {
          status: RequestStatus.Cancelled,
          cancelledAt: new Date().toISOString(),
        });
      } catch (error) {
        const message = isProblemDetails(error)
          ? error.detail
          : "Impossible d'annuler la demande.";
        toast.error(message);
      }
    },
    [storeUpdate],
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

  // ── respondToAlternatives ──────────────────────────────────────────────────

  const respondToAlternatives = useCallback(
    async (requestId: string, responses: AlternativeResponse[]): Promise<void> => {
      try {
        const response = await apiClient.post<RequestDto>(
          `/requests/${requestId}/respond-alternatives`,
          { responses },
        );
        storeUpdate(requestId, response.data);
      } catch (error) {
        const message = isProblemDetails(error)
          ? error.detail
          : 'Impossible de répondre aux alternatives.';
        toast.error(message);
      }
    },
    [storeUpdate],
  );

  return {
    cancelRequest,
    updateRequest,
    fetchRequests,
    respondToAlternatives,
  };
}
