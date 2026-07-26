import axios from 'axios';

export interface LoginResponse {
  token: string;
  role: 'Administrator' | 'ProjectManager' | 'ITAdmin';
  email: string;
  name: string;
  expiresAt: string;
}

export interface UserDto {
  userId: string;
  name: string;
  email: string;
  role: string;
}

const SESSION_TOKEN_KEY = 'sarathi_token';

const axiosInstance = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000',
  headers: {
    'Content-Type': 'application/json',
  },
});

axiosInstance.interceptors.request.use((config) => {
  const token = sessionStorage.getItem(SESSION_TOKEN_KEY);
  if (token) {
    config.headers['Authorization'] = `Bearer ${token}`;
  }
  return config;
});

axiosInstance.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      window.dispatchEvent(new CustomEvent('auth:unauthorized'));
    }
    return Promise.reject(error);
  }
);

export const authService = {
  loginWithMicrosoftToken: async (microsoftToken: string): Promise<LoginResponse> => {
    const response = await axiosInstance.post<LoginResponse>('/api/auth/login', {
      microsoftToken,
    });
    return response.data;
  },

  getMe: async (): Promise<UserDto> => {
    const response = await axiosInstance.get<UserDto>('/api/auth/me');
    return response.data;
  },

  logout: async (): Promise<void> => {
    await axiosInstance.post('/api/auth/logout');
  },

  refreshToken: async (): Promise<LoginResponse> => {
    const response = await axiosInstance.post<LoginResponse>('/api/auth/refresh');
    return response.data;
  },
};
