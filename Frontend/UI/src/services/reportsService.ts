import axios from 'axios';

const SESSION_TOKEN_KEY = 'sarathi_token';

const reportsApi = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000',
  headers: {
    'Content-Type': 'application/json',
  },
});

reportsApi.interceptors.request.use((config) => {
  const token = sessionStorage.getItem(SESSION_TOKEN_KEY);
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

reportsApi.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      window.dispatchEvent(new CustomEvent('auth:unauthorized'));
    }
    return Promise.reject(error);
  }
);

export interface ReportSectionSummaryDto {
  label: string;
  value: string;
}

export interface ReportSectionChartPointDto {
  label: string;
  value: number;
}

export interface ReportTableRowDto {
  cells: Record<string, string>;
}

export interface ReportSectionDto {
  key: string;
  title: string;
  description: string;
  summaries: ReportSectionSummaryDto[];
  chartPoints: ReportSectionChartPointDto[];
  columns: string[];
  rows: ReportTableRowDto[];
}

export interface ReportsOverviewDto {
  generatedAtUtc: string;
  sections: ReportSectionDto[];
}

export interface ExportReportResponseDto {
  fileName: string;
  contentType: string;
  contentBase64: string;
}

export const reportsService = {
  async getOverview(projectId?: number): Promise<ReportsOverviewDto> {
    const response = await reportsApi.get<ReportsOverviewDto>('/api/reports/overview', {
      params: projectId ? { projectId } : undefined,
    });
    return response.data;
  },

  async exportSection(sectionKey: string, projectId?: number): Promise<ExportReportResponseDto> {
    const response = await reportsApi.get<ExportReportResponseDto>(`/api/reports/export/${sectionKey}`, {
      params: projectId ? { projectId } : undefined,
    });
    return response.data;
  },
};