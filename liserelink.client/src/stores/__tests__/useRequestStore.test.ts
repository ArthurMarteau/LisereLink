import { describe, it, expect, beforeEach, vi } from 'vitest';
import { RequestStatus, ZoneType } from '@/constants/enums';
import type { RequestDto } from '@/types';

vi.mock('idb-keyval', () => ({
  get: vi.fn().mockResolvedValue(null),
  set: vi.fn().mockResolvedValue(undefined),
  del: vi.fn().mockResolvedValue(undefined),
}));

const { useRequestStore } = await import('../useRequestStore');

function makeRequest(overrides: Partial<RequestDto> = {}): RequestDto {
  return {
    id: crypto.randomUUID(),
    sellerId: 'seller-1',
    sellerFirstName: 'Alice',
    sellerLastName: 'Dupont',
    storeId: '002',
    zone: ZoneType.RTW,
    status: RequestStatus.Pending,
    lines: [],
    alternativeLines: [],
    createdAt: new Date().toISOString(),
    ...overrides,
  } as RequestDto;
}

beforeEach(() => {
  useRequestStore.setState({ requests: [], isLoading: false });
});

describe('useRequestStore — initial state', () => {
  it('requests is an empty array', () => {
    expect(useRequestStore.getState().requests).toEqual([]);
  });

  it('isLoading is false', () => {
    expect(useRequestStore.getState().isLoading).toBe(false);
  });
});

describe('useRequestStore — setRequests', () => {
  it('replaces the entire list', () => {
    const r1 = makeRequest();
    const r2 = makeRequest();
    useRequestStore.getState().setRequests([r1, r2]);
    expect(useRequestStore.getState().requests).toEqual([r1, r2]);
  });

  it('empties the list when given an empty array', () => {
    useRequestStore.getState().setRequests([makeRequest()]);
    useRequestStore.getState().setRequests([]);
    expect(useRequestStore.getState().requests).toEqual([]);
  });
});

describe('useRequestStore — addRequest', () => {
  it('prepends the request at index 0', () => {
    const existing = makeRequest();
    useRequestStore.getState().setRequests([existing]);
    const newReq = makeRequest();
    useRequestStore.getState().addRequest(newReq);
    expect(useRequestStore.getState().requests[0]).toEqual(newReq);
  });

  it('most recently added request is at index 0', () => {
    const first = makeRequest();
    const second = makeRequest();
    useRequestStore.getState().addRequest(first);
    useRequestStore.getState().addRequest(second);
    expect(useRequestStore.getState().requests[0]).toEqual(second);
    expect(useRequestStore.getState().requests[1]).toEqual(first);
  });
});

describe('useRequestStore — updateRequest', () => {
  it('updates only the targeted request by id', () => {
    const r = makeRequest({ status: RequestStatus.Pending });
    useRequestStore.getState().setRequests([r]);
    useRequestStore.getState().updateRequest(r.id, { status: RequestStatus.InProgress });
    expect(useRequestStore.getState().requests[0].status).toBe(RequestStatus.InProgress);
  });

  it('does not modify other requests in the list', () => {
    const r1 = makeRequest();
    const r2 = makeRequest();
    useRequestStore.getState().setRequests([r1, r2]);
    useRequestStore.getState().updateRequest(r1.id, { status: RequestStatus.Delivered });
    expect(useRequestStore.getState().requests[1]).toEqual(r2);
  });

  it('does not modify the list when id does not exist', () => {
    const r = makeRequest();
    useRequestStore.getState().setRequests([r]);
    useRequestStore
      .getState()
      .updateRequest('non-existent-id', { status: RequestStatus.Cancelled });
    expect(useRequestStore.getState().requests).toEqual([r]);
  });
});

describe('useRequestStore — removeRequest', () => {
  it('removes the request targeted by id', () => {
    const r1 = makeRequest();
    const r2 = makeRequest();
    useRequestStore.getState().setRequests([r1, r2]);
    useRequestStore.getState().removeRequest(r1.id);
    expect(useRequestStore.getState().requests).toEqual([r2]);
  });

  it('does not modify the list when id does not exist', () => {
    const r = makeRequest();
    useRequestStore.getState().setRequests([r]);
    useRequestStore.getState().removeRequest('non-existent-id');
    expect(useRequestStore.getState().requests).toEqual([r]);
  });
});

describe('useRequestStore — setLoading', () => {
  it('setLoading(true) sets isLoading to true', () => {
    useRequestStore.getState().setLoading(true);
    expect(useRequestStore.getState().isLoading).toBe(true);
  });

  it('setLoading(false) sets isLoading to false', () => {
    useRequestStore.getState().setLoading(true);
    useRequestStore.getState().setLoading(false);
    expect(useRequestStore.getState().isLoading).toBe(false);
  });
});

describe('useRequestStore — selectors', () => {
  const pending1 = makeRequest({ status: RequestStatus.Pending });
  const pending2 = makeRequest({ status: RequestStatus.Pending });
  const inProgress = makeRequest({ status: RequestStatus.InProgress });
  const awaiting = makeRequest({ status: RequestStatus.AwaitingSellerResponse });
  const delivered = makeRequest({ status: RequestStatus.Delivered });

  beforeEach(() => {
    useRequestStore.getState().setRequests([pending1, pending2, inProgress, awaiting, delivered]);
  });

  it('selectPendingRequests returns only Pending requests', () => {
    const result = useRequestStore.getState().selectPendingRequests();
    expect(result).toEqual([pending1, pending2]);
  });

  it('selectInProgressRequests returns only InProgress requests', () => {
    const result = useRequestStore.getState().selectInProgressRequests();
    expect(result).toEqual([inProgress]);
  });

  it('selectAwaitingResponseRequests returns only AwaitingSellerResponse requests', () => {
    const result = useRequestStore.getState().selectAwaitingResponseRequests();
    expect(result).toEqual([awaiting]);
  });

  it('selectPendingRequests does not return InProgress requests', () => {
    const result = useRequestStore.getState().selectPendingRequests();
    expect(result.some((r) => r.status === RequestStatus.InProgress)).toBe(false);
  });

  it('selectPendingRequests returns an empty array when there are no Pending requests', () => {
    useRequestStore.getState().setRequests([inProgress, delivered]);
    expect(useRequestStore.getState().selectPendingRequests()).toEqual([]);
  });

  it('selectors reflect the updated list after setRequests()', () => {
    const newPending = makeRequest({ status: RequestStatus.Pending });
    useRequestStore.getState().setRequests([newPending]);
    expect(useRequestStore.getState().selectPendingRequests()).toEqual([newPending]);
    expect(useRequestStore.getState().selectInProgressRequests()).toEqual([]);
  });
});
