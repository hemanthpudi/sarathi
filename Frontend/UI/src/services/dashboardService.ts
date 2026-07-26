import axios from 'axios';

const SESSION_TOKEN_KEY = 'sarathi_token';

const dashboardApi = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000',
  headers: {
    'Content-Type': 'application/json',
  },
});

dashboardApi.interceptors.request.use((config) => {
  const token = sessionStorage.getItem(SESSION_TOKEN_KEY);
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

dashboardApi.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      window.dispatchEvent(new CustomEvent('auth:unauthorized'));
    }
    return Promise.reject(error);
  }
);

export interface DashboardChartPointDto {
  label: string;
  value: number;
}

export interface DashboardKpiCalculationsDto {
  averageCompletionRate: number;
  averageSprintVelocity: number;
  averageDefectDensity: number;
  averageBacklogHealth: number;
  averageReleaseSuccessRate: number;
  generatedAtUtc: string;
  completionTrend: DashboardChartPointDto[];
  velocityTrend: DashboardChartPointDto[];
}

export const dashboardService = {
  async getKpiCalculations(): Promise<DashboardKpiCalculationsDto> {
    const response = await dashboardApi.get<DashboardKpiCalculationsDto>('/api/dashboard/kpi-calculations');
    return response.data;
  },
};
