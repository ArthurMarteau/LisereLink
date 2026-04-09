import { create } from 'zustand';
import type { Notification as AppNotification } from '@/types';

interface NotificationState {
  notifications: AppNotification[];
  unreadCount: number;
  addNotification: (notification: AppNotification) => void;
  markAllRead: () => void;
  clearNotifications: () => void;
}

export const useNotificationStore = create<NotificationState>()((set) => ({
  notifications: [],
  unreadCount: 0,
  addNotification: (notification) =>
    set((state) => ({
      notifications: [...state.notifications, notification],
      unreadCount: state.unreadCount + 1,
    })),
  markAllRead: () => set({ unreadCount: 0 }),
  clearNotifications: () => set({ notifications: [], unreadCount: 0 }),
}));
