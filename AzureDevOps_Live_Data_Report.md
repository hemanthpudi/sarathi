# Azure DevOps Live Data Report

Generated: 2026-07-21

## Verified sync result

- Azure DevOps sync job `4` completed successfully.
- Persisted live data after sync:
  - `Projects`: 5
  - `Sprints`: 20
  - `WorkItems`: 496
  - `KPISnapshots`: 5
  - `RiskAnalysis`: 5
  - `ProjectManagerAssignments`: 0

## Implemented backend changes

- Added Azure DevOps client support for documented `Projects`, `Teams`, `Sprints`, `WorkItems`, `Builds`, and `Releases` APIs.
- Completed work item mapping to include story points, effort, remaining work, created date, changed date, and closed date support in the client.
- Completed synchronization flow so it now derives and persists:
  - sprint aggregates
  - KPI snapshots
  - risk analysis
- Fixed reports API access so the frontend `Reports` route is no longer blocked for non-admin users.
- Added role-sensitive reports filtering for `ProjectManager` users based on assigned projects.

## Component status

| Dashboard component | Backend endpoint | Azure DevOps API used | Calculation performed | Status |
|---|---|---|---|---|
| Admin Configure: KPI thresholds | `/api/admin/configure` | None | None | Completed |
| Admin Configure: Azure DevOps configuration | `/api/integrations/azure-devops/configuration` | None | None | Completed |
| Admin Project Statistics: summary cards | `/api/admin/project-statistics` | Projects, Teams, Sprints, WorkItems (WIQL + workitems), Builds, Releases | Total projects, public/private split, high risk projects, avg completion rate, avg sprint velocity | Completed |
| Admin Project Statistics: project table | `/api/admin/project-statistics` | Projects, Teams, Sprints, WorkItems (WIQL + workitems), Builds, Releases | Completion rate, sprint velocity, defect density, risk score, risk level by project | Completed |
| PM Dashboard: KPI cards | `/api/project-manager/dashboard` | Projects, Teams, Sprints, WorkItems (WIQL + workitems), Builds, Releases | Assigned projects, active sprints, open work items, blocked work items, avg completion rate, avg sprint velocity | Pending |
| PM Dashboard: Sprint Velocity Trend chart | `/api/project-manager/dashboard` | Projects, Teams, Sprints, WorkItems (WIQL + workitems) | Velocity trend from KPI snapshots | Pending |
| PM Dashboard: Work Items by State chart | `/api/project-manager/dashboard` | WorkItems (WIQL + workitems) | State distribution by assigned project scope | Pending |
| PM Dashboard: Spotlight Projects table | `/api/project-manager/dashboard` | Projects, Teams, Sprints, WorkItems (WIQL + workitems), Builds, Releases | Delivery health, allocation, completion, velocity, blocked items | Pending |
| PM Assigned Projects table | `/api/project-manager/assigned-projects` | Projects, Teams, Sprints, WorkItems (WIQL + workitems), Builds, Releases | Completion, velocity, blocked items by assignment | Pending |
| PM Sprint Progress cards/list | `/api/project-manager/sprint-progress` | Teams, Sprints, WorkItems (WIQL + workitems) | Sprint completion rate, planned/completed points, completed item counts | Pending |
| PM Work Items summary cards | `/api/project-manager/work-items` | WorkItems (WIQL + workitems) | Total, open, in-progress, blocked work item counts | Pending |
| PM Work Items table | `/api/project-manager/work-items` | WorkItems (WIQL + workitems) | Progress percent, blocked state projection, sprint association | Pending |
| PM KPIs summary cards | `/api/project-manager/kpis` | Projects, Teams, Sprints, WorkItems (WIQL + workitems), Builds, Releases | Avg completion, velocity, defect density, backlog health, release success | Pending |
| PM KPIs charts | `/api/project-manager/kpis` | Projects, Teams, Sprints, WorkItems (WIQL + workitems), Builds, Releases | Completion and velocity trends from KPI snapshots | Pending |
| IT Admin Synchronization: summary cards | `/api/it-admin/synchronization` | Projects, Teams, Sprints, WorkItems (WIQL + workitems), Builds, Releases | Jobs running, success/failure counts, queued schedules, average duration | Completed |
| IT Admin Synchronization: sync type chart | `/api/it-admin/synchronization` | Projects, Teams, Sprints, WorkItems (WIQL + workitems), Builds, Releases | Breakdown by sync type from sync job history | Completed |
| IT Admin Synchronization: recent jobs table | `/api/it-admin/synchronization` | Projects, Teams, Sprints, WorkItems (WIQL + workitems), Builds, Releases | Processed/succeeded/failed counts and durations | Completed |
| IT Admin Sync Monitor: summary cards | `/api/it-admin/sync-monitor` | Projects, Teams, Sprints, WorkItems (WIQL + workitems), Builds, Releases | Jobs today, failed jobs, warnings, overdue schedules, maintenance windows | Completed |
| IT Admin Sync Monitor: duration/status charts | `/api/it-admin/sync-monitor` | Projects, Teams, Sprints, WorkItems (WIQL + workitems), Builds, Releases | Duration trend and status breakdown from sync jobs | Completed |
| IT Admin Sync Monitor: recent jobs table | `/api/it-admin/sync-monitor` | Projects, Teams, Sprints, WorkItems (WIQL + workitems), Builds, Releases | Job execution history | Completed |
| IT Admin Logs cards/table | `/api/it-admin/logs` | None | Total, warning, error log counts from operational log store | Completed |
| IT Admin Maintenance cards/table | `/api/it-admin/maintenance` | None | Active, scheduled, overdue maintenance counts | Completed |
| IT Admin Scheduling cards/table | `/api/it-admin/scheduling` | None | Enabled, disabled, overdue schedule counts | Completed |
| Reports: Portfolio section | `/api/reports/overview`, `/api/reports/export/{sectionKey}` | Projects, Teams, Sprints, WorkItems (WIQL + workitems), Builds, Releases | Portfolio summaries and chart points derived from synchronized data | Completed |
| Reports: Sprint section | `/api/reports/overview`, `/api/reports/export/{sectionKey}` | Teams, Sprints, WorkItems (WIQL + workitems) | Sprint completion and item summaries | Completed |
| Reports: Work Item section | `/api/reports/overview`, `/api/reports/export/{sectionKey}` | WorkItems (WIQL + workitems) | Blocked/in-progress counts and latest work item flow | Completed |
| Reports: KPI section | `/api/reports/overview`, `/api/reports/export/{sectionKey}` | Projects, Teams, Sprints, WorkItems (WIQL + workitems), Builds, Releases | KPI trend aggregation from snapshots | Completed |
| Notifications page | `/api/notifications`, `/api/notifications/read` | None | Unread count and notification list from notification store | Completed |

## Pending blockers

### 1. Role dashboard route is still frontend-static

The routed dashboard page used for these paths:

- `/admin/dashboard`
- `/pm/role-dashboard`
- `/itadmin/dashboard`

renders `Frontend/UI/src/reference-ui/App.tsx` through `Frontend/UI/src/pages/RoleDashboardPage.tsx`.

That reference UI currently uses hardcoded in-file constants for its cards, charts, tables, and summaries and does not call backend APIs. Because the user requirement forbids frontend changes, these components cannot be switched to live backend data from backend work alone.

Affected embedded dashboard areas include:

- Administrator embedded dashboard cards/charts/tables
- Project Manager embedded dashboard cards/charts/tables
- IT Manager embedded dashboard cards/charts/tables
- Portfolio dashboard cards/charts
- Project details tabs
- Sprint governance board and charts
- KPI dashboard cards/charts
- AI risk analysis cards/charts
- AI executive reports panels
- Embedded AI insights chat side panels
- Embedded user/role management tables
- Embedded sync monitor cards/charts
- Embedded audit logs table
- Embedded settings screen summaries

Status for all of the above remains `Pending` until the frontend stops using static reference data and starts consuming existing backend endpoints.

### 2. Project manager assignment source is missing

`ProjectManagerAssignments` is empty after live Azure DevOps synchronization.

Current PM endpoints are implemented and derive live data correctly once project scope exists, but the Requirements documentation provided does not define a source-of-truth Azure DevOps API for project-manager-to-project assignment mapping.

Because of that, PM-scoped components remain `Pending` in production behavior until assignments are populated from a defined source.

## Comparison with documented Azure DevOps APIs

| Documented Azure DevOps API | Backend status |
|---|---|
| Projects | Implemented and used |
| Teams | Implemented and used for sprint discovery |
| Sprints | Implemented and used |
| WorkItems (WIQL + workitems) | Implemented and used |
| WorkItemHistory | Not implemented |
| Repositories | Not implemented |
| PullRequests | Not implemented |
| Builds | Implemented and used |
| Releases | Implemented and used |
| Users | Not implemented |

## Summary

- Backend live synchronization to Azure DevOps is working.
- Administrator, IT Admin, Reports, and Notifications pages are backend-driven and operational.
- Project Manager backend endpoints are implemented, but real PM dashboard data is blocked by missing assignment records.
- The largest remaining gap is the embedded role dashboard frontend, which is still hardcoded and cannot be made live without frontend code changes.
