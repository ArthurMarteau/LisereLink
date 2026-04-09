import { HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import type { HubConnection } from '@microsoft/signalr';
import { useAuthStore } from '@/stores/authStore';

let connection: HubConnection | null = null;

function buildConnection(): HubConnection {
  return new HubConnectionBuilder()
    .withUrl(import.meta.env.VITE_SIGNALR_HUB_URL ?? '/hubs/requests', {
      accessTokenFactory: () => useAuthStore.getState().token ?? '',
    })
    .withAutomaticReconnect([0, 2000, 5000, 10000])
    .build();
}

export async function startConnection(): Promise<void> {
  if (!connection) {
    connection = buildConnection();
  }
  if (connection.state === HubConnectionState.Disconnected) {
    await connection.start();
  }
}

export async function stopConnection(): Promise<void> {
  if (connection) {
    await connection.stop();
    connection = null;
  }
}

export function getConnection(): HubConnection | null {
  return connection;
}
