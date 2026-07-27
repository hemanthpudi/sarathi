using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Sarathi.API.Application.DTOs.ProjectManager;
using Sarathi.API.Application.Interfaces;
using Sarathi.API.Persistence;

namespace Sarathi.API.Application.Services;

public class GeminiProjectManagerAiService : IProjectManagerAiService
{
    private readonly HttpClient _httpClient;
    private readonly AppDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeminiProjectManagerAiService> _logger;

    public GeminiProjectManagerAiService(HttpClient httpClient, AppDbContext dbContext, IConfiguration configuration, ILogger<GeminiProjectManagerAiService> logger)
    {
        _httpClient = httpClient;
        _dbContext = dbContext;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ProjectManagerAiAnalysisDto> AnalyzeProjectAsync(Guid userId, int projectId, string? question, CancellationToken cancellationToken = default)
    {
        var assigned = await _dbContext.ProjectManagerAssignments.AsNoTracking()
            .AnyAsync(item => item.ProjectManagerUserId == userId && item.ProjectId == projectId, cancellationToken);
        if (!assigned) throw new UnauthorizedAccessException("The project is not assigned to this project manager.");

        return await AnalyzeProjectAsync(projectId, question, cancellationToken);
    }

    public async Task<ProjectManagerAiAnalysisDto> AnalyzeProjectAsync(int projectId, string? question, CancellationToken cancellationToken = default)
    {

        var project = await _dbContext.Projects.AsNoTracking().FirstOrDefaultAsync(item => item.ProjectId == projectId, cancellationToken)
            ?? throw new KeyNotFoundException($"Project with ID {projectId} was not found.");
        var workItems = await _dbContext.WorkItems.AsNoTracking().Where(item => item.ProjectId == projectId).ToListAsync(cancellationToken);
        var sprints = await _dbContext.Sprints.AsNoTracking().Where(item => item.ProjectId == projectId).ToListAsync(cancellationToken);
        var latestKpi = await _dbContext.KpiSnapshots.AsNoTracking().Where(item => item.ProjectId == projectId).OrderByDescending(item => item.SnapshotDate).FirstOrDefaultAsync(cancellationToken);
        var latestRisk = await _dbContext.ProjectRiskAnalyses.AsNoTracking().Where(item => item.ProjectId == projectId).OrderByDescending(item => item.GeneratedDate).FirstOrDefaultAsync(cancellationToken);

        var completed = workItems.Count(item => IsCompleted(item.State));
        var blocked = workItems.Count(item => item.IsBlocked || item.State.Equals("Blocked", StringComparison.OrdinalIgnoreCase));
        var completion = latestKpi?.CompletionRate is > 0 ? latestKpi.CompletionRate.Value : Percent(completed, workItems.Count);
        var activeSprint = sprints.OrderBy(item => item.Status.Equals("Active", StringComparison.OrdinalIgnoreCase) ? 0 : 1).ThenByDescending(item => item.EndDate).FirstOrDefault();
        var velocity = latestKpi?.SprintVelocity is > 0
            ? latestKpi.SprintVelocity.Value
            : activeSprint is not null
                ? (decimal)activeSprint.CompletedStoryPoints
                : workItems.Where(item => IsCompleted(item.State)).Sum(item => item.StoryPoints ?? 0);
        var risk = latestRisk?.RiskScore ?? Math.Round(Math.Min(100, (100 - completion) * 0.6m + Percent(blocked, workItems.Count) * 0.4m), 2);

        var result = new ProjectManagerAiAnalysisDto
        {
            ProjectId = projectId,
            ProjectName = project.ProjectName,
            CompletionRate = Math.Round(completion, 2),
            SprintVelocity = Math.Round(velocity, 2),
            RiskScore = Math.Round(risk, 2),
            BlockedItems = blocked,
            ActiveSprints = sprints.Count(item => !item.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase)),
            GeneratedAtUtc = DateTime.UtcNow,
        };

        PopulateFallback(result);
        var apiKey = _configuration["Gemini:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey)) return result;

        try
        {
            var model = _configuration["gemini-2.5-flash"] ?? "gemini-2.5-flash";
            var prompt = BuildPrompt(result, question);
            using var request = new HttpRequestMessage(HttpMethod.Post, $"https://generativelanguage.googleapis.com/v1beta/models/{Uri.EscapeDataString(model)}:generateContent");
            request.Headers.Add("x-goog-api-key", apiKey);
            request.Content = JsonContent.Create(new { contents = new[] { new { parts = new[] { new { text = prompt } } } }, generationConfig = new { temperature = 0.2, maxOutputTokens = 700 } });
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            using var document = JsonDocument.Parse(await response.Content.ReadAsStreamAsync(cancellationToken));
            var text = document.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
            if (!string.IsNullOrWhiteSpace(text))
            {
                result.Report = text.Trim();
                result.Summary = text.Trim();
                result.RiskAnalysis = text.Trim();
                result.Provider = $"Gemini ({model})";
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Gemini analysis failed for project {ProjectId}; returning calculated metrics.", projectId);
        }

        return result;
    }

    private static void PopulateFallback(ProjectManagerAiAnalysisDto result)
    {
        var riskLabel = result.RiskScore >= 60 ? "requires immediate attention" : result.RiskScore >= 35 ? "needs active monitoring" : "is currently on track";
        result.Summary = $"{result.ProjectName} is {riskLabel}: completion is {result.CompletionRate:0.0}%, velocity is {result.SprintVelocity:0.0} points, and {result.BlockedItems} work items are blocked.";
        result.RiskAnalysis = $"Risk score {result.RiskScore:0.0}/100 is based on delivery completion and blocked work. Keep the active sprint commitment aligned with available capacity.";
        result.Report = result.Summary;
        result.Recommendations = new List<string>
        {
            result.BlockedItems > 0 ? "Resolve blocked work and dependencies before accepting new scope." : "Maintain daily review of sprint progress and dependency status.",
            result.CompletionRate < 85 ? "Review remaining work against the sprint commitment and re-plan early if needed." : "Preserve the current delivery cadence and verify release readiness.",
            "Use the next sprint review to validate velocity and backlog estimates."
        };
    }

    private static string BuildPrompt(ProjectManagerAiAnalysisDto metrics, string? question) =>
        $"You are a delivery-governance analyst. Analyze only the supplied project metrics. Give a concise factual response with risks and 3 prioritized actions. Do not invent metrics. Project: {metrics.ProjectName}; completion: {metrics.CompletionRate:0.0}%; velocity: {metrics.SprintVelocity:0.0}; risk score: {metrics.RiskScore:0.0}/100; blocked items: {metrics.BlockedItems}; active sprints: {metrics.ActiveSprints}. User question: {question ?? "Create a risk and delivery summary."}";

    private static bool IsCompleted(string state) => state.Equals("Done", StringComparison.OrdinalIgnoreCase) || state.Equals("Closed", StringComparison.OrdinalIgnoreCase) || state.Equals("Completed", StringComparison.OrdinalIgnoreCase);
    private static decimal Percent(int numerator, int denominator) => denominator == 0 ? 0 : decimal.Round((decimal)numerator / denominator * 100, 2);
}
