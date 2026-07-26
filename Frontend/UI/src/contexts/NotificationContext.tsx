import React from 'react';
import { Alert, Snackbar } from '@mui/material';
import { notificationService, type NotificationDto } from '../services/notificationService';
import { useAuth } from '../hooks/useAuth';

interface NotificationContextValue {
  notifications: NotificationDto[];
  unreadCount: number;
  markAsRead: (notificationId: number) => Promise<void>;
  createNotification: (payload: {
    title: string;
    message: string;
    category?: string;
    severity?: string;
    actionUrl?: string;
    isToast?: boolean;
  }) => Promise<void>;
}

export const NotificationContext = React.createContext<NotificationContextValue | undefined>(undefined);

export const NotificationProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { isAuthenticated, token, user } = useAuth();
  const [notifications, setNotifications] = React.useState<NotificationDto[]>([]);
  const [toastQueue, setToastQueue] = React.useState<NotificationDto[]>([]);
  const [activeToast, setActiveToast] = React.useState<NotificationDto | null>(null);

  const unreadCount = React.useMemo(() => notifications.filter((item) => !item.isRead).length, [notifications]);

  const loadNotifications = React.useCallback(async () => {
    if (!isAuthenticated) {
      setNotifications([]);
      return;
    }

    try {
      const response = await notificationService.getNotifications(50);
      setNotifications(response.items);
    } catch {
      // keep notifications best-effort; auth interceptor already handles 401
    }
  }, [isAuthenticated]);

  React.useEffect(() => {
    void loadNotifications();
  }, [loadNotifications]);

  React.useEffect(() => {
    if (!isAuthenticated || !token || !user) {
      return;
    }

    const connection = notificationService.createConnection(() => sessionStorage.getItem('sarathi_token'));
    connection.on('notificationReceived', (notification: NotificationDto) => {
      setNotifications((current) => [notification, ...current.filter((item) => item.id !== notification.id)]);
      if (notification.isToast) {
        setToastQueue((current) => [...current, notification]);
      }
    });

    void connection.start().catch(() => undefined);

    return () => {
      connection.off('notificationReceived');
      void connection.stop();
    };
  }, [isAuthenticated, token, user]);

  React.useEffect(() => {
    if (!activeToast && toastQueue.length > 0) {
      const [nextToast, ...remaining] = toastQueue;
      setActiveToast(nextToast);
      setToastQueue(remaining);
    }
  }, [activeToast, toastQueue]);

  const markAsRead = React.useCallback(async (notificationId: number) => {
    const updated = await notificationService.markAsRead(notificationId);
    setNotifications((current) => current.map((item) => item.id === updated.id ? updated : item));
  }, []);

  const createNotification = React.useCallback(async (payload: {
    title: string;
    message: string;
    category?: string;
    severity?: string;
    actionUrl?: string;
    isToast?: boolean;
  }) => {
    const created = await notificationService.createNotification(payload);
    setNotifications((current) => [created, ...current.filter((item) => item.id !== created.id)]);
    if (created.isToast) {
      setToastQueue((current) => [...current, created]);
    }
  }, []);

  const contextValue = React.useMemo<NotificationContextValue>(() => ({
    notifications,
    unreadCount,
    markAsRead,
    createNotification,
  }), [notifications, unreadCount, markAsRead, createNotification]);

  return (
    <NotificationContext.Provider value={contextValue}>
      {children}
      <Snackbar
        open={!!activeToast}
        autoHideDuration={4500}
        onClose={() => setActiveToast(null)}
        anchorOrigin={{ vertical: 'top', horizontal: 'right' }}
      >
        <Alert
          onClose={() => setActiveToast(null)}
          severity={mapSeverity(activeToast?.severity)}
          variant="filled"
          sx={{ width: '100%' }}
        >
          <strong>{activeToast?.title}</strong>
          <div>{activeToast?.message}</div>
        </Alert>
      </Snackbar>
    </NotificationContext.Provider>
  );
};

const mapSeverity = (severity?: string): 'info' | 'success' | 'warning' | 'error' => {
  switch ((severity ?? '').toLowerCase()) {
    case 'success':
      return 'success';
    case 'warning':
      return 'warning';
    case 'error':
    case 'critical':
      return 'error';
    default:
      return 'info';
  }
};