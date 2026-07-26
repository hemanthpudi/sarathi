import axios from 'axios';

const SESSION_TOKEN_KEY = 'sarathi_token';

const itAdminApi = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000',
  headers: {
    'Content-Type': 'application/json',
  },
});

itAdminApi.interceptors.request.use((config) => {
  const token = sessionStorage.getItem(SESSION_TOKEN_KEY);
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

itAdminApi.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      window.dispatchEvent(new CustomEvent('auth:unauthorized'));
    }
    return Promise.reject(error);
  }
);

export interface ItAdminChartPointDto {
  label: string;
  value: number;
}

export interface ItAdminSyncJobDto {
  jobId: number;
  syncType: string;
  scopeName: string;
  status: string;
  source: string;
  triggeredByDisplayName: string;
  itemsProcessed: number;
  itemsSucceeded: number;
  itemsFailed: number;
  durationSeconds: number | null;
  errorMessage: string | null;
  startedAtUtc: string;
  completedAtUtc: string | null;
}

export interface ItAdminSynchronizationDto {
  jobsRunning: number;
  successfulRuns24h: number;
  failedRuns24h: number;
  queuedSchedules: number;
  averageDurationSeconds: number;
  lastSuccessfulSyncUtc: string | null;
  generatedAtUtc: string;
  syncTypeBreakdown: ItAdminChartPointDto[];
  recentJobs: ItAdminSyncJobDto[];
}

export interface ItAdminSyncMonitoringDto {
  totalJobsToday: number;
  runningJobs: number;
  failedJobs: number;
  warningLogs: number;
  overdueSchedules: number;
  activeMaintenanceWindows: number;
  generatedAtUtc: string;
  durationTrend: ItAdminChartPointDto[];
  statusBreakdown: ItAdminChartPointDto[];
  recentJobs: ItAdminSyncJobDto[];
}

export interface ItAdminSystemHealthDto {
  status: string;
  databaseReachable: boolean;
  azureDevOpsConfigured: boolean;
  synchronizationEnabled: boolean;
  runningSynchronizationJobs: number;
  generatedAtUtc: string;
}

export interface ItAdminLogEntryDto {
  id: number;
  severity: string;
  category: string;
  message: string;
  sourceSystem: string;
  correlationId: string | null;
  createdAtUtc: string;
}

export interface ItAdminLogsDto {
  totalLogs: number;
  errorLogs: number;
  warningLogs: number;
  generatedAtUtc: string;
  items: ItAdminLogEntryDto[];
}

export interface ItAdminMaintenanceTaskDto {
  id: number;
  title: string;
  environmentName: string;
  status: string;
  ownerName: string;
  notes: string;
  isRecurring: boolean;
  scheduledStartUtc: string;
  scheduledEndUtc: string | null;
}

export interface ItAdminMaintenanceDto {
  activeTasks: number;
  scheduledThisWeek: number;
  overdueTasks: number;
  generatedAtUtc: string;
  items: ItAdminMaintenanceTaskDto[];
}

export interface ItAdminScheduleDto {
  id: number;
  name: string;
  jobType: string;
  scopeName: string;
  cronExpression: string;
  timeZoneId: string;
  runWindow: string;
  isEnabled: boolean;
  failureCount: number;
  lastRunUtc: string | null;
  nextRunUtc: string | null;
}

export interface ItAdminSchedulingDto {
  enabledSchedules: number;
  disabledSchedules: number;
  overdueSchedules: number;
  generatedAtUtc: string;
  items: ItAdminScheduleDto[];
}

export interface ItAdminUserDto {
  userId: string;
  name: string;
  email: string;
  role: string;
  lastLoginUtc: string | null;
}

export interface ItAdminLoginAuditDto {
  auditId: string;
  userId: string;
  userName: string;
  email: string;
  loginTime: string;
  logoutTime: string | null;
  ipAddress: string;
  status: string;
}

export interface TriggerItAdminSyncRequestDto {
  syncType: string;
  scopeName: string;
  source: string;
}

export interface TriggerItAdminSyncResponseDto {
  jobId: number;
  status: string;
  queuedAtUtc: string;
}

export const itAdminService = {
  async getSynchronization(): Promise<ItAdminSynchronizationDto> {
    const response = await itAdminApi.get<ItAdminSynchronizationDto>('/api/it-admin/synchronization');
    return response.data;
  },

  async runSynchronization(payload: TriggerItAdminSyncRequestDto): Promise<TriggerItAdminSyncResponseDto> {
    const response = await itAdminApi.post<TriggerItAdminSyncResponseDto>('/api/it-admin/synchronization/run', payload);
    return response.data;
  },

  async getSyncMonitor(): Promise<ItAdminSyncMonitoringDto> {
    const response = await itAdminApi.get<ItAdminSyncMonitoringDto>('/api/it-admin/sync-monitor');
    return response.data;
  },

  async getLogs(params?: { severity?: string; take?: number }): Promise<ItAdminLogsDto> {
    const response = await itAdminApi.get<ItAdminLogsDto>('/api/it-admin/logs', { params });
    return response.data;
  },

  async getSystemHealth(): Promise<ItAdminSystemHealthDto> {
    const response = await itAdminApi.get<ItAdminSystemHealthDto>('/api/it-admin/system-health');
    return response.data;
  },

  async getMaintenance(): Promise<ItAdminMaintenanceDto> {
    const response = await itAdminApi.get<ItAdminMaintenanceDto>('/api/it-admin/maintenance');
    return response.data;
  },

  async getScheduling(): Promise<ItAdminSchedulingDto> {
    const response = await itAdminApi.get<ItAdminSchedulingDto>('/api/it-admin/scheduling');
    return response.data;
  },

  async getUsers(): Promise<ItAdminUserDto[]> {
    const response = await itAdminApi.get<ItAdminUserDto[]>('/api/it-admin/users');
    return response.data;
  },

  async getLoginAudits(take = 100): Promise<ItAdminLoginAuditDto[]> {
    const response = await itAdminApi.get<ItAdminLoginAuditDto[]>('/api/it-admin/login-audits', {
      params: { take },
    });
    return response.data;
  },
};
