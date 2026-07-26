import axios from 'axios';

const SESSION_TOKEN_KEY = 'sarathi_token';

const projectManagerApi = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000',
  headers: {
    'Content-Type': 'application/json',
  },
});

projectManagerApi.interceptors.request.use((config) => {
  const token = sessionStorage.getItem(SESSION_TOKEN_KEY);
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

projectManagerApi.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      window.dispatchEvent(new CustomEvent('auth:unauthorized'));
    }
    return Promise.reject(error);
  }
);

export interface ProjectManagerChartPointDto {
  label: string;
  value: number;
}

export interface ProjectManagerAssignedProjectDto {
  projectId: number;
  projectName: string;
  visibility: string;
  projectRole: string;
  allocationPercent: number;
  deliveryHealth: string;
  completionRate: number | null;
  sprintVelocity: number | null;
  blockedItems: number;
  lastUpdated: string;
}

export interface ProjectManagerDashboardDto {
  assignedProjects: number;
  activeSprints: number;
  openWorkItems: number;
  blockedWorkItems: number;
  averageCompletionRate: number;
  averageSprintVelocity: number;
  generatedAtUtc: string;
  sprintVelocityTrend: ProjectManagerChartPointDto[];
  workItemsByState: ProjectManagerChartPointDto[];
  spotlightProjects: ProjectManagerAssignedProjectDto[];
}

export interface ProjectManagerSprintItemDto {
  sprintId: number;
  projectId: number;
  projectName: string;
  sprintName: string;
  startDate: string;
  endDate: string;
  plannedStoryPoints: number;
  completedStoryPoints: number;
  totalWorkItems: number;
  completedWorkItems: number;
  completionRate: number;
  status: string;
}

export interface ProjectManagerSprintProgressDto {
  generatedAtUtc: string;
  sprints: ProjectManagerSprintItemDto[];
}

export interface ProjectManagerWorkItemDto {
  workItemId: number;
  projectId: number;
  projectName: string;
  azureWorkItemId: string;
  title: string;
  workItemType: string;
  state: string;
  priority: string;
  assignedToName: string;
  storyPoints: number | null;
  progressPercent: number;
  isBlocked: boolean;
  sprintName: string;
  lastUpdatedUtc: string;
}

export interface ProjectManagerWorkItemsDto {
  totalWorkItems: number;
  openWorkItems: number;
  inProgressWorkItems: number;
  blockedWorkItems: number;
  generatedAtUtc: string;
  items: ProjectManagerWorkItemDto[];
}

export interface ProjectManagerKpisDto {
  averageCompletionRate: number;
  averageSprintVelocity: number;
  averageDefectDensity: number;
  averageBacklogHealth: number;
  averageReleaseSuccessRate: number;
  generatedAtUtc: string;
  completionTrend: ProjectManagerChartPointDto[];
  velocityTrend: ProjectManagerChartPointDto[];
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
  board: { name: string; items: ProjectManagerWorkItemDto[] }[];
}

export interface ProjectManagerAiAnalysisDto {
  projectId: number;
  projectName: string;
  completionRate: number;
  sprintVelocity: number;
  riskScore: number;
  blockedItems: number;
  activeSprints: number;
  summary: string;
  riskAnalysis: string;
  report: string;
  recommendations: string[];
  provider: string;
  generatedAtUtc: string;
}

export const projectManagerService = {
  async getDashboard(): Promise<ProjectManagerDashboardDto> {
    const response = await projectManagerApi.get<ProjectManagerDashboardDto>('/api/project-manager/dashboard');
    return response.data;
  },

  async getAssignedProjects(): Promise<ProjectManagerAssignedProjectDto[]> {
    const response = await projectManagerApi.get<ProjectManagerAssignedProjectDto[]>('/api/project-manager/assigned-projects');
    return response.data;
  },

  async getSprintProgress(): Promise<ProjectManagerSprintProgressDto> {
    const response = await projectManagerApi.get<ProjectManagerSprintProgressDto>('/api/project-manager/sprint-progress');
    return response.data;
  },

  async getWorkItems(params?: { projectId?: number; state?: string; take?: number }): Promise<ProjectManagerWorkItemsDto> {
    const response = await projectManagerApi.get<ProjectManagerWorkItemsDto>('/api/project-manager/work-items', {
      params,
    });
    return response.data;
  },

  async getKpis(): Promise<ProjectManagerKpisDto> {
    const response = await projectManagerApi.get<ProjectManagerKpisDto>('/api/project-manager/kpis');
    return response.data;
  },

  async getSprintGovernance(projectId: number): Promise<ProjectSprintGovernanceDto> {
    const response = await projectManagerApi.get<ProjectSprintGovernanceDto>(`/api/project-manager/projects/${projectId}/sprint-governance`);
    return response.data;
  },

  async getAiAnalysis(projectId: number, question?: string): Promise<ProjectManagerAiAnalysisDto> {
    const response = await projectManagerApi.post<ProjectManagerAiAnalysisDto>(`/api/project-manager/projects/${projectId}/ai-analysis`, { question });
    return response.data;
  },
};
