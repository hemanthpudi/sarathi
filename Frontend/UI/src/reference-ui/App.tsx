import { useState, useEffect, useMemo, useRef } from "react";
import sarathiLogo from "../imports/Media.jpg";
import * as XLSX from 'xlsx';
import {
  AreaChart, Area, BarChart, Bar, LineChart, Line,
  PieChart, Pie, Cell, RadarChart, Radar, PolarGrid, PolarAngleAxis,
  XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer,
  ComposedChart
} from "recharts";
import { motion } from "motion/react";
import {
  LayoutDashboard, FolderKanban, BarChart3, Brain, FileText,
  Users, Settings, Bell, Search, ChevronDown, ChevronRight,
  AlertTriangle, CheckCircle, XCircle, TrendingUp, TrendingDown,
  Clock, Activity, Zap, Shield, RefreshCw, Database, GitBranch,
  Layers, Target, Calendar, Filter, Download, Plus, MoreVertical,
  ArrowRight, Loader2, Eye, Edit2, Trash2, UserPlus, Key, Lock,
  Send, Bot, Sparkles, Flag, Bug, GitPullRequest, Package, Play,
  RotateCcw, ExternalLink, Building2, GitCommit, Terminal, Upload,
  Copy, Info, AlertCircle, X, Menu, Award, Cpu, Wifi, Home,
  ArrowUpRight, ArrowDownRight, Minus, CheckSquare, List,
  Percent, Star, Briefcase, GitMerge, Inbox, Hash, Check,
  Globe, Server, ChevronLeft, LogOut, HardDrive, Mail,
  BarChart2, User, PhoneCall, Gauge, Radio, Triangle, Hexagon
} from "lucide-react";
import { adminService, type AdminProjectStatisticItemDto, type AdminProjectStatisticsDto, type AdminProjectDetailsDto, type ProjectSprintGovernanceDto } from "../services/adminService";
import {
  projectManagerService,
  type ProjectManagerAssignedProjectDto,
  type ProjectManagerKpisDto,
  type ProjectManagerSprintProgressDto,
  type ProjectSprintGovernanceDto as ProjectManagerSprintGovernanceDto,
  type ProjectManagerAiAnalysisDto,
  type ProjectManagerWorkItemDto,
} from "../services/projectManagerService";
import { itAdminService, type ItAdminLogEntryDto, type ItAdminLogsDto, type ItAdminMaintenanceDto, type ItAdminSchedulingDto, type ItAdminSyncJobDto, type ItAdminSyncMonitoringDto, type ItAdminSynchronizationDto, type ItAdminSystemHealthDto, type ItAdminUserDto, type ItAdminLoginAuditDto, type TriggerItAdminSyncRequestDto } from "../services/itAdminService";
import { dashboardService, type DashboardKpiCalculationsDto } from "../services/dashboardService";
import { azureDevOpsService, type AzureDevOpsIntegrationConfigurationDto, type AzureDevOpsLiveSummaryDto, type UpdateAzureDevOpsIntegrationConfigurationRequestDto } from "../services/azureDevOpsService";
import {
  reportsService,
  type ReportSectionDto,
  type ReportsOverviewDto,
} from "../services/reportsService";

// changes made
// ──────────────────────────────────────────────
// ROLES
// ──────────────────────────────────────────────
type Role = "Administrator" | "Project Manager" | "Executive" | "PMO" | "Scrum Master" | "IT Manager";
export type EmbeddedDashboardRole = "Administrator" | "Project Manager" | "IT Manager";

type AiChatMessage = { role: "assistant" | "user"; msg: string };

interface EmbeddedDashboardProps {
  embeddedRole?: EmbeddedDashboardRole;
  embeddedEmail?: string;
  onEmbeddedLogout?: () => void;
}

const USER_ROLES: Record<string, Role> = {
  "pradeepadmin@gmail.com": "Administrator",
  "deepthiadmin@gmail.com": "Administrator",
  "sripriyaadmin@gmail.com": "Administrator",
  "tiruadmin@gmail.com": "Administrator",
  "admin@sarathi.ai": "Administrator",
  "pradeepmanager@gmail.com": "Project Manager",
  "hemanthmanager@gmail.com": "Project Manager",
  "deepthimanager@gmail.com": "Project Manager",
  "sripriyamanager@gmail.com": "Project Manager",
  "tirumanager@gmail.com": "Project Manager",
  "pm@sarathi.ai": "Project Manager",
  "executive@sarathi.ai": "Executive",
  "pmo@sarathi.ai": "PMO",
  "scrum@sarathi.ai": "Scrum Master",
  "sarathiadmin@gmail.com": "IT Manager",
  "itadmin@sarathi.ai": "IT Manager",
};

const ROLE_DEFAULT_SCREEN: Record<Role, string> = {
  Administrator: "/portfolio",
  "Project Manager": "/my-projects",
  Executive: "/executive",
  PMO: "/pmo",
  "Scrum Master": "/scrum",
  "IT Manager": "/application-health",
};

const ROLE_LABEL: Record<Role, string> = {
  Administrator: "Administrator",
  "Project Manager": "Project Manager",
  Executive: "Executive",
  PMO: "PMO",
  "Scrum Master": "Scrum Master",
  "IT Manager": "IT Manager",
};

const getDisplayName = (email: string): string => {
  const prefix = email.split("@")[0];
  return prefix
    .replace(/(admin|manager)$/i, "")
    .replace(/^(.)/, (c) => c.toUpperCase());
};

// ──────────────────────────────────────────────
// COLORS
// ──────────────────────────────────────────────
const C = {
  blue: "#2563EB",
  green: "#16A34A",
  amber: "#D97706",
  red: "#DC2626",
  purple: "#7C3AED",
  indigo: "#4F46E5",
  teal: "#0D9488",
  pink: "#DB2777",
};

// ──────────────────────────────────────────────
// SAMPLE DATA
// ──────────────────────────────────────────────
const PROJECTS = [
  { id: 1, name: "RetailX-Commerce", code: "RXC", health: 87, status: "on-track", team: 12, sprint: 14, completion: 76, velocity: 48, org: "Contoso Retail", budget: "₹2.4Cr", risk: 22 },
  { id: 2, name: "FinBank Mobile", code: "FBM", health: 62, status: "at-risk", team: 8, sprint: 9, completion: 54, velocity: 32, org: "FinBank Corp", budget: "₹1.8Cr", risk: 68 },
  { id: 3, name: "SmartHR Portal", code: "SHP", health: 91, status: "on-track", team: 15, sprint: 22, completion: 88, velocity: 56, org: "SmartHR Inc", budget: "₹3.1Cr", risk: 14 },
  { id: 4, name: "AI Support Desk", code: "ASD", health: 45, status: "critical", team: 6, sprint: 5, completion: 38, velocity: 24, org: "Contoso Corp", budget: "₹0.9Cr", risk: 85 },
  { id: 5, name: "Customer Analytics", code: "CAN", health: 78, status: "on-track", team: 10, sprint: 11, completion: 71, velocity: 40, org: "Analytics Co", budget: "₹1.5Cr", risk: 31 },
];

interface ProjectRecord {
  id: number;
  name: string;
  code: string;
  health: number;
  status: string;
  team: number;
  sprint: number;
  completion: number;
  velocity: number;
  org: string;
  budget: string;
  risk: number;
  projectManager: string;
  businessUnit: string;
  defectDensity: number;
  backlogHealth: number;
  releaseStatus: string;
  overallStatus: string;
  lastSync: string;
  aiHealth: string;
  assignedManagerEmail: string;
  startDate: string;
  targetDate: string;
}

interface SyncJobRecord {
  org: string;
  project: string;
  lastSync: string;
  nextSync: string;
  status: "success" | "failed" | "warning" | "pending" | "running";
  items: number;
  duration: string;
  errors: number;
}

const PROJECT_CATALOG: ProjectRecord[] = PROJECTS.map((p, i) => ({
  ...p,
  projectManager: ["Pradeep Rao", "Hemanth Kumar", "Deepthi Nair", "Sripriya Menon", "Tiru Sharma"][i],
  businessUnit: ["Retail", "Banking", "HR", "Support", "Analytics"][i],
  defectDensity: [0.6, 1.8, 0.4, 2.6, 0.9][i],
  backlogHealth: [82, 58, 91, 42, 74][i],
  releaseStatus: ["Ready", "Delayed", "Ready", "Blocked", "In Progress"][i],
  overallStatus: p.status === "critical" ? "Critical" : p.status === "at-risk" ? "At Risk" : "Healthy",
  lastSync: ["10 min ago", "Failed", "1 hr ago", "3 hr ago", "30 min ago"][i],
  aiHealth: p.risk > 75 ? "Critical" : p.risk > 50 ? "High Risk" : "Stable",
  assignedManagerEmail: ["pradeepmanager@gmail.com", "hemanthmanager@gmail.com", "deepthimanager@gmail.com", "sripriyamanager@gmail.com", "tirumanager@gmail.com"][i],
  startDate: ["2024-04-01", "2024-03-18", "2024-02-05", "2024-05-06", "2024-04-22"][i],
  targetDate: ["2024-08-15", "2024-09-10", "2024-07-28", "2024-10-02", "2024-08-30"][i],
}));

const toProjectCode = (name: string, fallbackId: number): string => {
  const initials = name
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 3)
    .map((part) => part[0]?.toUpperCase() ?? "")
    .join("");

  return initials || `P${fallbackId}`;
};

const toUiStatus = (riskLevel: string | null | undefined, riskScore: number): string => {
  const normalizedRisk = (riskLevel ?? "").toLowerCase();

  if (normalizedRisk === "critical" || riskScore >= 75) {
    return "critical";
  }

  if (normalizedRisk === "high" || normalizedRisk === "at risk" || riskScore >= 50) {
    return "at-risk";
  }

  return "on-track";
};

const mapAdminProjectsToCatalog = (items: AdminProjectStatisticItemDto[]): ProjectRecord[] =>
  items.map((project, index) => {
    const riskScore = Math.max(0, Math.min(100, project.riskScore ?? 0));
    const completion = Math.max(0, Math.min(100, project.completionRate ?? 0));
    const velocity = Math.max(0, project.sprintVelocity ?? 0);

    return {
      id: project.projectId,
      name: project.projectName,
      code: toProjectCode(project.projectName, project.projectId),
      health: Math.max(0, 100 - Math.round(riskScore)),
      status: toUiStatus(project.riskLevel, riskScore),
      team: 0,
      sprint: index + 1,
      completion,
      velocity,
      org: project.visibility,
      budget: "N/A",
      risk: Math.round(riskScore),
      projectManager: "Assigned PM",
      businessUnit: project.visibility,
      defectDensity: project.defectDensity ?? 0,
      backlogHealth: Math.max(0, 100 - Math.round(riskScore * 0.6)),
      releaseStatus: "In Progress",
      overallStatus: project.riskLevel ?? "Healthy",
      lastSync: new Date(project.lastUpdated).toLocaleString(),
      aiHealth: project.riskLevel ?? "Stable",
      assignedManagerEmail: "",
      startDate: "",
      targetDate: "",
    };
  });

const mapPmProjectsToCatalog = (items: ProjectManagerAssignedProjectDto[], managerEmail: string): ProjectRecord[] =>
  items.map((project, index) => {
    const completion = Math.max(0, Math.min(100, project.completionRate ?? 0));
    const velocity = Math.max(0, project.sprintVelocity ?? 0);
    const riskScore = project.deliveryHealth.toLowerCase() === "critical"
      ? 85
      : project.deliveryHealth.toLowerCase() === "at-risk" || project.deliveryHealth.toLowerCase() === "at risk"
        ? 65
        : 30;

    return {
      id: project.projectId,
      name: project.projectName,
      code: toProjectCode(project.projectName, project.projectId),
      health: Math.max(0, 100 - riskScore),
      status: toUiStatus(project.deliveryHealth, riskScore),
      team: 0,
      sprint: index + 1,
      completion,
      velocity,
      org: project.visibility,
      budget: "N/A",
      risk: riskScore,
      projectManager: project.projectRole,
      businessUnit: project.visibility,
      defectDensity: 0,
      backlogHealth: Math.max(0, 100 - Math.round(riskScore * 0.7)),
      releaseStatus: project.deliveryHealth,
      overallStatus: project.deliveryHealth,
      lastSync: new Date(project.lastUpdated).toLocaleString(),
      aiHealth: project.deliveryHealth,
      assignedManagerEmail: managerEmail.toLowerCase(),
      startDate: "",
      targetDate: "",
    };
  });

const mapItSyncJobs = (items: ItAdminSyncJobDto[]): SyncJobRecord[] =>
  items.map((job) => ({
    org: job.source,
    project: job.scopeName,
    lastSync: new Date(job.startedAtUtc).toLocaleString(),
    nextSync: job.status.toLowerCase() === "running" ? "In progress" : "Scheduled",
    status: normalizeMonitorStatus(job.status),
    items: job.itemsProcessed,
    duration: job.durationSeconds !== null ? `${job.durationSeconds.toFixed(1)}s` : "-",
    errors: job.itemsFailed,
  }));

const formatDurationSeconds = (seconds: number | null | undefined): string => {
  if (seconds === null || seconds === undefined || Number.isNaN(seconds)) {
    return "-";
  }

  return `${seconds.toFixed(1)}s`;
};

const formatRelativeTime = (value: string | null | undefined): string => {
  if (!value) {
    return "-";
  }

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return "-";
  }

  const minutes = Math.max(0, Math.round((Date.now() - date.getTime()) / 60000));
  if (minutes < 1) {
    return "just now";
  }
  if (minutes < 60) {
    return `${minutes} min ago`;
  }

  const hours = Math.round(minutes / 60);
  if (hours < 24) {
    return `${hours} hr ago`;
  }

  const days = Math.round(hours / 24);
  return `${days} day${days === 1 ? "" : "s"} ago`;
};

const normalizeMonitorStatus = (status: string): SyncJobRecord["status"] => {
  const normalized = status.trim().toLowerCase();
  if (normalized === "failed") {
    return "failed";
  }
  if (normalized === "warning" || normalized === "at risk") {
    return "warning";
  }
  if (normalized === "running" || normalized === "in progress") {
    return "running";
  }
  return "success";
};

const mapLiveSummaryToJobs = (summary: AzureDevOpsLiveSummaryDto | null): SyncJobRecord[] => {
  if (!summary) {
    return [];
  }

  return summary.projects.map((project) => ({
    org: project.organizationName,
    project: project.projectName,
    lastSync: formatRelativeTime(project.lastSyncUtc ?? project.latestActivityUtc),
    nextSync: summary.isConfigured ? "Live" : "Unavailable",
    status: normalizeMonitorStatus(project.status),
    items: project.items,
    duration: formatDurationSeconds(project.durationSeconds),
    errors: project.errors,
  }));
};

const mapTimelineFromJobs = (jobs: SyncJobRecord[]) =>
  jobs.slice(0, 5).map((job) => ({
    time: job.lastSync,
    project: job.project,
    status: job.status,
    items: job.items,
    message: "",
  }));

const parsePercentValue = (value: string): number => {
  const parsed = Number.parseFloat(value.replace(/[^0-9.-]/g, ""));
  return Number.isFinite(parsed) ? parsed : 0;
};

const DAY_MONTH_FORMATTER = new Intl.DateTimeFormat("en-GB", {
  day: "2-digit",
  month: "short",
});

const formatDayMonth = (value: string | Date | null | undefined): string => {
  if (!value) {
    return "";
  }

  const date = value instanceof Date ? value : new Date(value);
  if (Number.isNaN(date.getTime())) {
    return "";
  }

  return DAY_MONTH_FORMATTER.format(date).replace(",", "");
};

const isSuccessfulBuild = (result: string | null | undefined) =>
  (result ?? "").trim().toLowerCase() === "succeeded";

const getBuildSuccessRate = (builds: AdminProjectDetailsDto["builds"] | undefined): number => {
  const completedBuilds = (builds ?? []).filter((build) => build.result && build.result.trim().length > 0);
  if (completedBuilds.length === 0) {
    return 0;
  }

  const successfulBuilds = completedBuilds.filter((build) => isSuccessfulBuild(build.result)).length;
  return (successfulBuilds / completedBuilds.length) * 100;
};

const findSection = (overview: ReportsOverviewDto | null, key: string): ReportSectionDto | null => {
  if (!overview) {
    return null;
  }

  return overview.sections.find((section) => section.key === key) ?? null;
};

const getProjectById = (projectId?: string) =>
  PROJECT_CATALOG.find(p => String(p.id) === String(projectId)) ?? null;

const getAssignedProjects = (email: string) => {
  const assigned = PROJECT_CATALOG.filter(p => p.assignedManagerEmail === email.toLowerCase());
  return assigned.length ? assigned : [];
};

const SPRINT_TREND = [
  { sprint: "S10", planned: 45, completed: 42, velocity: 42 },
  { sprint: "S11", planned: 50, completed: 44, velocity: 44 },
  { sprint: "S12", planned: 48, completed: 48, velocity: 48 },
  { sprint: "S13", planned: 52, completed: 46, velocity: 46 },
  { sprint: "S14", planned: 55, completed: 50, velocity: 50 },
];

const BURNDOWN = [
  { day: "D1", ideal: 80, actual: 80, scope: 82 },
  { day: "D2", ideal: 72, actual: 76, scope: 82 },
  { day: "D3", ideal: 64, actual: 69, scope: 80 },
  { day: "D4", ideal: 56, actual: 62, scope: 80 },
  { day: "D5", ideal: 48, actual: 54, scope: 80 },
  { day: "D6", ideal: 40, actual: 45, scope: 78 },
  { day: "D7", ideal: 32, actual: 37, scope: 78 },
  { day: "D8", ideal: 24, actual: 28, scope: 78 },
  { day: "D9", ideal: 16, actual: 19, scope: 78 },
  { day: "D10", ideal: 8, actual: 10, scope: 78 },
];

const VELOCITY_DATA = [
  { project: "RetailX", velocity: 48, target: 50 },
  { project: "FinBank", velocity: 32, target: 45 },
  { project: "SmartHR", velocity: 56, target: 52 },
  { project: "AI Desk", velocity: 24, target: 40 },
  { project: "Analytics", velocity: 40, target: 42 },
];

const KPI_TREND = [
  { month: "Jan", sprintRate: 82, defect: 1.2, buildSuccess: 88, velocity: 38 },
  { month: "Feb", sprintRate: 85, defect: 1.0, buildSuccess: 91, velocity: 42 },
  { month: "Mar", sprintRate: 80, defect: 1.4, buildSuccess: 87, velocity: 40 },
  { month: "Apr", sprintRate: 88, defect: 0.9, buildSuccess: 93, velocity: 45 },
  { month: "May", sprintRate: 87, defect: 0.8, buildSuccess: 94, velocity: 48 },
  { month: "Jun", sprintRate: 91, defect: 0.7, buildSuccess: 96, velocity: 50 },
];

const RISK_RADAR = [
  { subject: "Schedule", A: 72, fullMark: 100 },
  { subject: "Quality", A: 45, fullMark: 100 },
  { subject: "Scope", A: 60, fullMark: 100 },
  { subject: "Technical", A: 55, fullMark: 100 },
  { subject: "Dependency", A: 38, fullMark: 100 },
  { subject: "Resource", A: 48, fullMark: 100 },
];

const RISK_TREND = [
  { month: "Jan", schedule: 65, quality: 50, scope: 55, technical: 60 },
  { month: "Feb", schedule: 68, quality: 52, scope: 58, technical: 62 },
  { month: "Mar", schedule: 70, quality: 48, scope: 60, technical: 58 },
  { month: "Apr", schedule: 74, quality: 44, scope: 62, technical: 56 },
  { month: "May", schedule: 72, quality: 45, scope: 60, technical: 55 },
  { month: "Jun", schedule: 72, quality: 45, scope: 60, technical: 55 },
];

const HEALTH_DONUT = [
  { name: "On Track", value: 3, color: C.green },
  { name: "At Risk", value: 1, color: C.amber },
  { name: "Critical", value: 1, color: C.red },
];

const PIPELINE_DATA = [
  { project: "RetailX-Commerce", branch: "main", status: "success", build: "#312", duration: "3m 42s", triggered: "2h ago" },
  { project: "FinBank Mobile", branch: "release/v2.4", status: "failed", build: "#147", duration: "1m 15s", triggered: "4h ago" },
  { project: "SmartHR Portal", branch: "main", status: "success", build: "#589", duration: "4m 08s", triggered: "30m ago" },
  { project: "AI Support Desk", branch: "feature/chat-v2", status: "running", build: "#89", duration: "2m 10s", triggered: "5m ago" },
  { project: "Customer Analytics", branch: "main", status: "success", build: "#203", duration: "5m 21s", triggered: "1h ago" },
];

const ACTIVITY_FEED = [
  { user: "SJ", name: "Sarah Johnson", action: "closed Sprint 14 for RetailX-Commerce", time: "5m ago", type: "sprint" },
  { user: "MC", name: "Michael Chen", action: "raised a critical bug in FinBank Mobile", time: "12m ago", type: "bug" },
  { user: "PP", name: "Priya Patel", action: "merged PR #142 — SmartHR Portal auth module", time: "28m ago", type: "pr" },
  { user: "AI", name: "Sarathi AI", action: "detected scope creep risk in AI Support Desk", time: "1h ago", type: "ai" },
  { user: "JW", name: "James Wilson", action: "exported Executive Report for Q2 2024", time: "2h ago", type: "report" },
  { user: "LR", name: "Lisa Rodriguez", action: "triggered manual sync for Customer Analytics", time: "2h ago", type: "sync" },
];

const TEAM_MEMBERS = [
  { id: 1, name: "Sarah Johnson", role: "Tech Lead", avatar: "SJ", project: "RetailX-Commerce", util: 95, tasks: 8, bugs: 2 },
  { id: 2, name: "Michael Chen", role: "Senior Dev", avatar: "MC", project: "FinBank Mobile", util: 78, tasks: 5, bugs: 4 },
  { id: 3, name: "Priya Patel", role: "Business Analyst", avatar: "PP", project: "SmartHR Portal", util: 88, tasks: 12, bugs: 1 },
  { id: 4, name: "James Wilson", role: "QA Lead", avatar: "JW", project: "AI Support Desk", util: 65, tasks: 4, bugs: 7 },
  { id: 5, name: "Lisa Rodriguez", role: "DevOps Engineer", avatar: "LR", project: "Customer Analytics", util: 82, tasks: 6, bugs: 0 },
  { id: 6, name: "Raj Kumar", role: "Senior Dev", avatar: "RK", project: "RetailX-Commerce", util: 91, tasks: 9, bugs: 1 },
];

const AUDIT_LOGS = [
  { time: "2024-06-10 14:32", user: "sarah.j@contoso.com", action: "Sync Triggered", resource: "RetailX-Commerce", status: "success", ip: "10.0.0.45" },
  { time: "2024-06-10 14:15", user: "admin@sarathi.ai", action: "User Invited", resource: "m.chen@finbank.com", status: "success", ip: "10.0.0.1" },
  { time: "2024-06-10 13:58", user: "system", action: "Pipeline Failed", resource: "FinBank Mobile / Build #147", status: "failed", ip: "-" },
  { time: "2024-06-10 13:30", user: "j.wilson@contoso.com", action: "Report Exported", resource: "Executive Summary - May 2024", status: "success", ip: "192.168.1.12" },
  { time: "2024-06-10 12:45", user: "system", action: "AI Analysis Complete", resource: "Risk Assessment - Q2", status: "success", ip: "-" },
  { time: "2024-06-10 11:20", user: "p.patel@smarthr.com", action: "Sprint Closed", resource: "SmartHR Portal / Sprint 22", status: "success", ip: "10.0.1.88" },
  { time: "2024-06-10 10:05", user: "system", action: "Sync Failed", resource: "AI Support Desk", status: "failed", ip: "-" },
  { time: "2024-06-10 09:30", user: "admin@sarathi.ai", action: "Role Updated", resource: "l.rodriguez@analytics.co", status: "success", ip: "10.0.0.1" },
  { time: "2024-06-10 09:00", user: "itadmin@sarathi.ai", action: "PAT Token Refreshed", resource: "Azure DevOps - Analytics Co", status: "success", ip: "10.0.0.2" },
];

const NOTIFICATIONS = [
  { id: 1, type: "critical", title: "Pipeline Failure", message: "FinBank Mobile build #147 failed at QA stage with 3 test failures", time: "5 min ago", read: false },
  { id: 2, type: "warning", title: "Sprint Delay Risk", message: "AI Support Desk Sprint 5 is 38% behind schedule with 3 days remaining", time: "23 min ago", read: false },
  { id: 3, type: "info", title: "AI Analysis Ready", message: "Q2 Risk Assessment report has been generated and is ready for review", time: "1 hr ago", read: false },
  { id: 4, type: "success", title: "Sync Completed", message: "RetailX-Commerce synchronized successfully — 142 items updated", time: "2 hr ago", read: true },
  { id: 5, type: "warning", title: "Critical Risk Detected", message: "Scope creep detected in AI Support Desk — estimated +12 story points", time: "3 hr ago", read: true },
  { id: 6, type: "info", title: "Approval Required", message: "Sprint 15 plan for RetailX-Commerce requires your approval before start", time: "5 hr ago", read: true },
  { id: 7, type: "success", title: "Sprint Completed", message: "SmartHR Portal Sprint 22 closed with 88% completion rate", time: "6 hr ago", read: true },
];

const SYNC_JOBS: SyncJobRecord[] = [
  { org: "Contoso Retail", project: "RetailX-Commerce", lastSync: "10 min ago", nextSync: "50 min", status: "success", items: 142, duration: "2.3s", errors: 0 },
  { org: "FinBank Corp", project: "FinBank Mobile", lastSync: "Failed", nextSync: "Manual", status: "failed", items: 0, duration: "-", errors: 3 },
  { org: "SmartHR Inc", project: "SmartHR Portal", lastSync: "1 hr ago", nextSync: "5 min", status: "success", items: 289, duration: "4.1s", errors: 0 },
  { org: "Contoso Corp", project: "AI Support Desk", lastSync: "3 hr ago", nextSync: "Pending", status: "warning", items: 47, duration: "1.8s", errors: 1 },
  { org: "Analytics Co", project: "Customer Analytics", lastSync: "30 min ago", nextSync: "30 min", status: "success", items: 196, duration: "3.2s", errors: 0 },
];

const USERS = [
  { id: 1, name: "Admin User", email: "admin@sarathi.ai", role: "Org Admin", status: "active", lastLogin: "Today 14:30", projects: 5 },
  { id: 2, name: "Sarah Johnson", email: "sarah.j@contoso.com", role: "Project Manager", status: "active", lastLogin: "Today 13:15", projects: 2 },
  { id: 3, name: "Michael Chen", email: "m.chen@finbank.com", role: "Project Manager", status: "active", lastLogin: "Yesterday", projects: 1 },
  { id: 4, name: "Priya Patel", email: "p.patel@smarthr.com", role: "Project Manager", status: "active", lastLogin: "Today 10:20", projects: 1 },
  { id: 5, name: "IT Admin", email: "itadmin@sarathi.ai", role: "IT Admin", status: "active", lastLogin: "Today 09:00", projects: 5 },
  { id: 6, name: "James Wilson", email: "j.wilson@contoso.com", role: "Project Manager", status: "inactive", lastLogin: "3 days ago", projects: 1 },
];

const SPRINT_BOARD = {
  todo: [
    { id: 1, title: "Implement payment gateway v3", points: 8, assignee: "RK", priority: "high" },
    { id: 2, title: "Refactor product catalog API", points: 5, assignee: "SJ", priority: "medium" },
    { id: 3, title: "Setup Redis cache layer", points: 3, assignee: "LR", priority: "low" },
  ],
  inProgress: [
    { id: 4, title: "User authentication with Entra ID", points: 13, assignee: "MC", priority: "critical" },
    { id: 5, title: "Sprint dashboard UI components", points: 8, assignee: "PP", priority: "high" },
  ],
  review: [
    { id: 6, title: "Checkout flow optimization", points: 5, assignee: "SJ", priority: "high" },
    { id: 7, title: "Mobile responsive fixes", points: 3, assignee: "JW", priority: "medium" },
  ],
  done: [
    { id: 8, title: "Database schema migration", points: 8, assignee: "RK", priority: "high" },
    { id: 9, title: "Email notification service", points: 5, assignee: "LR", priority: "medium" },
    { id: 10, title: "API rate limiting middleware", points: 3, assignee: "MC", priority: "low" },
  ],
};

const AI_CHAT = [
  { role: "assistant", msg: "Hello! I'm Sarathi AI. I can analyze your portfolio, predict delivery risks, generate insights, and answer questions about your projects. How can I help you today?" },
];

// ──────────────────────────────────────────────
// SHARED COMPONENTS
// ──────────────────────────────────────────────
const StatusBadge = ({ status }: { status: string }) => {
  const cfg: Record<string, { bg: string; text: string; dot: string; label: string }> = {
    "on-track": { bg: "bg-green-50", text: "text-green-700", dot: "bg-green-500", label: "On Track" },
    "at-risk": { bg: "bg-amber-50", text: "text-amber-700", dot: "bg-amber-500", label: "At Risk" },
    critical: { bg: "bg-red-50", text: "text-red-700", dot: "bg-red-500", label: "Critical" },
    success: { bg: "bg-green-50", text: "text-green-700", dot: "bg-green-500", label: "Success" },
    failed: { bg: "bg-red-50", text: "text-red-700", dot: "bg-red-500", label: "Failed" },
    warning: { bg: "bg-amber-50", text: "text-amber-700", dot: "bg-amber-500", label: "Warning" },
    running: { bg: "bg-blue-50", text: "text-blue-700", dot: "bg-blue-500", label: "Running" },
    active: { bg: "bg-blue-50", text: "text-blue-700", dot: "bg-blue-500", label: "Active" },
    inactive: { bg: "bg-gray-100", text: "text-gray-500", dot: "bg-gray-400", label: "Inactive" },
  };
  const c = cfg[status] || cfg.active;
  return (
    <span className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-medium ${c.bg} ${c.text}`}>
      <span className={`w-1.5 h-1.5 rounded-full ${c.dot}`} />
      {c.label}
    </span>
  );
};

const KPICard = ({ title, value, change, changeType, icon: Icon, color, subtitle }: {
  title: string; value: string | number; change?: string; changeType?: "up" | "down" | "stable";
  icon: any; color: string; subtitle?: string;
}) => (
  <motion.div
    whileHover={{ y: -3, boxShadow: "0 12px 32px rgba(0,0,0,0.1)" }}
    transition={{ duration: 0.2 }}
    className="bg-white rounded-xl p-5 shadow-sm border border-gray-100 cursor-default"
  >
    <div className="flex items-start justify-between mb-4">
      <div className={`p-2.5 rounded-xl ${color}`}>
        <Icon size={18} className="text-white" />
      </div>
      {change && (
        <span className={`inline-flex items-center gap-0.5 text-xs font-semibold px-2 py-1 rounded-lg ${
          changeType === "up" ? "text-green-700 bg-green-50" :
          changeType === "down" ? "text-red-700 bg-red-50" :
          "text-gray-500 bg-gray-50"
        }`}>
          {changeType === "up" ? <TrendingUp size={10} /> : changeType === "down" ? <TrendingDown size={10} /> : <Minus size={10} />}
          {change}
        </span>
      )}
    </div>
    <div className="text-2xl font-bold text-gray-900 mb-0.5">{value}</div>
    <div className="text-sm font-medium text-gray-600">{title}</div>
    {subtitle && <div className="text-xs text-gray-400 mt-0.5">{subtitle}</div>}
  </motion.div>
);

const ProgressBar = ({ value, color = "bg-blue-500", height = "h-2" }: { value: number; color?: string; height?: string }) => (
  <div className={`w-full bg-gray-100 rounded-full ${height} overflow-hidden`}>
    <motion.div
      initial={{ width: 0 }}
      animate={{ width: `${Math.min(value, 100)}%` }}
      transition={{ duration: 0.9, ease: "easeOut" }}
      className={`${height} ${color} rounded-full`}
    />
  </div>
);

const Card = ({ children, className = "", padding = "p-5" }: { children: React.ReactNode; className?: string; padding?: string }) => (
  <div className={`bg-white rounded-xl shadow-sm border border-gray-100 ${padding} ${className}`}>
    {children}
  </div>
);

const SectionHeader = ({ title, subtitle, actions }: { title: string; subtitle?: string; actions?: React.ReactNode }) => (
  <div className="flex items-start justify-between mb-5">
    <div>
      <h2 className="text-lg font-semibold text-gray-900">{title}</h2>
      {subtitle && <p className="text-sm text-gray-500 mt-0.5">{subtitle}</p>}
    </div>
    {actions && <div className="flex items-center gap-2 flex-shrink-0 ml-4">{actions}</div>}
  </div>
);

const Avatar = ({ initials, size = "sm", colorIdx }: { initials: string; size?: "xs" | "sm" | "md" | "lg"; colorIdx?: number }) => {
  const colors = ["bg-blue-500", "bg-purple-500", "bg-green-600", "bg-amber-500", "bg-red-500", "bg-indigo-500", "bg-teal-500"];
  const bg = colors[(colorIdx ?? initials.charCodeAt(0)) % colors.length];
  const sizes = { xs: "w-6 h-6 text-xs", sm: "w-8 h-8 text-xs", md: "w-9 h-9 text-sm", lg: "w-11 h-11 text-base" };
  return (
    <div className={`${bg} ${sizes[size]} rounded-full flex items-center justify-center text-white font-bold flex-shrink-0`}>
      {initials}
    </div>
  );
};

const AIInsightCard = ({ title, insight, type = "info", action }: { title: string; insight: string; type?: "info" | "warning" | "success" | "critical"; action?: string }) => {
  const cfg = {
    info: { border: "border-blue-100 bg-blue-50/60", icon: Info, iconColor: "text-blue-500" },
    warning: { border: "border-amber-100 bg-amber-50/60", icon: AlertTriangle, iconColor: "text-amber-500" },
    success: { border: "border-green-100 bg-green-50/60", icon: CheckCircle, iconColor: "text-green-500" },
    critical: { border: "border-red-100 bg-red-50/60", icon: AlertCircle, iconColor: "text-red-500" },
  };
  const c = cfg[type];
  return (
    <div className={`border rounded-xl p-4 ${c.border}`}>
      <div className="flex gap-3">
        <c.icon size={15} className={`${c.iconColor} mt-0.5 flex-shrink-0`} />
        <div className="flex-1 min-w-0">
          <div className="flex items-center gap-1.5 mb-1">
            <Sparkles size={11} className="text-purple-500" />
            <span className="text-xs font-bold text-purple-600 uppercase tracking-wider">AI Insight</span>
          </div>
          <p className="text-xs font-semibold text-gray-800 mb-1">{title}</p>
          <p className="text-xs text-gray-600 leading-relaxed">{insight}</p>
          {action && (
            <button className="mt-2 text-xs font-semibold text-blue-600 hover:text-blue-700 flex items-center gap-1 transition-colors">
              {action} <ArrowRight size={10} />
            </button>
          )}
        </div>
      </div>
    </div>
  );
};

const Tooltip_ = ({ active, payload, label }: any) => {
  if (!active || !payload?.length) return null;
  return (
    <div className="bg-white border border-gray-200 rounded-xl shadow-xl p-3 min-w-[140px]">
      <p className="text-xs font-semibold text-gray-500 mb-2">{label}</p>
      {payload.map((p: any, i: number) => (
        <div key={i} className="flex items-center justify-between gap-3 text-xs mb-1">
          <div className="flex items-center gap-1.5">
            <div className="w-2 h-2 rounded-full" style={{ backgroundColor: p.color || p.fill }} />
            <span className="text-gray-500 truncate">{p.name}</span>
          </div>
          <span className="font-bold text-gray-900">{p.value}</span>
        </div>
      ))}
    </div>
  );
};

const Btn = ({ children, variant = "primary", size = "sm", onClick, className = "", icon: Icon }: any) => {
  const variants = {
    primary: "bg-blue-600 text-white hover:bg-blue-700 shadow-sm",
    secondary: "bg-white text-gray-700 border border-gray-200 hover:bg-gray-50",
    ghost: "text-gray-600 hover:bg-gray-100",
    danger: "bg-red-50 text-red-600 border border-red-100 hover:bg-red-100",
  };
  const sizes = { xs: "px-2.5 py-1.5 text-xs", sm: "px-3 py-2 text-xs", md: "px-4 py-2.5 text-sm" };
  return (
    <button
      onClick={onClick}
      className={`inline-flex items-center gap-1.5 font-medium rounded-lg transition-all ${variants[variant as keyof typeof variants]} ${sizes[size as keyof typeof sizes]} ${className}`}
    >
      {Icon && <Icon size={size === "md" ? 15 : 13} />}
      {children}
    </button>
  );
};

const HealthBar = ({ value }: { value: number }) => {
  const color = value >= 80 ? "bg-green-500" : value >= 60 ? "bg-amber-500" : "bg-red-500";
  return (
    <div className="flex items-center gap-2">
      <div className="flex-1 h-1.5 bg-gray-100 rounded-full overflow-hidden">
        <div className={`h-1.5 rounded-full ${color}`} style={{ width: `${value}%` }} />
      </div>
      <span className={`text-xs font-bold ${value >= 80 ? "text-green-600" : value >= 60 ? "text-amber-600" : "text-red-600"}`}>{value}%</span>
    </div>
  );
};

const KpiCard = KPICard;

const MetricCard = ({ label, value, icon: Icon, tone = "blue" }: { label: string; value: string | number; icon: any; tone?: "blue" | "green" | "amber" | "red" | "purple" }) => {
  const tones = { blue: "bg-blue-50 text-blue-600", green: "bg-green-50 text-green-600", amber: "bg-amber-50 text-amber-600", red: "bg-red-50 text-red-600", purple: "bg-purple-50 text-purple-600" };
  return (
    <div className="bg-white rounded-xl p-4 shadow-sm border border-gray-100">
      <div className="flex items-center justify-between mb-2">
        <div className={`w-8 h-8 rounded-lg flex items-center justify-center ${tones[tone]}`}>
          <Icon size={15} />
        </div>
        <span className="text-lg font-bold text-gray-900">{value}</span>
      </div>
      <div className="text-xs text-gray-500 font-medium">{label}</div>
    </div>
  );
};

const RiskCard = ({ project }: { project: ProjectRecord }) => (
  <Card>
    <div className="flex items-start justify-between mb-4">
      <div>
        <div className="text-sm font-bold text-gray-900">{project.name}</div>
        <div className="text-xs text-gray-400">{project.businessUnit} · {project.projectManager}</div>
      </div>
      <StatusBadge status={project.risk > 75 ? "critical" : project.risk > 50 ? "warning" : "success"} />
    </div>
    <HealthBar value={100 - project.risk} />
    <p className="text-xs text-gray-500 mt-3">AI predicts {project.risk > 60 ? "delivery delay risk from blockers and low velocity." : "stable delivery with normal governance watch."}</p>
  </Card>
);

const HealthCard = ({ title, value, icon: Icon }: { title: string; value: number; icon: any }) => (
  <Card>
    <div className="flex items-center gap-3 mb-3">
      <div className="w-9 h-9 bg-blue-50 rounded-xl flex items-center justify-center">
        <Icon size={16} className="text-blue-600" />
      </div>
      <div>
        <div className="text-xs text-gray-400">{title}</div>
        <div className="text-sm font-bold text-gray-900">{value}%</div>
      </div>
    </div>
    <ProgressBar value={value} color={value >= 80 ? "bg-green-500" : value >= 60 ? "bg-amber-500" : "bg-red-500"} />
  </Card>
);

const SyncStatusCard = ({ job }: { job: SyncJobRecord }) => (
  <div className="p-3 bg-gray-50 rounded-xl flex items-center justify-between">
    <div>
      <div className="text-xs font-semibold text-gray-800">{job.project}</div>
      <div className="text-[10px] text-gray-400">{job.org} · Last sync: {job.lastSync}</div>
    </div>
    <StatusBadge status={job.status} />
  </div>
);

const ProjectCard = ({ project, onOpen }: { project: ProjectRecord; onOpen: (id: number) => void }) => (
  <motion.div whileHover={{ y: -4 }} transition={{ duration: 0.2 }}
    onClick={() => onOpen(project.id)}
    className="bg-white rounded-xl shadow-sm border border-gray-100 p-5 cursor-pointer hover:border-blue-200 transition-all">
    <div className="flex items-center justify-between mb-3">
      <div className="w-9 h-9 bg-gradient-to-br from-blue-500 to-indigo-600 rounded-xl flex items-center justify-center">
        <span className="text-white text-xs font-bold">{project.code}</span>
      </div>
      <StatusBadge status={project.status} />
    </div>
    <h3 className="text-sm font-bold text-gray-900 mb-0.5 leading-tight">{project.name}</h3>
    <p className="text-xs text-gray-400 mb-4">{project.businessUnit} · {project.projectManager}</p>
    <HealthBar value={project.health} />
    <div className="grid grid-cols-2 gap-2 text-xs mt-4">
      <div className="bg-gray-50 rounded-lg p-2"><div className="text-gray-400">Sprint</div><div className="font-bold text-gray-900">#{project.sprint}</div></div>
      <div className="bg-gray-50 rounded-lg p-2"><div className="text-gray-400">Risk</div><div className="font-bold text-gray-900">{project.risk}</div></div>
      <div className="bg-gray-50 rounded-lg p-2"><div className="text-gray-400">Velocity</div><div className="font-bold text-gray-900">{project.velocity}</div></div>
      <div className="bg-gray-50 rounded-lg p-2"><div className="text-gray-400">Release</div><div className="font-bold text-gray-900 truncate">{project.releaseStatus}</div></div>
    </div>
  </motion.div>
);

const ProjectTable = ({ projects, onOpen, onExport }: { projects: ProjectRecord[]; onOpen: (id: number) => void; onExport: () => void }) => {
  const [query, setQuery] = useState("");
  const [status, setStatus] = useState("all");
  const [risk, setRisk] = useState("all");
  const [unit, setUnit] = useState("all");
  const [sortKey, setSortKey] = useState<keyof ProjectRecord>("name");
  const units = Array.from(new Set(projects.map(p => p.businessUnit)));

  const filtered = useMemo(() => {
    const rows = projects
      .filter(p => `${p.name} ${p.projectManager} ${p.businessUnit}`.toLowerCase().includes(query.toLowerCase()))
      .filter(p => status === "all" || p.status === status)
      .filter(p => unit === "all" || p.businessUnit === unit)
      .filter(p => risk === "all" || (risk === "high" ? p.risk >= 60 : p.risk < 60))
      .sort((a, b) => String(a[sortKey]).localeCompare(String(b[sortKey]), undefined, { numeric: true }));
    return rows;
  }, [projects, query, status, risk, unit, sortKey]);

  return (
    <Card padding="p-0">
      <div className="p-5 border-b border-gray-100">
        <SectionHeader title="Projects" subtitle={`${filtered.length} projects matching current filters`}
          actions={<Btn variant="primary" icon={Download} onClick={onExport}>Export</Btn>} />
        <div className="grid grid-cols-1 md:grid-cols-6 gap-2">
          <div className="md:col-span-2 flex items-center gap-2 border border-gray-200 rounded-lg px-3 py-2 bg-gray-50">
            <Search size={13} className="text-gray-400" />
            <input value={query} onChange={e => setQuery(e.target.value)} placeholder="Search projects..."
              className="flex-1 text-xs outline-none bg-transparent" />
          </div>
          <select value={status} onChange={e => setStatus(e.target.value)} className="border border-gray-200 rounded-lg px-3 py-2 text-xs bg-white">
            <option value="all">All statuses</option><option value="on-track">On Track</option><option value="at-risk">At Risk</option><option value="critical">Critical</option>
          </select>
          <select value={risk} onChange={e => setRisk(e.target.value)} className="border border-gray-200 rounded-lg px-3 py-2 text-xs bg-white">
            <option value="all">All risks</option><option value="high">High risk</option><option value="low">Low risk</option>
          </select>
          <select value={unit} onChange={e => setUnit(e.target.value)} className="border border-gray-200 rounded-lg px-3 py-2 text-xs bg-white">
            <option value="all">All units</option>{units.map(u => <option key={u}>{u}</option>)}
          </select>
          <input type="date" className="border border-gray-200 rounded-lg px-3 py-2 text-xs bg-white" />
        </div>
      </div>
      <div className="overflow-x-auto max-h-[600px] overflow-y-auto">
        <table className="w-full text-xs">
          <thead className="sticky top-0 bg-white z-10"><tr className="border-b border-gray-50">
            {[
              ["name", "Project Name"], ["projectManager", "Project Manager"], ["businessUnit", "Business Unit"],
              ["sprint", "Current Sprint"], ["health", "Delivery Health"], ["risk", "Risk Score"],
              ["completion", "Sprint Completion"], ["velocity", "Velocity"], ["defectDensity", "Defect Density"],
              ["backlogHealth", "Backlog Health"], ["releaseStatus", "Release Status"], ["overallStatus", "Overall Status"],
              ["lastSync", "Last Sync Time"], ["aiHealth", "AI Health Indicator"],
            ].map(([key, label]) => (
              <th key={key} onClick={() => setSortKey(key as keyof ProjectRecord)}
                className="text-left px-4 py-3 text-gray-400 font-semibold whitespace-nowrap cursor-pointer hover:text-gray-700">{label}</th>
            ))}
          </tr></thead>
          <tbody>
            {filtered.map(p => (
              <tr key={p.id} onClick={() => onOpen(p.id)} className="border-b border-gray-50 hover:bg-gray-50 cursor-pointer">
                <td className="px-4 py-3 font-semibold text-blue-600 whitespace-nowrap">{p.name}</td>
                <td className="px-4 py-3 text-gray-600 whitespace-nowrap">{p.projectManager}</td>
                <td className="px-4 py-3 text-gray-500">{p.businessUnit}</td>
                <td className="px-4 py-3 text-gray-700">#{p.sprint}</td>
                <td className="px-4 py-3"><HealthBar value={p.health} /></td>
                <td className="px-4 py-3 font-bold text-gray-900">{p.risk}</td>
                <td className="px-4 py-3">{p.completion}%</td>
                <td className="px-4 py-3">{p.velocity}</td>
                <td className="px-4 py-3">{p.defectDensity}</td>
                <td className="px-4 py-3">{p.backlogHealth}%</td>
                <td className="px-4 py-3 whitespace-nowrap">{p.releaseStatus}</td>
                <td className="px-4 py-3"><StatusBadge status={p.status} /></td>
                <td className="px-4 py-3 text-gray-400 whitespace-nowrap">{p.lastSync}</td>
                <td className="px-4 py-3 font-semibold text-gray-700 whitespace-nowrap">{p.aiHealth}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      <div className="px-5 py-3 border-t border-gray-100">
        <span className="text-xs text-gray-400">Showing {filtered.length} of {projects.length} projects</span>
      </div>
    </Card>
  );
};

// ──────────────────────────────────────────────
// SCREEN: LOGIN
// ──────────────────────────────────────────────
const LoginScreen = ({ onLogin }: { onLogin: (email: string, role: Role) => void }) => {
  const [email, setEmail] = useState("");
  const [step, setStep] = useState<"idle" | "loading" | "success">("idle");
  const [error, setError] = useState("");

  const trimmed = email.trim().toLowerCase();
  const detectedRole = USER_ROLES[trimmed];

  const handleSignIn = () => {
    if (!email.includes("@")) { setError("Enter a valid work email."); return; }
    if (!detectedRole) { setError("This email is not registered in Sarathi AI. Contact your administrator."); return; }
    setError(""); setStep("loading");
    setTimeout(() => { setStep("success"); setTimeout(() => onLogin(trimmed, detectedRole), 900); }, 2200);
  };

  return (
    <div className="min-h-screen flex" style={{ fontFamily: "Inter, sans-serif" }}>
      {/* Left panel */}
      <div className="hidden lg:flex lg:w-1/2 bg-gradient-to-br from-blue-700 via-blue-600 to-indigo-700 flex-col justify-between p-12 relative overflow-hidden">
        <div className="absolute inset-0">
          <div className="absolute top-[-10%] left-[-10%] w-[60%] h-[60%] rounded-full bg-white/5 blur-3xl" />
          <div className="absolute bottom-[-5%] right-[-5%] w-[50%] h-[50%] rounded-full bg-indigo-400/10 blur-3xl" />
          <div className="absolute top-[40%] left-[30%] w-[40%] h-[40%] rounded-full bg-blue-400/10 blur-2xl" />
        </div>
        <div className="relative z-10">
          <div className="mb-16">
            <img src={sarathiLogo} alt="Sarathi AI" className="h-20 object-contain brightness-0 invert" />
          </div>
          <h1 className="text-4xl font-bold text-white leading-tight mb-4">
            Intelligent governance<br />for every sprint.
          </h1>
          <p className="text-blue-100 text-base leading-relaxed max-w-sm">
            AI-powered insights, real-time KPI monitoring, and predictive risk analysis—all integrated with your Azure DevOps ecosystem.
          </p>
        </div>
        <div className="relative z-10 grid grid-cols-3 gap-4">
          {[
            { label: "Projects Monitored", value: "5" },
            { label: "Sprints Tracked", value: "61" },
            { label: "AI Insights Daily", value: "340+" },
          ].map(stat => (
            <div key={stat.label} className="bg-white/10 backdrop-blur rounded-xl p-4">
              <div className="text-2xl font-bold text-white">{stat.value}</div>
              <div className="text-blue-200 text-xs mt-0.5">{stat.label}</div>
            </div>
          ))}
        </div>
      </div>

      {/* Right panel */}
      <div className="flex-1 flex items-center justify-center p-8 bg-gray-50">
        <motion.div
          initial={{ opacity: 0, y: 24 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.4 }}
          className="w-full max-w-md"
        >
          <div className="lg:hidden flex items-center gap-2 mb-8">
            <img src={sarathiLogo} alt="Sarathi AI" className="h-8 object-contain" />
          </div>

          <div className="bg-white rounded-2xl shadow-lg border border-gray-100 p-8">
            {step === "idle" ? (
              <>
                <div className="flex justify-center mb-5">
                  <img src={sarathiLogo} alt="Sarathi AI" className="h-12 object-contain" />
                </div>
                <div className="mb-7">
                  <h2 className="text-2xl font-bold text-gray-900 mb-1">Welcome back</h2>
                  <p className="text-gray-500 text-sm">Sign in with your registered Sarathi AI account</p>
                </div>

                <div className="space-y-4">
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1.5">Work email</label>
                    <input
                      type="email"
                      value={email}
                      onChange={e => { setEmail(e.target.value); setError(""); }}
                      onKeyDown={e => e.key === "Enter" && handleSignIn()}
                      placeholder="name@company.com"
                      className="w-full border border-gray-200 rounded-xl px-4 py-3 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-gray-50 transition-all"
                    />
                    {email && detectedRole && (
                      <p className="text-green-600 text-xs mt-1.5 flex items-center gap-1 font-medium">
                        <CheckCircle size={11} />
                        Recognized as {ROLE_LABEL[detectedRole]}
                      </p>
                    )}
                    {error && <p className="text-red-500 text-xs mt-1.5 flex items-center gap-1"><AlertCircle size={11} />{error}</p>}
                  </div>

                  <motion.button
                    whileTap={{ scale: 0.98 }}
                    onClick={handleSignIn}
                    className="w-full bg-blue-600 hover:bg-blue-700 text-white font-semibold py-3 rounded-xl text-sm transition-colors flex items-center justify-center gap-2 shadow-sm"
                  >
                    <Shield size={16} />
                    Sign In to Sarathi AI
                  </motion.button>
                </div>

                <div className="mt-6 pt-5 border-t border-gray-100 flex items-center justify-center gap-6 text-xs text-gray-400">
                  <span className="flex items-center gap-1"><Lock size={10} />Secured by Entra ID</span>
                  <span className="flex items-center gap-1"><Shield size={10} />SOC 2 Type II</span>
                  <span className="flex items-center gap-1"><Globe size={10} />ISO 27001</span>
                </div>
              </>
            ) : step === "loading" ? (
              <div className="text-center py-10">
                <div className="relative w-16 h-16 mx-auto mb-5">
                  <div className="w-16 h-16 rounded-full border-4 border-blue-100 border-t-blue-600 animate-spin" />
                  <div className="absolute inset-0 flex items-center justify-center">
                    <Shield size={20} className="text-blue-600" />
                  </div>
                </div>
                <p className="text-gray-800 font-semibold mb-1">Authenticating...</p>
                <p className="text-gray-400 text-sm">Verifying your Sarathi AI credentials</p>
                <div className="mt-4 space-y-1.5 text-left max-w-xs mx-auto">
                  {["Contacting identity provider", "Validating organization access", "Loading workspace"].map((s, i) => (
                    <motion.div
                      key={s}
                      initial={{ opacity: 0, x: -10 }}
                      animate={{ opacity: 1, x: 0 }}
                      transition={{ delay: i * 0.6 }}
                      className="flex items-center gap-2 text-xs text-gray-500"
                    >
                      <CheckCircle size={12} className="text-green-500" />
                      {s}
                    </motion.div>
                  ))}
                </div>
              </div>
            ) : (
              <div className="text-center py-10">
                <motion.div initial={{ scale: 0 }} animate={{ scale: 1 }} transition={{ type: "spring", bounce: 0.5 }}
                  className="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-4">
                  <CheckCircle size={32} className="text-green-600" />
                </motion.div>
                <p className="text-gray-900 font-semibold mb-1">Authentication successful</p>
                <p className="text-gray-400 text-sm">Redirecting to your dashboard...</p>
              </div>
            )}
          </div>

          <p className="text-center text-gray-400 text-xs mt-5">
            © 2025 Sarathi AI · Enterprise Delivery Governance Platform
          </p>
        </motion.div>
      </div>
    </div>
  );
};

// ──────────────────────────────────────────────
// SIDEBAR
// ──────────────────────────────────────────────
const NAV_GROUPS = [
  {
    label: "Dashboards",
    items: [
      { id: "/portfolio", label: "Portfolio", icon: FolderKanban, roles: ["Administrator", "Executive", "PMO"] as Role[] },
      { id: "/projects", label: "Projects", icon: LayoutDashboard, roles: ["Administrator"] as Role[] },
      { id: "/my-projects", label: "My Projects", icon: Briefcase, roles: ["Project Manager"] as Role[] },
      { id: "/executive", label: "Dashboard", icon: LayoutDashboard, roles: ["Executive"] as Role[] },
      { id: "/pmo", label: "Analytics", icon: BarChart2, roles: ["PMO"] as Role[] },
      { id: "/scrum", label: "Sprint Dashboard", icon: Target, roles: ["Scrum Master"] as Role[] },
      { id: "/application-health", label: "Application Health", icon: Server, roles: ["IT Manager"] as Role[] },
    ],
  },
  {
    label: "Projects",
    items: [
      { id: "/delivery-kpis", label: "Delivery KPIs", icon: BarChart3, roles: ["Administrator", "Project Manager"] as Role[] },
      { id: "/sprint-governance", label: "Sprint Governance", icon: Target, roles: ["Administrator", "Project Manager"] as Role[] },
      { id: "/risk-center", label: "Risk Center", icon: Shield, roles: ["Administrator"] as Role[] },
      { id: "/pm-risk-analysis", label: "Risk Analysis", icon: Shield, roles: ["Project Manager"] as Role[] },
      { id: "/ai-insights", label: "AI Insights", icon: Brain, roles: ["Project Manager"] as Role[] },
      { id: "/sprint-board", label: "Sprint Board", icon: List, roles: ["Scrum Master"] as Role[] },
      { id: "/velocity", label: "Velocity", icon: TrendingUp, roles: ["Scrum Master"] as Role[] },
      { id: "/burndown", label: "Burndown", icon: TrendingDown, roles: ["Scrum Master"] as Role[] },
    ],
  },
  {
    label: "Analytics",
    items: [
      { id: "/executive-reports", label: "Executive Reports", icon: FileText, roles: ["Administrator"] as Role[] },
      { id: "/reports", label: "Reports", icon: FileText, roles: ["Project Manager", "Executive", "PMO"] as Role[] },
      { id: "/ai-summary", label: "AI Summary", icon: Brain, roles: ["Executive"] as Role[] },
      { id: "/governance", label: "Governance", icon: CheckSquare, roles: ["PMO"] as Role[] },
    ],
  },
  {
    label: "Administration",
    items: [
      { id: "/azure-devops", label: "Azure DevOps", icon: GitBranch, roles: ["Administrator", "IT Manager"] as Role[] },
      { id: "/users", label: "Users", icon: Users, roles: ["IT Manager"] as Role[] },
      { id: "/audit-logs", label: "Audit Logs", icon: Terminal, roles: ["IT Manager"] as Role[] },
      { id: "/monitoring", label: "Monitoring", icon: Gauge, roles: ["IT Manager"] as Role[] },
    ],
  },
];

const ROUTE_ACCESS: Record<string, Role[]> = NAV_GROUPS
  .flatMap(group => group.items)
  .reduce((acc, item) => ({ ...acc, [item.id]: item.roles }), {
    "/notifications": ["Administrator", "Project Manager", "Executive", "PMO", "Scrum Master", "IT Manager"] as Role[],
    "/projects/:projectId": ["Administrator", "Project Manager"] as Role[],
    "/delivery-kpis/:projectId": ["Administrator"] as Role[],
  });

const normalizePath = (path: string) => {
  if (path.startsWith("/projects/")) return "/projects/:projectId";
  if (path.startsWith("/delivery-kpis/")) return "/delivery-kpis/:projectId";
  return path === "/" ? "/portfolio" : path;
};

const canAccessPath = (role: Role, path: string) => {
  const route = normalizePath(path);
  return ROUTE_ACCESS[route]?.includes(role) ?? false;
};

const getSafePathForRole = (role: Role, path: string) =>
  canAccessPath(role, path) ? path : ROLE_DEFAULT_SCREEN[role];

const Sidebar = ({ currentScreen, onNavigate, role, collapsed, onToggle }: {
  currentScreen: string; onNavigate: (s: string) => void; role: Role; collapsed: boolean; onToggle: () => void;
}) => {
  return (
    <motion.div
      animate={{ width: collapsed ? 64 : 240 }}
      transition={{ duration: 0.25, ease: "easeInOut" }}
      className="bg-slate-900 h-screen flex flex-col flex-shrink-0 overflow-hidden select-none"
      style={{ fontFamily: "Inter, sans-serif" }}
    >
      {/* Logo */}
     
{/* <div className="h-16 flex items-center px-3 border-b border-slate-800 flex-shrink-0">
  <img
    src={sarathiLogo}
    alt="Sarathi AI"
    className={`object-contain rounded-[20px] transition-all duration-300 ${
      collapsed ? "w-10 h-10" : "w-12 h-12"
    }`}
  />
</div> */}

      {/* Logo */}

{/* Logo */}
<div className="h-20 flex items-center pl-4 border-b border-slate-800 flex-shrink-0">
  <div className="bg-white p-2 rounded-[20px] shadow-md">
    <img
      src={sarathiLogo}
      alt="Sarathi AI"
      className={`object-contain rounded-[20px] ${
        collapsed ? "w-12 h-12" : "w-16 h-16"
      }`}
    />
  </div>
</div>


      {/* Nav */}
      <nav className="flex-1 overflow-y-auto py-3 px-2 space-y-4 scrollbar-hide">
        {NAV_GROUPS.map(group => {
          const visibleItems = group.items.filter(item => item.roles.includes(role));
          if (!visibleItems.length) return null;
          return (
            <div key={group.label}>
              {!collapsed && (
                <div className="px-3 mb-1 text-slate-500 text-[10px] font-bold uppercase tracking-widest">
                  {group.label}
                </div>
              )}
              <div className="space-y-0.5">
                {visibleItems.map(item => {
                  const active = currentScreen === item.id
                    || (item.id === "/projects" && currentScreen.startsWith("/projects/"))
                    || (item.id === "/delivery-kpis" && currentScreen.startsWith("/delivery-kpis/"));
                  return (
                    <motion.button
                      key={item.id}
                      whileTap={{ scale: 0.97 }}
                      onClick={() => onNavigate(item.id)}
                      title={collapsed ? item.label : undefined}
                      className={`w-full flex items-center gap-3 px-3 py-2.5 rounded-lg text-left transition-all duration-150 ${
                        active
                          ? "bg-blue-600 text-white shadow-sm"
                          : "text-slate-400 hover:bg-slate-800 hover:text-slate-100"
                      }`}
                    >
                      <item.icon size={16} className="flex-shrink-0" />
                      {!collapsed && <span className="text-xs font-medium truncate">{item.label}</span>}
                    </motion.button>
                  );
                })}
              </div>
            </div>
          );
        })}
      </nav>

      {/* Footer */}
      <div className="p-2 border-t border-slate-800 space-y-1">
        <button
          onClick={onToggle}
          className="w-full flex items-center justify-center p-2 text-slate-500 hover:text-slate-300 hover:bg-slate-800 rounded-lg transition-colors"
        >
          {collapsed ? <ChevronRight size={15} /> : <ChevronLeft size={15} />}
        </button>
      </div>
    </motion.div>
  );
};

// ──────────────────────────────────────────────
// TOP NAV
// ──────────────────────────────────────────────
const SCREEN_LABELS: Record<string, string> = {
  "/portfolio": "Portfolio", "/projects": "Projects", "/my-projects": "My Projects",
  "/executive": "Executive Dashboard", "/pmo": "PMO Dashboard", "/scrum": "Sprint Dashboard",
  "/application-health": "Application Health", "/delivery-kpis": "Delivery KPIs", "/delivery-kpis/:projectId": "Project Delivery KPIs",
  "/sprint-governance": "Sprint Governance", "/risk-center": "Risk Center", "/pm-risk-analysis": "Risk Analysis",
  "/ai-insights": "AI Insights", "/sprint-board": "Sprint Board", "/velocity": "Velocity",
  "/burndown": "Burndown", "/executive-reports": "Executive Reports", "/reports": "Reports",
  "/ai-summary": "AI Summary", "/governance": "Governance", "/azure-devops": "Azure DevOps",
  "/users": "Users", "/audit-logs": "Audit Logs", "/system-health": "System Health",
  "/monitoring": "Monitoring",
  "/notifications": "Notifications",
};

const ROLE_BADGE_COLOR: Record<Role, string> = {
  Administrator: "bg-blue-100 text-blue-700",
  "Project Manager": "bg-green-100 text-green-700",
  Executive: "bg-indigo-100 text-indigo-700",
  PMO: "bg-amber-100 text-amber-700",
  "Scrum Master": "bg-teal-100 text-teal-700",
  "IT Manager": "bg-purple-100 text-purple-700",
};

const TopNav = ({ currentScreen, role, email, unreadCount, onNavigate, onLogout }: {
  currentScreen: string; role: Role; email: string; unreadCount: number;
  onNavigate: (s: string) => void; onLogout: () => void;
}) => {
  const [profileOpen, setProfileOpen] = useState(false);
  const displayName = getDisplayName(email);

  return (
    <div className="h-14 bg-white border-b border-gray-100 flex items-center px-5 gap-3 flex-shrink-0" style={{ fontFamily: "Inter, sans-serif" }}>
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-1.5 text-sm">
          <span className="text-gray-400 text-xs">Sarathi AI</span>
          <ChevronRight size={12} className="text-gray-300" />
          <span className="text-gray-800 font-semibold text-xs truncate">{SCREEN_LABELS[currentScreen]}</span>
        </div>
        <div className="flex items-center gap-1.5 mt-0.5">
          <span className={`text-[10px] font-semibold px-1.5 py-0.5 rounded-full ${ROLE_BADGE_COLOR[role]}`}>
            {ROLE_LABEL[role]}
          </span>
        </div>
      </div>

      {role !== "IT Manager" && (
        <button onClick={() => onNavigate("/notifications")}
          className="relative p-2 text-gray-400 hover:text-gray-700 hover:bg-gray-50 rounded-lg transition-colors">
          <Bell size={17} />
          {unreadCount > 0 && (
            <motion.span initial={{ scale: 0 }} animate={{ scale: 1 }}
              className="absolute -top-0.5 -right-0.5 w-4 h-4 bg-red-500 text-white text-[9px] rounded-full flex items-center justify-center font-bold">
              {unreadCount}
            </motion.span>
          )}
        </button>
      )}

      <div className="relative">
        <button onClick={() => setProfileOpen(!profileOpen)}
          className="flex items-center gap-2 hover:bg-gray-50 rounded-lg px-2 py-1.5 transition-colors">
          <Avatar initials={displayName.slice(0, 2).toUpperCase()} size="sm" colorIdx={0} />
          <div className="hidden md:block text-left">
            <div className="text-xs font-semibold text-gray-900 leading-tight">{displayName}</div>
            <div className="text-[10px] text-gray-400">{email}</div>
          </div>
          <ChevronDown size={13} className="text-gray-400" />
        </button>
        {profileOpen && (
          <motion.div initial={{ opacity: 0, y: -8 }} animate={{ opacity: 1, y: 0 }}
            className="absolute right-0 top-full mt-1 w-56 bg-white border border-gray-100 rounded-xl shadow-2xl z-50 py-1.5 overflow-hidden">
            <div className="px-4 py-3 border-b border-gray-100">
              <div className="text-sm font-semibold text-gray-900">{displayName}</div>
              <div className="text-xs text-gray-400 mt-0.5">{email}</div>
              <div className={`text-[10px] font-semibold mt-1.5 px-1.5 py-0.5 rounded-full inline-block ${ROLE_BADGE_COLOR[role]}`}>
                {ROLE_LABEL[role]}
              </div>
            </div>
            <button onClick={() => { setProfileOpen(false); onLogout(); }}
              className="w-full text-left px-4 py-2.5 text-xs text-red-500 hover:bg-red-50 flex items-center gap-2 transition-colors">
              <LogOut size={13} />Sign Out
            </button>
          </motion.div>
        )}
      </div>
    </div>
  );
};

const findSummaryValue = (section: ReportSectionDto | null, labelFragment: string, fallback = "0"): number => {
  const value = section?.summaries.find((item) => item.label.toLowerCase().includes(labelFragment.toLowerCase()))?.value ?? fallback;
  return parsePercentValue(value);
};

const DeliveryKpiProjectsPage = ({
  projects = [],
  onOpenProject,
}: {
  projects?: ProjectRecord[];
  onOpenProject: (projectId: number) => void;
}) => {
  const [query, setQuery] = useState("");

  const filteredProjects = useMemo(
    () => projects.filter((project) => project.name.toLowerCase().includes(query.toLowerCase())),
    [projects, query],
  );

  return (
    <div className="space-y-6" style={{ fontFamily: "Inter, sans-serif" }}>
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-xl font-bold text-gray-900">Delivery KPIs</h2>
          <p className="text-sm text-gray-500">Select a synchronized project to open project-specific delivery KPI metrics.</p>
        </div>
      </div>

      <Card>
        <div className="flex flex-col gap-4">
          <div className="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-3">
            <SectionHeader title="Synchronized Projects" subtitle="Each card opens a dedicated KPI page for that project" />
            <div className="flex items-center gap-2 border border-gray-200 rounded-lg px-3 py-2 bg-gray-50 w-full lg:w-80">
              <Search size={13} className="text-gray-400 flex-shrink-0" />
              <input
                value={query}
                onChange={(event) => setQuery(event.target.value)}
                placeholder="Search projects..."
                className="flex-1 text-xs outline-none bg-transparent"
              />
            </div>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
            {filteredProjects.map((project) => (
              <motion.div
                key={project.id}
                whileHover={{ y: -3 }}
                onClick={() => onOpenProject(project.id)}
                className="bg-white rounded-xl shadow-sm border border-gray-100 p-5 cursor-pointer transition-all hover:border-blue-200"
              >
                <div className="flex items-center justify-between mb-3">
                  <div className="w-9 h-9 bg-gradient-to-br from-blue-500 to-indigo-600 rounded-xl flex items-center justify-center">
                    <span className="text-white text-xs font-bold">{project.code}</span>
                  </div>
                  <StatusBadge status={project.status} />
                </div>
                <h3 className="text-sm font-bold text-gray-900 mb-0.5">{project.name}</h3>
                <p className="text-xs text-gray-400 mb-4">{project.businessUnit} · {project.projectManager}</p>
                <HealthBar value={project.health} />
                <div className="grid grid-cols-2 gap-2 mt-4 text-xs">
                  <div className="bg-gray-50 rounded-lg p-2"><div className="text-gray-400">Completion</div><div className="font-bold text-gray-900">{project.completion}%</div></div>
                  <div className="bg-gray-50 rounded-lg p-2"><div className="text-gray-400">Velocity</div><div className="font-bold text-gray-900">{project.velocity}</div></div>
                </div>
              </motion.div>
            ))}
          </div>

          {filteredProjects.length === 0 && (
            <div className="text-sm text-gray-500">No synchronized projects match the current project name search.</div>
          )}
        </div>
      </Card>
    </div>
  );
};

// ──────────────────────────────────────────────
// SCREEN: ORG ADMIN DASHBOARD
// ──────────────────────────────────────────────
const OrgAdminDashboard = ({ onNavigate }: { onNavigate: (s: string) => void }) => {
  const kpis = [
    { title: "Portfolio Health", value: "87%", change: "+4%", changeType: "up" as const, icon: Award, color: "bg-blue-600", subtitle: "5 active projects" },
    { title: "Total Projects", value: "5", change: "2 critical", changeType: "down" as const, icon: FolderKanban, color: "bg-indigo-500", subtitle: "Across 4 organizations" },
    { title: "Active Sprints", value: "23", change: "+3 this week", changeType: "up" as const, icon: Target, color: "bg-purple-600", subtitle: "Across all projects" },
    { title: "Delivery Health", value: "76%", change: "-2%", changeType: "down" as const, icon: Activity, color: "bg-teal-600", subtitle: "vs 78% last sprint" },
    { title: "Sprint Completion", value: "87%", change: "+5%", changeType: "up" as const, icon: CheckCircle, color: "bg-green-600", subtitle: "Avg across sprints" },
    { title: "Defect Density", value: "0.8", change: "-12%", changeType: "up" as const, icon: Bug, color: "bg-amber-500", subtitle: "Per KLoC" },
    { title: "Build Success", value: "94%", change: "+2%", changeType: "up" as const, icon: Zap, color: "bg-orange-500", subtitle: "CI/CD pipelines" },
    { title: "Critical Risks", value: "3", change: "+1 new", changeType: "down" as const, icon: AlertTriangle, color: "bg-red-500", subtitle: "Require attention" },
  ];

  return (
    <div className="space-y-6" style={{ fontFamily: "Inter, sans-serif" }}>
      {/* KPI Row */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        {kpis.slice(0, 4).map(k => <KPICard key={k.title} {...k} />)}
      </div>
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        {kpis.slice(4).map(k => <KPICard key={k.title} {...k} />)}
      </div>

      {/* Charts Row */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-5">
        {/* Sprint Completion Trend */}
        <Card className="lg:col-span-2">
          <SectionHeader title="Sprint Completion Trend" subtitle="Planned vs completed story points"
            actions={<Btn variant="secondary" icon={Filter}>Filter</Btn>} />
          <ResponsiveContainer width="100%" height={220}>
            <ComposedChart data={SPRINT_TREND}>
              <CartesianGrid strokeDasharray="3 3" stroke="#F1F5F9" />
              <XAxis dataKey="sprint" tick={{ fontSize: 11, fill: "#94A3B8" }} axisLine={false} tickLine={false} />
              <YAxis tick={{ fontSize: 11, fill: "#94A3B8" }} axisLine={false} tickLine={false} />
              <Tooltip content={<Tooltip_ />} />
              <Legend wrapperStyle={{ fontSize: 11 }} />
              <Bar key="sc-planned" dataKey="planned" name="Planned" fill="#DBEAFE" radius={[4, 4, 0, 0]} />
              <Bar key="sc-completed" dataKey="completed" name="Completed" fill={C.blue} radius={[4, 4, 0, 0]} />
              <Line key="sc-velocity" type="monotone" dataKey="velocity" name="Velocity" stroke={C.purple} strokeWidth={2.5} dot={{ r: 4, fill: C.purple }} />
            </ComposedChart>
          </ResponsiveContainer>
        </Card>

        {/* Portfolio Health Donut */}
        <Card>
          <SectionHeader title="Portfolio Health" subtitle="Project status distribution" />
          <ResponsiveContainer width="100%" height={140}>
            <PieChart>
              <Pie data={HEALTH_DONUT} cx="50%" cy="50%" innerRadius={40} outerRadius={65} paddingAngle={3} dataKey="value">
                {HEALTH_DONUT.map((entry, i) => <Cell key={i} fill={entry.color} />)}
              </Pie>
              <Tooltip content={<Tooltip_ />} />
            </PieChart>
          </ResponsiveContainer>
          <div className="flex flex-col gap-2 mt-2">
            {HEALTH_DONUT.map(d => (
              <div key={d.name} className="flex items-center justify-between text-xs">
                <div className="flex items-center gap-2">
                  <div className="w-2.5 h-2.5 rounded-full" style={{ backgroundColor: d.color }} />
                  <span className="text-gray-600">{d.name}</span>
                </div>
                <span className="font-bold text-gray-900">{d.value} projects</span>
              </div>
            ))}
          </div>
        </Card>
      </div>

      {/* KPI Trend + Velocity */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-5">
        <Card>
          <SectionHeader title="KPI Trends" subtitle="6-month performance overview" />
          <ResponsiveContainer width="100%" height={200}>
            <LineChart data={KPI_TREND}>
              <CartesianGrid strokeDasharray="3 3" stroke="#F1F5F9" />
              <XAxis dataKey="month" tick={{ fontSize: 11, fill: "#94A3B8" }} axisLine={false} tickLine={false} />
              <YAxis tick={{ fontSize: 11, fill: "#94A3B8" }} axisLine={false} tickLine={false} />
              <Tooltip content={<Tooltip_ />} />
              <Legend wrapperStyle={{ fontSize: 11 }} />
              <Line key="kpi-sprintRate" type="monotone" dataKey="sprintRate" name="Sprint Rate %" stroke={C.blue} strokeWidth={2} dot={false} />
              <Line key="kpi-buildSuccess" type="monotone" dataKey="buildSuccess" name="Build Success %" stroke={C.green} strokeWidth={2} dot={false} />
              <Line key="kpi-velocity" type="monotone" dataKey="velocity" name="Velocity" stroke={C.purple} strokeWidth={2} dot={false} />
            </LineChart>
          </ResponsiveContainer>
        </Card>

        <Card>
          <SectionHeader title="Velocity Comparison" subtitle="Team velocity vs targets" />
          <ResponsiveContainer width="100%" height={200}>
            <BarChart data={VELOCITY_DATA} layout="vertical">
              <CartesianGrid strokeDasharray="3 3" stroke="#F1F5F9" horizontal={false} />
              <XAxis type="number" tick={{ fontSize: 11, fill: "#94A3B8" }} axisLine={false} tickLine={false} />
              <YAxis type="category" dataKey="project" tick={{ fontSize: 11, fill: "#64748B" }} axisLine={false} tickLine={false} width={55} />
              <Tooltip content={<Tooltip_ />} />
              <Legend wrapperStyle={{ fontSize: 11 }} />
              <Bar key="vel-target" dataKey="target" name="Target" fill="#F1F5F9" radius={[0, 4, 4, 0]} />
              <Bar key="vel-velocity" dataKey="velocity" name="Velocity" fill={C.blue} radius={[0, 4, 4, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </Card>
      </div>

      {/* Bottom Row */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-5">
        {/* Project Health */}
        <Card className="lg:col-span-1">
          <SectionHeader title="Project Health" subtitle="All projects at a glance"
            actions={<button onClick={() => onNavigate("portfolio")} className="text-xs text-blue-600 font-medium hover:text-blue-700 flex items-center gap-1">View all <ArrowRight size={12} /></button>} />
          <div className="space-y-3">
            {PROJECTS.map(p => (
              <div key={p.id} className="flex items-center gap-3 group cursor-pointer" onClick={() => onNavigate("project-details")}>
                <div className="w-8 h-8 bg-blue-50 rounded-lg flex items-center justify-center flex-shrink-0">
                  <span className="text-blue-700 text-xs font-bold">{p.code}</span>
                </div>
                <div className="flex-1 min-w-0">
                  <div className="flex items-center justify-between mb-1">
                    <span className="text-xs font-medium text-gray-800 truncate">{p.name}</span>
                    <StatusBadge status={p.status} />
                  </div>
                  <HealthBar value={p.health} />
                </div>
              </div>
            ))}
          </div>
        </Card>

        {/* AI Recommendations */}
        <Card className="lg:col-span-1">
          <SectionHeader title="AI Recommendations" subtitle="Sarathi AI insights"
            actions={<button onClick={() => onNavigate("ai-insights")} className="text-xs text-blue-600 font-medium hover:text-blue-700 flex items-center gap-1">Open AI <ArrowRight size={12} /></button>} />
          <div className="space-y-3">
            <AIInsightCard
              type="critical"
              title="FinBank Mobile at delivery risk"
              insight="Sprint velocity 29% below target. Escalate build #147 failure immediately — 3 test failures in payment module."
              action="View risk report"
            />
            <AIInsightCard
              type="warning"
              title="AI Support Desk scope creep detected"
              insight="12 unplanned story points added in last 2 sprints. Recommend scope freeze and stakeholder alignment."
              action="Review scope"
            />
            <AIInsightCard
              type="success"
              title="SmartHR Portal exceeding targets"
              insight="91% health score — best in portfolio. Team practices could serve as internal benchmark for others."
            />
          </div>
        </Card>

        {/* Activity Feed */}
        <Card className="lg:col-span-1">
          <SectionHeader title="Recent Activity" subtitle="Live team updates" />
          <div className="space-y-3">
            {ACTIVITY_FEED.map((item, i) => (
              <motion.div key={i} initial={{ opacity: 0, x: -10 }} animate={{ opacity: 1, x: 0 }} transition={{ delay: i * 0.05 }}
                className="flex gap-3 items-start">
                <Avatar initials={item.user} size="xs" colorIdx={i} />
                <div className="flex-1 min-w-0">
                  <p className="text-xs text-gray-700">
                    <span className="font-semibold">{item.name}</span>{" "}
                    <span className="text-gray-500">{item.action}</span>
                  </p>
                  <p className="text-[10px] text-gray-400 mt-0.5">{item.time}</p>
                </div>
              </motion.div>
            ))}
          </div>
        </Card>
      </div>

      {/* Pipeline Status */}
      <Card padding="p-0">
        <div className="p-5 border-b border-gray-100">
          <SectionHeader title="Pipeline Status" subtitle="CI/CD build health across all projects" />
        </div>
        <div className="overflow-x-auto">
          <table className="w-full text-xs">
            <thead>
              <tr className="border-b border-gray-50">
                {["Project", "Branch", "Build", "Status", "Duration", "Triggered"].map(h => (
                  <th key={h} className="text-left px-5 py-3 text-gray-400 font-semibold">{h}</th>
                ))}
              </tr>
            </thead>
            <tbody>
              {PIPELINE_DATA.map((row, i) => (
                <tr key={i} className="border-b border-gray-50 hover:bg-gray-50 transition-colors">
                  <td className="px-5 py-3 font-medium text-gray-900">{row.project}</td>
                  <td className="px-5 py-3 text-gray-500 font-mono">{row.branch}</td>
                  <td className="px-5 py-3 text-gray-500 font-mono">{row.build}</td>
                  <td className="px-5 py-3"><StatusBadge status={row.status} /></td>
                  <td className="px-5 py-3 text-gray-500">{row.duration}</td>
                  <td className="px-5 py-3 text-gray-400">{row.triggered}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </Card>
    </div>
  );
};

// ──────────────────────────────────────────────
// SCREEN: PM DASHBOARD
// ──────────────────────────────────────────────
const PMDashboard = () => {
  const [selectedProject, setSelectedProject] = useState(0);
  const proj = PROJECTS[selectedProject];

  return (
    <div className="space-y-6" style={{ fontFamily: "Inter, sans-serif" }}>
      {/* Project Selector */}
      <div className="flex items-center gap-4 flex-wrap">
        <div className="text-sm font-semibold text-gray-700">Active Project:</div>
        <div className="flex gap-2 flex-wrap">
          {PROJECTS.map((p, i) => (
            <motion.button key={p.id} whileTap={{ scale: 0.97 }} onClick={() => setSelectedProject(i)}
              className={`px-4 py-2 rounded-xl text-xs font-semibold transition-all ${
                selectedProject === i ? "bg-blue-600 text-white shadow-sm" : "bg-white border border-gray-200 text-gray-600 hover:border-blue-300"
              }`}>
              {p.name}
            </motion.button>
          ))}
        </div>
      </div>

      {/* KPIs */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <KPICard title="Sprint Progress" value={`${proj.completion}%`} change={"+6% vs last"} changeType="up" icon={Target} color="bg-blue-600" />
        <KPICard title="Sprint Velocity" value={proj.velocity} change={`Target: ${proj.velocity + 4}`} changeType="stable" icon={TrendingUp} color="bg-purple-600" />
        <KPICard title="Sprint No." value={proj.sprint} subtitle={`${proj.org}`} icon={Hash} color="bg-indigo-500" />
        <KPICard title="Risk Score" value={`${proj.risk}/100`} changeType={proj.risk > 60 ? "down" : proj.risk > 30 ? "stable" : "up"} change={proj.risk > 60 ? "High" : proj.risk > 30 ? "Medium" : "Low"} icon={Shield} color={proj.risk > 60 ? "bg-red-500" : proj.risk > 30 ? "bg-amber-500" : "bg-green-600"} />
      </div>

      {/* Charts Row */}
      <div className="grid grid-cols-1 lg:grid-cols-5 gap-5">
        {/* Burndown */}
        <Card className="lg:col-span-3">
          <SectionHeader title="Sprint Burndown" subtitle={`Sprint ${proj.sprint} — ${proj.name}`} />
          <ResponsiveContainer width="100%" height={220}>
            <LineChart data={BURNDOWN}>
              <CartesianGrid strokeDasharray="3 3" stroke="#F1F5F9" />
              <XAxis dataKey="day" tick={{ fontSize: 11, fill: "#94A3B8" }} axisLine={false} tickLine={false} />
              <YAxis tick={{ fontSize: 11, fill: "#94A3B8" }} axisLine={false} tickLine={false} />
              <Tooltip content={<Tooltip_ />} />
              <Legend wrapperStyle={{ fontSize: 11 }} />
              <Line key="pm-ideal" type="monotone" dataKey="ideal" name="Ideal" stroke="#CBD5E1" strokeWidth={2} strokeDasharray="6 3" dot={false} />
              <Line key="pm-actual" type="monotone" dataKey="actual" name="Actual" stroke={C.blue} strokeWidth={2.5} dot={{ r: 3, fill: C.blue }} />
              <Line key="pm-scope" type="monotone" dataKey="scope" name="Scope" stroke={C.amber} strokeWidth={1.5} strokeDasharray="3 3" dot={false} />
            </LineChart>
          </ResponsiveContainer>
        </Card>

        {/* Story Status */}
        <Card className="lg:col-span-2">
          <SectionHeader title="Work Item Status" subtitle="Current sprint breakdown" />
          <div className="space-y-4">
            {[
              { label: "Stories Completed", value: 18, total: 24, color: "bg-green-500" },
              { label: "In Progress", value: 4, total: 24, color: "bg-blue-500" },
              { label: "Blocked", value: 2, total: 24, color: "bg-red-500" },
              { label: "Backlog", value: 12, total: 36, color: "bg-gray-300" },
            ].map(item => (
              <div key={item.label}>
                <div className="flex justify-between text-xs mb-1.5">
                  <span className="text-gray-600 font-medium">{item.label}</span>
                  <span className="font-bold text-gray-900">{item.value} / {item.total}</span>
                </div>
                <ProgressBar value={(item.value / item.total) * 100} color={item.color} />
              </div>
            ))}
          </div>
          <div className="mt-5 pt-4 border-t border-gray-100 space-y-2">
            <div className="flex justify-between text-xs">
              <span className="text-gray-500">Open Bugs (P1/P2)</span>
              <span className="font-bold text-red-600">4</span>
            </div>
            <div className="flex justify-between text-xs">
              <span className="text-gray-500">PRs Awaiting Review</span>
              <span className="font-bold text-amber-600">7</span>
            </div>
            <div className="flex justify-between text-xs">
              <span className="text-gray-500">Completed Story Points</span>
              <span className="font-bold text-green-600">{proj.velocity}</span>
            </div>
          </div>
        </Card>
      </div>

      {/* Team + Actions */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-5">
        {/* Team Productivity */}
        <Card className="lg:col-span-2">
          <SectionHeader title="Team Productivity" subtitle="Utilization & task metrics" />
          <div className="space-y-3">
            {TEAM_MEMBERS.slice(0, 5).map((m, i) => (
              <div key={m.id} className="flex items-center gap-3">
                <Avatar initials={m.avatar} size="sm" colorIdx={i} />
                <div className="flex-1 min-w-0">
                  <div className="flex justify-between mb-1">
                    <div>
                      <span className="text-xs font-semibold text-gray-800">{m.name}</span>
                      <span className="text-xs text-gray-400 ml-1">· {m.role}</span>
                    </div>
                    <span className={`text-xs font-bold ${m.util > 90 ? "text-red-600" : m.util > 75 ? "text-amber-600" : "text-green-600"}`}>{m.util}%</span>
                  </div>
                  <ProgressBar value={m.util} color={m.util > 90 ? "bg-red-500" : m.util > 75 ? "bg-amber-500" : "bg-green-500"} />
                </div>
                <div className="flex gap-3 text-xs text-gray-400 flex-shrink-0">
                  <span className="flex items-center gap-0.5"><CheckSquare size={11} className="text-blue-400" />{m.tasks}</span>
                  <span className="flex items-center gap-0.5"><Bug size={11} className="text-red-400" />{m.bugs}</span>
                </div>
              </div>
            ))}
          </div>
        </Card>

        {/* Action Items + AI Summary */}
        <div className="space-y-4">
          <Card>
            <SectionHeader title="Action Items" />
            <div className="space-y-2">
              {[
                { text: "Resolve build #147 payment failures", priority: "P1", due: "Today" },
                { text: "Review Sprint 15 scope with stakeholders", priority: "P2", due: "Tomorrow" },
                { text: "Unblock Michael Chen on auth module", priority: "P1", due: "Today" },
                { text: "Update risk register for Q3 planning", priority: "P3", due: "Jun 14" },
              ].map((item, i) => (
                <div key={i} className="flex items-start gap-2 p-2.5 rounded-lg hover:bg-gray-50 transition-colors">
                  <span className={`mt-0.5 text-[10px] font-bold px-1.5 py-0.5 rounded ${
                    item.priority === "P1" ? "bg-red-100 text-red-700" : item.priority === "P2" ? "bg-amber-100 text-amber-700" : "bg-gray-100 text-gray-500"
                  }`}>{item.priority}</span>
                  <div className="flex-1">
                    <p className="text-xs text-gray-700">{item.text}</p>
                    <p className="text-[10px] text-gray-400">{item.due}</p>
                  </div>
                </div>
              ))}
            </div>
          </Card>

          <Card>
            <div className="flex items-center gap-2 mb-3">
              <Sparkles size={14} className="text-purple-500" />
              <span className="text-sm font-semibold text-gray-900">AI Project Summary</span>
            </div>
            <p className="text-xs text-gray-600 leading-relaxed">
              <span className="font-semibold text-gray-800">{proj.name}</span> is at{" "}
              <span className={`font-bold ${proj.health >= 80 ? "text-green-600" : proj.health >= 60 ? "text-amber-600" : "text-red-600"}`}>{proj.health}% health</span>.
              Current sprint velocity is {proj.completion < 70 ? "below" : "meeting"} targets. {proj.risk > 60 ? "High risk score requires immediate attention." : proj.risk > 30 ? "Moderate risks are being managed." : "Project is tracking well."}
            </p>
          </Card>
        </div>
      </div>
    </div>
  );
};

// ──────────────────────────────────────────────
// SCREEN: IT ADMIN DASHBOARD
// ──────────────────────────────────────────────
const ITAdminDashboard = ({ onNavigate }: { onNavigate: (s: string) => void }) => {
  const [syncing, setSyncing] = useState<number | null>(null);

  const triggerSync = (i: number) => {
    setSyncing(i);
    setTimeout(() => setSyncing(null), 2500);
  };

  return (
    <div className="space-y-6" style={{ fontFamily: "Inter, sans-serif" }}>
      {/* System Health KPIs */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <KPICard title="API Health" value="99.8%" change="Healthy" changeType="up" icon={Wifi} color="bg-green-600" subtitle="Azure DevOps REST API" />
        <KPICard title="Sync Success Rate" value="80%" change="1 failed" changeType="down" icon={RefreshCw} color="bg-blue-600" subtitle="Last 24 hours" />
        <KPICard title="DB Connections" value="24/50" change="Optimal" changeType="up" icon={Database} color="bg-indigo-500" subtitle="Active connections" />
        <KPICard title="Failed Sync Alerts" value="2" change="Action needed" changeType="down" icon={AlertTriangle} color="bg-red-500" subtitle="Requires manual fix" />
      </div>

      {/* Integration Status + System Health */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-5">
        <Card className="lg:col-span-2" padding="p-0">
          <div className="p-5 border-b border-gray-100 flex items-center justify-between">
            <div>
              <h3 className="text-sm font-semibold text-gray-900">Azure DevOps Integration Status</h3>
              <p className="text-xs text-gray-400">Live synchronization across all connected organizations</p>
            </div>
            <Btn variant="primary" icon={RefreshCw} onClick={() => {}}>Sync All</Btn>
          </div>
          <div className="overflow-x-auto">
            <table className="w-full text-xs">
              <thead>
                <tr className="border-b border-gray-50">
                  {["Organization", "Project", "Last Sync", "Next Sync", "Items", "Status", "Action"].map(h => (
                    <th key={h} className="text-left px-4 py-3 text-gray-400 font-semibold">{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {SYNC_JOBS.map((job, i) => (
                  <tr key={i} className="border-b border-gray-50 hover:bg-gray-50 transition-colors">
                    <td className="px-4 py-3 text-gray-500">{job.org}</td>
                    <td className="px-4 py-3 font-medium text-gray-900">{job.project}</td>
                    <td className={`px-4 py-3 ${job.status === "failed" ? "text-red-500 font-semibold" : "text-gray-500"}`}>{job.lastSync}</td>
                    <td className="px-4 py-3 text-gray-400">{job.nextSync}</td>
                    <td className="px-4 py-3 font-mono text-gray-700">{job.items || "-"}</td>
                    <td className="px-4 py-3"><StatusBadge status={job.status} /></td>
                    <td className="px-4 py-3">
                      <button onClick={() => triggerSync(i)}
                        className={`flex items-center gap-1 text-blue-600 hover:text-blue-700 font-medium transition-colors ${syncing === i ? "opacity-50 pointer-events-none" : ""}`}>
                        {syncing === i ? <Loader2 size={12} className="animate-spin" /> : <RefreshCw size={12} />}
                        {syncing === i ? "Syncing..." : "Sync"}
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </Card>

        {/* System Health */}
        <Card>
          <SectionHeader title="System Health" subtitle="Infrastructure overview" />
          <div className="space-y-4">
            {[
              { label: "API Gateway", value: 99, icon: Globe, color: "text-green-500" },
              { label: "Database (SQL)", value: 82, icon: Database, color: "text-blue-500" },
              { label: "Redis Cache", value: 76, icon: HardDrive, color: "text-purple-500" },
              { label: "Azure Functions", value: 94, icon: Zap, color: "text-amber-500" },
              { label: "AI Services", value: 88, icon: Brain, color: "text-indigo-500" },
            ].map(item => (
              <div key={item.label} className="flex items-center gap-3">
                <item.icon size={14} className={item.color} />
                <div className="flex-1 min-w-0">
                  <div className="flex justify-between mb-1">
                    <span className="text-xs text-gray-600 font-medium">{item.label}</span>
                    <span className={`text-xs font-bold ${item.value > 90 ? "text-green-600" : item.value > 75 ? "text-amber-600" : "text-red-600"}`}>{item.value}%</span>
                  </div>
                  <ProgressBar value={item.value} color={item.value > 90 ? "bg-green-500" : item.value > 75 ? "bg-amber-500" : "bg-red-500"} />
                </div>
              </div>
            ))}
          </div>
          <div className="mt-5 pt-4 border-t border-gray-100">
            <div className="text-xs text-gray-500 mb-2 font-semibold">Quick Actions</div>
            <div className="grid grid-cols-2 gap-2">
              <Btn variant="secondary" icon={RefreshCw} size="xs" onClick={() => onNavigate("sync")}>Sync Monitor</Btn>
              <Btn variant="secondary" icon={Terminal} size="xs" onClick={() => onNavigate("audit")}>Audit Logs</Btn>
              <Btn variant="secondary" icon={Users} size="xs" onClick={() => onNavigate("users")}>Users</Btn>
            </div>
          </div>
        </Card>
      </div>

      {/* Alerts + Audit Preview */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-5">
        <Card>
          <SectionHeader title="Failed Sync Alerts" subtitle="Requires immediate action" />
          <div className="space-y-3">
            {SYNC_JOBS.filter(j => j.status === "failed" || j.status === "warning").map((job, i) => (
              <div key={i} className={`p-4 rounded-xl border ${job.status === "failed" ? "bg-red-50 border-red-100" : "bg-amber-50 border-amber-100"}`}>
                <div className="flex items-start justify-between">
                  <div>
                    <div className="flex items-center gap-2 mb-1">
                      <AlertTriangle size={13} className={job.status === "failed" ? "text-red-500" : "text-amber-500"} />
                      <span className="text-xs font-bold text-gray-900">{job.project}</span>
                    </div>
                    <p className="text-xs text-gray-600">{job.org} · {job.errors} error{job.errors !== 1 ? "s" : ""} · Last sync: {job.lastSync}</p>
                  </div>
                  <Btn variant={job.status === "failed" ? "danger" : "secondary"} icon={RefreshCw} size="xs">Retry</Btn>
                </div>
              </div>
            ))}
          </div>
        </Card>

        <Card>
          <SectionHeader title="Recent Audit Events" subtitle="System activity log"
            actions={<button onClick={() => onNavigate("audit")} className="text-xs text-blue-600 font-semibold hover:text-blue-700 flex items-center gap-1">View all <ArrowRight size={11} /></button>} />
          <div className="space-y-2">
            {AUDIT_LOGS.slice(0, 5).map((log, i) => (
              <div key={i} className="flex items-start gap-3 p-2.5 rounded-lg hover:bg-gray-50 transition-colors">
                <div className={`w-1.5 h-1.5 rounded-full mt-1.5 flex-shrink-0 ${log.status === "success" ? "bg-green-500" : "bg-red-500"}`} />
                <div className="flex-1 min-w-0">
                  <p className="text-xs text-gray-700"><span className="font-semibold">{log.action}</span> · {log.resource}</p>
                  <p className="text-[10px] text-gray-400">{log.user} · {log.time}</p>
                </div>
              </div>
            ))}
          </div>
        </Card>
      </div>
    </div>
  );
};

// ──────────────────────────────────────────────
// SCREEN: PORTFOLIO DASHBOARD
// ──────────────────────────────────────────────
const PortfolioDashboard = ({ onNavigate }: { onNavigate: (s: string) => void }) => (
  <div className="space-y-6" style={{ fontFamily: "Inter, sans-serif" }}>
    <div className="flex items-center justify-between">
      <div>
        <h2 className="text-xl font-bold text-gray-900">Portfolio Overview</h2>
        <p className="text-sm text-gray-500">5 projects · 4 organizations · FY2024 Q2</p>
      </div>
      <div className="flex gap-2">
        <Btn variant="secondary" icon={Filter}>Filter</Btn>
        <Btn variant="primary" icon={Download}>Export</Btn>
      </div>
    </div>

    {/* Project Cards */}
    <div className="grid grid-cols-1 lg:grid-cols-5 gap-4">
      {PROJECTS.map((p, i) => (
        <motion.div key={p.id} whileHover={{ y: -4 }} transition={{ duration: 0.2 }}
          onClick={() => onNavigate("project-details")}
          className="bg-white rounded-xl shadow-sm border border-gray-100 p-5 cursor-pointer hover:border-blue-200 transition-all">
          <div className="flex items-center justify-between mb-3">
            <div className="w-9 h-9 bg-gradient-to-br from-blue-500 to-indigo-600 rounded-xl flex items-center justify-center">
              <span className="text-white text-xs font-bold">{p.code}</span>
            </div>
            <StatusBadge status={p.status} />
          </div>
          <h3 className="text-sm font-bold text-gray-900 mb-0.5 leading-tight">{p.name}</h3>
          <p className="text-xs text-gray-400 mb-4">{p.org}</p>
          <div className="mb-3">
            <div className="flex justify-between text-xs mb-1">
              <span className="text-gray-500">Health</span>
              <span className={`font-bold ${p.health >= 80 ? "text-green-600" : p.health >= 60 ? "text-amber-600" : "text-red-600"}`}>{p.health}%</span>
            </div>
            <ProgressBar value={p.health} color={p.health >= 80 ? "bg-green-500" : p.health >= 60 ? "bg-amber-500" : "bg-red-500"} />
          </div>
          <div className="grid grid-cols-2 gap-2 text-xs">
            <div className="bg-gray-50 rounded-lg p-2">
              <div className="text-gray-400">Sprint</div>
              <div className="font-bold text-gray-900">#{p.sprint}</div>
            </div>
            <div className="bg-gray-50 rounded-lg p-2">
              <div className="text-gray-400">Team</div>
              <div className="font-bold text-gray-900">{p.team} devs</div>
            </div>
            <div className="bg-gray-50 rounded-lg p-2">
              <div className="text-gray-400">Velocity</div>
              <div className="font-bold text-gray-900">{p.velocity} pts</div>
            </div>
            <div className="bg-gray-50 rounded-lg p-2">
              <div className="text-gray-400">Budget</div>
              <div className="font-bold text-gray-900">{p.budget}</div>
            </div>
          </div>
        </motion.div>
      ))}
    </div>

    {/* KPI Matrix + Trend */}
    <div className="grid grid-cols-1 lg:grid-cols-2 gap-5">
      <Card>
        <SectionHeader title="Health Matrix" subtitle="Cross-project KPI comparison" />
        <div className="overflow-x-auto">
          <table className="w-full text-xs">
            <thead>
              <tr className="border-b border-gray-100">
                {["Project", "Health", "Velocity", "Sprint %", "Defects", "Risk"].map(h => (
                  <th key={h} className="text-left py-2 pr-3 text-gray-400 font-semibold">{h}</th>
                ))}
              </tr>
            </thead>
            <tbody>
              {PROJECTS.map(p => (
                <tr key={p.id} className="border-b border-gray-50 hover:bg-gray-50">
                  <td className="py-3 pr-3 font-semibold text-gray-800">{p.code}</td>
                  <td className="py-3 pr-3">
                    <span className={`font-bold ${p.health >= 80 ? "text-green-600" : p.health >= 60 ? "text-amber-600" : "text-red-600"}`}>{p.health}%</span>
                  </td>
                  <td className="py-3 pr-3 text-gray-700">{p.velocity}</td>
                  <td className="py-3 pr-3 text-gray-700">{p.completion}%</td>
                  <td className="py-3 pr-3 text-gray-700">0.{Math.floor(Math.random() * 9 + 1)}</td>
                  <td className="py-3 pr-3"><StatusBadge status={p.risk > 60 ? "critical" : p.risk > 30 ? "warning" : "success"} /></td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </Card>

      <Card>
        <SectionHeader title="Delivery Forecast" subtitle="Predicted completion by Q3 end" />
        <ResponsiveContainer width="100%" height={200}>
          <BarChart data={PROJECTS.map(p => ({ name: p.code, forecast: p.completion, target: 85 }))}>
            <CartesianGrid strokeDasharray="3 3" stroke="#F1F5F9" />
            <XAxis dataKey="name" tick={{ fontSize: 11, fill: "#94A3B8" }} axisLine={false} tickLine={false} />
            <YAxis tick={{ fontSize: 11, fill: "#94A3B8" }} axisLine={false} tickLine={false} domain={[0, 100]} />
            <Tooltip content={<Tooltip_ />} />
            <Legend wrapperStyle={{ fontSize: 11 }} />
            <Bar key="pf-forecast" dataKey="forecast" name="Forecast %" fill={C.blue} radius={[4, 4, 0, 0]} />
            <Bar key="pf-target" dataKey="target" name="Target %" fill="#DBEAFE" radius={[4, 4, 0, 0]} />
          </BarChart>
        </ResponsiveContainer>
      </Card>
    </div>
  </div>
);

// ──────────────────────────────────────────────
// SCREEN: PROJECT DETAILS
// ──────────────────────────────────────────────
const ProjectDetails = ({ 
  projectDetails,
  azureOrgUrl,
  onSyncNow,
  isLoading,
  error,
}: { 
  projectDetails?: AdminProjectDetailsDto | null;
  azureOrgUrl?: string;
  onSyncNow?: () => Promise<void>;
  isLoading?: boolean;
  error?: string | null;
}) => {
  const [tab, setTab] = useState("overview");
  const [isSyncing, setIsSyncing] = useState(false);
  const tabs = ["overview", "epics", "sprints", "bugs", "team", "pipelines", "repos"];

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-96">
        <div className="text-center">
          <Loader2 size={48} className="text-blue-500 animate-spin mx-auto mb-4" />
          <p className="text-gray-500">Loading project details...</p>
        </div>
      </div>
    );
  }

  if (!projectDetails) {
    return (
      <div className="flex items-center justify-center h-96">
        <div className="text-center">
          <div className="text-xl font-semibold text-gray-900 mb-3">Project details unavailable</div>
          <p className="text-sm text-gray-500 mb-4">{error ?? 'No details were returned for this project.'}</p>
          {azureOrgUrl && (
            <Btn variant="secondary" icon={ExternalLink} onClick={() => window.open(azureOrgUrl, '_blank')}>Open Azure DevOps</Btn>
          )}
        </div>
      </div>
    );
  }

  const proj = projectDetails;
  const repositories = proj.repositories ?? [];
  const builds = proj.builds ?? [];
  const projectCode = proj.projectName.split(/\s+/).filter(Boolean).slice(0, 3).map(p => p[0]?.toUpperCase() ?? "").join("") || "PRJ";
  const riskStatus = proj.riskScore && proj.riskScore >= 75 ? "critical" : proj.riskScore && proj.riskScore >= 50 ? "at-risk" : "on-track";
  
  const epics = proj.workItems.filter(w => w.workItemType.toLowerCase() === "epic");
  const bugs = proj.workItems.filter(w => w.workItemType.toLowerCase() === "bug");
  const activeSprint = proj.sprints.find(s => s.status.toLowerCase() === "active" || s.status.toLowerCase() === "current") || proj.sprints[0];

  const handleSync = async () => {
    if (!onSyncNow) return;
    setIsSyncing(true);
    try {
      await onSyncNow();
    } finally {
      setIsSyncing(false);
    }
  };

  const handleAzureDevOps = () => {
    if (azureOrgUrl) {
      window.open(`${azureOrgUrl}/${proj.projectName}`, '_blank');
    }
  };

  return (
    <div className="space-y-5" style={{ fontFamily: "Inter, sans-serif" }}>
      {/* Header */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-5">
        <div className="flex items-start gap-4">
          <div className="w-12 h-12 bg-gradient-to-br from-blue-500 to-indigo-600 rounded-xl flex items-center justify-center">
            <span className="text-white font-bold">{projectCode}</span>
          </div>
          <div className="flex-1">
            <div className="flex items-center gap-3 mb-1">
              <h1 className="text-xl font-bold text-gray-900">{proj.projectName}</h1>
              <StatusBadge status={riskStatus} />
            </div>
            <div className="flex items-center gap-4 text-xs text-gray-400 flex-wrap">
              <span className="flex items-center gap-1"><Building2 size={11} />{proj.visibility}</span>
              <span className="flex items-center gap-1"><Users size={11} />{proj.teamMembers.length} members</span>
              <span className="flex items-center gap-1"><Target size={11} />{proj.activeSprints} active sprint{proj.activeSprints !== 1 ? 's' : ''}</span>
              <span className="flex items-center gap-1"><Award size={11} />Health: {proj.averageCompletionRate?.toFixed(0) ?? 0}%</span>
              <span className="flex items-center gap-1"><Clock size={11} />Last sync: {new Date(proj.lastUpdated).toLocaleString()}</span>
            </div>
          </div>
          <div className="flex gap-2">
            {azureOrgUrl && (
              <Btn variant="secondary" icon={ExternalLink} onClick={handleAzureDevOps}>Azure DevOps</Btn>
            )}
            {onSyncNow && (
              <Btn variant="primary" icon={RefreshCw} onClick={handleSync} disabled={isSyncing}>
                {isSyncing ? 'Syncing...' : 'Sync Now'}
              </Btn>
            )}
          </div>
        </div>
        {/* Tabs */}
        <div className="flex gap-1 mt-5 border-b border-gray-100 -mx-5 px-5">
          {tabs.map(t => (
            <button key={t} onClick={() => setTab(t)}
              className={`px-4 py-2 text-xs font-semibold capitalize border-b-2 transition-all -mb-px ${
                tab === t ? "border-blue-600 text-blue-600" : "border-transparent text-gray-400 hover:text-gray-700"
              }`}>{t}</button>
          ))}
        </div>
      </div>

      {/* Tab Content */}
      <motion.div key={tab} initial={{ opacity: 0, y: 6 }} animate={{ opacity: 1, y: 0 }} transition={{ duration: 0.2 }}>
        {tab === "overview" && (
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-5">
            <div className="lg:col-span-2 space-y-5">
              <div className="grid grid-cols-3 gap-4">
                <KPICard title="Completion" value={`${proj.averageCompletionRate?.toFixed(0) ?? 0}%`} icon={CheckCircle} color="bg-green-600" />
                <KPICard title="Velocity" value={proj.averageSprintVelocity?.toFixed(0) ?? "0"} icon={TrendingUp} color="bg-blue-600" />
                <KPICard title="Risk Score" value={`${proj.riskScore?.toFixed(0) ?? 0}/100`} icon={Shield} color={proj.riskScore && proj.riskScore > 60 ? "bg-red-500" : "bg-amber-500"} />
              </div>
              <Card>
                <SectionHeader title="Project Statistics" subtitle="Work items and sprint overview" />
                <div className="grid grid-cols-2 gap-4 mt-4">
                  <div>
                    <div className="text-2xl font-bold text-gray-900">{proj.totalWorkItems}</div>
                    <div className="text-xs text-gray-500">Total Work Items</div>
                  </div>
                  <div>
                    <div className="text-2xl font-bold text-green-600">{proj.completedWorkItems}</div>
                    <div className="text-xs text-gray-500">Completed</div>
                  </div>
                  <div>
                    <div className="text-2xl font-bold text-blue-600">{proj.totalSprints}</div>
                    <div className="text-xs text-gray-500">Total Sprints</div>
                  </div>
                  <div>
                    <div className="text-2xl font-bold text-purple-600">{proj.activeSprints}</div>
                    <div className="text-xs text-gray-500">Active Sprints</div>
                  </div>
                </div>
              </Card>
            </div>
            <div className="space-y-4">
              <Card>
                <div className="text-xs font-semibold text-gray-700 mb-3">Project Info</div>
                <div className="space-y-2 text-xs">
                  <div><span className="text-gray-500">Description:</span> <span className="text-gray-700">{proj.description || 'No description'}</span></div>
                  <div><span className="text-gray-500">Azure ID:</span> <span className="text-gray-700 font-mono">{proj.azureProjectId}</span></div>
                  <div><span className="text-gray-500">Created:</span> <span className="text-gray-700">{new Date(proj.createdDate).toLocaleDateString()}</span></div>
                  <div><span className="text-gray-500">Risk Level:</span> <span className={`font-semibold ${proj.riskLevel === 'High' || proj.riskLevel === 'Critical' ? 'text-red-600' : 'text-green-600'}`}>{proj.riskLevel || 'Low'}</span></div>
                </div>
              </Card>
            </div>
          </div>
        )}
        {tab === "epics" && (
          <Card padding="p-0">
            <div className="p-5 border-b border-gray-100">
              <SectionHeader title="Epics" subtitle={`${epics.length} epics in this project`} />
            </div>
            {epics.length > 0 ? (
              <table className="w-full text-xs">
                <thead><tr className="border-b border-gray-50">
                  {["ID", "Title", "State", "Assigned To", "Story Points", "Progress"].map(h => <th key={h} className="text-left px-5 py-3 text-gray-400 font-semibold">{h}</th>)}
                </tr></thead>
                <tbody>
                  {epics.map(epic => (
                    <tr key={epic.workItemId} className="border-b border-gray-50 hover:bg-gray-50">
                      <td className="px-5 py-3 font-mono text-blue-600">{epic.azureWorkItemId}</td>
                      <td className="px-5 py-3 font-medium text-gray-800">{epic.title}</td>
                      <td className="px-5 py-3"><StatusBadge status={epic.state.toLowerCase().includes('done') ? 'success' : epic.state.toLowerCase().includes('active') ? 'active' : 'warning'} /></td>
                      <td className="px-5 py-3 text-gray-600">{epic.assignedToName || 'Unassigned'}</td>
                      <td className="px-5 py-3 text-gray-700">{epic.storyPoints || 'N/A'}</td>
                      <td className="px-5 py-3">{epic.progressPercent}%</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            ) : (
              <div className="p-12 text-center text-gray-500">No epics found</div>
            )}
          </Card>
        )}
        {tab === "sprints" && (
          <Card padding="p-0">
            <div className="p-5 border-b border-gray-100">
              <SectionHeader title="Sprints" subtitle={`${proj.sprints.length} sprints tracked`} />
            </div>
            {proj.sprints.length > 0 ? (
              <table className="w-full text-xs">
                <thead><tr className="border-b border-gray-50">
                  {["Sprint", "Status", "Start Date", "End Date", "Story Points", "Completion", "Work Items"].map(h => <th key={h} className="text-left px-5 py-3 text-gray-400 font-semibold">{h}</th>)}
                </tr></thead>
                <tbody>
                  {proj.sprints.map(sprint => (
                    <tr key={sprint.sprintId} className="border-b border-gray-50 hover:bg-gray-50">
                      <td className="px-5 py-3 font-semibold text-gray-900">{sprint.sprintName}</td>
                      <td className="px-5 py-3"><StatusBadge status={sprint.status.toLowerCase() === 'active' || sprint.status.toLowerCase() === 'current' ? 'active' : sprint.status.toLowerCase() === 'completed' ? 'success' : 'warning'} /></td>
                      <td className="px-5 py-3 text-gray-600">{new Date(sprint.startDate).toLocaleDateString()}</td>
                      <td className="px-5 py-3 text-gray-600">{new Date(sprint.endDate).toLocaleDateString()}</td>
                      <td className="px-5 py-3 text-gray-700">{sprint.completedStoryPoints} / {sprint.plannedStoryPoints}</td>
                      <td className="px-5 py-3">{sprint.totalWorkItems > 0 ? Math.round((sprint.completedWorkItems / sprint.totalWorkItems) * 100) : 0}%</td>
                      <td className="px-5 py-3 text-gray-700">{sprint.completedWorkItems} / {sprint.totalWorkItems}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            ) : (
              <div className="p-12 text-center text-gray-500">No sprints found</div>
            )}
          </Card>
        )}
        {tab === "bugs" && (
          <Card padding="p-0">
            <div className="p-5 border-b border-gray-100">
              <SectionHeader title="Bugs" subtitle={`${bugs.length} bugs tracked`} />
            </div>
            {bugs.length > 0 ? (
              <table className="w-full text-xs">
                <thead><tr className="border-b border-gray-50">
                  {["ID", "Title", "Priority", "Assigned To", "State", "Sprint"].map(h => <th key={h} className="text-left px-5 py-3 text-gray-400 font-semibold">{h}</th>)}
                </tr></thead>
                <tbody>
                  {bugs.map(bug => (
                    <tr key={bug.workItemId} className="border-b border-gray-50 hover:bg-gray-50">
                      <td className="px-5 py-3 font-mono text-blue-600">{bug.azureWorkItemId}</td>
                      <td className="px-5 py-3 font-medium text-gray-800">{bug.title}</td>
                      <td className="px-5 py-3"><span className={`font-semibold ${bug.priority === '1' || bug.priority.toLowerCase() === 'critical' ? "text-red-600" : bug.priority === '2' || bug.priority.toLowerCase() === 'high' ? "text-amber-600" : "text-blue-600"}`}>{bug.priority}</span></td>
                      <td className="px-5 py-3 text-gray-600">{bug.assignedToName || 'Unassigned'}</td>
                      <td className="px-5 py-3"><StatusBadge status={bug.state.toLowerCase().includes('done') || bug.state.toLowerCase().includes('closed') ? 'success' : bug.state.toLowerCase().includes('active') || bug.state.toLowerCase().includes('progress') ? 'active' : 'warning'} /></td>
                      <td className="px-5 py-3 text-gray-500">{bug.sprintName || 'No sprint'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            ) : (
              <div className="p-12 text-center text-gray-500">No bugs found</div>
            )}
          </Card>
        )}
        {tab === "team" && (
          <Card padding="p-0">
            <div className="p-5 border-b border-gray-100">
              <SectionHeader title="Team Members" subtitle={`${proj.teamMembers.length} members assigned`} />
            </div>
            {proj.teamMembers.length > 0 ? (
              <table className="w-full text-xs">
                <thead><tr className="border-b border-gray-50">
                  {["Name", "Email", "Role", "Project Role", "Allocation"].map(h => <th key={h} className="text-left px-5 py-3 text-gray-400 font-semibold">{h}</th>)}
                </tr></thead>
                <tbody>
                  {proj.teamMembers.map((member, i) => (
                    <tr key={i} className="border-b border-gray-50 hover:bg-gray-50">
                      <td className="px-5 py-3 font-medium text-gray-900">{member.name}</td>
                      <td className="px-5 py-3 text-gray-600">{member.email}</td>
                      <td className="px-5 py-3 text-gray-700">{member.role}</td>
                      <td className="px-5 py-3 text-gray-700">{member.projectRole || 'Not Assigned'}</td>
                      <td className="px-5 py-3 text-gray-700">{member.allocationPercent}%</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            ) : (
              <div className="p-12 text-center text-gray-500">No team members assigned</div>
            )}
          </Card>
        )}
        {(tab === "pipelines" || tab === "repos") && (
          <Card>
            {tab === "repos" ? (
              repositories.length > 0 ? (
                <div className="overflow-x-auto"><table className="w-full text-xs"><thead><tr className="border-b border-gray-50"><th className="text-left px-5 py-3 text-gray-400">Repository</th><th className="text-left px-5 py-3 text-gray-400">Default branch</th><th className="text-left px-5 py-3 text-gray-400">Size</th></tr></thead><tbody>{repositories.map(repository => <tr key={repository.repositoryId} className="border-b border-gray-50"><td className="px-5 py-3 font-medium">{repository.repositoryName}</td><td className="px-5 py-3 text-gray-600">{repository.defaultBranch || "-"}</td><td className="px-5 py-3 text-gray-600">{repository.size ?? "-"}</td></tr>)}</tbody></table></div>
              ) : <div className="p-12 text-center text-gray-500">No repositories found</div>
            ) : (
              builds.length > 0 ? (
                <div className="overflow-x-auto"><table className="w-full text-xs"><thead><tr className="border-b border-gray-50"><th className="text-left px-5 py-3 text-gray-400">Pipeline</th><th className="text-left px-5 py-3 text-gray-400">Build</th><th className="text-left px-5 py-3 text-gray-400">Status</th><th className="text-left px-5 py-3 text-gray-400">Result</th></tr></thead><tbody>{builds.map(build => <tr key={build.buildId} className="border-b border-gray-50"><td className="px-5 py-3 font-medium">{build.definitionName}</td><td className="px-5 py-3 text-gray-600">{build.buildNumber}</td><td className="px-5 py-3 text-gray-600">{build.status}</td><td className="px-5 py-3 text-gray-600">{build.result || "-"}</td></tr>)}</tbody></table></div>
              ) : <div className="p-12 text-center text-gray-500">No pipeline builds found</div>
            )}
          </Card>
        )}
      </motion.div>
    </div>
  );
};

// ──────────────────────────────────────────────
// SCREEN: SPRINT GOVERNANCE
// ──────────────────────────────────────────────
const SprintGovernance = ({
  reportsOverview = null,
  pmSprintProgress = null,
  pmWorkItems = null,
  adminGovernance = null,
  projects = [],
  selectedProjectId = null,
  onProjectSelect,
}: {
  reportsOverview?: ReportsOverviewDto | null;
  pmSprintProgress?: ProjectManagerSprintProgressDto | null;
  pmWorkItems?: ProjectManagerWorkItemDto[] | null;
  adminGovernance?: ProjectSprintGovernanceDto | ProjectManagerSprintGovernanceDto | null;
  projects?: ProjectRecord[];
  selectedProjectId?: number | null;
  onProjectSelect?: (projectId: number) => void;
}) => {
  const sprintSection = findSection(reportsOverview, "sprints");

  const priorityColor = { critical: "bg-red-100 text-red-700", high: "bg-amber-100 text-amber-700", medium: "bg-blue-100 text-blue-700", low: "bg-gray-100 text-gray-500" };

  const getAssigneeInitials = (name: string): string => {
    const initials = name
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map((part) => part[0]?.toUpperCase() ?? "")
      .join("");

    return initials || "NA";
  };

  const mapPriority = (priority: string): "critical" | "high" | "medium" | "low" => {
    const normalized = priority.toLowerCase();

    if (normalized === "critical" || normalized === "p0" || normalized === "1") {
      return "critical";
    }

    if (normalized === "high" || normalized === "p1" || normalized === "2") {
      return "high";
    }

    if (normalized === "medium" || normalized === "p2" || normalized === "3") {
      return "medium";
    }

    return "low";
  };

  const classifyState = (state: string): "todo" | "inProgress" | "review" | "done" => {
    const normalized = state.trim().toLowerCase();

    if (["done", "closed", "resolved", "completed"].some((token) => normalized.includes(token))) {
      return "done";
    }

    if (["review", "qa", "test", "ready"].some((token) => normalized.includes(token))) {
      return "review";
    }

    if (["in progress", "active", "doing", "blocked"].some((token) => normalized.includes(token))) {
      return "inProgress";
    }

    return "todo";
  };

  const mappedBoard = (() => {
    if (adminGovernance) {
      const byName = new Map(adminGovernance.board.map((column) => [column.name, column.items]));
      const mapItems = (name: string) => (byName.get(name) ?? []).map((item) => ({
        id: item.workItemId,
        title: item.title,
        points: Math.max(0, Math.round(item.storyPoints ?? 0)),
        assignee: getAssigneeInitials(item.assignedToName),
        priority: mapPriority(item.priority),
      }));

      return { todo: mapItems("To Do"), inProgress: mapItems("In Progress"), review: mapItems("In Review"), done: mapItems("Done") };
    }

    if (!pmWorkItems || pmWorkItems.length === 0) {
      return SPRINT_BOARD;
    }

    const seed = {
      todo: [] as typeof SPRINT_BOARD.todo,
      inProgress: [] as typeof SPRINT_BOARD.inProgress,
      review: [] as typeof SPRINT_BOARD.review,
      done: [] as typeof SPRINT_BOARD.done,
    };

    for (const item of pmWorkItems.slice(0, 24)) {
      const bucket = classifyState(item.state);
      seed[bucket].push({
        id: item.workItemId,
        title: item.title,
        points: Math.max(0, Math.round(item.storyPoints ?? 0)),
        assignee: getAssigneeInitials(item.assignedToName),
        priority: mapPriority(item.priority),
      });
    }

    return seed;
  })();

  const columns = [
    { id: "todo", label: "To Do", color: "border-gray-200 bg-gray-50", items: mappedBoard.todo },
    { id: "inProgress", label: "In Progress", color: "border-blue-200 bg-blue-50/40", items: mappedBoard.inProgress },
    { id: "review", label: "In Review", color: "border-amber-200 bg-amber-50/40", items: mappedBoard.review },
    { id: "done", label: "Done", color: "border-green-200 bg-green-50/40", items: mappedBoard.done },
  ];

  const activeSprint = (() => {
    const items = pmSprintProgress?.sprints ?? [];
    if (items.length === 0) {
      return null;
    }

    return items.find((item) => !item.status.toLowerCase().includes("completed")) ?? items[0];
  })();

  const sprintHealth = adminGovernance?.sprintHealth ?? activeSprint?.completionRate ?? 74;
  const capacityUsed = adminGovernance?.capacityUsedPercent ?? (activeSprint && activeSprint.plannedStoryPoints > 0
    ? (activeSprint.completedStoryPoints / activeSprint.plannedStoryPoints) * 100
    : 82);
  const completedItems = adminGovernance?.completedItems ?? activeSprint?.completedWorkItems ?? 10;
  const delayedItems = adminGovernance?.delayedItems ?? (activeSprint
    ? Math.max(0, (activeSprint.totalWorkItems - activeSprint.completedWorkItems) - mappedBoard.inProgress.length)
    : 2);

  const burndownData = (() => {
    if (!activeSprint) {
      return BURNDOWN;
    }

    const total = Math.max(activeSprint.plannedStoryPoints, 1);
    const remaining = Math.max(activeSprint.plannedStoryPoints - activeSprint.completedStoryPoints, 0);

    return Array.from({ length: 10 }, (_, index) => {
      const day = index + 1;
      const ideal = Math.max(0, total - ((total / 10) * index));
      const actual = Math.max(0, total - (((total - remaining) / 10) * index));

      return {
        day: `D${day}`,
        ideal: Number(ideal.toFixed(2)),
        actual: Number(actual.toFixed(2)),
      };
    });
  })();

  const sprintTrendData = (() => {
    const items = pmSprintProgress?.sprints ?? [];
    if (items.length === 0) {
      return SPRINT_TREND;
    }

    return [...items]
      .slice(0, 5)
      .reverse()
      .map((sprint) => ({
        sprint: sprint.sprintName,
        planned: sprint.plannedStoryPoints,
        completed: sprint.completedStoryPoints,
        velocity: sprint.completedStoryPoints,
      }));
  })();

  const sprintSubtitle = adminGovernance?.sprintName ?? activeSprint?.sprintName
    ?? sprintSection?.rows[0]?.cells["Sprint"]
    ?? "Sprint 14";

  return (
    <div className="space-y-5" style={{ fontFamily: "Inter, sans-serif" }}>
      {onProjectSelect && projects.length > 0 && (
        <div className="flex items-center gap-3">
          <label className="text-sm font-semibold text-gray-700" htmlFor="sprint-project">Project</label>
          <select id="sprint-project" value={selectedProjectId ?? ""} onChange={(event) => onProjectSelect(Number(event.target.value))} className="rounded-lg border border-gray-200 bg-white px-3 py-2 text-sm text-gray-700">
            {projects.map((project) => <option key={project.id} value={project.id}>{project.name}</option>)}
          </select>
        </div>
      )}
      {/* Header KPIs */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <KPICard title="Sprint Health" value={`${sprintHealth.toFixed(1)}%`} change="Live" changeType="up" icon={Activity} color="bg-blue-600" subtitle={sprintSubtitle} />
        <KPICard title="Capacity Used" value={`${capacityUsed.toFixed(1)}%`} change="Live" changeType="up" icon={Gauge} color="bg-green-600" subtitle={adminGovernance ? `${adminGovernance.completedStoryPoints} / ${adminGovernance.plannedStoryPoints} story points` : activeSprint ? `${activeSprint.completedStoryPoints} / ${activeSprint.plannedStoryPoints} story points` : "41 / 50 story points"} />
        <KPICard title="Completed Items" value={String(completedItems)} change="Live" changeType="stable" icon={CheckCircle} color="bg-indigo-500" subtitle={adminGovernance ? `of ${adminGovernance.plannedItems} planned` : activeSprint ? `of ${activeSprint.totalWorkItems} planned` : "of 15 planned"} />
        <KPICard title="Delayed Items" value={String(delayedItems)} change="Live" changeType="down" icon={Clock} color="bg-red-500" subtitle="Need attention" />
      </div>

      {/* Sprint Board */}
      <div>
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-lg font-semibold text-gray-900">Sprint Board <span className="text-sm text-gray-400 font-normal">· {sprintSubtitle}</span></h2>
          <div className="flex gap-2">
            <Btn variant="secondary" icon={Filter}>Filter</Btn>
            <Btn variant="primary" icon={Plus}>Add Story</Btn>
          </div>
        </div>
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
          {columns.map(col => (
            <div key={col.id} className={`border rounded-xl ${col.color} p-3`}>
              <div className="flex items-center justify-between mb-3">
                <span className="text-xs font-bold text-gray-700">{col.label}</span>
                <span className="text-xs font-bold text-gray-500 bg-white rounded-full w-5 h-5 flex items-center justify-center shadow-sm">{col.items.length}</span>
              </div>
              <div className="space-y-2.5">
                {col.items.map(item => (
                  <motion.div key={item.id} whileHover={{ scale: 1.02 }}
                    className="bg-white rounded-lg p-3 shadow-sm border border-gray-100 cursor-pointer">
                    <p className="text-xs text-gray-700 font-medium mb-2 leading-relaxed">{item.title}</p>
                    <div className="flex items-center justify-between">
                      <span className={`text-[10px] font-bold px-1.5 py-0.5 rounded ${priorityColor[item.priority as keyof typeof priorityColor]}`}>
                        {item.priority}
                      </span>
                      <div className="flex items-center gap-1.5">
                        <Avatar initials={item.assignee} size="xs" colorIdx={item.assignee.charCodeAt(0)} />
                        <span className="text-[10px] bg-blue-50 text-blue-600 font-bold px-1.5 py-0.5 rounded">{item.points}pt</span>
                      </div>
                    </div>
                  </motion.div>
                ))}
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Charts */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-5">
        <Card>
          <SectionHeader title="Sprint Burndown" subtitle="Remaining work vs ideal" />
          <ResponsiveContainer width="100%" height={200}>
            <LineChart data={burndownData}>
              <CartesianGrid strokeDasharray="3 3" stroke="#F1F5F9" />
              <XAxis dataKey="day" tick={{ fontSize: 11, fill: "#94A3B8" }} axisLine={false} tickLine={false} />
              <YAxis tick={{ fontSize: 11, fill: "#94A3B8" }} axisLine={false} tickLine={false} />
              <Tooltip content={<Tooltip_ />} />
              <Legend wrapperStyle={{ fontSize: 11 }} />
              <Line key="sg-ideal" type="monotone" dataKey="ideal" name="Ideal" stroke="#CBD5E1" strokeWidth={2} strokeDasharray="5 3" dot={false} />
              <Line key="sg-actual" type="monotone" dataKey="actual" name="Actual" stroke={C.blue} strokeWidth={2.5} dot={{ r: 3 }} />
            </LineChart>
          </ResponsiveContainer>
        </Card>
        <Card>
          <SectionHeader title="Velocity Trend" subtitle="5-sprint comparison" />
          <ResponsiveContainer width="100%" height={200}>
            <BarChart data={sprintTrendData}>
              <CartesianGrid strokeDasharray="3 3" stroke="#F1F5F9" />
              <XAxis dataKey="sprint" tick={{ fontSize: 11, fill: "#94A3B8" }} axisLine={false} tickLine={false} />
              <YAxis tick={{ fontSize: 11, fill: "#94A3B8" }} axisLine={false} tickLine={false} />
              <Tooltip content={<Tooltip_ />} />
              <Legend wrapperStyle={{ fontSize: 11 }} />
              <Bar key="sg-planned" dataKey="planned" name="Planned" fill="#DBEAFE" radius={[4, 4, 0, 0]} />
              <Bar key="sg-completed" dataKey="completed" name="Completed" fill={C.blue} radius={[4, 4, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </Card>
      </div>
    </div>
  );
};

// ──────────────────────────────────────────────
// SCREEN: KPI DASHBOARD
// ──────────────────────────────────────────────
const KPIDashboard = ({
  reportsOverview = null,
  pmKpis = null,
  projects = [],
  selectedProjectId = null,
  selectedProjectDetails = null,
  onNavigate,
  isAdministrator = false,
  isLoading = false,
}: {
  reportsOverview?: ReportsOverviewDto | null;
  pmKpis?: ProjectManagerKpisDto | null;
  projects?: ProjectRecord[];
  selectedProjectId?: number | null;
  selectedProjectDetails?: AdminProjectDetailsDto | null;
  onNavigate?: (path: string) => void;
  isAdministrator?: boolean;
  isLoading?: boolean;
}) => {
  const kpiSection = findSection(reportsOverview, "kpis");
  const adminProjects = isAdministrator ? projects : [];
  const selectedAdminProject = isAdministrator
    ? adminProjects.find((project) => project.id === selectedProjectId) ?? adminProjects[0] ?? null
    : null;

  const buildSuccessRate = useMemo(
    () => getBuildSuccessRate(selectedProjectDetails?.builds),
    [selectedProjectDetails],
  );

  const buildSuccessByLabel = useMemo(() => {
    const groups = new Map<string, { total: number; successful: number }>();

    (selectedProjectDetails?.builds ?? []).forEach((build) => {
      const label = formatDayMonth(build.finishTime ?? build.startTime);
      if (!label || !build.result) {
        return;
      }

      const current = groups.get(label) ?? { total: 0, successful: 0 };
      current.total += 1;
      if (isSuccessfulBuild(build.result)) {
        current.successful += 1;
      }
      groups.set(label, current);
    });

    return new Map(
      Array.from(groups.entries()).map(([label, value]) => [label, value.total === 0 ? 0 : (value.successful / value.total) * 100]),
    );
  }, [selectedProjectDetails]);

  const velocityByLabel = useMemo(() => {
    if (!isAdministrator) {
      return new Map((pmKpis?.velocityTrend ?? []).map((point) => [point.label, point.value]));
    }

    const groups = new Map<string, { total: number; count: number }>();

    (selectedProjectDetails?.sprints ?? []).forEach((sprint) => {
      const label = formatDayMonth(sprint.endDate);
      if (!label) {
        return;
      }

      const current = groups.get(label) ?? { total: 0, count: 0 };
      current.total += sprint.completedStoryPoints;
      current.count += 1;
      groups.set(label, current);
    });

    return new Map(
      Array.from(groups.entries()).map(([label, value]) => [label, value.count === 0 ? 0 : value.total / value.count]),
    );
  }, [isAdministrator, pmKpis, selectedProjectDetails]);

  const avgCompletion = pmKpis?.averageCompletionRate ?? selectedProjectDetails?.averageCompletionRate ?? findSummaryValue(kpiSection, "completion");
  const avgVelocity = pmKpis?.averageSprintVelocity ?? selectedProjectDetails?.averageSprintVelocity ?? findSummaryValue(kpiSection, "velocity");
  const avgDefectDensity = pmKpis?.averageDefectDensity ?? findSummaryValue(kpiSection, "defect density");
  const avgBacklogHealth = pmKpis?.averageBacklogHealth ?? findSummaryValue(kpiSection, "backlog health");
  const avgReleaseSuccess = pmKpis?.averageReleaseSuccessRate ?? findSummaryValue(kpiSection, "release success");

  const projectTrendData = useMemo(() => {
    const sprints = selectedProjectDetails?.sprints ?? [];

    return sprints
      .slice()
      .sort((left, right) => new Date(left.endDate).getTime() - new Date(right.endDate).getTime())
      .map((sprint) => {
        const completionRate = sprint.plannedStoryPoints > 0
          ? (sprint.completedStoryPoints / sprint.plannedStoryPoints) * 100
          : sprint.totalWorkItems > 0
            ? (sprint.completedWorkItems / sprint.totalWorkItems) * 100
            : 0;

        const label = formatDayMonth(sprint.endDate);

        return {
          month: label || sprint.sprintName,
          sprintRate: completionRate,
          defect: avgDefectDensity,
          buildSuccess: buildSuccessRate,
          velocity: sprint.completedStoryPoints,
        };
      });
  }, [avgDefectDensity, buildSuccessRate, selectedProjectDetails]);

  const kpiTrendData = (() => {
    if (pmKpis && pmKpis.completionTrend.length > 0) {
      return pmKpis.completionTrend.map((point) => ({
        month: point.label,
        sprintRate: point.value,
        defect: avgDefectDensity,
        buildSuccess: avgReleaseSuccess,
        velocity: velocityByLabel.get(point.label) ?? avgVelocity,
      }));
    }

    if (kpiSection && kpiSection.chartPoints.length > 0) {
      return kpiSection.chartPoints.map((point) => ({
        month: point.label,
        sprintRate: point.value,
        defect: parsePercentValue(kpiSection.rows.find((row) => formatDayMonth(row.cells["Snapshot"]) === point.label)?.cells["Defect Density"] ?? `${avgDefectDensity}`),
        buildSuccess: buildSuccessByLabel.get(point.label) ?? buildSuccessRate,
        velocity: velocityByLabel.get(point.label) ?? parsePercentValue(kpiSection.rows.find((row) => formatDayMonth(row.cells["Snapshot"]) === point.label)?.cells["Velocity"] ?? `${avgVelocity}`),
      }));
    }

    if (projectTrendData.length > 0) {
      return projectTrendData.map((point) => ({
        month: point.month,
        sprintRate: point.sprintRate,
        defect: point.defect,
        buildSuccess: buildSuccessByLabel.get(point.month) ?? point.buildSuccess,
        velocity: velocityByLabel.get(point.month) ?? point.velocity,
      }));
    }

    return KPI_TREND;
  })();

  const activeSprints = selectedProjectDetails?.activeSprints ?? 0;
  const teamMembers = selectedProjectDetails?.teamMembers.length ?? 0;
  const blockedItems = selectedProjectDetails?.workItems.filter((item) => item.isBlocked || item.state.toLowerCase() === "blocked").length ?? 0;
  const riskScore = Math.round(selectedProjectDetails?.riskScore ?? 0);

  const handleExport = () => {
    if (!selectedAdminProject || !selectedProjectDetails || !kpiSection) {
      return;
    }

    const summarySheet = XLSX.utils.json_to_sheet([
      { Metric: "Project", Value: selectedProjectDetails.projectName },
      { Metric: "Project Code", Value: selectedAdminProject.code },
      { Metric: "Visibility", Value: selectedProjectDetails.visibility },
      { Metric: "Sprint Completion Rate", Value: `${avgCompletion.toFixed(1)}%` },
      { Metric: "Team Velocity", Value: `${avgVelocity.toFixed(2)} pts` },
      { Metric: "Defect Density", Value: avgDefectDensity.toFixed(2) },
      { Metric: "Backlog Health", Value: `${avgBacklogHealth.toFixed(1)}%` },
      { Metric: "Active Sprints", Value: activeSprints },
      { Metric: "Team Members", Value: teamMembers },
      { Metric: "Blocked Work Items", Value: blockedItems },
      { Metric: "Build Success", Value: `${buildSuccessRate.toFixed(1)}%` },
      { Metric: "Release Success", Value: `${avgReleaseSuccess.toFixed(1)}%` },
    ]);

    const trendSheet = XLSX.utils.json_to_sheet(
      kpiTrendData.map((point) => ({
        Snapshot: point.month,
        Completion: `${point.sprintRate.toFixed(1)}%`,
        Velocity: point.velocity.toFixed(2),
        DefectDensity: point.defect.toFixed(2),
        BuildSuccess: `${point.buildSuccess.toFixed(1)}%`,
      })),
    );

    const sprintSheet = XLSX.utils.json_to_sheet(
      selectedProjectDetails.sprints.map((sprint) => ({
        Sprint: sprint.sprintName,
        Status: sprint.status,
        StartDate: sprint.startDate,
        EndDate: sprint.endDate,
        PlannedStoryPoints: sprint.plannedStoryPoints,
        CompletedStoryPoints: sprint.completedStoryPoints,
        TotalWorkItems: sprint.totalWorkItems,
        CompletedWorkItems: sprint.completedWorkItems,
      })),
    );

    const buildSheet = XLSX.utils.json_to_sheet(
      selectedProjectDetails.builds.map((build) => ({
        Definition: build.definitionName,
        BuildNumber: build.buildNumber,
        Status: build.status,
        Result: build.result ?? "-",
        SourceBranch: build.sourceBranch,
        StartTime: build.startTime,
        FinishTime: build.finishTime ?? "-",
      })),
    );

    const workbook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(workbook, summarySheet, "Summary");
    XLSX.utils.book_append_sheet(workbook, trendSheet, "KPI Trends");
    XLSX.utils.book_append_sheet(workbook, sprintSheet, "Sprints");
    XLSX.utils.book_append_sheet(workbook, buildSheet, "Builds");
    XLSX.writeFile(workbook, `Sarathi-${selectedAdminProject.code}-Delivery-KPIs-${new Date().toISOString().split("T")[0]}.xlsx`);
  };

  return (
    <div className="space-y-6" style={{ fontFamily: "Inter, sans-serif" }}>
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-xl font-bold text-gray-900">KPI Dashboard</h2>
          <p className="text-sm text-gray-500">{isAdministrator
            ? `${selectedAdminProject?.name ?? "Select a project"} delivery KPIs from live synchronized data`
            : "Portfolio-wide key performance indicators"}</p>
        </div>
        <div className="flex gap-2">
          {isAdministrator && onNavigate && (
            <Btn variant="secondary" icon={ChevronLeft} onClick={() => onNavigate("/delivery-kpis")}>Back to Projects</Btn>
          )}
          <Btn variant="secondary" icon={Download} onClick={handleExport}>Export</Btn>
        </div>
      </div>

      {isLoading && (
        <div className="text-xs text-gray-400">Loading selected project KPI data...</div>
      )}

      {/* KPI Cards */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <KPICard title="Sprint Completion Rate" value={`${avgCompletion.toFixed(1)}%`} change="Live" changeType="up" icon={Target} color="bg-blue-600" subtitle="Target: 85%" />
        <KPICard title="Team Velocity" value={`${avgVelocity.toFixed(2)} pts`} change="Live" changeType="up" icon={TrendingUp} color="bg-purple-600" subtitle="Avg across sprints" />
        <KPICard title="Defect Density" value={avgDefectDensity.toFixed(2)} change="Live" changeType="up" icon={Bug} color="bg-green-600" subtitle="Per KLoC · Target: <1.0" />
        <KPICard title="Backlog Health" value={`${avgBacklogHealth.toFixed(1)}%`} change="Live" changeType="up" icon={List} color="bg-indigo-500" subtitle="Groomed & estimated" />
        <KPICard title="Active Sprints" value={String(activeSprints)} change="Live" changeType="stable" icon={Clock} color="bg-teal-600" subtitle="Current sprint execution" />
        <KPICard title="Team Members" value={String(teamMembers)} change="Live" changeType="stable" icon={Users} color="bg-amber-500" subtitle="Assigned delivery team" />
        <KPICard title="Build Success" value={`${buildSuccessRate.toFixed(1)}%`} change="Live" changeType={buildSuccessRate >= 85 ? "up" : "stable"} icon={Zap} color="bg-orange-500" subtitle="Completed builds" />
        <KPICard title="Risk Score" value={`${riskScore}/100`} change={blockedItems > 0 ? `${blockedItems} blocked items` : "Live"} changeType={riskScore >= 60 ? "down" : riskScore > 0 ? "stable" : "up"} icon={Shield} color={riskScore >= 60 ? "bg-red-600" : "bg-green-700"} subtitle={selectedProjectDetails?.riskLevel ?? "Delivery risk"} />
      </div>

      {/* Charts */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-5">
        <Card>
          <SectionHeader title="Performance Trends" subtitle="Key metrics over time" />
          <ResponsiveContainer width="100%" height={220}>
            <AreaChart data={kpiTrendData}>
              <CartesianGrid strokeDasharray="3 3" stroke="#F1F5F9" />
              <XAxis dataKey="month" tick={{ fontSize: 11, fill: "#94A3B8" }} axisLine={false} tickLine={false} />
              <YAxis tick={{ fontSize: 11, fill: "#94A3B8" }} axisLine={false} tickLine={false} />
              <Tooltip content={<Tooltip_ />} />
              <Legend wrapperStyle={{ fontSize: 11 }} />
              <Area key="kd-sprintRate" type="monotone" dataKey="sprintRate" name="Sprint Rate %" stroke={C.blue} fill="#DBEAFE" strokeWidth={2} />
              <Area key="kd-buildSuccess" type="monotone" dataKey="buildSuccess" name="Build Success %" stroke={C.green} fill="#DCFCE7" strokeWidth={2} />
            </AreaChart>
          </ResponsiveContainer>
        </Card>

        <Card>
          <SectionHeader title="Defect & Velocity Trends" subtitle="Quality vs delivery speed" />
          <ResponsiveContainer width="100%" height={220}>
            <ComposedChart data={kpiTrendData}>
              <CartesianGrid strokeDasharray="3 3" stroke="#F1F5F9" />
              <XAxis dataKey="month" tick={{ fontSize: 11, fill: "#94A3B8" }} axisLine={false} tickLine={false} />
              <YAxis yAxisId="left" tick={{ fontSize: 11, fill: "#94A3B8" }} axisLine={false} tickLine={false} />
              <YAxis yAxisId="right" orientation="right" tick={{ fontSize: 11, fill: "#94A3B8" }} axisLine={false} tickLine={false} />
              <Tooltip content={<Tooltip_ />} />
              <Legend wrapperStyle={{ fontSize: 11 }} />
              <Bar key="kd-velocity" yAxisId="left" dataKey="velocity" name="Velocity" fill="#EDE9FE" radius={[4, 4, 0, 0]} />
              <Line key="kd-defect" yAxisId="right" type="monotone" dataKey="defect" name="Defect Density" stroke={C.red} strokeWidth={2.5} dot={{ r: 3 }} />
            </ComposedChart>
          </ResponsiveContainer>
        </Card>
      </div>

    </div>
  );
};

// ──────────────────────────────────────────────
// SCREEN: AI RISK ANALYSIS
// ──────────────────────────────────────────────
const AIRiskAnalysis = ({ projects = [], reportsOverview = null, projectScopeLabel = "projects" }: { projects?: ProjectRecord[]; reportsOverview?: ReportsOverviewDto | null; projectScopeLabel?: string }) => {
  const [selectedProjectId, setSelectedProjectId] = useState<number | "all">("all");
  useEffect(() => {
    if (selectedProjectId !== "all" && !projects.some((project) => project.id === selectedProjectId)) setSelectedProjectId("all");
  }, [projects, selectedProjectId]);
  const scopedProjects = selectedProjectId === "all" ? projects : projects.filter((project) => project.id === selectedProjectId);
  const selectedProject = selectedProjectId === "all" ? null : scopedProjects[0] ?? null;
  const portfolioSection = findSection(reportsOverview, "portfolio");
  const kpiSection = findSection(reportsOverview, "kpis");
  const workItemSection = findSection(reportsOverview, "workitems");
  const highRiskCount = portfolioSection?.summaries.find((item) => item.label.toLowerCase().includes("high risk"))?.value ?? "0";

  const fallbackRisk = scopedProjects.length
    ? Math.round(scopedProjects.reduce((acc, project) => acc + project.risk, 0) / scopedProjects.length)
    : 0;
  const overallRisk = Math.max(0, Math.min(100, fallbackRisk));

  const avgCompletion = parsePercentValue(kpiSection?.summaries.find((item) => item.label.toLowerCase().includes("completion"))?.value ?? "0%");
  const avgReleaseSuccess = parsePercentValue(kpiSection?.summaries.find((item) => item.label.toLowerCase().includes("release success"))?.value ?? "0%");
  const blockedItems = Number.parseInt(workItemSection?.summaries.find((item) => item.label.toLowerCase().includes("blocked"))?.value ?? "0", 10) || 0;
  const workItemCount = Number.parseInt(workItemSection?.summaries.find((item) => item.label.toLowerCase() === "items")?.value ?? "0", 10) || 0;
  const blockedRatio = workItemCount > 0 ? Math.round((blockedItems / workItemCount) * 100) : 0;

  const riskRadarData = [
    { subject: "Schedule", A: Math.max(0, Math.min(100, 100 - avgCompletion)), fullMark: 100 },
    { subject: "Quality", A: Math.max(0, Math.min(100, 100 - avgReleaseSuccess)), fullMark: 100 },
    { subject: "Scope", A: Math.max(0, Math.min(100, overallRisk + 8)), fullMark: 100 },
    { subject: "Technical", A: Math.max(0, Math.min(100, overallRisk)), fullMark: 100 },
    { subject: "Dependency", A: Math.max(0, Math.min(100, blockedRatio + 25)), fullMark: 100 },
    { subject: "Resource", A: Math.max(0, Math.min(100, overallRisk - 6)), fullMark: 100 },
  ];

  const riskTrendData = (kpiSection?.chartPoints.length ?? 0) > 0
    ? kpiSection!.chartPoints.slice(-6).map((point) => {
        const completionRisk = Math.max(0, Math.min(100, 100 - point.value));
        return {
          month: point.label,
          schedule: completionRisk,
          quality: Math.max(0, Math.min(100, completionRisk - 8)),
          scope: Math.max(0, Math.min(100, completionRisk + 6)),
          technical: Math.max(0, Math.min(100, completionRisk - 3)),
        };
      })
    : RISK_TREND;

  const topRiskProjects = [...scopedProjects].sort((a, b) => b.risk - a.risk).slice(0, 4);

  return (
    <div className="space-y-6" style={{ fontFamily: "Inter, sans-serif" }}>
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-xl font-bold text-gray-900">AI Risk Analysis</h2>
          <p className="text-sm text-gray-500">Predictive risk scoring across portfolio — powered by Sarathi AI</p>
        </div>
        <div className="flex gap-2">
          <select value={selectedProjectId} onChange={(event) => setSelectedProjectId(event.target.value === "all" ? "all" : Number(event.target.value))} className="rounded-lg border border-gray-200 bg-white px-3 py-2 text-sm font-medium text-gray-700">
            <option value="all">All visible {projectScopeLabel}</option>
            {projects.map((project) => <option key={project.id} value={project.id}>{project.name}</option>)}
          </select>
          <Btn variant="secondary" icon={Download}>Risk Report</Btn>
          <Btn variant="primary" icon={RefreshCw}>Refresh Analysis</Btn>
        </div>
      </div>

      {/* Risk Score + Radar */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-5">
        <Card className="flex flex-col justify-center py-6">
          <div className="text-sm font-semibold text-gray-600 mb-2">{selectedProject ? "Project Risk Score" : "Visible Portfolio Risk"}</div>
          <div className="flex items-end justify-between gap-4">
            <div><span className="text-5xl font-black text-gray-900">{overallRisk}</span><span className="ml-1 text-sm text-gray-400">/ 100</span></div>
            <StatusBadge status={overallRisk > 70 ? "critical" : overallRisk > 40 ? "warning" : "success"} />
          </div>
          <ProgressBar value={overallRisk} color={overallRisk > 70 ? "bg-red-500" : overallRisk > 40 ? "bg-amber-500" : "bg-green-500"} height="h-3" />
          <div className={`mt-3 text-sm font-bold ${overallRisk > 70 ? "text-red-600" : overallRisk > 40 ? "text-amber-600" : "text-green-600"}`}>
            {overallRisk > 70 ? "High Risk" : overallRisk > 40 ? "Moderate Risk" : "Low Risk"}
          </div>
          <p className="text-xs text-gray-400 mt-1">{selectedProject ? `${selectedProject.completion.toFixed(1)}% completion · ${selectedProject.velocity.toFixed(1)} velocity` : `${highRiskCount} high-risk projects contributing to portfolio risk`}</p>
        </Card>

        {/* Radar */}
        <Card className="lg:col-span-2">
          <SectionHeader title="Risk Category Breakdown" subtitle="Multi-dimensional risk radar" />
          <ResponsiveContainer width="100%" height={220}>
            <RadarChart data={riskRadarData}>
              <PolarGrid stroke="#E2E8F0" />
              <PolarAngleAxis dataKey="subject" tick={{ fontSize: 11, fill: "#64748B" }} />
              <Radar key="risk-radar" name="Risk Score" dataKey="A" stroke={C.blue} fill={C.blue} fillOpacity={0.15} strokeWidth={2} />
              <Tooltip content={<Tooltip_ />} />
            </RadarChart>
          </ResponsiveContainer>
        </Card>
      </div>

      {/* Risk by Project + Trend */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-5">
        <Card>
          <SectionHeader title="Risk by Project" subtitle="Current risk scores" />
          <div className="space-y-4">
            {scopedProjects.map(p => (
              <div key={p.id} className="flex items-center gap-3">
                <div className="w-8 flex-shrink-0">
                  <span className="text-xs font-bold text-gray-500">{p.code}</span>
                </div>
                <div className="flex-1">
                  <div className="flex justify-between mb-1">
                    <span className="text-xs text-gray-700 font-medium">{p.name}</span>
                    <span className={`text-xs font-bold ${p.risk > 60 ? "text-red-600" : p.risk > 30 ? "text-amber-600" : "text-green-600"}`}>{p.risk}/100</span>
                  </div>
                  <ProgressBar value={p.risk} color={p.risk > 60 ? "bg-red-500" : p.risk > 30 ? "bg-amber-500" : "bg-green-500"} height="h-2.5" />
                </div>
                <StatusBadge status={p.risk > 60 ? "critical" : p.risk > 30 ? "warning" : "success"} />
              </div>
            ))}
          </div>
        </Card>

        <Card>
          <SectionHeader title="Risk Trend" subtitle="4-dimensional risk over time" />
          <ResponsiveContainer width="100%" height={220}>
            <LineChart data={riskTrendData}>
              <CartesianGrid strokeDasharray="3 3" stroke="#F1F5F9" />
              <XAxis dataKey="month" tick={{ fontSize: 11, fill: "#94A3B8" }} axisLine={false} tickLine={false} />
              <YAxis tick={{ fontSize: 11, fill: "#94A3B8" }} axisLine={false} tickLine={false} domain={[0, 100]} />
              <Tooltip content={<Tooltip_ />} />
              <Legend wrapperStyle={{ fontSize: 11 }} />
              <Line key="rt-schedule" type="monotone" dataKey="schedule" name="Schedule" stroke={C.red} strokeWidth={2} dot={false} />
              <Line key="rt-quality" type="monotone" dataKey="quality" name="Quality" stroke={C.amber} strokeWidth={2} dot={false} />
              <Line key="rt-scope" type="monotone" dataKey="scope" name="Scope" stroke={C.purple} strokeWidth={2} dot={false} />
              <Line key="rt-technical" type="monotone" dataKey="technical" name="Technical" stroke={C.blue} strokeWidth={2} dot={false} />
            </LineChart>
          </ResponsiveContainer>
        </Card>
      </div>

      {/* AI Mitigation Cards */}
      <div>
        <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center gap-2">
          <Sparkles size={16} className="text-purple-500" />
          AI Mitigation Recommendations
        </h3>
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
          {topRiskProjects.map((project, index) => (
            <AIInsightCard
              key={project.id}
              type={project.risk >= 75 ? "critical" : project.risk >= 50 ? "warning" : "success"}
              title={`${project.name}: ${project.risk >= 75 ? "Escalate immediately" : project.risk >= 50 ? "Mitigation required" : "Stable trajectory"}`}
              insight={`Current risk ${project.risk}/100 with completion ${project.completion.toFixed(1)}% and velocity ${project.velocity.toFixed(2)}. Focus on blocked work and release readiness for sustained delivery health.`}
              action={index < 2 ? "Review action plan" : undefined}
            />
          ))}
        </div>
      </div>
    </div>
  );
};

// ──────────────────────────────────────────────
// SCREEN: AI EXECUTIVE REPORTS
// ──────────────────────────────────────────────
const AIExecutiveReports = ({ projects = [], reportsOverview = null, isAdministrator = false }: { projects?: ProjectRecord[]; reportsOverview?: ReportsOverviewDto | null; isAdministrator?: boolean }) => {
  const [generating, setGenerating] = useState(false);
  const [generated, setGenerated] = useState(true);
  const [selectedProjectId, setSelectedProjectId] = useState<number | null>(projects[0]?.id ?? null);
  const [aiAnalysis, setAiAnalysis] = useState<ProjectManagerAiAnalysisDto | null>(null);
  const [selectedReportsOverview, setSelectedReportsOverview] = useState<ReportsOverviewDto | null>(reportsOverview);

  useEffect(() => {
    if (!projects.some((project) => project.id === selectedProjectId)) setSelectedProjectId(projects[0]?.id ?? null);
  }, [projects, selectedProjectId]);

  useEffect(() => {
    setSelectedReportsOverview(reportsOverview);
  }, [reportsOverview]);

  const loadSelectedOverview = async (projectId: number | null) => {
    if (!projectId) {
      setSelectedReportsOverview(null);
      return;
    }

    try {
      const overview = await reportsService.getOverview(projectId);
      setSelectedReportsOverview(overview);
    } catch (error) {
      console.error('Failed to load selected executive report overview:', error);
      setSelectedReportsOverview(reportsOverview);
    }
  };

  useEffect(() => {
    const projectId = selectedProjectId;
    if (!projectId) return;
    let disposed = false;
    void loadSelectedOverview(projectId);
    if (isAdministrator) {
      setAiAnalysis(null);
      return () => { disposed = true; };
    }

    const request = projectManagerService.getAiAnalysis(projectId, "Create a concise delivery report with KPI insights.");
    request
      .then((data) => { if (!disposed) setAiAnalysis(data); })
      .catch(() => { if (!disposed) setAiAnalysis(null); });
    return () => { disposed = true; };
  }, [selectedProjectId, isAdministrator, reportsOverview]);

  const selectedProject = projects.find((project) => project.id === selectedProjectId) ?? null;
  const reportProjects = selectedProject ? [selectedProject] : projects;

  const portfolioSection = findSection(selectedReportsOverview, "portfolio");
  const kpiSection = findSection(selectedReportsOverview, "kpis");
  const totalProjects = reportProjects.length;
  const avgHealth = totalProjects
    ? Math.round(reportProjects.reduce((acc, project) => acc + project.health, 0) / totalProjects)
    : 0;

  const avgCompletion = parsePercentValue(kpiSection?.summaries.find((item) => item.label.toLowerCase().includes("completion"))?.value ?? "0%");
  const avgVelocity = parsePercentValue(kpiSection?.summaries.find((item) => item.label.toLowerCase().includes("velocity"))?.value ?? "0");
  const avgReleaseSuccess = parsePercentValue(kpiSection?.summaries.find((item) => item.label.toLowerCase().includes("release success"))?.value ?? "0%");
  const generatedAt = selectedReportsOverview?.generatedAtUtc ? new Date(selectedReportsOverview.generatedAtUtc).toLocaleString() : "Live data unavailable";

  const topProjectsByHealth = [...reportProjects].sort((a, b) => b.health - a.health).slice(0, 5);
  const topRiskProjects = [...reportProjects].sort((a, b) => b.risk - a.risk).slice(0, 2);

  const kpiInsightCards = [
    {
      metric: "Sprint Completion Rate",
      value: `${avgCompletion.toFixed(1)}%`,
      insight: `Current completion trend for the selected project is ${avgCompletion.toFixed(1)}%. Use this baseline for sprint planning governance and unblock workflows in underperforming areas.`,
    },
    {
      metric: "Average Sprint Velocity",
      value: avgVelocity.toFixed(2),
      insight: `Average velocity for the selected scope is ${avgVelocity.toFixed(2)}. Review dependencies and workload balancing if the value is trending down.`,
    },
    {
      metric: "Release Success Rate",
      value: `${avgReleaseSuccess.toFixed(1)}%`,
      insight: `Release reliability is ${avgReleaseSuccess.toFixed(1)}%. Maintain quality gates and prioritize flaky pipeline remediation in critical streams.`,
    },
  ];

  const handleExportExcel = () => {
    if (!selectedReportsOverview || !selectedProject) {
      return;
    }

    const workbook = XLSX.utils.book_new();

    selectedReportsOverview.sections.forEach((section) => {
      const rows = section.rows.map((row) => {
        const mapped: Record<string, string> = {};
        section.columns.forEach((column) => {
          mapped[column] = row.cells[column] ?? "";
        });
        return mapped;
      });

      const sheet = XLSX.utils.json_to_sheet(rows.length > 0 ? rows : [{ Message: "No rows available" }]);
      XLSX.utils.book_append_sheet(workbook, sheet, section.title.slice(0, 31));
    });

    XLSX.writeFile(workbook, `Sarathi-${selectedProject.code}-Executive-Report-${new Date().toISOString().split("T")[0]}.xlsx`);
  };

  const handleExportPdf = () => {
    if (!selectedReportsOverview || !selectedProject) {
      return;
    }

    const printWindow = window.open("", "_blank", "noopener,noreferrer,width=1024,height=768");
    if (!printWindow) {
      return;
    }

    const sectionsHtml = selectedReportsOverview.sections.map((section) => `
      <section style="margin: 0 0 24px 0; page-break-inside: avoid;">
        <h2 style="font-size: 18px; margin: 0 0 8px 0;">${section.title}</h2>
        <p style="margin: 0 0 12px 0; color: #475569;">${section.description}</p>
        <table style="width: 100%; border-collapse: collapse; font-size: 12px;">
          <thead>
            <tr>
              ${section.columns.map((column) => `<th style="border: 1px solid #cbd5e1; padding: 8px; text-align: left; background: #f8fafc;">${column}</th>`).join("")}
            </tr>
          </thead>
          <tbody>
            ${section.rows.map((row) => `
              <tr>
                ${section.columns.map((column) => `<td style="border: 1px solid #e2e8f0; padding: 8px;">${row.cells[column] ?? "-"}</td>`).join("")}
              </tr>
            `).join("")}
          </tbody>
        </table>
      </section>
    `).join("");

    printWindow.document.write(`
      <html>
        <head>
          <title>Sarathi Executive Report</title>
          <style>
            body { font-family: Inter, Arial, sans-serif; padding: 32px; color: #0f172a; }
            h1 { margin: 0 0 8px 0; }
            .meta { margin: 0 0 24px 0; color: #64748b; font-size: 12px; }
          </style>
        </head>
        <body>
          <h1>${selectedProject.name} Executive Report</h1>
          <div class="meta">Generated ${new Date(selectedReportsOverview.generatedAtUtc).toLocaleString()}</div>
          ${sectionsHtml}
          <script>window.onload = () => { window.print(); };</script>
        </body>
      </html>
    `);
    printWindow.document.close();
  };

  return (
    <div className="space-y-6" style={{ fontFamily: "Inter, sans-serif" }}>
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-xl font-bold text-gray-900">AI Executive Reports</h2>
          <p className="text-sm text-gray-500">AI-generated executive summaries powered by live backend report sections</p>
        </div>
        <div className="flex gap-2">
          <select value={selectedProjectId ?? ""} onChange={(event) => setSelectedProjectId(Number(event.target.value))} className="rounded-lg border border-gray-200 bg-white px-3 py-2 text-sm font-medium text-gray-700">
            {projects.map((project) => <option key={project.id} value={project.id}>{project.name}</option>)}
          </select>
          <Btn variant="secondary" icon={Download} onClick={handleExportPdf}>Export PDF</Btn>
          <Btn variant="secondary" icon={Upload} onClick={handleExportExcel}>Export Excel</Btn>
          <Btn variant="primary" icon={Sparkles} onClick={() => {
            setGenerating(true);
            setGenerated(false);
            void loadSelectedOverview(selectedProjectId).finally(() => {
              setTimeout(() => {
                setGenerating(false);
                setGenerated(true);
              }, 600);
            });
          }}>
            {generating ? "Generating..." : "Regenerate"}
          </Btn>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-5">
        <div className="lg:col-span-2 space-y-5">
          <Card>
            <div className="flex items-center gap-2 mb-4">
              <div className="w-8 h-8 bg-purple-100 rounded-lg flex items-center justify-center">
                <Brain size={16} className="text-purple-600" />
              </div>
              <div>
                <div className="text-sm font-bold text-gray-900">Executive Summary</div>
                <div className="text-xs text-gray-400">Sarathi AI · Generated {generatedAt}</div>
              </div>
            </div>
            <div className="prose prose-sm max-w-none">
              <p className="text-sm text-gray-700 leading-relaxed mb-3">
                {aiAnalysis?.report ?? "Loading AI analysis from the selected project metrics..."}
              </p>
              <p className="text-sm text-gray-700 leading-relaxed mb-3">
                <strong className="text-gray-900">{selectedProject?.name ?? "Selected project"}</strong> has a health score of <strong className="text-gray-900">{selectedProject?.health ?? avgHealth}%</strong>, completion of <strong className="text-gray-900">{selectedProject?.completion.toFixed(1) ?? "0.0"}%</strong>, and delivery risk of <strong className="text-gray-900">{selectedProject?.risk ?? 0}/100</strong>.
              </p>
              <p className="text-sm text-gray-700 leading-relaxed mb-3">
                <strong className="text-gray-900">Key achievements this period:</strong> The selected project is sustaining strong completion and release outcomes. Completion is currently <strong className="text-gray-900">{avgCompletion.toFixed(1)}%</strong>, with release success at <strong className="text-gray-900">{avgReleaseSuccess.toFixed(1)}%</strong>.
              </p>
              <p className="text-sm text-gray-700 leading-relaxed">
                <strong className="text-gray-900">Critical risks:</strong> Highest-risk items currently include {topRiskProjects.map((project) => project.name).join(" and ") || "none"}. Recommended focus areas are sprint throughput recovery, blocker reduction, and release quality hardening.
              </p>
            </div>
          </Card>

          <Card>
            <SectionHeader title="Project Health Summary" subtitle="AI-analyzed status for the selected project" />
            <div className="space-y-4">
              {topProjectsByHealth.map((project) => (
                <div key={project.id} className={`p-4 rounded-xl border ${project.status === "critical" ? "border-red-100 bg-red-50/40" : project.status === "at-risk" ? "border-amber-100 bg-amber-50/40" : "border-green-100 bg-green-50/40"}`}>
                  <div className="flex items-center justify-between mb-2">
                    <div className="flex items-center gap-2">
                      <span className="text-sm font-bold text-gray-900">{project.name}</span>
                      <StatusBadge status={project.status} />
                    </div>
                    <span className={`text-lg font-black ${project.health >= 80 ? "text-green-600" : project.health >= 60 ? "text-amber-600" : "text-red-600"}`}>{project.health}%</span>
                  </div>
                  <div className="grid grid-cols-3 gap-3 text-xs">
                    <div><span className="text-gray-400">Sprint:</span> <span className="font-semibold">#{project.sprint}</span></div>
                    <div><span className="text-gray-400">Velocity:</span> <span className="font-semibold">{project.velocity} pts</span></div>
                    <div><span className="text-gray-400">Completion:</span> <span className="font-semibold">{project.completion}%</span></div>
                  </div>
                </div>
              ))}
            </div>
          </Card>

          <Card>
            <SectionHeader title="KPI Insights" subtitle="AI-interpreted performance analysis" />
            <div className="space-y-3">
              {kpiInsightCards.map((item, i) => (
                <div key={i} className="p-3 bg-gray-50 rounded-xl">
                  <div className="flex items-center justify-between mb-1">
                    <span className="text-xs font-bold text-gray-800">{item.metric}</span>
                    <div className="flex items-center gap-1">
                      <TrendingUp size={12} className="text-green-500" />
                      <span className="text-xs font-bold text-green-600">{item.value}</span>
                    </div>
                  </div>
                  <p className="text-xs text-gray-600">{item.insight}</p>
                </div>
              ))}
            </div>
          </Card>
        </div>

        <div className="space-y-4">
          <Card>
            <SectionHeader title="Report Metadata" />
            <div className="space-y-2.5 text-xs">
              {[
                { label: "Report Period", value: generated ? "Live Snapshot" : "Refreshing" },
                { label: "Generated By", value: "Sarathi AI v2.4" },
                { label: "Data Sources", value: "Azure DevOps, Internal KPIs" },
                { label: "Projects Covered", value: `${totalProjects}` },
                { label: "Data Freshness", value: generatedAt },
                { label: "Confidence Score", value: portfolioSection ? "95%" : "Fallback" },
              ].map(item => (
                <div key={item.label} className="flex justify-between">
                  <span className="text-gray-400">{item.label}</span>
                  <span className="font-semibold text-gray-800">{item.value}</span>
                </div>
              ))}
            </div>
          </Card>

          <Card>
            <SectionHeader title="AI Recommendations" />
            <div className="space-y-3">
              {topRiskProjects.map((project, index) => (
                <AIInsightCard
                  key={project.id}
                  type={index === 0 ? "critical" : "warning"}
                  title={`${project.name}: Executive Attention`}
                  insight={`Risk is ${project.risk}/100 with current completion at ${project.completion.toFixed(1)}%. Recommend explicit mitigation checkpoints in weekly governance reviews.`}
                />
              ))}
              <AIInsightCard type="info" title="Portfolio cadence improving" insight={`Average velocity is ${avgVelocity.toFixed(2)} with sustained completion trend at ${avgCompletion.toFixed(1)}%.`} />
            </div>
          </Card>

          <div className="space-y-2">
            <Btn variant="secondary" icon={Download} size="md" className="w-full justify-center" onClick={handleExportPdf}>Download PDF Report</Btn>
            <Btn variant="secondary" icon={Upload} size="md" className="w-full justify-center" onClick={handleExportExcel}>Export to Excel</Btn>
            <Btn variant="ghost" icon={Mail} size="md" className="w-full justify-center">Email to Stakeholders</Btn>
          </div>
        </div>
      </div>
    </div>
  );
};

// ──────────────────────────────────────────────
// SCREEN: AI INSIGHTS (Chat)
// ──────────────────────────────────────────────
const AIInsights = ({
  projects = [],
  messages,
  onMessagesChange,
  selectedProjectId,
  onSelectedProjectIdChange,
}: {
  projects?: ProjectRecord[];
  messages: AiChatMessage[];
  onMessagesChange: (messages: AiChatMessage[]) => void;
  selectedProjectId: number | null;
  onSelectedProjectIdChange: (projectId: number | null) => void;
}) => {
  const [input, setInput] = useState("");
  const [typing, setTyping] = useState(false);
  const messagesEndRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!selectedProjectId && projects[0]) {
      onSelectedProjectIdChange(projects[0].id);
    }
    if (selectedProjectId && !projects.some(project => project.id === selectedProjectId)) {
      onSelectedProjectIdChange(projects[0]?.id ?? null);
    }
  }, [projects, selectedProjectId, onSelectedProjectIdChange]);

  const selectedProject = projects.find(project => project.id === selectedProjectId) ?? null;
  const projectId = selectedProject?.id;

  const initialAIMessage = selectedProject
    ? `Hello! I'm Sarathi AI for ${selectedProject.name}. Ask me about delivery health, sprint progress, blockers, risks, work items, or recommendations for this project.`
    : "Hello! I'm Sarathi AI. Select an assigned project and ask about delivery health, sprint progress, blockers, risks, work items, or recommendations.";

  const clearChat = () => {
    onMessagesChange([{ role: "assistant", msg: initialAIMessage }]);
  };

  const sendMessage = async () => {
    if (!input.trim()) return;
    const userMsg = { role: "user" as const, msg: input };
    const nextMessages = [...messages, userMsg];
    onMessagesChange(nextMessages);
    setInput("");
    setTyping(true);
    if (!projectId) {
      onMessagesChange([...nextMessages, { role: "assistant", msg: "No assigned project is currently selected. Please select a project before asking questions." }]);
      setTyping(false);
      return;
    }

    try {
      const analysis = await projectManagerService.getAiAnalysis(projectId, userMsg.msg);
      onMessagesChange([...nextMessages, {
        role: "assistant",
        msg: analysis?.report ?? "No assigned project metrics are available yet. Synchronize Azure DevOps and select an assigned project.",
      }]);
    } catch {
      onMessagesChange([...nextMessages, { role: "assistant", msg: "I could not retrieve the current project analysis. Please try again after the next synchronization." }]);
    } finally {
      setTyping(false);
    }
  };

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages, typing]);

  const conversationStarted = messages.some(m => m.role === "user");

  return (
    <div className="h-[calc(100vh-8rem)] flex gap-5" style={{ fontFamily: "Inter, sans-serif" }}>
      {/* Chat */}
      <div className="flex-1 flex flex-col bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
        {/* Chat Header */}
        <div className="px-5 py-4 border-b border-gray-100 flex flex-col gap-3">
          <div className="flex items-center gap-3">
            <div className="w-9 h-9 bg-gradient-to-br from-blue-600 to-purple-600 rounded-xl flex items-center justify-center">
              <Bot size={18} className="text-white" />
            </div>
            <div className="flex-1">
              <div className="text-sm font-bold text-gray-900">Sarathi AI Assistant</div>
              <div className="text-xs text-green-500 font-medium flex items-center gap-1">
                <span className="w-1.5 h-1.5 bg-green-500 rounded-full animate-pulse" />
                Online · Assigned project only
              </div>
            </div>
            <Btn variant="ghost" icon={RotateCcw} size="xs" onClick={clearChat}>Clear chat</Btn>
          </div>
          <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
            <div className="text-xs text-gray-500">
              The assistant answers only about the selected assigned project. Irrelevant questions will be rejected.
            </div>
            <div className="flex items-center gap-2">
              <label htmlFor="project-select" className="text-xs text-gray-500 font-medium">Assigned project</label>
              <select
                id="project-select"
                value={selectedProjectId ?? ""}
                onChange={(event) => onSelectedProjectIdChange(event.target.value ? Number(event.target.value) : null)}
                className="border border-gray-200 rounded-xl px-3 py-2 text-xs bg-white"
              >
                <option value="">Select a project</option>
                {projects.map(project => (
                  <option key={project.id} value={project.id}>{project.name}</option>
                ))}
              </select>
            </div>
          </div>
        </div>

        {/* Messages */}
        <div className="flex-1 overflow-y-auto p-5">
          {!conversationStarted ? (
            <div className="h-full flex flex-col items-center justify-center text-center gap-6 px-10">
              <div className="space-y-2">
                <div className="text-sm font-medium text-gray-500">Sarathi AI is ready when you are.</div>
                <div className="text-3xl font-semibold text-gray-900">Ready when you are.</div>
              </div>

            </div>
          ) : (
            <div className="space-y-4">
              {messages.map((m, i) => (
                <motion.div key={i} initial={{ opacity: 0, y: 10 }} animate={{ opacity: 1, y: 0 }}
                  className={`flex gap-3 ${m.role === "user" ? "justify-end" : "justify-start"}`}>
                  {m.role === "assistant" && (
                    <div className="w-7 h-7 bg-gradient-to-br from-blue-600 to-purple-600 rounded-lg flex items-center justify-center flex-shrink-0 mt-0.5">
                      <Sparkles size={13} className="text-white" />
                    </div>
                  )}
                  <div className={`max-w-[80%] rounded-2xl px-4 py-3 ${
                    m.role === "user"
                      ? "bg-blue-600 text-white rounded-tr-sm"
                      : "bg-gray-50 text-gray-800 rounded-tl-sm"
                  }`}>
                    <p className={`text-xs leading-relaxed whitespace-pre-line ${m.role === "user" ? "text-white" : "text-gray-700"}`}>{m.msg}</p>
                  </div>
                  {m.role === "user" && <Avatar initials="AU" size="xs" colorIdx={0} />}
                </motion.div>
              ))}
              {typing && (
                <div className="flex gap-3">
                  <div className="w-7 h-7 bg-gradient-to-br from-blue-600 to-purple-600 rounded-lg flex items-center justify-center flex-shrink-0">
                    <Sparkles size={13} className="text-white" />
                  </div>
                  <div className="bg-gray-50 rounded-2xl rounded-tl-sm px-4 py-3">
                    <div className="flex gap-1">
                      {[0, 0.2, 0.4].map((d, i) => (
                        <motion.div key={i} animate={{ y: [0, -4, 0] }} transition={{ repeat: Infinity, duration: 0.8, delay: d }}
                          className="w-1.5 h-1.5 bg-gray-400 rounded-full" />
                      ))}
                    </div>
                  </div>
                </div>
              )}
              <div ref={messagesEndRef} />
            </div>
          )}
        </div>

        {/* Input */}
        <div className="p-4 border-t border-gray-100">
          <div className="flex gap-2">
            <input
              value={input}
              onChange={e => setInput(e.target.value)}
              onKeyDown={e => e.key === "Enter" && sendMessage()}
              placeholder={selectedProject ? `Ask anything about ${selectedProject.name}...` : "Select a project before asking a question."}
              disabled={!selectedProject}
              className="flex-1 border border-gray-200 rounded-xl px-4 py-2.5 text-xs focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent bg-gray-50 disabled:bg-gray-100"
            />
            <motion.button
              whileTap={{ scale: 0.95 }}
              onClick={sendMessage}
              disabled={!input.trim()}
              className="w-10 h-10 bg-blue-600 hover:bg-blue-700 disabled:opacity-40 text-white rounded-xl flex items-center justify-center transition-colors flex-shrink-0">
              <Send size={15} />
            </motion.button>
          </div>
        </div>
      </div>

    </div>
  );
};

// ──────────────────────────────────────────────
// SCREEN: USER & ROLE MANAGEMENT
// ──────────────────────────────────────────────
const UserRoleManagement = () => {
  const [tab, setTab] = useState("users");
  const [showInvite, setShowInvite] = useState(false);

  return (
    <div className="space-y-5" style={{ fontFamily: "Inter, sans-serif" }}>
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-xl font-bold text-gray-900">User & Role Management</h2>
          <p className="text-sm text-gray-500">{USERS.length} users · 3 roles · Microsoft Entra ID sync</p>
        </div>
        <Btn variant="primary" icon={UserPlus} onClick={() => setShowInvite(true)}>Invite User</Btn>
      </div>

      {/* Tabs */}
      <div className="flex gap-1 border-b border-gray-200">
        {["users", "roles", "permissions", "activity"].map(t => (
          <button key={t} onClick={() => setTab(t)}
            className={`px-4 py-2.5 text-xs font-semibold capitalize border-b-2 transition-all -mb-px ${
              tab === t ? "border-blue-600 text-blue-600" : "border-transparent text-gray-400 hover:text-gray-700"
            }`}>{t}</button>
        ))}
      </div>

      {tab === "users" && (
        <Card padding="p-0">
          <div className="p-5 border-b border-gray-100 flex items-center gap-3">
            <div className="flex-1 flex items-center gap-2 border border-gray-200 rounded-lg px-3 py-2">
              <Search size={13} className="text-gray-400" />
              <input placeholder="Search users..." className="flex-1 text-xs outline-none" />
            </div>
            <Btn variant="secondary" icon={Filter}>Filter by role</Btn>
          </div>
          <table className="w-full text-xs">
            <thead><tr className="border-b border-gray-50">
              {["User", "Email", "Role", "Projects", "Last Login", "Status", "Actions"].map(h => (
                <th key={h} className="text-left px-5 py-3 text-gray-400 font-semibold">{h}</th>
              ))}
            </tr></thead>
            <tbody>
              {USERS.map((u, i) => (
                <tr key={u.id} className="border-b border-gray-50 hover:bg-gray-50 transition-colors">
                  <td className="px-5 py-3"><div className="flex items-center gap-2"><Avatar initials={u.name.slice(0, 2).toUpperCase()} size="xs" colorIdx={i} /><span className="font-semibold text-gray-800">{u.name}</span></div></td>
                  <td className="px-5 py-3 text-gray-500">{u.email}</td>
                  <td className="px-5 py-3"><span className={`font-semibold text-xs ${u.role === "Org Admin" ? "text-blue-600" : u.role === "IT Admin" ? "text-purple-600" : "text-green-600"}`}>{u.role}</span></td>
                  <td className="px-5 py-3 text-gray-700">{u.projects}</td>
                  <td className="px-5 py-3 text-gray-400">{u.lastLogin}</td>
                  <td className="px-5 py-3"><StatusBadge status={u.status} /></td>
                  <td className="px-5 py-3">
                    <div className="flex gap-1">
                      <button className="p-1.5 text-gray-400 hover:text-blue-500 hover:bg-blue-50 rounded-lg transition-colors"><Edit2 size={12} /></button>
                      <button className="p-1.5 text-gray-400 hover:text-red-500 hover:bg-red-50 rounded-lg transition-colors"><Trash2 size={12} /></button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </Card>
      )}

      {tab === "roles" && (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-5">
          {[
            { name: "Administrator", icon: Building2, color: "bg-blue-600", count: 1, perms: ["View all projects", "Portfolio health", "Delivery governance", "Manual configuration", "Executive reports", "AI insights access"] },
            { name: "Project Manager", icon: Briefcase, color: "bg-green-600", count: 4, perms: ["View assigned projects", "Sprint management", "KPI dashboard", "AI insights access", "Team management", "Report generation"] },
            { name: "IT Manager", icon: Server, color: "bg-purple-600", count: 1, perms: ["Application health", "Azure DevOps config", "Sync management", "Audit logs", "User administration", "Maintenance"] },
          ].map(role => (
            <Card key={role.name}>
              <div className="flex items-center gap-3 mb-4">
                <div className={`w-10 h-10 ${role.color} rounded-xl flex items-center justify-center`}>
                  <role.icon size={18} className="text-white" />
                </div>
                <div>
                  <div className="text-sm font-bold text-gray-900">{role.name}</div>
                  <div className="text-xs text-gray-400">{role.count} user{role.count !== 1 ? "s" : ""}</div>
                </div>
              </div>
              <div className="space-y-2">
                {role.perms.map(p => (
                  <div key={p} className="flex items-center gap-2 text-xs text-gray-600">
                    <Check size={12} className="text-green-500 flex-shrink-0" />
                    {p}
                  </div>
                ))}
              </div>
              <div className="mt-4 flex gap-2">
                <Btn variant="secondary" size="xs" icon={Edit2}>Edit Role</Btn>
                <Btn variant="ghost" size="xs" icon={UserPlus}>Add User</Btn>
              </div>
            </Card>
          ))}
        </div>
      )}

      {tab === "permissions" && (
        <Card>
          <SectionHeader title="Permission Matrix" subtitle="Feature access by role" />
          <div className="overflow-x-auto">
            <table className="w-full text-xs">
              <thead>
                <tr className="border-b border-gray-100">
                  <th className="text-left py-3 pr-6 text-gray-400 font-semibold">Feature</th>
                  <th className="text-center py-3 px-4 text-blue-600 font-bold">Administrator</th>
                  <th className="text-center py-3 px-4 text-green-600 font-bold">Project Manager</th>
                  <th className="text-center py-3 px-4 text-purple-600 font-bold">IT Manager</th>
                </tr>
              </thead>
              <tbody>
                {[
                  { feature: "Portfolio Dashboard", orgAdmin: true, pm: false, it: false },
                  { feature: "All Projects", orgAdmin: true, pm: false, it: false },
                  { feature: "Project Details", orgAdmin: true, pm: true, it: false },
                  { feature: "Sprint Governance", orgAdmin: true, pm: true, it: false },
                  { feature: "Delivery KPIs", orgAdmin: true, pm: true, it: false },
                  { feature: "Risk Center", orgAdmin: true, pm: false, it: false },
                  { feature: "AI Insights", orgAdmin: false, pm: true, it: false },
                  { feature: "Executive Reports", orgAdmin: true, pm: false, it: false },
                  { feature: "Application Health", orgAdmin: false, pm: false, it: true },
                  { feature: "User Administration", orgAdmin: false, pm: false, it: true },
                  { feature: "Audit Logs", orgAdmin: false, pm: false, it: true },
                  { feature: "Maintenance", orgAdmin: false, pm: false, it: true },
                  { feature: "Azure DevOps Configuration", orgAdmin: true, pm: false, it: true },
                ].map(row => (
                  <tr key={row.feature} className="border-b border-gray-50 hover:bg-gray-50">
                    <td className="py-3 pr-6 font-medium text-gray-700">{row.feature}</td>
                    {[row.orgAdmin, row.pm, row.it].map((has, i) => (
                      <td key={i} className="py-3 px-4 text-center">
                        {has ? <Check size={15} className="text-green-500 mx-auto" /> : <X size={15} className="text-gray-200 mx-auto" />}
                      </td>
                    ))}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </Card>
      )}

      {tab === "activity" && (
        <Card padding="p-0">
          <div className="p-5 border-b border-gray-100">
            <SectionHeader title="User Activity Log" subtitle="Recent authentication and access events" />
          </div>
          <table className="w-full text-xs">
            <thead><tr className="border-b border-gray-50">
              {["Timestamp", "User", "Action", "Resource", "IP Address", "Status"].map(h => (
                <th key={h} className="text-left px-5 py-3 text-gray-400 font-semibold">{h}</th>
              ))}
            </tr></thead>
            <tbody>
              {AUDIT_LOGS.slice(0, 6).map((log, i) => (
                <tr key={i} className="border-b border-gray-50 hover:bg-gray-50">
                  <td className="px-5 py-3 font-mono text-gray-500 text-[10px]">{log.time}</td>
                  <td className="px-5 py-3 text-gray-700 truncate max-w-[140px]">{log.user}</td>
                  <td className="px-5 py-3 font-medium text-gray-800">{log.action}</td>
                  <td className="px-5 py-3 text-gray-500 truncate max-w-[160px]">{log.resource}</td>
                  <td className="px-5 py-3 font-mono text-gray-400">{log.ip}</td>
                  <td className="px-5 py-3"><StatusBadge status={log.status} /></td>
                </tr>
              ))}
            </tbody>
          </table>
        </Card>
      )}

      {/* Invite Modal */}
      {showInvite && (
        <div className="fixed inset-0 bg-black/40 backdrop-blur-sm z-50 flex items-center justify-center p-4">
          <motion.div initial={{ opacity: 0, scale: 0.95 }} animate={{ opacity: 1, scale: 1 }}
            className="bg-white rounded-2xl shadow-2xl w-full max-w-md p-6">
            <div className="flex items-center justify-between mb-5">
              <h3 className="text-lg font-bold text-gray-900">Invite User</h3>
              <button onClick={() => setShowInvite(false)} className="p-2 hover:bg-gray-100 rounded-lg"><X size={16} /></button>
            </div>
            <div className="space-y-4">
              <div>
                <label className="block text-xs font-semibold text-gray-700 mb-1.5">Email Address</label>
                <input placeholder="colleague@company.com" className="w-full border border-gray-200 rounded-xl px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
              </div>
              <div>
                <label className="block text-xs font-semibold text-gray-700 mb-1.5">Role</label>
                <select className="w-full border border-gray-200 rounded-xl px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500">
                  <option>Project Manager</option>
                  <option>Organization Admin</option>
                  <option>IT Admin</option>
                </select>
              </div>
              <div>
                <label className="block text-xs font-semibold text-gray-700 mb-1.5">Assign to Projects</label>
                <div className="space-y-2">
                  {PROJECTS.slice(0, 3).map(p => (
                    <label key={p.id} className="flex items-center gap-2 cursor-pointer">
                      <input type="checkbox" className="rounded" />
                      <span className="text-xs text-gray-700">{p.name}</span>
                    </label>
                  ))}
                </div>
              </div>
            </div>
            <div className="flex gap-2 mt-6">
              <Btn variant="secondary" onClick={() => setShowInvite(false)} className="flex-1 justify-center" size="md">Cancel</Btn>
              <Btn variant="primary" icon={Mail} onClick={() => setShowInvite(false)} className="flex-1 justify-center" size="md">Send Invitation</Btn>
            </div>
          </motion.div>
        </div>
      )}
    </div>
  );
};

// ──────────────────────────────────────────────
// SCREEN: SYNC MONITOR
// ──────────────────────────────────────────────
const SyncMonitor = () => {
  const [liveSummary, setLiveSummary] = useState<AzureDevOpsLiveSummaryDto | null>(null);
  const [configuration, setConfiguration] = useState<AzureDevOpsIntegrationConfigurationDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isSyncingAll, setIsSyncingAll] = useState(false);
  const [syncingProject, setSyncingProject] = useState<string | null>(null);
  const [savingConfiguration, setSavingConfiguration] = useState(false);
  const [lastSyncClickedAt, setLastSyncClickedAt] = useState<string | null>(null);
  const [configForm, setConfigForm] = useState<UpdateAzureDevOpsIntegrationConfigurationRequestDto>({
    organizationUrl: "",
    projectFilter: "",
    syncEnabled: true,
    personalAccessToken: "",
  });

  const hasLiveAzureDevOpsConnection = (value: AzureDevOpsIntegrationConfigurationDto | null) =>
    Boolean(value?.organizationUrl?.trim()) && Boolean(value?.hasPersonalAccessTokenConfigured);

  const loadMonitor = async () => {
    setLoading(true);
    try {
      const currentConfiguration = await azureDevOpsService.getConfiguration();
      setConfiguration(currentConfiguration);
      setConfigForm({
        organizationUrl: currentConfiguration.organizationUrl,
        projectFilter: currentConfiguration.projectFilter,
        syncEnabled: currentConfiguration.syncEnabled,
        personalAccessToken: "",
      });

      if (!hasLiveAzureDevOpsConnection(currentConfiguration)) {
        setLiveSummary({
          isConfigured: false,
          organizationUrl: currentConfiguration.organizationUrl,
          message: currentConfiguration.organizationUrl.trim()
            ? "Azure DevOps PAT is not configured."
            : "Azure DevOps organization URL is not configured.",
          generatedAtUtc: new Date().toISOString(),
          totalProjects: 0,
          successfulProjects: 0,
          warningProjects: 0,
          failedProjects: 0,
          totalItems: 0,
          averageSyncTimeSeconds: 0,
          projects: [],
          timeline: [],
          metrics: [],
        });
        setError(currentConfiguration.organizationUrl.trim()
          ? "Azure DevOps PAT is not configured."
          : "Azure DevOps organization URL is not configured.");
        return;
      }

      const summary = await azureDevOpsService.getLiveSummary();
      setLiveSummary(summary);
      setError(summary.isConfigured ? null : summary.message);
    } catch (exception) {
      console.error('Failed to load Azure DevOps monitor:', exception);
      setLiveSummary(null);
      setError('Unable to load live Azure DevOps data right now.');
    } finally {
      setLoading(false);
    }
  };

  const loadConfiguration = async () => {
    try {
      const currentConfiguration = await azureDevOpsService.getConfiguration();
      setConfiguration(currentConfiguration);
      setConfigForm({
        organizationUrl: currentConfiguration.organizationUrl,
        projectFilter: currentConfiguration.projectFilter,
        syncEnabled: currentConfiguration.syncEnabled,
        personalAccessToken: "",
      });
    } catch (exception) {
      console.error('Failed to load Azure DevOps configuration:', exception);
    }
  };

  useEffect(() => {
    void loadConfiguration();
    void loadMonitor();
    const intervalId = window.setInterval(() => {
      void loadMonitor();
    }, 60000);

    return () => window.clearInterval(intervalId);
  }, []);

  const monitorJobs = liveSummary ? mapLiveSummaryToJobs(liveSummary) : [];
  const timelineEvents = liveSummary?.timeline.length ? liveSummary.timeline.map((event) => ({
    time: formatRelativeTime(event.timeUtc),
    project: event.projectName,
    status: normalizeMonitorStatus(event.status),
    items: event.items,
    message: event.message,
  })) : [];

  const chartData = liveSummary?.metrics.length
    ? liveSummary.metrics.map((metric) => ({ project: metric.projectName.split(' ')[0], items: metric.items }))
    : [];

  const totalProjects = liveSummary?.totalProjects ?? monitorJobs.length;
  const successRate = totalProjects > 0
    ? Math.round(((liveSummary?.successfulProjects ?? monitorJobs.filter((job) => job.status === 'success').length) / totalProjects) * 100)
    : 0;
  const failedCount = liveSummary?.failedProjects ?? monitorJobs.filter((job) => job.status === 'failed').length;
  const totalItems = liveSummary?.totalItems ?? monitorJobs.reduce((sum, job) => sum + job.items, 0);
  const averageDuration = liveSummary?.averageSyncTimeSeconds ?? (monitorJobs.length > 0
    ? monitorJobs.reduce((sum, job) => sum + Number.parseFloat(job.duration.replace(/[^0-9.]/g, '') || '0'), 0) / monitorJobs.length
    : 0);

  const queueSync = async (scopeName: string) => {
    setSyncingProject(scopeName);
    setError(null);

    try {
      await azureDevOpsService.queueSynchronization({
        syncType: 'Full',
        scopeName,
        source: 'Manual',
      });

      await loadMonitor();
    } catch (exception) {
      console.error('Failed to queue Azure DevOps sync:', exception);
      setError(`Unable to queue sync for ${scopeName}.`);
    } finally {
      setSyncingProject(null);
    }
  };

  const handleSyncAll = async () => {
    const clickedAt = new Date().toISOString();
    setLastSyncClickedAt(clickedAt);
    setIsSyncingAll(true);
    try {
      await queueSync('All Projects');
    } finally {
      setIsSyncingAll(false);
    }
  };

  const handleConfigurationSave = async () => {
    setSavingConfiguration(true);
    setError(null);

    try {
      const updatedConfiguration = await azureDevOpsService.updateConfiguration(configForm);
      setConfiguration(updatedConfiguration);
      setConfigForm((current) => ({
        ...current,
        personalAccessToken: "",
      }));
      await loadMonitor();
    } catch (exception) {
      console.error('Failed to save Azure DevOps configuration:', exception);
      setError('Unable to save Azure DevOps configuration.');
    } finally {
      setSavingConfiguration(false);
    }
  };

  return (
    <div className="space-y-6" style={{ fontFamily: "Inter, sans-serif" }}>
      <div className="flex items-center justify-between gap-4 flex-wrap">
        <div>
          <h2 className="text-xl font-bold text-gray-900">Synchronization Monitor</h2>
          <p className="text-sm text-gray-500">Live Azure DevOps data fetched on demand and refreshed automatically.</p>
        </div>
        <div className="flex gap-2 flex-wrap">
          <Btn variant="primary" icon={RefreshCw} onClick={() => void handleSyncAll()}>
            {isSyncingAll ? 'Syncing...' : 'Sync All Projects'}
          </Btn>
        </div>
      </div>

      {error && (
        <div className="rounded-2xl border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800">
          {error}
        </div>
      )}

      {loading && (
        <div className="rounded-2xl border border-blue-100 bg-blue-50 px-4 py-3 text-sm text-blue-700">
          Loading live Azure DevOps snapshot...
        </div>
      )}

      {liveSummary && (
        <div className="rounded-2xl border border-slate-200 bg-white px-4 py-3 text-xs text-slate-600 shadow-sm">
          {lastSyncClickedAt
            ? `Synced at ${new Date(lastSyncClickedAt).toLocaleString()} · ${liveSummary.message}`
            : `Last refreshed ${formatRelativeTime(liveSummary.generatedAtUtc)} · ${liveSummary.message}`}
        </div>
      )}

      <Card>
        <SectionHeader
          title="Azure DevOps Connection"
          subtitle="Configure organization access once here so the live monitor can fetch real data."
          actions={configuration?.hasPersonalAccessTokenConfigured ? <StatusBadge status="success" /> : <StatusBadge status="warning" />}
        />
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label className="block text-xs font-semibold text-gray-700 mb-1.5">Organization URL</label>
            <input
              value={configForm.organizationUrl}
              onChange={(event) => setConfigForm((current) => ({ ...current, organizationUrl: event.target.value }))}
              placeholder="https://dev.azure.com/your-org/"
              className="w-full border border-gray-200 rounded-xl px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
          <div>
            <label className="block text-xs font-semibold text-gray-700 mb-1.5">Project Filter</label>
            <input
              value={configForm.projectFilter}
              onChange={(event) => setConfigForm((current) => ({ ...current, projectFilter: event.target.value }))}
              placeholder="Comma-separated project names or leave blank for all"
              className="w-full border border-gray-200 rounded-xl px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
          <div>
            <label className="block text-xs font-semibold text-gray-700 mb-1.5">Personal Access Token</label>
            <input
              type="password"
              value={configForm.personalAccessToken ?? ""}
              onChange={(event) => setConfigForm((current) => ({ ...current, personalAccessToken: event.target.value }))}
              placeholder={configuration?.hasPersonalAccessTokenConfigured ? "PAT saved already - enter a new value to rotate" : "Enter Azure DevOps PAT"}
              className="w-full border border-gray-200 rounded-xl px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>
          <div className="flex items-end">
            <label className="flex items-center gap-2 text-xs font-medium text-gray-700">
              <input
                type="checkbox"
                checked={configForm.syncEnabled}
                onChange={(event) => setConfigForm((current) => ({ ...current, syncEnabled: event.target.checked }))}
              />
              Enable Azure DevOps sync
            </label>
          </div>
        </div>
        <div className="flex gap-2 mt-4">
          <Btn variant="primary" size="md" onClick={() => void handleConfigurationSave()} className="justify-center">
            {savingConfiguration ? 'Saving...' : 'Save Configuration'}
          </Btn>
          <Btn variant="secondary" size="md" onClick={() => void loadMonitor()} className="justify-center">
            Refresh Live Data
          </Btn>
        </div>
      </Card>

      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <KPICard title="Total Synced Today" value={totalItems} change={`+${totalProjects} live projects`} changeType="up" icon={RefreshCw} color="bg-blue-600" />
        <KPICard title="Success Rate" value={`${successRate}%`} change={`${liveSummary?.successfulProjects ?? monitorJobs.filter((job) => job.status === 'success').length}/${totalProjects} projects`} changeType="stable" icon={CheckCircle} color="bg-green-600" />
        <KPICard title="Failed Syncs" value={failedCount} change={failedCount > 0 ? 'Attention required' : 'Healthy'} changeType={failedCount > 0 ? 'down' : 'stable'} icon={XCircle} color="bg-red-500" />
        <KPICard title="Avg Sync Time" value={formatDurationSeconds(averageDuration)} change="Live Azure DevOps" changeType="stable" icon={Clock} color="bg-indigo-500" />
      </div>


      <div className="grid grid-cols-1 lg:grid-cols-2 gap-5">
        <Card>
          <SectionHeader title="Sync Timeline" subtitle="Live Azure DevOps refresh history" />
          <div className="space-y-3">
            {timelineEvents.map((event, i) => (
              <div key={`${event.project}-${i}`} className="flex items-start gap-3">
                <div className={`mt-0.5 w-2 h-2 rounded-full flex-shrink-0 ${event.status === "success" ? "bg-green-500" : event.status === "failed" ? "bg-red-500" : event.status === "running" ? "bg-blue-500" : "bg-amber-500"}`} />
                <div className="flex-1 flex items-center justify-between gap-3">
                  <div>
                    <p className="text-xs font-medium text-gray-800">{event.project}</p>
                    <p className="text-[10px] text-gray-400">{event.time} · {event.items > 0 ? `${event.items} items` : "No items"}{event.message ? ` · ${event.message}` : ""}</p>
                  </div>
                  <StatusBadge status={event.status} />
                </div>
              </div>
            ))}
          </div>
        </Card>

        <Card>
          <SectionHeader title="Sync Metrics" subtitle="Items fetched per project from Azure DevOps" />
          <ResponsiveContainer width="100%" height={200}>
            <BarChart data={chartData}>
              <CartesianGrid strokeDasharray="3 3" stroke="#F1F5F9" />
              <XAxis dataKey="project" tick={{ fontSize: 10, fill: "#94A3B8" }} axisLine={false} tickLine={false} />
              <YAxis tick={{ fontSize: 11, fill: "#94A3B8" }} axisLine={false} tickLine={false} />
              <Tooltip content={<Tooltip_ />} />
              <Bar key="sync-items" dataKey="items" name="Items Synced" fill={C.blue} radius={[4, 4, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </Card>
      </div>
    </div>
  );
};

// ──────────────────────────────────────────────
// SCREEN: AUDIT LOGS
// ──────────────────────────────────────────────
const AuditLogsScreen = () => {
  const [tab, setTab] = useState("all");
  const [search, setSearch] = useState("");
  const [userFilter, setUserFilter] = useState("");
  const [refreshStatus, setRefreshStatus] = useState<{ type: "success" | "error"; message: string } | null>(null);

  const users = useMemo(() => Array.from(new Set(AUDIT_LOGS.map((item) => item.user))).sort(), []);

  const refreshAuditLogs = () => {
    setRefreshStatus(null);
    try {
      setRefreshStatus({ type: "success", message: "Audit logs refreshed successfully." });
    } catch {
      setRefreshStatus({ type: "error", message: "Failed to refresh audit logs. Please try again." });
    } finally {
      window.setTimeout(() => setRefreshStatus(null), 3500);
    }
  };

  const filtered = AUDIT_LOGS.filter((l) =>
    (userFilter === "" || l.user === userFilter) &&
    (l.action.toLowerCase().includes(search.toLowerCase()) ||
      l.user.toLowerCase().includes(search.toLowerCase()) ||
      l.resource.toLowerCase().includes(search.toLowerCase()))
  );

  return (
    <div className="space-y-5" style={{ fontFamily: "Inter, sans-serif" }}>
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-xl font-bold text-gray-900">Audit Logs</h2>
          <p className="text-sm text-gray-500">Complete audit trail for all system events and user actions</p>
        </div>
        <div className="flex items-center gap-2">
          <Btn variant="secondary" size="md" icon={RefreshCw} onClick={() => void refreshAuditLogs()} className="font-semibold min-w-[140px] px-5 py-3 hover:bg-slate-100">Refresh</Btn>
          <Btn variant="secondary" icon={Download}>Export CSV</Btn>
        </div>
      </div>
      {refreshStatus && (
        <div className={`rounded-xl border px-4 py-3 text-sm font-medium ${refreshStatus.type === "success" ? "border-emerald-100 bg-emerald-50 text-emerald-700" : "border-red-100 bg-red-50 text-red-700"}`}>
          {refreshStatus.message}
        </div>
      )}

      <div className="flex gap-1 border-b border-gray-200">
        {["all", "login", "sync", "ai", "admin"].map(t => (
          <button key={t} onClick={() => setTab(t)}
            className={`px-4 py-2.5 text-xs font-semibold capitalize border-b-2 transition-all -mb-px ${
              tab === t ? "border-blue-600 text-blue-600" : "border-transparent text-gray-400 hover:text-gray-700"
            }`}>{t}</button>
        ))}
      </div>

      <Card padding="p-0">
        <div className="p-5 border-b border-gray-100 space-y-3 md:space-y-0 md:flex md:items-center md:justify-between gap-3">
          <div className="flex-1 min-w-0 flex items-center gap-2 rounded-xl border border-gray-200 bg-gray-50 px-3 py-2">
            <Search size={13} className="text-gray-400" />
            <input value={search} onChange={e => setSearch(e.target.value)}
              placeholder="Search by action or resource..."
              className="flex-1 text-sm outline-none bg-transparent placeholder:text-gray-400 text-gray-700" />
          </div>
          <div className="flex flex-col gap-2 sm:flex-row sm:items-center">
            <label htmlFor="user-filter" className="text-sm font-semibold text-gray-600">Filter by user</label>
            <select
              id="user-filter"
              value={userFilter}
              onChange={(e) => setUserFilter(e.target.value)}
              className="rounded-xl border border-gray-200 bg-white px-3 py-2 text-sm text-gray-700 outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-100"
            >
              <option value="">All users</option>
              {users.map((user) => <option key={user} value={user}>{user}</option>)}
            </select>
          </div>
        </div>
        <table className="w-full text-xs">
          <thead><tr className="border-b border-gray-50">
            {["Timestamp", "User", "Action", "Resource", "IP Address", "Status"].map(h => (
              <th key={h} className="text-left px-5 py-3 text-gray-400 font-semibold">{h}</th>
            ))}
          </tr></thead>
          <tbody>
            {filtered.map((log, i) => (
              <motion.tr key={i} initial={{ opacity: 0 }} animate={{ opacity: 1 }} transition={{ delay: i * 0.03 }}
                className="border-b border-gray-50 hover:bg-gray-50 transition-colors">
                <td className="px-5 py-4 font-mono text-slate-600 whitespace-nowrap">{log.time}</td>
                <td className="px-5 py-4 text-slate-700 truncate max-w-[140px]">{log.user}</td>
                <td className="px-5 py-4 font-semibold text-slate-900">{log.action}</td>
                <td className="px-5 py-4 text-slate-600 truncate max-w-[160px]">{log.resource}</td>
                <td className="px-5 py-4 font-mono text-slate-500">{log.ip}</td>
              </motion.tr>
            ))}
          </tbody>
        </table>
        <div className="px-5 py-3 border-t border-gray-100 flex items-center justify-between">
          <span className="text-xs text-gray-400">Showing {filtered.length} of {AUDIT_LOGS.length} events</span>
          <div className="flex gap-1">
            <button className="px-2 py-1 text-xs border border-gray-200 rounded-lg text-gray-500 hover:bg-gray-50">Previous</button>
            <button className="px-2 py-1 text-xs border border-blue-200 rounded-lg text-blue-600 bg-blue-50">1</button>
            <button className="px-2 py-1 text-xs border border-gray-200 rounded-lg text-gray-500 hover:bg-gray-50">Next</button>
          </div>
        </div>
      </Card>
    </div>
  );
};

// ──────────────────────────────────────────────
// SCREEN: NOTIFICATIONS
// ──────────────────────────────────────────────
const mapAzureDevOpsSummaryToNotifications = (summary: AzureDevOpsLiveSummaryDto) => {
  let nextId = 1;
  const notifications = [] as { id: number; type: "critical" | "warning" | "info" | "success"; title: string; message: string; time: string; read: boolean }[];

  if (!summary.isConfigured) {
    notifications.push({
      id: nextId++,
      type: "warning",
      title: "Azure DevOps integration incomplete",
      message: summary.message,
      time: formatRelativeTime(summary.generatedAtUtc),
      read: false,
    });
  }

  if (summary.failedProjects > 0) {
    notifications.push({
      id: nextId++,
      type: "critical",
      title: `${summary.failedProjects} Azure DevOps project sync failure${summary.failedProjects === 1 ? "" : "s"}`,
      message: `${summary.failedProjects} project${summary.failedProjects === 1 ? "" : "s"} failed to synchronize. Review Azure DevOps sync status immediately.`,
      time: formatRelativeTime(summary.generatedAtUtc),
      read: false,
    });
  }

  if (summary.warningProjects > 0) {
    notifications.push({
      id: nextId++,
      type: "warning",
      title: `${summary.warningProjects} Azure DevOps project warning${summary.warningProjects === 1 ? "" : "s"}`,
      message: `${summary.warningProjects} project${summary.warningProjects === 1 ? "" : "s"} have warning status in Azure DevOps synchronization.`,
      time: formatRelativeTime(summary.generatedAtUtc),
      read: false,
    });
  }

  if (summary.successfulProjects > 0) {
    notifications.push({
      id: nextId++,
      type: "success",
      title: `${summary.successfulProjects} Azure DevOps project syncs succeeded`,
      message: `Live Azure DevOps sync completed successfully for ${summary.successfulProjects} project${summary.successfulProjects === 1 ? "" : "s"}.`,
      time: formatRelativeTime(summary.generatedAtUtc),
      read: true,
    });
  }

  summary.timeline.slice(0, 6).forEach((event) => {
    const type = event.status.toLowerCase().includes("fail") ? "critical" : event.status.toLowerCase().includes("warn") ? "warning" : "info";
    notifications.push({
      id: nextId++,
      type,
      title: `${event.projectName} ${event.status}`,
      message: event.message || `Azure DevOps synchronization event reported for ${event.projectName}.`,
      time: formatRelativeTime(event.timeUtc),
      read: false,
    });
  });

  return notifications;
};

const NotificationsScreen = () => {
  const [notifs, setNotifs] = useState(NOTIFICATIONS);
  const [filter, setFilter] = useState("all");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let disposed = false;

    const loadNotifications = async () => {
      setLoading(true);
      try {
        const summary = await azureDevOpsService.getLiveSummary();
        if (disposed) return;

        const mapped = mapAzureDevOpsSummaryToNotifications(summary);
        setNotifs(mapped.length ? mapped : NOTIFICATIONS);
        setError(null);
      } catch (exception) {
        if (!disposed) {
          console.error('Failed to load Azure DevOps notifications:', exception);
          setError('Unable to load live Azure DevOps notifications. Showing fallback notifications.');
          setNotifs(NOTIFICATIONS);
        }
      } finally {
        if (!disposed) {
          setLoading(false);
        }
      }
    };

    void loadNotifications();

    return () => {
      disposed = true;
    };
  }, []);

  const markRead = (id: number) => setNotifs(prev => prev.map(n => n.id === id ? { ...n, read: true } : n));
  const markAllRead = () => setNotifs(prev => prev.map(n => ({ ...n, read: true })));
  const refreshLiveData = async () => {
    setLoading(true);
    setError(null);
    try {
      const summary = await azureDevOpsService.getLiveSummary();
      setNotifs(mapAzureDevOpsSummaryToNotifications(summary));
    } catch (exception) {
      console.error('Failed to refresh Azure DevOps notifications:', exception);
      setError('Unable to refresh live Azure DevOps notifications.');
    } finally {
      setLoading(false);
    }
  };

  const notifIcon = { critical: AlertCircle, warning: AlertTriangle, info: Info, success: CheckCircle };
  const notifColor = { critical: "text-red-500", warning: "text-amber-500", info: "text-blue-500", success: "text-green-500" };
  const notifBg = { critical: "bg-red-50 border-red-100", warning: "bg-amber-50 border-amber-100", info: "bg-blue-50 border-blue-100", success: "bg-green-50 border-green-100" };

  const filtered = filter === "all" ? notifs : notifs.filter(n => n.type === filter);
  const unread = notifs.filter(n => !n.read).length;

  return (
    <div className="space-y-5" style={{ fontFamily: "Inter, sans-serif" }}>
      <div className="flex items-center justify-between gap-4 flex-wrap">
        <div>
          <h2 className="text-xl font-bold text-gray-900">Notifications</h2>
          <p className="text-sm text-gray-500">{unread} unread · {notifs.length} total</p>
        </div>
        <div className="flex gap-2 flex-wrap">
          <Btn variant="secondary" onClick={markAllRead}>Mark all read</Btn>
          <Btn variant="secondary" icon={RefreshCw} onClick={refreshLiveData}>
            {loading ? "Refreshing..." : "Refresh Live Data"}
          </Btn>
          <Btn variant="secondary" icon={Settings}>Notification settings</Btn>
        </div>
      </div>

      {error && (
        <div className="rounded-2xl border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800">
          {error}
        </div>
      )}

      <div className="flex gap-2 flex-wrap">
        {["all", "critical", "warning", "info", "success"].map(f => (
          <button key={f} onClick={() => setFilter(f)}
            className={`px-3 py-1.5 rounded-lg text-xs font-semibold capitalize transition-all ${
              filter === f ? "bg-blue-600 text-white" : "bg-white border border-gray-200 text-gray-500 hover:border-blue-300"
            }`}>{f}</button>
        ))}
      </div>

      <div className="space-y-3">
        {loading ? (
          <div className="rounded-2xl border border-blue-100 bg-blue-50 px-4 py-6 text-sm text-blue-700">Loading live Azure DevOps notifications...</div>
        ) : filtered.length === 0 ? (
          <div className="rounded-2xl border border-gray-200 bg-white px-4 py-10 text-sm text-gray-500 text-center">No notifications available from Azure DevOps.</div>
        ) : filtered.map((n) => {
          const Icon = notifIcon[n.type as keyof typeof notifIcon];
          return (
            <motion.div key={n.id} layout initial={{ opacity: 0, y: 6 }} animate={{ opacity: 1, y: 0 }}
              onClick={() => markRead(n.id)}
              className={`relative border rounded-xl p-4 cursor-pointer transition-all hover:shadow-sm ${
                n.read ? "bg-white border-gray-100" : `border ${notifBg[n.type as keyof typeof notifBg]}`
              }`}>
              {!n.read && <div className="absolute top-4 right-4 w-2 h-2 bg-blue-500 rounded-full" />}
              <div className="flex gap-3">
                <Icon size={16} className={notifColor[n.type as keyof typeof notifColor]} />
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2 mb-0.5">
                    <span className="text-sm font-semibold text-gray-900">{n.title}</span>
                    <span className={`text-[10px] font-bold px-1.5 py-0.5 rounded capitalize ${
                      n.type === "critical" ? "bg-red-100 text-red-600" :
                      n.type === "warning" ? "bg-amber-100 text-amber-600" :
                      n.type === "success" ? "bg-green-100 text-green-600" :
                      "bg-blue-100 text-blue-600"
                    }`}>{n.type}</span>
                  </div>
                  <p className="text-xs text-gray-600 leading-relaxed">{n.message}</p>
                  <p className="text-[10px] text-gray-400 mt-1">{n.time}</p>
                </div>
              </div>
            </motion.div>
          );
        })}
      </div>
    </div>
  );
};

// ──────────────────────────────────────────────
// SCREEN: SETTINGS
// ──────────────────────────────────────────────
const SettingsScreen = () => {
  const [tab, setTab] = useState("profile");
  const [saved, setSaved] = useState(false);
  const [azureConfig, setAzureConfig] = useState<AzureDevOpsIntegrationConfigurationDto | null>(null);
  const [azureForm, setAzureForm] = useState<UpdateAzureDevOpsIntegrationConfigurationRequestDto>({
    organizationUrl: "",
    projectFilter: "",
    syncEnabled: true,
    personalAccessToken: "",
  });
  const [azureStatus, setAzureStatus] = useState<string>("Load configuration to manage Azure DevOps");
  const [azureBusy, setAzureBusy] = useState(false);
  const [azureTestedAt, setAzureTestedAt] = useState<string | null>(null);

  useEffect(() => {
    if (tab !== "azure devops") {
      return;
    }

    let isDisposed = false;

    const loadAzureConfig = async () => {
      try {
        const config = await azureDevOpsService.getConfiguration();
        if (isDisposed) {
          return;
        }

        setAzureConfig(config);
        setAzureForm({
          organizationUrl: config.organizationUrl,
          projectFilter: config.projectFilter,
          syncEnabled: config.syncEnabled,
          personalAccessToken: "",
        });
        setAzureStatus(config.hasPersonalAccessTokenConfigured
          ? "Connection ready for live Azure DevOps data"
          : "Azure DevOps PAT is not configured");
      } catch (exception) {
        if (!isDisposed) {
          console.error('Failed to load Azure DevOps settings:', exception);
          setAzureStatus('Unable to load Azure DevOps configuration');
        }
      }
    };

    void loadAzureConfig();

    return () => {
      isDisposed = true;
    };
  }, [tab]);

  const save = async () => {
    setAzureBusy(true);
    try {
      const updated = await azureDevOpsService.updateConfiguration(azureForm);
      setAzureConfig(updated);
      setAzureForm((current) => ({ ...current, personalAccessToken: "" }));
      setAzureStatus(updated.hasPersonalAccessTokenConfigured ? "Azure DevOps configuration saved" : "Saved without PAT");
      setSaved(true);
      window.setTimeout(() => setSaved(false), 2000);
    } catch (exception) {
      console.error('Failed to save Azure DevOps settings:', exception);
      setAzureStatus('Failed to save Azure DevOps configuration');
    } finally {
      setAzureBusy(false);
    }
  };

  const testConnection = async () => {
    setAzureBusy(true);
    try {
      if ((azureForm.personalAccessToken ?? "").trim()) {
        const result = await azureDevOpsService.testConnection({
          organizationUrl: azureForm.organizationUrl || azureConfig?.organizationUrl || "",
          personalAccessToken: azureForm.personalAccessToken,
          projectFilter: azureForm.projectFilter,
        });

        setAzureStatus(result.message);
        setAzureTestedAt(result.testedAtUtc);
        return;
      }

      const summary = await azureDevOpsService.getLiveSummary();
      setAzureStatus(summary.message);
      setAzureTestedAt(summary.generatedAtUtc);
    } catch (exception) {
      console.error('Failed to test Azure DevOps connection:', exception);
      setAzureStatus('Azure DevOps connection test failed');
    } finally {
      setAzureBusy(false);
    }
  };

  return (
    <div className="space-y-5" style={{ fontFamily: "Inter, sans-serif" }}>
      <div>
        <h2 className="text-xl font-bold text-gray-900">Settings</h2>
        <p className="text-sm text-gray-500">Configure your Sarathi AI workspace and integrations</p>
      </div>

      <div className="flex gap-1 border-b border-gray-200">
        {["profile", "azure devops", "ai settings", "notifications", "security", "theme"].map(t => (
          <button key={t} onClick={() => setTab(t)}
            className={`px-4 py-2.5 text-xs font-semibold capitalize border-b-2 transition-all -mb-px whitespace-nowrap ${
              tab === t ? "border-blue-600 text-blue-600" : "border-transparent text-gray-400 hover:text-gray-700"
            }`}>{t}</button>
        ))}
      </div>

      {tab === "profile" && (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-5">
          <Card className="lg:col-span-2">
            <SectionHeader title="Profile Information" />
            <div className="flex items-center gap-5 mb-6 pb-5 border-b border-gray-100">
              <Avatar initials="AU" size="lg" colorIdx={0} />
              <div>
                <div className="text-base font-bold text-gray-900 mb-0.5">Admin User</div>
                <div className="text-xs text-gray-400 mb-2">admin@sarathi.ai · Organization Admin</div>
                <Btn variant="secondary" size="xs" icon={Upload}>Change Photo</Btn>
              </div>
            </div>
            <div className="grid grid-cols-2 gap-4">
              {[
                { label: "First Name", value: "Admin" },
                { label: "Last Name", value: "User" },
                { label: "Email", value: "admin@sarathi.ai" },
                { label: "Phone", value: "+91 98765 43210" },
                { label: "Organization", value: "Sarathi Technologies" },
                { label: "Timezone", value: "Asia/Kolkata (IST)" },
              ].map(field => (
                <div key={field.label}>
                  <label className="block text-xs font-semibold text-gray-600 mb-1.5">{field.label}</label>
                  <input defaultValue={field.value} className="w-full border border-gray-200 rounded-xl px-4 py-2.5 text-xs focus:outline-none focus:ring-2 focus:ring-blue-500 bg-gray-50" />
                </div>
              ))}
            </div>
            <div className="flex gap-2 mt-5 pt-4 border-t border-gray-100">
              <Btn variant="primary" size="md" onClick={save} className="flex items-center gap-1.5">
                {saved ? <><Check size={14} />Saved!</> : "Save Changes"}
              </Btn>
              <Btn variant="secondary" size="md">Cancel</Btn>
            </div>
          </Card>
          <Card>
            <SectionHeader title="Account Status" />
            <div className="space-y-3 text-xs">
              {[
                { label: "Account Status", value: "Active", color: "text-green-600" },
                { label: "Role", value: "Organization Admin", color: "text-blue-600" },
                { label: "Last Login", value: "Today 14:30 IST", color: "text-gray-700" },
                { label: "MFA Status", value: "Enabled", color: "text-green-600" },
                { label: "Session Expires", value: "In 7 hours", color: "text-gray-700" },
                { label: "Member Since", value: "Jan 15, 2024", color: "text-gray-700" },
              ].map(item => (
                <div key={item.label} className="flex justify-between py-2 border-b border-gray-50 last:border-0">
                  <span className="text-gray-400">{item.label}</span>
                  <span className={`font-semibold ${item.color}`}>{item.value}</span>
                </div>
              ))}
            </div>
          </Card>
        </div>
      )}

      {tab === "azure devops" && (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-5">
          <Card>
            <SectionHeader title="Azure DevOps Configuration" subtitle={azureStatus} />
            <div className="space-y-4">
              {[
                { label: "Organization URL", value: azureForm.organizationUrl, type: "url", field: "organizationUrl" as const },
                { label: "PAT Token", value: azureForm.personalAccessToken ?? "", type: "password", field: "personalAccessToken" as const },
                { label: "Project Filter", value: azureForm.projectFilter, type: "text", field: "projectFilter" as const },
              ].map(field => (
                <div key={field.label}>
                  <label className="block text-xs font-semibold text-gray-600 mb-1.5">{field.label}</label>
                  <div className="flex gap-2">
                    <input
                      type={field.type}
                      value={field.value}
                      onChange={(event) => setAzureForm((current) => ({ ...current, [field.field]: event.target.value }))}
                      className="flex-1 border border-gray-200 rounded-xl px-4 py-2.5 text-xs focus:outline-none focus:ring-2 focus:ring-blue-500 bg-gray-50" />
                    {field.type === "password" && <Btn variant="secondary" size="xs" icon={Key} onClick={() => void testConnection()}>Test</Btn>}
                  </div>
                </div>
              ))}
              <div className="flex gap-2 pt-2">
                <Btn variant="secondary" icon={Wifi} size="md" onClick={() => void testConnection()}>
                  {azureBusy ? "Testing..." : "Test Connection"}
                </Btn>
                <Btn variant="primary" size="md" onClick={() => void save()}>
                  {saved ? <><Check size={14} />Saved!</> : azureBusy ? "Saving..." : "Save Configuration"}
                </Btn>
              </div>
              {azureTestedAt && <p className="text-[10px] text-gray-400">Last tested {new Date(azureTestedAt).toLocaleString()}</p>}
            </div>
          </Card>
        </div>
      )}

      {tab === "ai settings" && (
        <Card>
          <SectionHeader title="AI Configuration" subtitle="Configure Sarathi AI behavior and analysis settings" />
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            {[
              { label: "AI Model", options: ["Sarathi AI v2.4 (Recommended)", "Sarathi AI v2.3", "Custom"], type: "select" },
              { label: "Analysis Frequency", options: ["Real-time", "Every hour", "Every 6 hours", "Daily"], type: "select" },
              { label: "Risk Threshold (Alert %)", value: "65", type: "number" },
              { label: "Velocity Deviation Alert (%)", value: "20", type: "number" },
            ].map(field => (
              <div key={field.label}>
                <label className="block text-xs font-semibold text-gray-600 mb-1.5">{field.label}</label>
                {field.type === "select" ? (
                  <select className="w-full border border-gray-200 rounded-xl px-4 py-2.5 text-xs focus:outline-none focus:ring-2 focus:ring-blue-500 bg-gray-50">
                    {field.options?.map(o => <option key={o}>{o}</option>)}
                  </select>
                ) : (
                  <input type={field.type} defaultValue={field.value}
                    className="w-full border border-gray-200 rounded-xl px-4 py-2.5 text-xs focus:outline-none focus:ring-2 focus:ring-blue-500 bg-gray-50" />
                )}
              </div>
            ))}
          </div>
          <div className="mt-6 space-y-3 border-t border-gray-100 pt-5">
            {["Enable predictive delivery forecasting", "Enable risk trend analysis", "Enable AI executive report generation", "Enable real-time anomaly detection"].map(opt => (
              <label key={opt} className="flex items-center justify-between cursor-pointer p-2 hover:bg-gray-50 rounded-xl">
                <span className="text-xs text-gray-700">{opt}</span>
                <div className="w-10 h-5 bg-blue-600 rounded-full relative flex-shrink-0">
                  <div className="absolute right-0.5 top-0.5 w-4 h-4 bg-white rounded-full shadow-sm" />
                </div>
              </label>
            ))}
          </div>
          <div className="flex gap-2 mt-5">
            <Btn variant="primary" size="md" onClick={save}>Save AI Settings</Btn>
          </div>
        </Card>
      )}

      {(tab === "notifications" || tab === "security" || tab === "theme") && (
        <Card>
          <div className="py-12 flex flex-col items-center justify-center text-center">
            <div className="w-16 h-16 bg-blue-50 rounded-2xl flex items-center justify-center mb-4">
              <Settings size={28} className="text-blue-400" />
            </div>
            <h3 className="text-lg font-semibold text-gray-700 mb-2 capitalize">{tab} Settings</h3>
            <p className="text-sm text-gray-400 max-w-sm">
              {tab === "notifications" ? "Configure alert preferences, email notifications, and in-app notification rules for all events." :
               tab === "security" ? "Manage MFA, session policies, API key rotation, and security audit settings." :
               "Customize appearance, color scheme, data density, and layout preferences."}
            </p>
          </div>
        </Card>
      )}
    </div>
  );
};

const PortfolioEnterpriseDashboard = ({ 
  onNavigate, 
  projects = [],
  dashboardKpis,
  adminStats,
}: { 
  onNavigate: (s: string) => void; 
  projects?: ProjectRecord[];
  dashboardKpis?: DashboardKpiCalculationsDto | null;
  adminStats?: AdminProjectStatisticsDto | null;
}) => {
  const active = projects.filter(p => p.status !== "completed").length;
  const critical = projects.filter(p => p.risk >= 75).length;
  const delayed = projects.filter(p => p.status !== "on-track").length;
  const overallHealth = projects.length
    ? `${Math.round(projects.reduce((acc, project) => acc + project.health, 0) / projects.length)}%`
    : "0%";
  const orgHealth = dashboardKpis
    ? `${Math.round(dashboardKpis.averageBacklogHealth)}%`
    : projects.length
    ? `${Math.round(projects.reduce((acc, project) => acc + (100 - project.risk), 0) / projects.length)}%`
    : "0%";
  const highRiskProjects = projects.filter(p => p.risk >= 60).sort((a, b) => b.risk - a.risk).slice(0, 5);
  return (
    <div className="space-y-6" style={{ fontFamily: "Inter, sans-serif" }}>
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-xl font-bold text-gray-900">Portfolio Dashboard</h2>
          <p className="text-sm text-gray-500">Enterprise delivery governance across all projects</p>
        </div>
      </div>
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <KpiCard title="Total Projects" value={projects.length} icon={FolderKanban} color="bg-blue-600" subtitle="Portfolio scope" />
        <KpiCard title="Active Projects" value={active} icon={Activity} color="bg-green-600" subtitle="Currently running" />
        <KpiCard title="Completed Projects" value="0" icon={CheckCircle} color="bg-indigo-500" subtitle="This quarter" />
        <KpiCard title="Delayed Projects" value={delayed} icon={Clock} color="bg-amber-500" subtitle="Schedule variance" />
        <KpiCard title="High Risk Projects" value={projects.filter(p => p.risk >= 60).length} icon={AlertTriangle} color="bg-red-500" subtitle="Needs intervention" />
        <KpiCard title="Critical Risk Projects" value={critical} icon={Shield} color="bg-red-600" subtitle="Executive escalation" />
        <KpiCard title="Overall Delivery Health" value={overallHealth} icon={Award} color="bg-teal-600" subtitle="Weighted portfolio score" />
        <KpiCard title="Overall Organization Health" value={orgHealth} icon={Building2} color="bg-purple-600" subtitle="Backlog health score" />
      </div>
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-5">
        <Card>
          <SectionHeader title="Portfolio KPI Summary" subtitle="Delivery health, velocity and sprint completion" />
          <ResponsiveContainer width="100%" height={220}>
            <ComposedChart data={projects.map(p => ({ project: p.code, health: p.health, completion: p.completion, velocity: p.velocity }))}>
              <CartesianGrid strokeDasharray="3 3" stroke="#F1F5F9" />
              <XAxis dataKey="project" tick={{ fontSize: 11, fill: "#94A3B8" }} axisLine={false} tickLine={false} />
              <YAxis tick={{ fontSize: 11, fill: "#94A3B8" }} axisLine={false} tickLine={false} />
              <Tooltip content={<Tooltip_ />} /><Legend wrapperStyle={{ fontSize: 11 }} />
              <Bar dataKey="completion" name="Sprint Completion" fill={C.blue} radius={[4, 4, 0, 0]} />
              <Line type="monotone" dataKey="health" name="Delivery Health" stroke={C.green} strokeWidth={2.5} />
            </ComposedChart>
          </ResponsiveContainer>
        </Card>
        <Card>
          <SectionHeader title="Overall Organization Health" subtitle="Backlog health and release success trends" />
          <ResponsiveContainer width="100%" height={220}>
            <ComposedChart data={dashboardKpis ? [
              { metric: 'Completion', value: Math.round(dashboardKpis.averageCompletionRate) },
              { metric: 'Velocity', value: Math.round(dashboardKpis.averageSprintVelocity) },
              { metric: 'Backlog', value: Math.round(dashboardKpis.averageBacklogHealth) },
              { metric: 'Release', value: Math.round(dashboardKpis.averageReleaseSuccessRate) },
            ] : [
              { metric: 'Completion', value: 0 },
              { metric: 'Velocity', value: 0 },
              { metric: 'Backlog', value: 0 },
              { metric: 'Release', value: 0 },
            ]}>
              <CartesianGrid strokeDasharray="3 3" stroke="#F1F5F9" />
              <XAxis dataKey="metric" tick={{ fontSize: 11, fill: "#94A3B8" }} axisLine={false} tickLine={false} />
              <YAxis tick={{ fontSize: 11, fill: "#94A3B8" }} axisLine={false} tickLine={false} domain={[0, 100]} />
              <Tooltip content={<Tooltip_ />} />
              <Bar dataKey="value" name="Health Score" fill={C.purple} radius={[4, 4, 0, 0]} />
            </ComposedChart>
          </ResponsiveContainer>
        </Card>
      </div>
      <Card>
        <SectionHeader title="High Risk Projects" subtitle="Projects requiring immediate attention" />
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-gray-200">
                <th className="text-left py-3 px-4 text-gray-600 font-semibold">Project</th>
                <th className="text-center py-3 px-4 text-gray-600 font-semibold">Risk</th>
                <th className="text-center py-3 px-4 text-gray-600 font-semibold">Health</th>
                <th className="text-left py-3 px-4 text-gray-600 font-semibold">Owner</th>
              </tr>
            </thead>
            <tbody>
              {highRiskProjects.length > 0 ? (
                highRiskProjects.map((project) => (
                  <tr key={project.id} className="border-b border-gray-100 hover:bg-gray-50 transition-colors">
                    <td className="py-3 px-4">
                      <div className="flex items-center gap-2">
                        <div className="w-8 h-8 bg-gradient-to-br from-blue-500 to-indigo-600 rounded-lg flex items-center justify-center">
                          <span className="text-white text-xs font-bold">{project.code}</span>
                        </div>
                        <div>
                          <div className="font-semibold text-gray-900">{project.name}</div>
                          <div className="text-xs text-gray-500">{project.businessUnit}</div>
                        </div>
                      </div>
                    </td>
                    <td className="py-3 px-4 text-center">
                      <span className={`inline-flex items-center px-2.5 py-1 rounded-full text-xs font-semibold ${
                        project.risk >= 75 ? 'bg-red-100 text-red-700' : 
                        project.risk >= 60 ? 'bg-orange-100 text-orange-700' : 
                        'bg-yellow-100 text-yellow-700'
                      }`}>
                        {project.risk}%
                      </span>
                    </td>
                    <td className="py-3 px-4 text-center">
                      <span className={`inline-flex items-center px-2.5 py-1 rounded-full text-xs font-semibold ${
                        project.health >= 70 ? 'bg-green-100 text-green-700' : 
                        project.health >= 50 ? 'bg-yellow-100 text-yellow-700' : 
                        'bg-red-100 text-red-700'
                      }`}>
                        {project.health}%
                      </span>
                    </td>
                    <td className="py-3 px-4 text-gray-700">{project.projectManager}</td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={4} className="py-8 text-center text-gray-500">
                    No high-risk projects found
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </Card>
    </div>
  );
};

const ProjectsPage = ({ 
  onNavigate, 
  projects = [],
  onProjectSelect,
  onSyncNow,
  azureOrgUrl
}: { 
  onNavigate: (s: string) => void; 
  projects?: ProjectRecord[];
  onProjectSelect?: (id: number) => Promise<void>;
  onSyncNow?: () => Promise<void>;
  azureOrgUrl?: string;
}) => {
  const [isSyncing, setIsSyncing] = useState(false);

  const handleExport = () => {
    const exportData = projects.map(p => ({
      'Project Name': p.name,
      'Project Code': p.code,
      'Project Manager': p.projectManager,
      'Business Unit': p.businessUnit,
      'Current Sprint': p.sprint,
      'Delivery Health': p.health,
      'Risk Score': p.risk,
      'Sprint Completion': `${p.completion}%`,
      'Velocity': p.velocity,
      'Defect Density': p.defectDensity,
      'Backlog Health': `${p.backlogHealth}%`,
      'Release Status': p.releaseStatus,
      'Overall Status': p.overallStatus,
      'Last Sync': p.lastSync,
      'AI Health': p.aiHealth,
    }));

    const ws = XLSX.utils.json_to_sheet(exportData);
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'Projects');
    XLSX.writeFile(wb, `Sarathi-Projects-${new Date().toISOString().split('T')[0]}.xlsx`);
  };

  const handleSync = async () => {
    if (!onSyncNow) return;
    setIsSyncing(true);
    try {
      await onSyncNow();
    } finally {
      setIsSyncing(false);
    }
  };

  const handleAzureDevOps = () => {
    if (azureOrgUrl) {
      window.open(azureOrgUrl, '_blank');
    }
  };

  return (
    <div className="space-y-5" style={{ fontFamily: "Inter, sans-serif" }}>
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-xl font-bold text-gray-900">Projects</h2>
          <p className="text-sm text-gray-500">All projects with governance, delivery and Azure DevOps health</p>
        </div>
        <div className="flex gap-2">
          {azureOrgUrl && (
            <Btn variant="secondary" icon={ExternalLink} onClick={handleAzureDevOps}>
              Azure DevOps
            </Btn>
          )}
          {onSyncNow && (
            <Btn variant="primary" icon={RefreshCw} onClick={handleSync} disabled={isSyncing}>
              {isSyncing ? 'Syncing...' : 'Sync Now'}
            </Btn>
          )}
        </div>
      </div>
      <ProjectTable 
        projects={projects} 
        onOpen={onProjectSelect || ((id) => onNavigate(`/projects/${id}`))} 
        onExport={handleExport}
      />
    </div>
  );
};

const MyProjectsPage = ({ email, projectsOverride }: { email: string; onNavigate: (s: string) => void; projectsOverride?: ProjectRecord[] }) => {
  const loading = projectsOverride === undefined;
  const projects = projectsOverride ?? [];
  const [selectedId, setSelectedId] = useState<number | null>(null);

  useEffect(() => {
    if (loading) {
      return;
    }

    if (!selectedId && projects[0]) {
      setSelectedId(projects[0].id);
    } else if (selectedId && !projects.some((project) => project.id === selectedId)) {
      setSelectedId(projects[0]?.id ?? null);
    }
  }, [projects, selectedId, loading]);

  if (loading) {
    return (
      <div className="space-y-5" style={{ fontFamily: "Inter, sans-serif" }}>
        <div>
          <h2 className="text-xl font-bold text-gray-900">Project Manager Dashboard</h2>
          <p className="text-sm text-gray-500">Loading assigned projects...</p>
        </div>
        <Card>
          <div className="py-16 text-center text-gray-500">Fetching live project assignments. Please wait.</div>
        </Card>
      </div>
    );
  }

  if (!projects.length) {
    return (
      <div className="space-y-5" style={{ fontFamily: "Inter, sans-serif" }}>
        <div>
          <h2 className="text-xl font-bold text-gray-900">Project Manager Dashboard</h2>
          <p className="text-sm text-gray-500">No assigned projects found.</p>
        </div>
        <Card>
          <div className="py-16 text-center text-gray-500">No project data is available at the moment.</div>
        </Card>
      </div>
    );
  }

  const selectedProject = projects.find(p => p.id === selectedId) ?? projects[0]!;

  return (
    <div className="space-y-5" style={{ fontFamily: "Inter, sans-serif" }}>
      <div>
        <h2 className="text-xl font-bold text-gray-900">Project Manager Dashboard</h2>
        <p className="text-sm text-gray-500">Assigned projects, sprint status and delivery execution</p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {projects.map(p => (
          <motion.div key={p.id} whileHover={{ y: -3 }} onClick={() => setSelectedId(p.id)}
            className={`bg-white rounded-xl shadow-sm border p-5 cursor-pointer transition-all ${selectedProject.id === p.id ? "border-blue-300 ring-2 ring-blue-50" : "border-gray-100 hover:border-blue-200"}`}>
            <div className="flex items-center justify-between mb-3">
              <div className="w-9 h-9 bg-gradient-to-br from-blue-500 to-indigo-600 rounded-xl flex items-center justify-center">
                <span className="text-white text-xs font-bold">{p.code}</span>
              </div>
              <StatusBadge status={p.status} />
            </div>
            <h3 className="text-sm font-bold text-gray-900 mb-0.5">{p.name}</h3>
            <p className="text-xs text-gray-400 mb-4">{p.businessUnit} � Sprint {p.sprint}</p>
            <HealthBar value={p.health} />
            <div className="grid grid-cols-2 gap-2 mt-4 text-xs">
              <div className="bg-gray-50 rounded-lg p-2"><div className="text-gray-400">Sprint</div><div className="font-bold text-gray-900">{p.completion}%</div></div>
              <div className="bg-gray-50 rounded-lg p-2"><div className="text-gray-400">Velocity</div><div className="font-bold text-gray-900">{p.velocity}</div></div>
            </div>
          </motion.div>
        ))}
      </div>

      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <KpiCard title="Sprint Progress" value={`${selectedProject.completion}%`} icon={Target} color="bg-blue-600" />
        <KpiCard title="Velocity" value={selectedProject.velocity} icon={TrendingUp} color="bg-purple-600" />
        <KpiCard title="Team Members" value={selectedProject.team} icon={Users} color="bg-green-600" />
        <KpiCard title="Release Status" value={selectedProject.releaseStatus} icon={Package} color="bg-indigo-500" />
      </div>

      <Card>
        <SectionHeader title="Sprint Burndown" subtitle={`${selectedProject.name} � Sprint ${selectedProject.sprint}`} />
        <ResponsiveContainer width="100%" height={220}>
          <LineChart data={BURNDOWN}>
            <CartesianGrid strokeDasharray="3 3" stroke="#F1F5F9" />
            <XAxis dataKey="day" tick={{ fontSize: 11, fill: "#94A3B8" }} axisLine={false} tickLine={false} />
            <YAxis tick={{ fontSize: 11, fill: "#94A3B8" }} axisLine={false} tickLine={false} />
            <Tooltip content={<Tooltip_ />} />
            <Legend wrapperStyle={{ fontSize: 11 }} />
            <Line type="monotone" dataKey="ideal" name="Ideal" stroke="#CBD5E1" strokeWidth={2} strokeDasharray="6 3" dot={false} />
            <Line type="monotone" dataKey="actual" name="Actual" stroke={C.blue} strokeWidth={2.5} dot={{ r: 3, fill: C.blue }} />
            <Line type="monotone" dataKey="scope" name="Scope" stroke={C.amber} strokeWidth={1.5} strokeDasharray="3 3" dot={false} />
          </LineChart>
        </ResponsiveContainer>
      </Card>
    </div>
  );
};
const ProjectManagerRiskAnalysisPage = ({ email, onNavigate, projectsOverride }: { email: string; onNavigate: (s: string) => void; projectsOverride?: ProjectRecord[] }) => {
  const loading = projectsOverride === undefined;
  const projects = projectsOverride ?? [];
  const [selectedId, setSelectedId] = useState<number | null>(null);

  useEffect(() => {
    if (loading) {
      return;
    }

    if (!selectedId && projects[0]) {
      setSelectedId(projects[0].id);
    } else if (selectedId && !projects.some((project) => project.id === selectedId)) {
      setSelectedId(projects[0]?.id ?? null);
    }
  }, [projects, selectedId, loading]);

  if (loading) {
    return (
      <div className="space-y-5" style={{ fontFamily: "Inter, sans-serif" }}>
        <div>
          <h2 className="text-xl font-bold text-gray-900">Risk Analysis</h2>
          <p className="text-sm text-gray-500">Loading assigned projects...</p>
        </div>
        <Card>
          <div className="py-16 text-center text-gray-500">Fetching live project assignments. Please wait.</div>
        </Card>
      </div>
    );
  }

  if (!projects.length) {
    return (
      <div className="space-y-5" style={{ fontFamily: "Inter, sans-serif" }}>
        <div>
          <h2 className="text-xl font-bold text-gray-900">Risk Analysis</h2>
          <p className="text-sm text-gray-500">No assigned projects found.</p>
        </div>
        <Card>
          <div className="py-16 text-center text-gray-500">No project data is available at the moment.</div>
        </Card>
      </div>
    );
  }

  const selectedProject = projects.find(p => p.id === selectedId) ?? projects[0]!;
  const riskLevel = selectedProject.risk >= 75 ? "Critical" : selectedProject.risk >= 60 ? "High" : selectedProject.risk >= 35 ? "Medium" : "Low";
  const [aiAnalysis, setAiAnalysis] = useState<ProjectManagerAiAnalysisDto | null>(null);

  useEffect(() => {
    if (!projects.some((project) => project.id === selectedId)) {
      setSelectedId(projects[0]?.id ?? null);
    }
  }, [projects, selectedId]);

  useEffect(() => {
    if (!selectedProject?.id) return;
    let disposed = false;
    projectManagerService.getAiAnalysis(selectedProject.id)
      .then((data) => { if (!disposed) setAiAnalysis(data); })
      .catch(() => { if (!disposed) setAiAnalysis(null); });
    return () => { disposed = true; };
  }, [selectedProject?.id]);

  return (
    <div className="space-y-5" style={{ fontFamily: "Inter, sans-serif" }}>
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-xl font-bold text-gray-900">Risk Analysis</h2>
          <p className="text-sm text-gray-500">Assigned project risk, root cause and mitigation workflow</p>
        </div>
        <Btn variant="primary" icon={FileText}>Risk Report</Btn>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-[280px_1fr] gap-5">
        <Card>
          <SectionHeader title="Assigned Projects" />
          <div className="space-y-2">
            {projects.map(p => (
              <button key={p.id} onClick={() => setSelectedId(p.id)}
                className={`w-full text-left p-3 rounded-xl transition-all ${selectedId === p.id ? "bg-blue-50 border border-blue-100" : "bg-gray-50 hover:bg-gray-100 border border-transparent"}`}>
                <div className="flex items-center justify-between mb-1">
                  <span className="text-xs font-bold text-gray-800">{p.name}</span>
                  <span className={`text-xs font-bold ${p.risk >= 60 ? "text-red-600" : p.risk >= 35 ? "text-amber-600" : "text-green-600"}`}>{p.risk}</span>
                </div>
                <ProgressBar value={p.risk} color={p.risk >= 60 ? "bg-red-500" : p.risk >= 35 ? "bg-amber-500" : "bg-green-500"} />
              </button>
            ))}
          </div>
        </Card>

        <div className="space-y-5">
          <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
            <KpiCard title="Risk Score" value={`${aiAnalysis?.riskScore ?? selectedProject.risk}/100`} icon={Shield} color={(aiAnalysis?.riskScore ?? selectedProject.risk) >= 60 ? "bg-red-500" : "bg-amber-500"} subtitle={riskLevel} />
            <KpiCard title="Predicted Delay" value={selectedProject.risk >= 60 ? "2-3w" : "0-1w"} icon={Clock} color="bg-amber-500" />
            <KpiCard title="Confidence" value="88%" icon={Brain} color="bg-purple-600" />
            <KpiCard title="Blocked Stories" value={aiAnalysis?.blockedItems ?? 0} icon={Lock} color="bg-red-500" />
          </div>
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-5">
            <RiskCard project={selectedProject} />
            <Card className="lg:col-span-2">
              <SectionHeader title="Root Cause & Mitigation" subtitle={selectedProject.name}
                actions={<Btn variant="secondary" icon={Eye} onClick={() => onNavigate(`/projects/${selectedProject.id}`)}>Open Details</Btn>} />
              <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                {[
                  ["Risk Analysis", aiAnalysis?.riskAnalysis ?? "Loading live project metrics..."],
                  ["AI Recommendation", aiAnalysis?.recommendations[0] ?? "No recommendation available."],
                  ["Owner Action", aiAnalysis?.recommendations[1] ?? "Review blockers with the delivery team."],
                  ["Next Review", aiAnalysis?.recommendations[2] ?? "Review in the next sprint ceremony."],
                ].map(([label, value]) => (
                  <div key={label} className="bg-gray-50 rounded-xl p-3">
                    <div className="text-xs text-gray-400 mb-1">{label}</div>
                    <div className="text-xs font-semibold text-gray-800 leading-relaxed">{value}</div>
                  </div>
                ))}
              </div>
            </Card>
          </div>
        </div>
      </div>
    </div>
  );
};

type ItOperationsSection = "health" | "users" | "audit" | "monitoring" | "maintenance";

const ITLiveOperationsPage = ({ section, onNavigate }: { section: ItOperationsSection; onNavigate: (path: string) => void }) => {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [synchronization, setSynchronization] = useState<ItAdminSynchronizationDto | null>(null);
  const [monitoring, setMonitoring] = useState<ItAdminSyncMonitoringDto | null>(null);
  const [health, setHealth] = useState<ItAdminSystemHealthDto | null>(null);
  const [logs, setLogs] = useState<ItAdminLogsDto | null>(null);
  const [maintenance, setMaintenance] = useState<ItAdminMaintenanceDto | null>(null);
  const [scheduling, setScheduling] = useState<ItAdminSchedulingDto | null>(null);
  const [users, setUsers] = useState<ItAdminUserDto[]>([]);
  const [audits, setAudits] = useState<ItAdminLoginAuditDto[]>([]);
  const [searchQuery, setSearchQuery] = useState("");
  const [roleFilter, setRoleFilter] = useState("all");
  const [auditSearchQuery, setAuditSearchQuery] = useState("");
  const [auditUserFilter, setAuditUserFilter] = useState("");
  const [refreshStatus, setRefreshStatus] = useState<{ type: "success" | "error"; message: string } | null>(null);

  const load = async () => {
    setLoading(true);
    setError(null);
    try {
      const [sync, monitor, systemHealth, logData, maintenanceData, scheduleData, userData, auditData] = await Promise.all([
        itAdminService.getSynchronization(), itAdminService.getSyncMonitor(), itAdminService.getSystemHealth(),
        itAdminService.getLogs({ take: 100 }), itAdminService.getMaintenance(), itAdminService.getScheduling(),
        itAdminService.getUsers(), itAdminService.getLoginAudits(100),
      ]);
      setSynchronization(sync); setMonitoring(monitor); setHealth(systemHealth); setLogs(logData);
      setMaintenance(maintenanceData); setScheduling(scheduleData); setUsers(userData); setAudits(auditData);
    } catch (exception) {
      console.error("Failed to load IT administration data", exception);
      setError("Unable to load the current Azure DevOps and platform data.");
    } finally { setLoading(false); }
  };

  const refreshSection = async (successMessage: string, errorMessage: string) => {
    setRefreshStatus(null);
    try {
      await load();
      setRefreshStatus({ type: "success", message: successMessage });
    } catch (exception) {
      setRefreshStatus({ type: "error", message: errorMessage });
    } finally {
      window.setTimeout(() => setRefreshStatus(null), 3500);
    }
  };

  const refreshUsers = async () => refreshSection("Users refreshed successfully.", "Failed to refresh users. Please try again.");
  const refreshAuditLogs = async () => refreshSection("Audit logs refreshed successfully.", "Failed to refresh audit logs. Please try again.");
  const refreshMonitoring = async () => refreshSection("Monitoring refreshed successfully.", "Failed to refresh monitoring. Please try again.");
  const refreshMaintenance = async () => refreshSection("Maintenance refreshed successfully.", "Failed to refresh maintenance. Please try again.");

  const auditUsers = useMemo(() => Array.from(new Set(audits.map((item) => item.userName || item.email))).sort(), [audits]);

  const filteredAuditLogs = useMemo(() => {
    const query = auditSearchQuery.trim().toLowerCase();
    return audits.filter((audit) => {
      const userLabel = audit.userName || audit.email;
      return (
        (auditUserFilter === "" || userLabel === auditUserFilter) &&
        (
          userLabel.toLowerCase().includes(query) ||
          audit.email.toLowerCase().includes(query) ||
          audit.ipAddress.toLowerCase().includes(query) ||
          audit.status.toLowerCase().includes(query)
        )
      );
    });
  }, [audits, auditSearchQuery, auditUserFilter]);

  const roleBadgeClass = (role: string) => {
    const normalized = role.trim();
    const badgeMap: Record<string, string> = {
      "Administrator": "bg-blue-50 text-blue-700",
      "IT Admin": "bg-purple-50 text-purple-700",
      "Project Manager": "bg-emerald-50 text-emerald-700",
      "Org Admin": "bg-slate-50 text-slate-700",
      "ProjectManager": "bg-emerald-50 text-emerald-700",
      "ITAdmin": "bg-purple-50 text-purple-700",
    };
    return badgeMap[normalized] ?? "bg-gray-50 text-gray-700";
  };

  const filteredUsers = useMemo(() => {
    const query = searchQuery.trim().toLowerCase();
    return users.filter((user) => {
      const matchesQuery = query.length === 0 || [user.name, user.email, user.role].some((value) => value?.toLowerCase().includes(query));
      const matchesRole = roleFilter === "all" || user.role === roleFilter;
      return matchesQuery && matchesRole;
    });
  }, [users, searchQuery, roleFilter]);

  useEffect(() => { void load(); }, []);
  const date = (value: string | null | undefined) => value ? new Date(value).toLocaleString() : "Never";
  const runSync = async () => { await itAdminService.runSynchronization({ syncType: "Full", scopeName: "All Projects", source: "Manual" }); await load(); };
  const title = { health: "Application Health", users: "Users", audit: "Audit Logs", monitoring: "Monitoring", maintenance: "Maintenance" }[section];

  if (section === "users") return (
    <div className="space-y-5">
      <SectionHeader
        title={title}
        subtitle="Live Microsoft Entra users synchronized by the IT Admin API"
        actions={
          <Btn variant="secondary" size="md" icon={RefreshCw} onClick={() => void refreshUsers()} className="font-semibold px-5 py-3 hover:bg-slate-100">Refresh</Btn>
        }
      />

      <div className="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
        <div className="flex-1 min-w-0">
          <label htmlFor="user-search" className="sr-only">Search users</label>
          <div className="flex items-center gap-2 rounded-xl border border-gray-200 bg-white px-3 py-2 shadow-sm">
            <Search size={14} className="text-gray-400" />
            <input
              id="user-search"
              type="search"
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              placeholder="Search by name, email or role"
              className="min-w-0 flex-1 bg-transparent text-sm text-gray-700 outline-none placeholder:text-gray-400"
            />
          </div>
        </div>

        <div className="flex items-center gap-2">
          <label htmlFor="role-filter" className="text-xs font-semibold text-gray-600">Role</label>
          <select
            id="role-filter"
            value={roleFilter}
            onChange={(e) => setRoleFilter(e.target.value)}
            className="rounded-xl border border-gray-200 bg-white px-3 py-2 text-sm text-gray-700 outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-100"
          >
            <option value="all">All roles</option>
            <option value="Administrator">Administrator</option>
            <option value="ITAdmin">ITAdmin</option>
            <option value="ProjectManager">ProjectManager</option>
           
          </select>
        </div>
      </div>

      {refreshStatus && (
        <div className={`rounded-xl border px-4 py-3 text-sm font-medium ${refreshStatus.type === "success" ? "border-emerald-100 bg-emerald-50 text-emerald-700" : "border-red-100 bg-red-50 text-red-700"}`}>
          {refreshStatus.message}
        </div>
      )}

      <Card padding="p-0">
        <div className="overflow-x-auto">
          <table className="min-w-full border-separate border-spacing-0 text-[14px]">
            <thead>
              <tr className="bg-slate-50 text-left text-[15px] uppercase tracking-wider text-slate-600">
                {['User', 'Email', 'Role', 'Last Login', 'Status'].map((h) => (
                  <th key={h} className="px-6 py-4 font-bold">{h}</th>
                ))}
              </tr>
            </thead>
            <tbody>
              {loading ? (
                <tr>
                  <td colSpan={5} className="px-6 py-8 text-center text-sm text-gray-500">Loading users...</td>
                </tr>
              ) : filteredUsers.length > 0 ? (
                filteredUsers.map((user) => (
                  <tr key={user.userId} className="border-b border-gray-100 hover:bg-slate-50 transition-colors">
                    <td className="px-6 py-4 font-semibold text-slate-900">{user.name}</td>
                    <td className="px-6 py-4 text-slate-600">{user.email}</td>
                    <td className="px-6 py-4">
                      <span className={`inline-flex rounded-full px-2.5 py-1 text-xs font-semibold ${roleBadgeClass(user.role)}`}>
                        {user.role}
                      </span>
                    </td>
                    <td className="px-6 py-4 text-slate-600">{date(user.lastLoginUtc)}</td>
                    <td className="px-6 py-4">
                      <StatusBadge status={(user.lastLoginUtc ? 'active' : 'inactive').toLowerCase()} />
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={5} className="px-6 py-8 text-center text-sm text-gray-500">No users match the current search or filter.</td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </Card>
    </div>
  );
  if (section === "audit") return (
    <div className="space-y-5">
      <SectionHeader title={title} subtitle="Live login and logout audit events" actions={<Btn variant="secondary" size="md" icon={RefreshCw} onClick={() => void refreshAuditLogs()} className="font-semibold px-5 py-3 hover:bg-slate-100">Refresh</Btn>} />
      {refreshStatus && (
        <div className={`rounded-xl border px-4 py-3 text-sm font-medium ${refreshStatus.type === "success" ? "border-emerald-100 bg-emerald-50 text-emerald-700" : "border-red-100 bg-red-50 text-red-700"}`}>
          {refreshStatus.message}
        </div>
      )}
      <Card padding="p-0">
        <div className="p-5 border-b border-gray-100 space-y-3 md:space-y-0 md:flex md:items-center md:justify-between gap-3">
          <div className="flex-1 min-w-0 flex items-center gap-2 rounded-xl border border-gray-200 bg-gray-50 px-3 py-2">
            <Search size={14} className="text-gray-400" />
            <input
              value={auditSearchQuery}
              onChange={(e) => setAuditSearchQuery(e.target.value)}
              placeholder="Search by user or IP..."
              className="flex-1 text-sm outline-none bg-transparent placeholder:text-gray-400 text-gray-700"
            />
          </div>
          <div className="flex flex-col gap-2 sm:flex-row sm:items-center">
            <label htmlFor="audit-user-filter" className="text-sm font-semibold text-gray-600">Filter by user</label>
            <select
              id="audit-user-filter"
              value={auditUserFilter}
              onChange={(e) => setAuditUserFilter(e.target.value)}
              className="rounded-xl border border-gray-200 bg-white px-3 py-2 text-sm text-gray-700 outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-100"
            >
              <option value="">All users</option>
              {auditUsers.map((user) => <option key={user} value={user}>{user}</option>)}
            </select>
          </div>
        </div>
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-gray-50 bg-slate-50 text-left text-[15px] uppercase tracking-wider text-slate-600">
              {["Timestamp", "User", "IP Address"].map(h => (
                <th key={h} className="px-5 py-4 font-bold">{h}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {filteredAuditLogs.map((audit) => (
              <tr key={audit.auditId} className="border-b border-gray-50 hover:bg-slate-50 transition-colors">
                <td className="px-5 py-4 text-slate-600">{date(audit.loginTime)}</td>
                <td className="px-5 py-4 text-slate-700">{audit.userName || audit.email}</td>
                <td className="px-5 py-4 text-slate-500">{audit.ipAddress || "-"}</td>
              </tr>
            ))}
            {!loading && filteredAuditLogs.length === 0 && (
              <tr>
                <td colSpan={3} className="px-5 py-8 text-center text-sm text-gray-500">No audit events found.</td>
              </tr>
            )}
          </tbody>
        </table>
      </Card>
    </div>
  );
  if (section === "maintenance") return <div className="space-y-5"><SectionHeader title={title} subtitle="Live maintenance tasks and scheduler state" actions={<Btn variant="secondary" size="md" icon={RefreshCw} onClick={() => void refreshMaintenance()} className="font-semibold px-5 py-3 hover:bg-slate-100">Refresh</Btn>} />{refreshStatus && (<div className={`rounded-xl border px-4 py-3 text-sm font-medium ${refreshStatus.type === "success" ? "border-emerald-100 bg-emerald-50 text-emerald-700" : "border-red-100 bg-red-50 text-red-700"}`}>{refreshStatus.message}</div>)}<div className="grid grid-cols-2 lg:grid-cols-3 gap-4"><KpiCard title="Active Tasks" value={maintenance?.activeTasks ?? 0} change="Live" changeType="stable" icon={Database} color="bg-blue-600" /><KpiCard title="Scheduled This Week" value={maintenance?.scheduledThisWeek ?? 0} change="Live" changeType="stable" icon={Calendar} color="bg-green-600" /><KpiCard title="Overdue Tasks" value={maintenance?.overdueTasks ?? 0} change="Live" changeType="down" icon={AlertTriangle} color="bg-red-500" /></div><Card padding="p-0"><table className="w-full text-xs"><thead><tr className="border-b border-gray-50">{["Task", "Environment", "Owner", "Status", "Scheduled"].map(h => <th key={h} className="text-left px-5 py-3 text-gray-400">{h}</th>)}</tr></thead><tbody>{(maintenance?.items ?? []).map(task => <tr key={task.id} className="border-b border-gray-50"><td className="px-5 py-3 font-semibold">{task.title}</td><td className="px-5 py-3">{task.environmentName}</td><td className="px-5 py-3">{task.ownerName}</td><td className="px-5 py-3"><StatusBadge status={task.status.toLowerCase()} /></td><td className="px-5 py-3">{date(task.scheduledStartUtc)}</td></tr>)}</tbody></table></Card></div>;

  const eventRows = [...(logs?.items ?? []).map(item => ({ time: item.createdAtUtc, type: item.category, message: item.message, status: item.severity })), ...(monitoring?.recentJobs ?? []).map(job => ({ time: job.startedAtUtc, type: "Azure DevOps Sync", message: `${job.scopeName} ${job.status}`, status: job.status }))].sort((a, b) => new Date(b.time).getTime() - new Date(a.time).getTime());
  if (section === "monitoring") return <div className="space-y-5"><SectionHeader title={title} subtitle="Live Azure DevOps synchronization and platform monitoring" actions={<Btn variant="secondary" size="md" icon={RefreshCw} onClick={() => void refreshMonitoring()} className="font-semibold px-5 py-3 hover:bg-slate-100">Refresh</Btn>} />{refreshStatus && (<div className={`rounded-xl border px-4 py-3 text-sm font-medium ${refreshStatus.type === "success" ? "border-emerald-100 bg-emerald-50 text-emerald-700" : "border-red-100 bg-red-50 text-red-700"}`}>{refreshStatus.message}</div>)}<div className="grid grid-cols-2 lg:grid-cols-4 gap-4"><KpiCard title="Failed Jobs" value={monitoring?.failedJobs ?? 0} change="Live" changeType="down" icon={XCircle} color="bg-red-500" /><KpiCard title="Warnings" value={monitoring?.warningLogs ?? 0} change="Live" changeType="stable" icon={AlertTriangle} color="bg-amber-500" /><KpiCard title="Sync Jobs Today" value={monitoring?.totalJobsToday ?? 0} change="Live" changeType="stable" icon={RefreshCw} color="bg-blue-600" /><KpiCard title="Database" value={health?.databaseReachable ? "Healthy" : "Unavailable"} change="Live" changeType={health?.databaseReachable ? "up" : "down"} icon={Database} color="bg-purple-600" /></div><Card padding="p-0"><table className="w-full text-xs"><thead><tr className="border-b border-gray-50">{["Timestamp", "Type", "Event", "Status"].map(h => <th key={h} className="text-left px-5 py-3 text-gray-400">{h}</th>)}</tr></thead><tbody>{eventRows.slice(0, 100).map((event, index) => <tr key={`${event.time}-${index}`} className="border-b border-gray-50"><td className="px-5 py-3">{date(event.time)}</td><td className="px-5 py-3">{event.type}</td><td className="px-5 py-3">{event.message}</td><td className="px-5 py-3"><StatusBadge status={event.status.toLowerCase().includes("fail") || event.status.toLowerCase().includes("error") ? "failed" : "success"} /></td></tr>)}</tbody></table></Card></div>;

  return <div className="space-y-5"><SectionHeader title={title} subtitle="Live Azure DevOps integration and application status" actions={<div className="flex gap-2"><Btn variant="secondary" icon={Terminal} onClick={() => onNavigate("/audit-logs")}>Audit Logs</Btn><Btn variant="primary" icon={RefreshCw} onClick={() => void runSync()}>Manual Sync</Btn></div>} />{error && <div className="text-sm text-red-600">{error}</div>}<div className="grid grid-cols-2 lg:grid-cols-5 gap-4"><KpiCard title="Azure DevOps" value={health?.azureDevOpsConfigured ? "Connected" : "Not configured"} change="Live" changeType={health?.azureDevOpsConfigured ? "up" : "down"} icon={GitBranch} color="bg-blue-600" /><KpiCard title="Database" value={health?.databaseReachable ? "Online" : "Offline"} change="Live" changeType={health?.databaseReachable ? "up" : "down"} icon={Database} color="bg-green-600" /><KpiCard title="Sync Service" value={health?.synchronizationEnabled ? "Enabled" : "Disabled"} change="Live" changeType="stable" icon={RefreshCw} color="bg-purple-600" /><KpiCard title="Running Jobs" value={health?.runningSynchronizationJobs ?? 0} change="Live" changeType="stable" icon={Activity} color="bg-indigo-500" /><KpiCard title="Failed (24h)" value={synchronization?.failedRuns24h ?? 0} change="Live" changeType="down" icon={AlertTriangle} color="bg-amber-500" /></div><Card padding="p-0"><div className="p-5 border-b border-gray-100"><SectionHeader title="Azure DevOps Synchronization" subtitle={loading ? "Loading live data..." : `Last successful sync: ${date(synchronization?.lastSuccessfulSyncUtc)}`} /></div><table className="w-full text-xs"><thead><tr className="border-b border-gray-50">{["Scope", "Trigger", "Started", "Items", "Status"].map(h => <th key={h} className="text-left px-5 py-3 text-gray-400">{h}</th>)}</tr></thead><tbody>{(synchronization?.recentJobs ?? []).map(job => <tr key={job.jobId} className="border-b border-gray-50"><td className="px-5 py-3 font-semibold">{job.scopeName}</td><td className="px-5 py-3">{job.source}</td><td className="px-5 py-3">{date(job.startedAtUtc)}</td><td className="px-5 py-3">{job.itemsProcessed}</td><td className="px-5 py-3"><StatusBadge status={job.status.toLowerCase()} /></td></tr>)}</tbody></table></Card><Card><SectionHeader title="Scheduler" subtitle={`${scheduling?.enabledSchedules ?? 0} enabled schedules · ${scheduling?.overdueSchedules ?? 0} overdue`} /></Card></div>;
};

const ITMonitoringPage = () => (
  <div className="space-y-6" style={{ fontFamily: "Inter, sans-serif" }}>
    <div>
      <h2 className="text-xl font-bold text-gray-900">Monitoring</h2>
      <p className="text-sm text-gray-500">Failed jobs, alerts and error logs</p>
    </div>
    <div className="grid grid-cols-2 lg:grid-cols-3 gap-4">
      <KpiCard title="Failed Jobs" value="2" change="Needs retry" changeType="down" icon={XCircle} color="bg-red-500" />
      <KpiCard title="Alerts" value="7" change="3 critical" changeType="down" icon={Bell} color="bg-amber-500" />
      <KpiCard title="Error Logs" value="5" change="Last hour" changeType="down" icon={AlertCircle} color="bg-red-600" />
    </div>
    <Card padding="p-0">
      <div className="p-5 border-b border-gray-100"><SectionHeader title="Monitoring Events" subtitle="Operational incidents and retry status" /></div>
      <table className="w-full text-xs">
        <thead><tr className="border-b border-gray-50">{["Time", "Type", "Component", "Message", "Status"].map(h => <th key={h} className="text-left px-5 py-3 text-gray-400 font-semibold">{h}</th>)}</tr></thead>
        <tbody>
          {[
            ["10:45", "Failed Job", "Sync Scheduler", "FinBank retry exhausted", "failed"],
            ["10:30", "Alert", "API Gateway", "Latency above threshold", "warning"],
            ["10:12", "Error", "Azure OpenAI", "Transient token refresh failure", "warning"],
          ].map(row => <tr key={row.join("-")} className="border-b border-gray-50 hover:bg-gray-50">{row.map((cell, i) => <td key={i} className="px-5 py-3 text-gray-600">{i === 4 ? <StatusBadge status={cell} /> : cell}</td>)}</tr>)}
        </tbody>
      </table>
    </Card>
  </div>
);

const ITMaintenancePage = () => (
  <div className="space-y-6" style={{ fontFamily: "Inter, sans-serif" }}>
    <div>
      <h2 className="text-xl font-bold text-gray-900">Maintenance</h2>
      <p className="text-sm text-gray-500">Database backup, Azure OpenAI status, cache, scheduler and diagnostics</p>
    </div>
    <div className="grid grid-cols-1 lg:grid-cols-3 gap-5">
      {[
        ["Database Backup", Database, "Last backup completed today at 02:00 IST.", "Run Backup"],
        ["Azure OpenAI Status", Brain, "Model endpoint online with normal token latency.", "Test Endpoint"],
        ["Cache", HardDrive, "Cache hit rate is 91%; no purge required.", "Clear Cache"],
        ["Scheduler", Activity, "8 jobs active; 1 retry policy pending review.", "Open Scheduler"],
        ["Diagnostics", Terminal, "Generate support bundle for API, sync and AI logs.", "Run Diagnostics"],
      ].map(([title, Icon, desc, action]: any) => (
        <Card key={title}>
          <div className="w-10 h-10 bg-blue-50 rounded-xl flex items-center justify-center mb-3"><Icon size={17} className="text-blue-600" /></div>
          <div className="text-sm font-bold text-gray-900 mb-1">{title}</div>
          <p className="text-xs text-gray-500 leading-relaxed mb-4">{desc}</p>
          <Btn variant="secondary">{action}</Btn>
        </Card>
      ))}
    </div>
  </div>
);

const RoleSummaryDashboard = ({ role }: { role: Role }) => {
  const sections: Record<Role, string[]> = {
    Administrator: ["Portfolio Summary", "Delivery KPIs", "Risk Center"],
    "Project Manager": ["Assigned Delivery", "Sprint Governance", "AI Insights"],
    Executive: ["Portfolio Summary", "Delivery Health", "Business KPIs", "Executive Summary", "High Risks", "Critical Projects", "Forecast", "Recommendations", "Reports"],
    PMO: ["Cross Project KPIs", "Governance", "Delivery Trends", "Risk Heatmap", "Resource Trends", "Portfolio Analytics"],
    "Scrum Master": ["Sprint Progress", "Sprint Board", "Velocity", "Burndown", "Burnup", "Sprint Risks", "Blocked Stories", "Daily Progress"],
    "IT Manager": ["Application Health", "Azure DevOps Integration", "Audit Logs", "User Administration", "Monitoring", "Maintenance"],
  };
  return (
    <div className="space-y-6" style={{ fontFamily: "Inter, sans-serif" }}>
      <div>
        <h2 className="text-xl font-bold text-gray-900">{role} Dashboard</h2>
        <p className="text-sm text-gray-500">Role-specific governance workspace</p>
      </div>
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <MetricCard label="Delivery Health" value="76%" icon={Award} tone="green" />
        <MetricCard label="Critical Projects" value="2" icon={AlertTriangle} tone="red" />
        <MetricCard label="Velocity" value="40" icon={TrendingUp} tone="blue" />
        <MetricCard label="Forecast" value="82%" icon={Activity} tone="purple" />
      </div>
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-5">
        <Card className="lg:col-span-2">
          <SectionHeader title={sections[role][0]} subtitle={sections[role].slice(1, 4).join(" · ")} />
          <ResponsiveContainer width="100%" height={220}>
            <LineChart data={KPI_TREND}>
              <CartesianGrid strokeDasharray="3 3" stroke="#F1F5F9" /><XAxis dataKey="month" tick={{ fontSize: 11, fill: "#94A3B8" }} axisLine={false} tickLine={false} /><YAxis tick={{ fontSize: 11, fill: "#94A3B8" }} axisLine={false} tickLine={false} /><Tooltip content={<Tooltip_ />} /><Legend wrapperStyle={{ fontSize: 11 }} />
              <Line type="monotone" dataKey="sprintRate" name="Sprint Completion" stroke={C.blue} strokeWidth={2} />
              <Line type="monotone" dataKey="buildSuccess" name="Release Success" stroke={C.green} strokeWidth={2} />
            </LineChart>
          </ResponsiveContainer>
        </Card>
        <Card>
          <SectionHeader title="Recommendations" />
          <div className="space-y-3">{sections[role].slice(0, 5).map((s, i) => <AIInsightCard key={s} type={i === 1 ? "warning" : "info"} title={s} insight={`Review ${s.toLowerCase()} with current project telemetry and AI guidance.`} />)}</div>
        </Card>
      </div>
    </div>
  );
};

const ITManagerWorkspace = ({ onNavigate, syncJobs = [] }: { onNavigate: (s: string) => void; syncJobs?: SyncJobRecord[] }) => (
  <div className="space-y-6" style={{ fontFamily: "Inter, sans-serif" }}>
    <div className="flex items-center justify-between">
      <div>
        <h2 className="text-xl font-bold text-gray-900">Application Health</h2>
        <p className="text-sm text-gray-500">Operational administration, integrations, logs and maintenance</p>
      </div>
      <div className="flex gap-2">
        <Btn variant="secondary" icon={Terminal} onClick={() => onNavigate("/audit-logs")}>Audit Logs</Btn>
        <Btn variant="primary" icon={RefreshCw} onClick={() => onNavigate("/azure-devops")}>Sync Monitor</Btn>
      </div>
    </div>

    <div className="grid grid-cols-2 lg:grid-cols-5 gap-4">
      <KpiCard title="Azure SQL Status" value="82%" change="Healthy" changeType="up" icon={Database} color="bg-blue-600" />
      <KpiCard title="API Status" value="99.8%" change="Online" changeType="up" icon={Wifi} color="bg-green-600" />
      <KpiCard title="Memory" value="68%" change="Normal" changeType="stable" icon={Cpu} color="bg-purple-600" />
      <KpiCard title="CPU" value="54%" change="Normal" changeType="stable" icon={Gauge} color="bg-indigo-500" />
      <KpiCard title="Storage" value="72%" change="Watch" changeType="stable" icon={HardDrive} color="bg-amber-500" />
    </div>

    <div className="grid grid-cols-1 lg:grid-cols-3 gap-5">
      <Card className="lg:col-span-2" padding="p-0">
        <div className="p-5 border-b border-gray-100 flex items-center justify-between">
          <div>
            <h3 className="text-sm font-semibold text-gray-900">Azure DevOps Integration</h3>
            <p className="text-xs text-gray-400">Connection status, PAT validation and synchronization jobs</p>
          </div>
          <div className="flex gap-2">
            <Btn variant="secondary" icon={Key}>Validate PAT</Btn>
            <Btn variant="primary" icon={RefreshCw}>Manual Sync</Btn>
          </div>
        </div>
        <div className="overflow-x-auto">
          <table className="w-full text-xs">
            <thead><tr className="border-b border-gray-50">
              {["Connection", "Organization", "Last Sync", "Items", "Errors", "Status", "Action"].map(h => <th key={h} className="text-left px-5 py-3 text-gray-400 font-semibold">{h}</th>)}
            </tr></thead>
            <tbody>
              {syncJobs.map(job => (
                <tr key={job.org + job.project} className="border-b border-gray-50 hover:bg-gray-50">
                  <td className="px-5 py-3 font-semibold text-gray-800">{job.project}</td>
                  <td className="px-5 py-3 text-gray-500">{job.org}</td>
                  <td className="px-5 py-3 text-gray-500">{job.lastSync}</td>
                  <td className="px-5 py-3 font-mono text-gray-700">{job.items}</td>
                  <td className="px-5 py-3 font-mono text-red-500">{job.errors}</td>
                  <td className="px-5 py-3"><StatusBadge status={job.status} /></td>
                  <td className="px-5 py-3"><button className="text-blue-600 font-semibold flex items-center gap-1"><RotateCcw size={12} />Retry</button></td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </Card>

      <Card>
        <SectionHeader title="Sync History" subtitle="Recent synchronization logs" />
        <div className="space-y-2">
          {syncJobs.slice(0, 5).map(job => <SyncStatusCard key={job.project} job={job} />)}
        </div>
      </Card>
    </div>

    <div className="grid grid-cols-1 lg:grid-cols-3 gap-5">
      <Card>
        <SectionHeader title="Audit Logs" subtitle="Login, user, admin, sync, AI and API logs"
          actions={<button onClick={() => onNavigate("/audit-logs")} className="text-xs text-blue-600 font-semibold flex items-center gap-1">Open <ArrowRight size={11} /></button>} />
        <div className="grid grid-cols-2 gap-2">
          {["Login Logs", "Logout Logs", "User Activity", "Admin Actions", "Synchronization Logs", "AI Logs", "API Logs"].map((label, i) => (
            <div key={label} className="bg-gray-50 rounded-xl p-3">
              <div className="text-xs font-semibold text-gray-800">{label}</div>
              <div className="text-[10px] text-gray-400 mt-1">{[18, 12, 44, 9, 21, 16, 33][i]} events</div>
            </div>
          ))}
        </div>
      </Card>

      <Card>
        <SectionHeader title="User Administration" subtitle="Access support actions"
          actions={<button onClick={() => onNavigate("/users")} className="text-xs text-blue-600 font-semibold flex items-center gap-1">Manage <ArrowRight size={11} /></button>} />
        <div className="grid grid-cols-2 gap-2">
          <Btn variant="secondary" icon={Users}>Users</Btn>
          <Btn variant="secondary" icon={Key}>Roles</Btn>
          <Btn variant="secondary" icon={RotateCcw}>Reset Access</Btn>
          <Btn variant="danger" icon={Lock}>Disable User</Btn>
          <Btn variant="secondary" icon={Clock} className="col-span-2 justify-center">Login History</Btn>
        </div>
      </Card>

      <Card>
        <SectionHeader title="Monitoring" subtitle="Failed jobs, alerts and error logs" />
        <div className="space-y-2">
          {[
            { label: "Failed Jobs", value: "2", icon: XCircle, tone: "red" as const },
            { label: "Alerts", value: "7", icon: Bell, tone: "amber" as const },
            { label: "Notifications", value: "14", icon: Mail, tone: "blue" as const },
            { label: "Error Logs", value: "5", icon: AlertCircle, tone: "red" as const },
          ].map(item => <MetricCard key={item.label} label={item.label} value={item.value} icon={item.icon} tone={item.tone} />)}
        </div>
      </Card>
    </div>

    <Card>
      <SectionHeader title="Maintenance" subtitle="Database backup, Azure OpenAI status, cache, scheduler and diagnostics" />
      <div className="grid grid-cols-2 lg:grid-cols-5 gap-2">
        <Btn variant="secondary" icon={Database}>Database Backup</Btn>
        <Btn variant="secondary" icon={Brain}>Azure OpenAI Status</Btn>
        <Btn variant="secondary" icon={HardDrive}>Cache</Btn>
        <Btn variant="secondary" icon={Activity}>Scheduler</Btn>
        <Btn variant="secondary" icon={Terminal}>Diagnostics</Btn>
      </div>
    </Card>
  </div>
);

// ──────────────────────────────────────────────
// MAIN APP
// ──────────────────────────────────────────────
const useLocationPath = () => {
  const [path, setPath] = useState(() => window.location.pathname || "/");
  useEffect(() => {
    const onPop = () => setPath(window.location.pathname || "/");
    window.addEventListener("popstate", onPop);
    return () => window.removeEventListener("popstate", onPop);
  }, []);
  const navigate = (nextPath: string) => {
    if (window.location.pathname !== nextPath) window.history.pushState({}, "", nextPath);
    setPath(nextPath);
  };
  return { path, navigate };
};

const LEGACY_ROUTE_MAP: Record<string, string> = {
  "org-admin": "/portfolio",
  "pm-dashboard": "/my-projects",
  "it-admin": "/application-health",
  portfolio: "/portfolio",
  "project-details": "/projects/1",
  sprint: "/sprint-governance",
  kpi: "/delivery-kpis",
  "ai-risk": "/risk-center",
  "ai-reports": "/executive-reports",
  "ai-insights": "/ai-insights",
  users: "/users",
  sync: "/azure-devops",
  audit: "/audit-logs",
  notifications: "/notifications",
};

const ProtectedRoute = ({ role, path, children }: { role: Role; path: string; children: React.ReactNode }) => {
  if (!canAccessPath(role, path)) {
    return (
      <Card>
        <div className="py-10 flex flex-col items-center justify-center text-center">
          <Lock size={28} className="text-red-400 mb-3" />
          <h3 className="text-lg font-semibold text-gray-800">Access restricted</h3>
          <p className="text-sm text-gray-400 mt-1">Your role does not have access to this area.</p>
        </div>
      </Card>
    );
  }
  return <>{children}</>;
};

const RoleLayout = ({ children }: { children: React.ReactNode }) => <>{children}</>;

export default function App({ embeddedRole, embeddedEmail, onEmbeddedLogout }: EmbeddedDashboardProps = {}) {
  const [loggedIn, setLoggedIn] = useState(() => Boolean(embeddedRole));
  const [email, setEmail] = useState(() => embeddedEmail ?? "");
  const [role, setRole] = useState<Role>(() => embeddedRole ?? "Administrator");
  const [collapsed, setCollapsed] = useState(false);
  const [liveProjects, setLiveProjects] = useState<ProjectRecord[] | null>(null);
  const [liveSyncJobs, setLiveSyncJobs] = useState<SyncJobRecord[] | null>(null);
  const [liveReportsOverview, setLiveReportsOverview] = useState<ReportsOverviewDto | null>(null);
  const [livePmSprintProgress, setLivePmSprintProgress] = useState<ProjectManagerSprintProgressDto | null>(null);
  const [livePmKpis, setLivePmKpis] = useState<ProjectManagerKpisDto | null>(null);
  const [livePmWorkItems, setLivePmWorkItems] = useState<ProjectManagerWorkItemDto[] | null>(null);
  const [liveDashboardKpis, setLiveDashboardKpis] = useState<DashboardKpiCalculationsDto | null>(null);
  const [liveAdminStats, setLiveAdminStats] = useState<AdminProjectStatisticsDto | null>(null);
  const [selectedAdminProjectId, setSelectedAdminProjectId] = useState<number | null>(null);
  const [selectedProjectDetails, setSelectedProjectDetails] = useState<AdminProjectDetailsDto | null>(null);
  const [selectedProjectDetailsError, setSelectedProjectDetailsError] = useState<string | null>(null);
  const [isProjectDetailsLoading, setIsProjectDetailsLoading] = useState(false);
  const [selectedDeliveryKpiProjectId, setSelectedDeliveryKpiProjectId] = useState<number | null>(null);
  const [liveAdminDeliveryKpiOverview, setLiveAdminDeliveryKpiOverview] = useState<ReportsOverviewDto | null>(null);
  const [liveAdminDeliveryKpiProjectDetails, setLiveAdminDeliveryKpiProjectDetails] = useState<AdminProjectDetailsDto | null>(null);
  const [selectedSprintGovernanceProjectId, setSelectedSprintGovernanceProjectId] = useState<number | null>(null);
  const [aiInsightsMessages, setAiInsightsMessages] = useState<AiChatMessage[]>([
    { role: "assistant", msg: "Hello! I'm Sarathi AI. Select an assigned project and ask about delivery health, sprint progress, blockers, risks, work items, or recommendations." },
  ]);
  const [aiInsightsSelectedProjectId, setAiInsightsSelectedProjectId] = useState<number | null>(null);
  const [liveAdminSprintGovernance, setLiveAdminSprintGovernance] = useState<ProjectSprintGovernanceDto | null>(null);
  const [selectedPmSprintGovernanceProjectId, setSelectedPmSprintGovernanceProjectId] = useState<number | null>(null);
  const [livePmSprintGovernance, setLivePmSprintGovernance] = useState<ProjectManagerSprintGovernanceDto | null>(null);
  const [isDeliveryKpiLoading, setIsDeliveryKpiLoading] = useState(false);
  const [azureOrgUrl, setAzureOrgUrl] = useState<string>("");
  const { path, navigate } = useLocationPath();

  const goTo = (target: string) => {
    const nextPath = LEGACY_ROUTE_MAP[target] ?? target;
    navigate(getSafePathForRole(role, nextPath));
  };

  const handleLogin = (userEmail: string, userRole: Role) => {
    setEmail(userEmail);
    setRole(userRole);
    setLoggedIn(true);
    navigate(ROLE_DEFAULT_SCREEN[userRole]);
  };

  const handleLogout = () => {
    if (onEmbeddedLogout) {
      onEmbeddedLogout();
      return;
    }

    setLoggedIn(false);
    setEmail("");
    setRole("Administrator");
    setAiInsightsMessages([
      { role: "assistant", msg: "Hello! I'm Sarathi AI. Select an assigned project and ask about delivery health, sprint progress, blockers, risks, work items, or recommendations." },
    ]);
    setAiInsightsSelectedProjectId(null);
    navigate("/");
  };

  const unread = NOTIFICATIONS.filter(n => !n.read).length;
  const safePath = getSafePathForRole(role, path);
  const routeKey = normalizePath(safePath);
  const projectId = safePath.startsWith("/projects/") ? Number.parseInt(safePath.split("/")[2] ?? "", 10) : null;
  const deliveryKpiProjectId = safePath.startsWith("/delivery-kpis/") ? Number.parseInt(safePath.split("/")[2] ?? "", 10) : null;

  useEffect(() => {
    if (loggedIn && safePath !== path) navigate(safePath);
  }, [loggedIn, safePath, path]);

  useEffect(() => {
    if (loggedIn && role === "Administrator" && routeKey === "/projects/:projectId" && projectId && !Number.isNaN(projectId)) {
      setSelectedAdminProjectId(projectId);
    }
  }, [loggedIn, projectId, role, routeKey]);

  useEffect(() => {
    if (loggedIn && role === "Administrator" && routeKey === "/delivery-kpis/:projectId" && deliveryKpiProjectId && !Number.isNaN(deliveryKpiProjectId)) {
      setSelectedDeliveryKpiProjectId(deliveryKpiProjectId);
    }
  }, [deliveryKpiProjectId, loggedIn, role, routeKey]);

  useEffect(() => {
    if (!loggedIn) {
      setLiveProjects(null);
      setLiveSyncJobs(null);
      setLiveReportsOverview(null);
      setLivePmSprintProgress(null);
      setLivePmKpis(null);
      setLivePmWorkItems(null);
      setLiveDashboardKpis(null);
      setLiveAdminStats(null);
      setSelectedAdminProjectId(null);
      setSelectedProjectDetails(null);
      setSelectedDeliveryKpiProjectId(null);
      setLiveAdminDeliveryKpiOverview(null);
      setLiveAdminDeliveryKpiProjectDetails(null);
      setSelectedSprintGovernanceProjectId(null);
      setLiveAdminSprintGovernance(null);
      setSelectedPmSprintGovernanceProjectId(null);
      setLivePmSprintGovernance(null);
      setIsDeliveryKpiLoading(false);
      return;
    }

    let isDisposed = false;

    const loadLiveData = async () => {
      const reportsResult = await reportsService.getOverview()
        .then((data) => ({ ok: true as const, data }))
        .catch(() => ({ ok: false as const, data: null }));

      if (!isDisposed) {
        setLiveReportsOverview(reportsResult.ok ? reportsResult.data : null);
      }

      try {
        if (role === "Administrator") {
          const [statistics, kpiData, config] = await Promise.all([
            adminService.getProjectStatistics(25),
            dashboardService.getKpiCalculations(),
            adminService.getConfiguration(),
          ]);
          if (!isDisposed) {
            setLiveProjects(mapAdminProjectsToCatalog(statistics.projects));
            setLiveAdminStats(statistics);
            setLiveDashboardKpis(kpiData);
            setAzureOrgUrl(config.azureDevOpsOrganizationUrl);
            setLiveSyncJobs(null);
            setLivePmSprintProgress(null);
            setLivePmKpis(null);
            setLivePmWorkItems(null);
            setSelectedAdminProjectId((current) => {
              if (routeKey === "/projects/:projectId" && projectId && !Number.isNaN(projectId)) {
                return projectId;
              }
              return current && statistics.projects.some((project) => project.projectId === current)
                ? current
                : statistics.projects[0]?.projectId ?? null;
            });
            setSelectedDeliveryKpiProjectId((current) => {
              if (routeKey === "/delivery-kpis/:projectId" && deliveryKpiProjectId && !Number.isNaN(deliveryKpiProjectId)) {
                return deliveryKpiProjectId;
              }
              return current && statistics.projects.some((project) => project.projectId === current)
                ? current
                : statistics.projects[0]?.projectId ?? null;
            });
            setSelectedSprintGovernanceProjectId((current) => current && statistics.projects.some((project) => project.projectId === current)
              ? current
              : statistics.projects[0]?.projectId ?? null);
          }
          return;
        }

        if (role === "Project Manager") {
          const [assignedProjects, sprintProgress, kpis, workItems] = await Promise.all([
            projectManagerService.getAssignedProjects(),
            projectManagerService.getSprintProgress(),
            projectManagerService.getKpis(),
            projectManagerService.getWorkItems({ take: 50 }),
          ]);

          if (!isDisposed) {
            setLiveProjects(mapPmProjectsToCatalog(assignedProjects, email));
            setLiveSyncJobs(null);
            setLivePmSprintProgress(sprintProgress);
            setLivePmKpis(kpis);
            setLivePmWorkItems(workItems.items);
            setSelectedPmSprintGovernanceProjectId((current) => current && assignedProjects.some((project) => project.projectId === current)
              ? current
              : assignedProjects[0]?.projectId ?? null);
          }
          return;
        }

        if (role === "IT Manager") {
          if (!isDisposed) {
            setLiveProjects(null);
            setLivePmSprintProgress(null);
            setLivePmKpis(null);
            setLivePmWorkItems(null);
          }
          const syncData = await itAdminService.getSynchronization();
          if (!isDisposed) {
            setLiveSyncJobs(mapItSyncJobs(syncData.recentJobs));
          }
          return;
        }
      } catch {
        if (!isDisposed) {
          setLiveProjects(null);
          setLiveSyncJobs(null);
          setLivePmSprintProgress(null);
          setLivePmKpis(null);
          setLivePmWorkItems(null);
        }
      }
    };

    void loadLiveData();

    return () => {
      isDisposed = true;
    };
  }, [loggedIn, role, email]);

  useEffect(() => {
    if (!loggedIn || role !== "Administrator" || routeKey !== "/projects/:projectId" || !selectedAdminProjectId) {
      if (routeKey !== "/projects/:projectId") {
        setSelectedProjectDetails(null);
        setSelectedProjectDetailsError(null);
        setIsProjectDetailsLoading(false);
      }
      return;
    }

    let isDisposed = false;
    setSelectedProjectDetails(null);
    setSelectedProjectDetailsError(null);
    setIsProjectDetailsLoading(true);

    const loadProjectDetails = async () => {
      try {
        const details = await adminService.getProjectDetails(selectedAdminProjectId);
        if (!isDisposed) {
          setSelectedProjectDetails(details);
          setSelectedProjectDetailsError(null);
        }
      } catch (error) {
        if (!isDisposed) {
          console.error('Failed to load project details:', error);
          const message = error instanceof Error
            ? error.message
            : 'Unable to load project details.';
          setSelectedProjectDetails(null);
          setSelectedProjectDetailsError(message);
        }
      } finally {
        if (!isDisposed) {
          setIsProjectDetailsLoading(false);
        }
      }
    };

    void loadProjectDetails();

    return () => {
      isDisposed = true;
    };
  }, [loggedIn, role, routeKey, selectedAdminProjectId]);

  useEffect(() => {
    if (!loggedIn || role !== "Administrator" || routeKey !== "/delivery-kpis/:projectId" || !selectedDeliveryKpiProjectId) {
      setLiveAdminDeliveryKpiOverview(null);
      setLiveAdminDeliveryKpiProjectDetails(null);
      setIsDeliveryKpiLoading(false);
      return;
    }

    let isDisposed = false;
    setIsDeliveryKpiLoading(true);

    const loadDeliveryKpis = async () => {
      try {
        const [overview, projectDetails] = await Promise.all([
          reportsService.getOverview(selectedDeliveryKpiProjectId),
          adminService.getProjectDetails(selectedDeliveryKpiProjectId),
        ]);

        if (!isDisposed) {
          setLiveAdminDeliveryKpiOverview(overview);
          setLiveAdminDeliveryKpiProjectDetails(projectDetails);
        }
      } catch {
        if (!isDisposed) {
          setLiveAdminDeliveryKpiOverview(null);
          setLiveAdminDeliveryKpiProjectDetails(null);
        }
      } finally {
        if (!isDisposed) {
          setIsDeliveryKpiLoading(false);
        }
      }
    };

    void loadDeliveryKpis();

    return () => {
      isDisposed = true;
    };
  }, [loggedIn, role, routeKey, selectedDeliveryKpiProjectId]);

  useEffect(() => {
    if (!loggedIn || role !== "Administrator" || routeKey !== "/sprint-governance" || !selectedSprintGovernanceProjectId) {
      setLiveAdminSprintGovernance(null);
      return;
    }

    let isDisposed = false;
    adminService.getSprintGovernance(selectedSprintGovernanceProjectId)
      .then((data) => { if (!isDisposed) setLiveAdminSprintGovernance(data); })
      .catch(() => { if (!isDisposed) setLiveAdminSprintGovernance(null); });

    return () => { isDisposed = true; };
  }, [loggedIn, role, routeKey, selectedSprintGovernanceProjectId]);

  useEffect(() => {
    if (!loggedIn || role !== "Project Manager" || routeKey !== "/sprint-governance" || !selectedPmSprintGovernanceProjectId) {
      setLivePmSprintGovernance(null);
      return;
    }

    let isDisposed = false;
    projectManagerService.getSprintGovernance(selectedPmSprintGovernanceProjectId)
      .then((data) => { if (!isDisposed) setLivePmSprintGovernance(data); })
      .catch(() => { if (!isDisposed) setLivePmSprintGovernance(null); });

    return () => { isDisposed = true; };
  }, [loggedIn, role, routeKey, selectedPmSprintGovernanceProjectId]);

  if (!loggedIn) {
    return <LoginScreen onLogin={handleLogin} />;
  }

  const handleProjectSelect = async (projectId: number) => {
    setSelectedAdminProjectId(projectId);
    goTo(`/projects/${projectId}`);
  };

  const handleOpenDeliveryKpiProject = (projectId: number) => {
    setSelectedDeliveryKpiProjectId(projectId);
    goTo(`/delivery-kpis/${projectId}`);
  };

  const handleSyncNow = async () => {
    try {
      await azureDevOpsService.queueSynchronization({
        syncType: 'Full',
        scopeName: 'All Projects',
        source: 'Manual'
      });

      // The background worker processes the queued Azure DevOps job; refresh the current view immediately.
      const statistics = await adminService.getProjectStatistics(25);
      setLiveProjects(mapAdminProjectsToCatalog(statistics.projects));
      setLiveAdminStats(statistics);

      if (selectedAdminProjectId) {
        try {
          const details = await adminService.getProjectDetails(selectedAdminProjectId);
          setSelectedProjectDetails(details);
        } catch (detailsError) {
          console.error('Failed to refresh project details after sync:', detailsError);
        }
      }
    } catch (error) {
      console.error('Failed to trigger sync:', error);
    }
  };

  const ROUTES: Record<string, React.ReactNode> = {
    "/portfolio": <PortfolioEnterpriseDashboard onNavigate={goTo} projects={liveProjects ?? []} dashboardKpis={liveDashboardKpis} adminStats={liveAdminStats} />,
    "/projects": <ProjectsPage onNavigate={goTo} projects={liveProjects ?? []} onProjectSelect={handleProjectSelect} onSyncNow={handleSyncNow} azureOrgUrl={azureOrgUrl} />,
    "/projects/:projectId": <ProjectDetails projectDetails={selectedProjectDetails} azureOrgUrl={azureOrgUrl} onSyncNow={handleSyncNow} isLoading={isProjectDetailsLoading} error={selectedProjectDetailsError} />,
    "/my-projects": <MyProjectsPage email={email} onNavigate={goTo} projectsOverride={liveProjects ?? undefined} />,
    "/executive": <RoleSummaryDashboard role="Executive" />,
    "/pmo": <RoleSummaryDashboard role="PMO" />,
    "/scrum": <RoleSummaryDashboard role="Scrum Master" />,
    "/application-health": <ITLiveOperationsPage section="health" onNavigate={goTo} />,
    "/delivery-kpis": role === "Administrator"
      ? <DeliveryKpiProjectsPage projects={liveProjects ?? []} onOpenProject={handleOpenDeliveryKpiProject} />
      : <KPIDashboard
          reportsOverview={liveReportsOverview}
          pmKpis={livePmKpis}
          projects={liveProjects ?? undefined}
          isAdministrator={false}
          isLoading={false}
        />,
    "/delivery-kpis/:projectId": <KPIDashboard
      reportsOverview={liveAdminDeliveryKpiOverview}
      pmKpis={null}
      projects={liveProjects ?? []}
      selectedProjectId={selectedDeliveryKpiProjectId}
      selectedProjectDetails={liveAdminDeliveryKpiProjectDetails}
      onNavigate={goTo}
      isAdministrator
      isLoading={isDeliveryKpiLoading}
    />,
    "/sprint-governance": <SprintGovernance reportsOverview={liveReportsOverview} pmSprintProgress={livePmSprintProgress} pmWorkItems={livePmWorkItems} adminGovernance={role === "Administrator" ? liveAdminSprintGovernance : livePmSprintGovernance} projects={role === "Administrator" || role === "Project Manager" ? liveProjects ?? [] : []} selectedProjectId={role === "Administrator" ? selectedSprintGovernanceProjectId : selectedPmSprintGovernanceProjectId} onProjectSelect={role === "Administrator" ? setSelectedSprintGovernanceProjectId : role === "Project Manager" ? setSelectedPmSprintGovernanceProjectId : undefined} />,
    "/risk-center": <AIRiskAnalysis projects={liveProjects ?? []} reportsOverview={liveReportsOverview} projectScopeLabel={role === "Project Manager" ? "assigned projects" : "projects"} />,
    "/pm-risk-analysis": <ProjectManagerRiskAnalysisPage email={email} onNavigate={goTo} projectsOverride={liveProjects ?? undefined} />,
    "/ai-insights": <AIInsights
      projects={role === "Project Manager" ? liveProjects ?? [] : []}
      messages={aiInsightsMessages}
      onMessagesChange={setAiInsightsMessages}
      selectedProjectId={aiInsightsSelectedProjectId}
      onSelectedProjectIdChange={setAiInsightsSelectedProjectId}
    />,
    "/sprint-board": <SprintGovernance />,
    "/velocity": <KPIDashboard
      reportsOverview={role === "Administrator" ? liveAdminDeliveryKpiOverview : liveReportsOverview}
      pmKpis={livePmKpis}
      projects={role === "Administrator" ? liveProjects ?? [] : undefined}
      selectedProjectId={selectedDeliveryKpiProjectId}
      selectedProjectDetails={liveAdminDeliveryKpiProjectDetails}
      onNavigate={goTo}
      isAdministrator={role === "Administrator"}
      isLoading={isDeliveryKpiLoading}
    />,
    "/burndown": <ProjectDetails />,
    "/executive-reports": <AIExecutiveReports projects={liveProjects ?? []} reportsOverview={liveReportsOverview} isAdministrator={role === "Administrator"} />,
    "/reports": <AIExecutiveReports projects={liveProjects ?? []} reportsOverview={liveReportsOverview} isAdministrator={role === "Administrator"} />,
    "/ai-summary": <AIInsights
      messages={aiInsightsMessages}
      onMessagesChange={setAiInsightsMessages}
      selectedProjectId={aiInsightsSelectedProjectId}
      onSelectedProjectIdChange={setAiInsightsSelectedProjectId}
      projects={role === "Project Manager" ? liveProjects ?? [] : []}
    />,
    "/governance": <RoleSummaryDashboard role="PMO" />,
    "/azure-devops": <SyncMonitor />,
    "/users": <ITLiveOperationsPage section="users" onNavigate={goTo} />,
    "/audit-logs": <ITLiveOperationsPage section="audit" onNavigate={goTo} />,
    "/monitoring": <ITLiveOperationsPage section="monitoring" onNavigate={goTo} />,
    "/notifications": <NotificationsScreen />,
  };

  return (
    <div className="flex h-screen overflow-hidden bg-slate-100" style={{ fontFamily: "Inter, sans-serif" }}>
      <Sidebar currentScreen={safePath} onNavigate={goTo} role={role} collapsed={collapsed} onToggle={() => setCollapsed(!collapsed)} />
      <div className="flex-1 flex flex-col min-w-0 overflow-hidden">
        <TopNav currentScreen={routeKey} role={role} email={email} unreadCount={unread} onNavigate={goTo} onLogout={handleLogout} />
        <main className="flex-1 overflow-y-auto p-6">
          <motion.div
            key={safePath}
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.22, ease: "easeOut" }}
          >
            <RoleLayout>
              <ProtectedRoute role={role} path={safePath}>
                {ROUTES[routeKey] ?? ROUTES[normalizePath(ROLE_DEFAULT_SCREEN[role])]}
              </ProtectedRoute>
            </RoleLayout>
          </motion.div>
        </main>
      </div>
    </div>
  );
}
