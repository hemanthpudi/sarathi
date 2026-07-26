import axios from 'axios';
import * as signalR from '@microsoft/signalr';

const SESSION_TOKEN_KEY = 'sarathi_token';

const notificationApi = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000',
  headers: {
    'Content-Type': 'application/json',
  },
});

notificationApi.interceptors.request.use((config) => {
  const token = sessionStorage.getItem(SESSION_TOKEN_KEY);
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

notificationApi.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      window.dispatchEvent(new CustomEvent('auth:unauthorized'));
    }
    return Promise.reject(error);
  }
);

export interface NotificationDto {
  id: number;
  title: string;
  message: string;
  category: string;
  severity: string;
  actionUrl: string | null;
  isRead: boolean;
  isToast: boolean;
  createdAtUtc: string;
}

export interface NotificationsOverviewDto {
  unreadCount: number;
  generatedAtUtc: string;
  items: NotificationDto[];
}

export interface CreateNotificationRequestDto {
  userId?: string;
  title: string;
  message: string;
  category?: string;
  severity?: string;
  actionUrl?: string;
  isToast?: boolean;
}

export const notificationService = {
  async getNotifications(take = 25): Promise<NotificationsOverviewDto> {
    const response = await notificationApi.get<NotificationsOverviewDto>('/api/notifications', {
      params: { take },
    });
    return response.data;
  },

  async createNotification(payload: CreateNotificationRequestDto): Promise<NotificationDto> {
    const response = await notificationApi.post<NotificationDto>('/api/notifications', payload);
    return response.data;
  },

  async markAsRead(notificationId: number): Promise<NotificationDto> {
    const response = await notificationApi.post<NotificationDto>('/api/notifications/read', { notificationId });
    return response.data;
  },

  createConnection(tokenFactory: () => string | null) {
    const apiBaseUrl = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';

    return new signalR.HubConnectionBuilder()
      .withUrl(`${apiBaseUrl}/hubs/notifications`, {
        accessTokenFactory: () => tokenFactory() ?? '',
      })
      .withAutomaticReconnect()
      .build();
  },
};