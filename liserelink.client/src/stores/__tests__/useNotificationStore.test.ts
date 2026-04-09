import { describe, it, expect, beforeEach } from 'vitest';
import { useNotificationStore } from '../useNotificationStore';
import type { Notification as AppNotification } from '@/types';

function makeNotification(overrides: Partial<AppNotification> = {}): AppNotification {
  return {
    id: crypto.randomUUID(),
    message: 'Nouvelle demande créée',
    type: 'info',
    createdAt: new Date().toISOString(),
    ...overrides,
  };
}

beforeEach(() => {
  useNotificationStore.setState({ notifications: [], unreadCount: 0 });
});

describe('useNotificationStore — initial state', () => {
  it('notifications is an empty array', () => {
    expect(useNotificationStore.getState().notifications).toEqual([]);
  });

  it('unreadCount is 0', () => {
    expect(useNotificationStore.getState().unreadCount).toBe(0);
  });
});

describe('useNotificationStore — addNotification', () => {
  it('adds the notification to the list', () => {
    const notif = makeNotification();
    useNotificationStore.getState().addNotification(notif);
    expect(useNotificationStore.getState().notifications).toContainEqual(notif);
  });

  it('increments unreadCount by 1', () => {
    useNotificationStore.getState().addNotification(makeNotification());
    expect(useNotificationStore.getState().unreadCount).toBe(1);
  });

  it('three adds result in unreadCount === 3', () => {
    useNotificationStore.getState().addNotification(makeNotification());
    useNotificationStore.getState().addNotification(makeNotification());
    useNotificationStore.getState().addNotification(makeNotification());
    expect(useNotificationStore.getState().unreadCount).toBe(3);
  });
});

describe('useNotificationStore — markAllRead', () => {
  it('resets unreadCount to 0', () => {
    useNotificationStore.getState().addNotification(makeNotification());
    useNotificationStore.getState().addNotification(makeNotification());
    useNotificationStore.getState().markAllRead();
    expect(useNotificationStore.getState().unreadCount).toBe(0);
  });

  it('does not remove notifications from the list', () => {
    const notif = makeNotification();
    useNotificationStore.getState().addNotification(notif);
    useNotificationStore.getState().markAllRead();
    expect(useNotificationStore.getState().notifications).toContainEqual(notif);
  });

  it('keeps unreadCount at 0 when already 0 (no negative count)', () => {
    useNotificationStore.getState().markAllRead();
    expect(useNotificationStore.getState().unreadCount).toBe(0);
  });
});

describe('useNotificationStore — clearNotifications', () => {
  it('empties the notifications array', () => {
    useNotificationStore.getState().addNotification(makeNotification());
    useNotificationStore.getState().clearNotifications();
    expect(useNotificationStore.getState().notifications).toEqual([]);
  });

  it('resets unreadCount to 0', () => {
    useNotificationStore.getState().addNotification(makeNotification());
    useNotificationStore.getState().clearNotifications();
    expect(useNotificationStore.getState().unreadCount).toBe(0);
  });

  it('after clearNotifications() then addNotification(), unreadCount is 1', () => {
    useNotificationStore.getState().addNotification(makeNotification());
    useNotificationStore.getState().addNotification(makeNotification());
    useNotificationStore.getState().clearNotifications();
    useNotificationStore.getState().addNotification(makeNotification());
    expect(useNotificationStore.getState().unreadCount).toBe(1);
  });
});
