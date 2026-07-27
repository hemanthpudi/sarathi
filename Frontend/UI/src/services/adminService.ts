import axios from 'axios';

const SESSION_TOKEN_KEY = 'sarathi_token';

const adminApi = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000',
  headers: {
    'Content-Type': 'application/json',
  },
});

adminApi.interceptors.request.use((config) => {
  const token = sessionStorage.getItem(SESSION_TOKEN_KEY);
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

adminApi.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      window.dispatchEvent(new CustomEvent('auth:unauthorized'));
    }
    return Promise.reject(error);
  }
);

export interface AdminConfigurationDto {
  kpiRefreshIntervalMinutes: number;
  sprintVelocityTarget: number;
  completionRateTarget: number;
  defectDensityThreshold: number;
  backlogHealthThreshold: number;
  releaseSuccessRateTarget: number;
  azureDevOpsOrganizationUrl: string;
  azureDevOpsProjectFilter: string;
  updatedAtUtc: string;
}

export interface UpdateAdminConfigurationRequestDto {
  kpiRefreshIntervalMinutes: number;
  sprintVelocityTarget: number;
  completionRateTarget: number;
  defectDensityThreshold: number;
  backlogHealthThreshold: number;
  releaseSuccessRateTarget: number;
  azureDevOpsOrganizationUrl: string;
  azureDevOpsProjectFilter: string;
}

export interface AdminProjectStatisticItemDto {
  projectId: number;
  projectName: string;
  visibility: string;
  lastUpdated: string;
  completionRate: number | null;
  sprintVelocity: number | null;
  defectDensity: number | null;
  riskScore: number | null;
  riskLevel: string | null;
  projectManagerName: string | null;
  projectManagerEmail: string | null;
}

export interface AdminProjectStatisticsDto {
  totalProjects: number;
  publicProjects: number;
  privateProjects: number;
  highRiskProjects: number;
  averageCompletionRate: number;
  averageSprintVelocity: number;
  generatedAtUtc: string;
  projects: AdminProjectStatisticItemDto[];
}

export interface AdminProjectDetailsDto {
  projectId: number;
  projectName: string;
  azureProjectId: string;
  description: string;
  visibility: string;
  createdDate: string;
  lastUpdated: string;
  totalWorkItems: number;
  completedWorkItems: number;
  totalSprints: number;
  activeSprints: number;
  averageCompletionRate: number | null;
  averageSprintVelocity: number | null;
  riskScore: number | null;
  riskLevel: string | null;
  workItems: AdminProjectWorkItemDto[];
  sprints: AdminProjectSprintDto[];
  repositories: AdminProjectRepositoryDto[];
  builds: AdminProjectBuildDto[];
  teamMembers: AdminProjectTeamMemberDto[];
}

export interface AdminProjectWorkItemDto {
  workItemId: number;
  azureWorkItemId: string;
  title: string;
  workItemType: string;
  state: string;
  priority: string;
  assignedToName: string;
  storyPoints: number | null;
  progressPercent: number;
  isBlocked: boolean;
  lastUpdatedUtc: string;
  sprintName: string | null;
}

export interface AdminProjectSprintDto {
  sprintId: number;
  azureIterationId: string;
  sprintName: string;
  startDate: string;
  endDate: string;
  plannedStoryPoints: number;
  completedStoryPoints: number;
  totalWorkItems: number;
  completedWorkItems: number;
  status: string;
}

export interface AdminProjectTeamMemberDto {
  name: string;
  email: string;
  role: string;
  projectRole: string;
  allocationPercent: number;
}

export interface AdminProjectRepositoryDto {
  repositoryId: number;
  azureRepoId: string;
  repositoryName: string;
  defaultBranch: string | null;
  size: number | null;
  url: string | null;
  lastUpdatedUtc: string | null;
}

export interface AdminProjectBuildDto {
  buildId: number;
  azureBuildId: number;
  definitionName: string;
  buildNumber: string;
  status: string;
  result: string | null;
  sourceBranch: string;
  startTime: string;
  finishTime: string | null;
  triggerType: string | null;
}

export interface ProjectSprintGovernanceItemDto {
  workItemId: number;
  azureWorkItemId: string;
  title: string;
  workItemType: string;
  state: string;
  priority: string;
  assignedToName: string;
  storyPoints: number | null;
  progressPercent: number;
  isBlocked: boolean;
  isDelayed: boolean;
  lastUpdatedUtc: string;
}

export interface ProjectSprintGovernanceDto {
  projectId: number;
  projectName: string;
  deliveryHealth: string;
  sprintName: string;
  sprintId: number;
  sprintHealth: number;
  capacityUsedPercent: number;
  completedStoryPoints: number;
  plannedStoryPoints: number;
  completedItems: number;
  plannedItems: number;
  delayedItems: number;
  sprintStartDate: string | null;
  sprintEndDate: string | null;
  generatedAtUtc: string;
  board: { name: string; items: ProjectSprintGovernanceItemDto[] }[];
}

export const adminService = {
  async getConfiguration(): Promise<AdminConfigurationDto> {
    const response = await adminApi.get<AdminConfigurationDto>('/api/admin/configure');
    return response.data;
  },

  async updateConfiguration(payload: UpdateAdminConfigurationRequestDto): Promise<AdminConfigurationDto> {
    const response = await adminApi.put<AdminConfigurationDto>('/api/admin/configure', payload);
    return response.data;
  },

  async getProjectStatistics(take = 20): Promise<AdminProjectStatisticsDto> {
    const response = await adminApi.get<AdminProjectStatisticsDto>('/api/admin/project-statistics', {
      params: { take },
    });
    return response.data;
  },

  async getProjectDetails(projectId: number): Promise<AdminProjectDetailsDto> {
    const response = await adminApi.get<AdminProjectDetailsDto>(`/api/admin/projects/${projectId}`);
    return response.data;
  },

  async getSprintGovernance(projectId: number): Promise<ProjectSprintGovernanceDto> {
    const response = await adminApi.get<ProjectSprintGovernanceDto>(`/api/admin/projects/${projectId}/sprint-governance`);
    return response.data;
  },
};
