import { describe, it, expect, beforeEach, vi } from 'vitest';
import { renderHook, act } from '@testing-library/react';
import { toast } from 'sonner';
import apiClient from '@/services/apiClient';

// idb-keyval requires IndexedDB which is not available in jsdom
vi.mock('idb-keyval', () => ({
  get: vi.fn().mockResolvedValue(undefined),
  set: vi.fn().mockResolvedValue(undefined),
  del: vi.fn().mockResolvedValue(undefined),
}));
import { useRequestActions } from '@/hooks/useRequestActions';
import { useAuthStore } from '@/stores/authStore';
import { useRequestStore } from '@/stores/useRequestStore';
import { RequestStatus, UserRole, ZoneType } from '@/constants/enums';
import type { RequestDto } from '@/types';

vi.mock('@/services/apiClient', () => ({
  default: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}));

vi.mock('sonner', () => ({
  toast: {
    error: vi.fn(),
    success: vi.fn(),
  },
}));

// ── Fixtures ─────────────────────────────────────────────────────────────────

const mockUser = {
  id: 'seller-1',
  firstName: 'Alice',
  lastName: 'Dupont',
  role: UserRole.Seller,
};

const mockRequest: RequestDto = {
  id: 'req-1',
  sellerId: 'seller-1',
  sellerFirstName: 'Alice',
  sellerLastName: 'Dupont',
  storeId: '002',
  zone: ZoneType.RTW,
  status: RequestStatus.Pending,
  lines: [],
  alternativeLines: [],
  createdAt: new Date().toISOString(),
};

// ── Setup ─────────────────────────────────────────────────────────────────────

beforeEach(() => {
  vi.clearAllMocks();
  useRequestStore.setState({ requests: [], isLoading: false });
  useAuthStore.setState({
    token: 'test-token',
    user: mockUser,
    selectedStoreId: '002',
    selectedStoreName: 'Paris 2',
    selectedZone: ZoneType.RTW,
  });
});

// ── Tests ─────────────────────────────────────────────────────────────────────

describe('useRequestActions', () => {
  describe('cancelRequest', () => {
    it('calls DELETE /api/requests/{id} and sets status Cancelled in store on success', async () => {
      useRequestStore.setState({ requests: [mockRequest] });
      vi.mocked(apiClient.delete).mockResolvedValueOnce({});
      const { result } = renderHook(() => useRequestActions());

      await act(async () => {
        await result.current.cancelRequest(mockRequest.id);
      });

      expect(apiClient.delete).toHaveBeenCalledWith(`/requests/${mockRequest.id}`);
      const stored = useRequestStore.getState().requests[0];
      expect(stored.status).toBe(RequestStatus.Cancelled);
      expect(stored.cancelledAt).toBeDefined();
    });

    it('shows sonner error toast if request is not Pending (business error)', async () => {
      const inProgressRequest: RequestDto = { ...mockRequest, status: RequestStatus.InProgress };
      useRequestStore.setState({ requests: [inProgressRequest] });
      const { result } = renderHook(() => useRequestActions());

      await act(async () => {
        await result.current.cancelRequest(inProgressRequest.id);
      });

      expect(apiClient.delete).not.toHaveBeenCalled();
      expect(toast.error).toHaveBeenCalled();
    });

    it('shows sonner error toast on network error', async () => {
      useRequestStore.setState({ requests: [mockRequest] });
      vi.mocked(apiClient.delete).mockRejectedValueOnce(new Error('Erreur réseau'));
      const { result } = renderHook(() => useRequestActions());

      await act(async () => {
        await result.current.cancelRequest(mockRequest.id);
      });

      expect(toast.error).toHaveBeenCalled();
    });
  });

  describe('updateRequest', () => {
    it('calls PUT /api/requests/{id} and updates store on success', async () => {
      const updatedRequest: RequestDto = { ...mockRequest, status: RequestStatus.InProgress };
      useRequestStore.setState({ requests: [mockRequest] });
      vi.mocked(apiClient.put).mockResolvedValueOnce({ data: updatedRequest });
      const { result } = renderHook(() => useRequestActions());

      await act(async () => {
        await result.current.updateRequest(mockRequest.id, { status: RequestStatus.InProgress });
      });

      expect(apiClient.put).toHaveBeenCalledWith(`/requests/${mockRequest.id}`, {
        status: RequestStatus.InProgress,
      });
      expect(useRequestStore.getState().requests[0]).toMatchObject({
        status: RequestStatus.InProgress,
      });
    });

    it('only allowed when status === Pending — shows error toast otherwise', async () => {
      const inProgressRequest: RequestDto = { ...mockRequest, status: RequestStatus.InProgress };
      useRequestStore.setState({ requests: [inProgressRequest] });
      const { result } = renderHook(() => useRequestActions());

      await act(async () => {
        await result.current.updateRequest(inProgressRequest.id, {
          status: RequestStatus.Cancelled,
        });
      });

      expect(apiClient.put).not.toHaveBeenCalled();
      expect(toast.error).toHaveBeenCalled();
    });

    it('updates only the modified fields in the store', async () => {
      const updatedRequest: RequestDto = { ...mockRequest, stockistId: 'stockist-1' };
      useRequestStore.setState({ requests: [mockRequest] });
      vi.mocked(apiClient.put).mockResolvedValueOnce({ data: updatedRequest });
      const { result } = renderHook(() => useRequestActions());

      await act(async () => {
        await result.current.updateRequest(mockRequest.id, {
          status: RequestStatus.Pending,
          stockistId: 'stockist-1',
        });
      });

      const stored = useRequestStore.getState().requests[0];
      expect(stored.stockistId).toBe('stockist-1');
      expect(stored.storeId).toBe(mockRequest.storeId);
    });
  });

  describe('fetchRequests', () => {
    it('calls GET /api/requests?storeId={code}&zone={zone} with both params from authStore', async () => {
      vi.mocked(apiClient.get).mockResolvedValueOnce({
        data: { items: [], totalCount: 0, page: 1, pageSize: 50 },
      });
      const { result } = renderHook(() => useRequestActions());

      await act(async () => {
        await result.current.fetchRequests();
      });

      expect(apiClient.get).toHaveBeenCalledWith('/requests', {
        params: { storeId: '002', zone: ZoneType.RTW, page: 1, pageSize: 50 },
      });
    });

    it('sets isLoading during fetch', async () => {
      let resolve!: (value: unknown) => void;
      vi.mocked(apiClient.get).mockReturnValueOnce(
        new Promise((res) => {
          resolve = res;
        }),
      );
      const { result } = renderHook(() => useRequestActions());

      const pending = result.current.fetchRequests();
      expect(useRequestStore.getState().isLoading).toBe(true);

      await act(async () => {
        resolve({ data: { items: [], totalCount: 0, page: 1, pageSize: 50 } });
        await pending;
      });
      expect(useRequestStore.getState().isLoading).toBe(false);
    });

    it('handles empty response (sets empty array, no error)', async () => {
      vi.mocked(apiClient.get).mockResolvedValueOnce({
        data: { items: [], totalCount: 0, page: 1, pageSize: 50 },
      });
      const { result } = renderHook(() => useRequestActions());

      await act(async () => {
        await result.current.fetchRequests();
      });

      expect(useRequestStore.getState().requests).toEqual([]);
      expect(toast.error).not.toHaveBeenCalled();
    });

    it('passes storeId as store code string (not Guid)', async () => {
      useAuthStore.setState({ selectedStoreId: '003' });
      vi.mocked(apiClient.get).mockResolvedValueOnce({
        data: { items: [], totalCount: 0, page: 1, pageSize: 50 },
      });
      const { result } = renderHook(() => useRequestActions());

      await act(async () => {
        await result.current.fetchRequests();
      });

      expect(apiClient.get).toHaveBeenCalledWith(
        '/requests',
        expect.objectContaining({
          params: expect.objectContaining({ storeId: '003' }),
        }),
      );
    });
  });

  describe('respondToAlternatives', () => {
    it('calls POST /requests/{id}/respond-alternatives with responses payload', async () => {
      vi.mocked(apiClient.post).mockResolvedValueOnce({
        data: { ...mockRequest, status: RequestStatus.InProgress },
      });
      const { result } = renderHook(() => useRequestActions());

      await act(async () => {
        await result.current.respondToAlternatives(mockRequest.id, [
          { alternativeLineId: 'alt-1', accepted: true },
        ]);
      });

      expect(apiClient.post).toHaveBeenCalledWith(
        `/requests/${mockRequest.id}/respond-alternatives`,
        { responses: [{ alternativeLineId: 'alt-1', accepted: true }] },
      );
    });

    it('updates request in store after accept', async () => {
      const awaiting: RequestDto = {
        ...mockRequest,
        status: RequestStatus.AwaitingSellerResponse,
      };
      const inProgress: RequestDto = { ...mockRequest, status: RequestStatus.InProgress };
      useRequestStore.setState({ requests: [awaiting] });
      vi.mocked(apiClient.post).mockResolvedValueOnce({ data: inProgress });
      const { result } = renderHook(() => useRequestActions());

      await act(async () => {
        await result.current.respondToAlternatives(mockRequest.id, [
          { alternativeLineId: 'alt-1', accepted: true },
        ]);
      });

      expect(useRequestStore.getState().requests[0].status).toBe(RequestStatus.InProgress);
    });

    it('updates request in store after reject', async () => {
      const awaiting: RequestDto = {
        ...mockRequest,
        status: RequestStatus.AwaitingSellerResponse,
      };
      const unavailable: RequestDto = { ...mockRequest, status: RequestStatus.Unavailable };
      useRequestStore.setState({ requests: [awaiting] });
      vi.mocked(apiClient.post).mockResolvedValueOnce({ data: unavailable });
      const { result } = renderHook(() => useRequestActions());

      await act(async () => {
        await result.current.respondToAlternatives(mockRequest.id, [
          { alternativeLineId: 'alt-1', accepted: false },
        ]);
      });

      expect(useRequestStore.getState().requests[0].status).toBe(RequestStatus.Unavailable);
    });

    it('shows error toast on network failure', async () => {
      vi.mocked(apiClient.post).mockRejectedValueOnce(new Error('Erreur réseau'));
      const { result } = renderHook(() => useRequestActions());

      await act(async () => {
        await result.current.respondToAlternatives(mockRequest.id, [
          { alternativeLineId: 'alt-1', accepted: true },
        ]);
      });

      expect(toast.error).toHaveBeenCalled();
    });
  });
});
