import { useEffect } from 'react';
import { toast } from 'sonner';
import { getConnection } from '@/services/signalRClient';
import { useAuthStore } from '@/stores/authStore';
import { useRequestStore } from '@/stores/useRequestStore';
import type { RequestDto } from '@/types';

export function useStockistSignalR() {
  const selectedZone = useAuthStore((s) => s.selectedZone);
  const addRequest = useRequestStore((s) => s.addRequest);
  const updateRequest = useRequestStore((s) => s.updateRequest);

  useEffect(() => {
    const connection = getConnection();
    if (!connection) return;

    const handleNewRequest = (request: RequestDto) => {
      if (request.zone !== selectedZone) return;
      addRequest(request);
      toast.info(`Nouvelle demande — ${request.sellerFirstName} ${request.sellerLastName}`);
      if (navigator.vibrate) navigator.vibrate([200, 100, 200]);
    };

    const handleRequestUpdated = (request: RequestDto) => {
      updateRequest(request.id, request);
    };

    connection.on('NewRequest', handleNewRequest);
    connection.on('RequestUpdated', handleRequestUpdated);

    return () => {
      connection.off('NewRequest', handleNewRequest);
      connection.off('RequestUpdated', handleRequestUpdated);
    };
  }, [selectedZone, addRequest, updateRequest]);
}
