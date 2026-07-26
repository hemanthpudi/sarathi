using Sarathi.API.Application.DTOs.ProjectManager;

namespace Sarathi.API.Application.Services;

public static class ProjectManagerAiChatPolicy
{
    /// <summary>
    /// Comprehensive list of keywords supported for Project Manager AI analysis.
    /// Supports questions about: project summary, status, sprints, epics, features, 
    /// user stories, tasks, bugs, team workload, risks, KPIs, delivery metrics, 
    /// pipelines, repositories, sync status, project health, pending work, and recommendations.
    /// </summary>
    private static readonly string[] RelevantTerms =
    [
        // Project & Overview
        "project", "summary", "overview", "status", "health", "current state",
        
        // Sprint Management
        "sprint", "active sprint", "sprint progress", "sprint velocity", "backlog",
        "sprint planning", "sprint board", "burndown", "burn", "velocity",
        
        // Work Items by Type
        "epic", "feature", "story", "user story", "task", "bug", "defect",
        "work item", "workitem", "issue", "item", "pending", "open",
        
        // Work Item States & Properties
        "blocked", "blocker", "dependency", "done", "todo", "in progress",
        "ready", "approved", "committed", "completion", "progress", "state",
        "priority", "story point", "estimate", "assigned",
        
        // Team & Capacity
        "team", "team workload", "workload", "allocation", "capacity", "member",
        "owner", "assignee", "developer", "tester", "delivery health",
        
        // Risks & Issues
        "risk", "issue", "blocker", "critical", "high priority", "concern",
        "risk analysis", "risk score", "risk level", "risk summary",
        
        // Metrics & KPIs
        "metric", "kpi", "completion rate", "completion", "velocity", "performance",
        "defect density", "backlog health", "release success", "burn rate",
        "efficiency", "quality", "performance metric",
        
        // Delivery & Release
        "delivery", "release", "milestone", "deadline", "timeline", "schedule",
        "deliverable", "due date", "eta", "forecast", "delivery date",
        
        // Infrastructure & Build
        "pipeline", "build", "repository", "repo", "branch", "code",
        "build status", "build result", "azure devops", "devops",
        
        // Synchronization & Data
        "sync", "synchronization", "sync status", "data sync", "last sync",
        "sync job", "sync error", "azure", "integration",
        
        // Analysis & Reporting
        "analysis", "analyze", "report", "detail", "detail view", "drill down",
        "trend", "recommendation", "suggest", "advice", "action", "next step",
        "insight", "explanation", "interpretation",
        
        // Common Intent Markers
        "what", "how", "why", "when", "which", "who", "where",
        "describe", "explain", "show", "get", "list", "view", "display",
        "calculate", "determine", "identify", "assess", "evaluate",
        
        // Greetings & Help
        "hello", "hi", "hey", "greetings", "help", "who are you",
        "what can you do", "introduce", "capabilities", "support"
    ];

    public static bool IsRelevantQuestion(string? question, string projectName)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            return true;
        }

        var normalized = Normalize(question);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return true;
        }

        if (normalized.Contains(projectName.ToLowerInvariant()))
        {
            return true;
        }

        if (normalized.Contains("assigned project") || normalized.Contains("this project") || normalized.Contains("current project"))
        {
            return true;
        }

        if (normalized.Contains("weather") || normalized.Contains("news") || normalized.Contains("sports") || normalized.Contains("stock") || normalized.Contains("movie") || normalized.Contains("joke") || normalized.Contains("travel"))
        {
            return false;
        }

        return RelevantTerms.Any(term => normalized.Contains(term));
    }

    public static string BuildScopeResponse(string projectName)
    {
        return $"""
            I can only answer questions about {projectName} using synchronized project data. I can help with:
            
            • Project summary, status, and overall health
            • Sprint progress and velocity
            • Work items (Epics, Features, User Stories, Tasks, Bugs)
            • Blocked items and dependencies
            • Team workload and allocation
            • Risks and issues
            • KPIs and delivery metrics
            • Pipelines and builds
            • Repositories and branches
            • Synchronization status
            • Recommended actions
            
            Ask me anything about these topics for {projectName}.
            """;
    }

    public static string BuildPrompt(ProjectManagerAiAnalysisDto metrics, string? question)
    {
        return $"""
        You are a project-manager AI assistant for Sarathi. Answer using only the project details supplied below.
        If the user asks something unrelated to the assigned project, reply with a short message that the question is irrelevant to the assigned project and invite them to ask about delivery health, sprint progress, blockers, risks, work items, or recommendations for this project.

        Project: {metrics.ProjectName}
        Completion: {metrics.CompletionRate:0.0}%
        Sprint velocity: {metrics.SprintVelocity:0.0}
        Risk score: {metrics.RiskScore:0.0}/100
        Blocked items: {metrics.BlockedItems}
        Active sprints: {metrics.ActiveSprints}
        User question: {question ?? "Create a concise project summary."}
        """;
    }

    private static string Normalize(string text) => text.ToLowerInvariant().Trim();
}
