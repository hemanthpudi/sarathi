import axios from 'axios';

const SESSION_TOKEN_KEY = 'sarathi_token';

const azureDevOpsApi = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000',
  headers: { 'Content-Type': 'application/json' },
});

azureDevOpsApi.interceptors.request.use((config) => {
  const token = sessionStorage.getItem(SESSION_TOKEN_KEY);
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

azureDevOpsApi.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      window.dispatchEvent(new CustomEvent('auth:unauthorized'));
    }
    return Promise.reject(error);
  },
);

export interface AzureDevOpsIntegrationConfigurationDto {
  organizationUrl: string;
  projectFilter: string;
  syncEnabled: boolean;
  hasPersonalAccessTokenConfigured: boolean;
  personalAccessTokenUpdatedAtUtc: string | null;
  updatedAtUtc: string;
}

export interface UpdateAzureDevOpsIntegrationConfigurationRequestDto {
  organizationUrl: string;
  projectFilter: string;
  syncEnabled: boolean;
  personalAccessToken?: string;
}

export interface AzureDevOpsLiveProjectDto {
  projectName: string;
  organizationName: string;
  status: string;
  lastSyncUtc: string | null;
  items: number;
  durationSeconds: number | null;
  errors: number;
  latestActivityUtc: string | null;
  latestActivityMessage: string;
}

export interface AzureDevOpsLiveTimelineItemDto {
  projectName: string;
  status: string;
  items: number;
  timeUtc: string | null;
  message: string;
}

export interface AzureDevOpsLiveMetricDto {
  projectName: string;
  items: number;
}

export interface AzureDevOpsLiveSummaryDto {
  isConfigured: boolean;
  organizationUrl: string;
  message: string;
  generatedAtUtc: string;
  totalProjects: number;
  successfulProjects: number;
  warningProjects: number;
  failedProjects: number;
  totalItems: number;
  averageSyncTimeSeconds: number;
  projects: AzureDevOpsLiveProjectDto[];
  timeline: AzureDevOpsLiveTimelineItemDto[];
  metrics: AzureDevOpsLiveMetricDto[];
}

export interface AzureDevOpsScheduleDto {
  id: number;
  name: string;
  syncType: string;
  scopeName: string;
  source: string;
  intervalMinutes: number;
  isEnabled: boolean;
  lastRunUtc: string | null;
  nextRunUtc: string | null;
}

export interface UpsertAzureDevOpsScheduleRequestDto {
  id?: number | null;
  name: string;
  syncType: string;
  scopeName: string;
  intervalMinutes: number;
  isEnabled: boolean;
}

export interface QueueAzureDevOpsSyncRequestDto {
  syncType: string;
  scopeName: string;
  source: string;
}

export interface AzureDevOpsSyncQueueResponseDto {
  jobId: number;
  status: string;
  queuedAtUtc: string;
}

export interface AzureDevOpsConnectionTestRequestDto {
  organizationUrl: string;
  personalAccessToken?: string;
  projectFilter?: string;
}

export interface AzureDevOpsConnectionTestResultDto {
  isConnected: boolean;
  message: string;
  testedAtUtc: string;
  projectCount: number;
}

export const azureDevOpsService = {
  async getConfiguration(): Promise<AzureDevOpsIntegrationConfigurationDto> {
    const response = await azureDevOpsApi.get<AzureDevOpsIntegrationConfigurationDto>(
      '/api/integrations/azure-devops/configuration',
    );
    return response.data;
  },

  async updateConfiguration(
    payload: UpdateAzureDevOpsIntegrationConfigurationRequestDto,
  ): Promise<AzureDevOpsIntegrationConfigurationDto> {
    const response = await azureDevOpsApi.put<AzureDevOpsIntegrationConfigurationDto>(
      '/api/integrations/azure-devops/configuration',
      payload,
    );
    return response.data;
  },

  async getLiveSummary(): Promise<AzureDevOpsLiveSummaryDto> {
    const response = await azureDevOpsApi.get<AzureDevOpsLiveSummaryDto>('/api/integrations/azure-devops/live-summary');
    return response.data;
  },

  async getSchedules(): Promise<AzureDevOpsScheduleDto[]> {
    const response = await azureDevOpsApi.get<AzureDevOpsScheduleDto[]>('/api/integrations/azure-devops/schedules');
    return response.data;
  },

  async upsertSchedule(payload: UpsertAzureDevOpsScheduleRequestDto): Promise<AzureDevOpsScheduleDto> {
    const response = await azureDevOpsApi.put<AzureDevOpsScheduleDto>('/api/integrations/azure-devops/schedules', payload);
    return response.data;
  },

  async queueSynchronization(payload: QueueAzureDevOpsSyncRequestDto): Promise<AzureDevOpsSyncQueueResponseDto> {
    const response = await azureDevOpsApi.post<AzureDevOpsSyncQueueResponseDto>('/api/integrations/azure-devops/sync-jobs', payload);
    return response.data;
  },

  async testConnection(payload: AzureDevOpsConnectionTestRequestDto): Promise<AzureDevOpsConnectionTestResultDto> {
    const response = await azureDevOpsApi.post<AzureDevOpsConnectionTestResultDto>('/api/integrations/azure-devops/test-connection', payload);
    return response.data;
  },
};
