using System.Linq;
using System.Net;
using System.Net.Http.Headers;
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
    private readonly IProjectContextBuilder _contextBuilder;

    public GeminiProjectManagerAiService(
        HttpClient httpClient,
        AppDbContext dbContext,
        IConfiguration configuration,
        ILogger<GeminiProjectManagerAiService> logger,
        IProjectContextBuilder contextBuilder)
    {
        _httpClient = httpClient;
        _dbContext = dbContext;
        _configuration = configuration;
        _logger = logger;
        _contextBuilder = contextBuilder;
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
        // Build comprehensive project context from synchronized database data
        ProjectContextDto projectContext;
        try
        {
            projectContext = await _contextBuilder.BuildProjectContextAsync(projectId, cancellationToken);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogError(ex, "Project {ProjectId} not found", projectId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building project context for project {ProjectId}", projectId);
            throw;
        }

        var project = projectContext.Project;
        var workItems = projectContext.Epics.Count + projectContext.Features.Count + projectContext.UserStories.Count + projectContext.Tasks.Count + projectContext.Bugs.Count;
        var blocked = projectContext.Epics.Count(w => w.IsBlocked) +
                      projectContext.Features.Count(w => w.IsBlocked) +
                      projectContext.UserStories.Count(w => w.IsBlocked) +
                      projectContext.Tasks.Count(w => w.IsBlocked) +
                      projectContext.Bugs.Count(w => w.IsBlocked);

        var completion = projectContext.Kpis.CompletionRate ?? 0;
        var velocity = projectContext.Kpis.SprintVelocity ?? 0;
        var risk = projectContext.Risks.FirstOrDefault()?.RiskScore ?? Math.Round(Math.Min(100, (100 - completion) * 0.6m + Percent(blocked, workItems) * 0.4m), 2);

        var result = new ProjectManagerAiAnalysisDto
        {
            ProjectId = projectId,
            ProjectName = project.ProjectName,
            CompletionRate = Math.Round(completion, 2),
            SprintVelocity = Math.Round(velocity, 2),
            RiskScore = Math.Round(risk, 2),
            BlockedItems = blocked,
            ActiveSprints = projectContext.Sprints.Count(s => !s.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase)),
            GeneratedAtUtc = DateTime.UtcNow,
        };

        PopulateFallback(result);

        var apiKey = _configuration["Gemini:ApiKey"]
            ?? _configuration["GeminiApiKey"]
            ?? Environment.GetEnvironmentVariable("GeminiApiKey");
        var accessToken = _configuration["Gemini:AccessToken"]
            ?? _configuration["GeminiAccessToken"]
            ?? Environment.GetEnvironmentVariable("GeminiAccessToken");
        if (string.IsNullOrWhiteSpace(apiKey) && string.IsNullOrWhiteSpace(accessToken)) return result;

        try
        {
            var model = _configuration["Gemini:Model"] ?? _configuration["GeminiModel"] ?? "gemini-3.5-flash";
            var apiVersion = _configuration["Gemini:ApiVersion"] ?? _configuration["GeminiApiVersion"] ?? "v1beta";
            var baseUrl = _configuration["Gemini:BaseUrl"] ?? _configuration["GeminiBaseUrl"] ?? "https://generativelanguage.googleapis.com";

            _logger.LogInformation(
                "Gemini config resolved: model={Model} apiVersion={ApiVersion} baseUrl={BaseUrl} hasApiKey={HasApiKey} hasAccessToken={HasAccessToken}",
                model, apiVersion, baseUrl, !string.IsNullOrWhiteSpace(apiKey), !string.IsNullOrWhiteSpace(accessToken));

            var normalizedQuestion = string.IsNullOrWhiteSpace(question) ? "Provide an analysis of this project." : question;

            if (!ProjectManagerAiChatPolicy.IsRelevantQuestion(normalizedQuestion, result.ProjectName))
            {
                var scopeMessage = ProjectManagerAiChatPolicy.BuildScopeResponse(result.ProjectName);
                result.Provider = "Policy";
                result.Report = scopeMessage;
                result.Summary = scopeMessage;
                result.RiskAnalysis = scopeMessage;
                return result;
            }

            // Check if sufficient project data is available
            if (!IsProjectDataAvailable(projectContext))
            {
                result.Provider = "Data Unavailable";
                result.Report = $"Insufficient project data available for analysis. Please synchronize Azure DevOps data and ensure the project has been populated with work items, sprints, and KPIs before requesting AI analysis.";
                result.Summary = result.Report;
                result.RiskAnalysis = result.Report;
                return result;
            }

            var text = await TryGenerateTextAsync(baseUrl, apiVersion, model, projectContext, normalizedQuestion, apiKey, accessToken, cancellationToken);
            if (!string.IsNullOrWhiteSpace(text))
            {
                result.Provider = $"Gemini ({model})";
                result.Report = text.Trim();
                result.Summary = text.Trim();
                result.RiskAnalysis = text.Trim();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Gemini analysis failed for project {ProjectId}; returning calculated metrics.", projectId);
        }

        return result;
    }

    // Gemini models only ever support generateContent. The legacy generateText (PaLM2) method
    // does not exist for any Gemini model, so there is no meaningful fallback to try — a single
    // call is made, and any failure is logged and treated as "no AI text available".
    private async Task<string?> TryGenerateTextAsync(
        string baseUrl,
        string apiVersion,
        string model,
        ProjectContextDto projectContext,
        string question,
        string? apiKey,
        string? accessToken,
        CancellationToken cancellationToken)
    {
        var requestUri = new Uri($"{baseUrl.TrimEnd('/')}/{apiVersion}/models/{Uri.EscapeDataString(model)}:generateContent");
        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
        if (!string.IsNullOrWhiteSpace(apiKey)) request.Headers.Add("x-goog-api-key", apiKey);
        if (!string.IsNullOrWhiteSpace(accessToken)) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Serialize project context as JSON for Gemini
        var contextJson = JsonSerializer.Serialize(projectContext, new JsonSerializerOptions { WriteIndented = false });

        // Build comprehensive instruction prompt for response quality
        var instructionPrompt = BuildProjectAnalysisPrompt(question);

        request.Content = JsonContent.Create(new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new
                        {
                            text = $"""
                            {instructionPrompt}
                            
                            PROJECT CONTEXT (JSON):
                            {contextJson}
                            
                            USER QUESTION:
                            {question}
                            """
                        }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.3,
                maxOutputTokens = 1024
            }
        });

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogError("Gemini model not found: {RequestUri} — check Gemini:Model / Gemini:ApiVersion config. Response: {Body}", requestUri, Truncate(responseBody, 1000));
            return null;
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
        {
            _logger.LogWarning("Gemini authentication failed: {StatusCode} at {RequestUri}", response.StatusCode, requestUri);
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            var requestBodyForLog = await request.Content!.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "Gemini Error {StatusCode} at {RequestUri}: {Body}",
                response.StatusCode,
                requestUri,
                Truncate(responseBody, 2000));

            return null;
        }

        using var document = JsonDocument.Parse(responseBody);
        var root = document.RootElement;

        if (root.TryGetProperty("candidates", out var candidatesCheck) &&
            candidatesCheck.ValueKind == JsonValueKind.Array &&
            candidatesCheck.GetArrayLength() > 0 &&
            candidatesCheck[0].TryGetProperty("finishReason", out var finishReasonElement))
        {
            var finishReason = finishReasonElement.GetString();
            if (finishReason is "MAX_TOKENS")
            {
                _logger.LogWarning("Gemini response was truncated (finishReason=MAX_TOKENS).");
            }
        }

        return ExtractGeminiText(root);
    }

    private static string? ExtractGeminiText(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            if (element.TryGetProperty("text", out var textElement) && textElement.ValueKind == JsonValueKind.String)
            {
                return textElement.GetString();
            }

            if (element.TryGetProperty("output", out var outputElement))
            {
                var outputText = ExtractGeminiText(outputElement);
                if (!string.IsNullOrWhiteSpace(outputText)) return outputText;
            }

            if (element.TryGetProperty("candidates", out var candidates) && candidates.ValueKind == JsonValueKind.Array)
            {
                foreach (var candidate in candidates.EnumerateArray())
                {
                    var candidateText = ExtractGeminiText(candidate);
                    if (!string.IsNullOrWhiteSpace(candidateText)) return candidateText;
                }
            }

            if (element.TryGetProperty("content", out var contentElement))
            {
                var contentText = ExtractGeminiText(contentElement);
                if (!string.IsNullOrWhiteSpace(contentText)) return contentText;
            }

            if (element.TryGetProperty("parts", out var partsElement) && partsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var part in partsElement.EnumerateArray())
                {
                    var partText = ExtractGeminiText(part);
                    if (!string.IsNullOrWhiteSpace(partText)) return partText;
                }
            }

            foreach (var property in element.EnumerateObject())
            {
                var nestedText = ExtractGeminiText(property.Value);
                if (!string.IsNullOrWhiteSpace(nestedText)) return nestedText;
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var itemText = ExtractGeminiText(item);
                if (!string.IsNullOrWhiteSpace(itemText)) return itemText;
            }
        }

        return null;
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

    private static bool IsProjectDataAvailable(ProjectContextDto projectContext)
    {
        // Check if minimal project data is available
        if (projectContext?.Project == null)
            return false;

        // Ensure there is at least some work item, sprint, or KPI data
        var hasWorkItems = (projectContext.Epics?.Count ?? 0) +
                           (projectContext.Features?.Count ?? 0) +
                           (projectContext.UserStories?.Count ?? 0) +
                           (projectContext.Tasks?.Count ?? 0) +
                           (projectContext.Bugs?.Count ?? 0) > 0;

        var hasSprints = (projectContext.Sprints?.Count ?? 0) > 0;
        var hasKpis = projectContext.Kpis?.CompletionRate.HasValue == true ||
                      projectContext.Kpis?.SprintVelocity.HasValue == true;

        return hasWorkItems || hasSprints || hasKpis;
    }

    /// <summary>
    /// Builds conversational Q&A instructions for Gemini so responses behave like ChatGPT.
    /// The AI answers the user's question directly, explains the concept simply, then connects it to the selected project with synchronized data only.
    /// </summary>
    private static string BuildProjectAnalysisPrompt(string userQuestion)
    {
        return $"""
            You are a helpful project management AI assistant for Sarathi.
            Answer the user's question as a conversational Q&A assistant, not as a report generator.

            RESPONSE STYLE:
            - Sound like ChatGPT: natural, direct, and easy to read
            - Be concise but useful
            - Use short paragraphs and bullet points when helpful
            - Keep formatting clean and simple
            - Use headings only when they improve readability
            - Do not use fixed report sections such as Direct Answer, Plain English Explanation, Project Context, Health Assessment, Pattern Analysis, Business Impact, or Recommended Actions

            WHEN ANSWERING:
            1. First answer the user's question directly.
            2. If the question is about a project management concept, explain the concept in simple language.
            3. Then explain why that concept is used.
            4. Finally relate it to the selected project using only the synchronized project data provided.
            5. Only include project-specific information when it is relevant to the question.

            RULES:
            - Base everything on the PROVIDED PROJECT CONTEXT only.
            - Never invent data, metrics, timelines, risks, team members, or repositories.
            - If the data is missing, say that it is not available in the latest sync.
            - Do not generate generic advice that is not supported by the data.
            - Keep the answer conversational and natural.

            FORMAT PREFERENCES:
            - Use simple markdown headings like # or ## when helpful
            - Use bullet points for clarity
            - Avoid unnecessary symbols, decorative formatting, or repeated section labels
            - Keep the response concise and focused on the question

            USER QUESTION: {userQuestion}

            Answer the question in a conversational, helpful, and project-aware way using only the synchronized project data.
            """;
    }

    private static bool IsCompleted(string state) => state.Equals("Done", StringComparison.OrdinalIgnoreCase) || state.Equals("Closed", StringComparison.OrdinalIgnoreCase) || state.Equals("Completed", StringComparison.OrdinalIgnoreCase);
    private static decimal Percent(int numerator, int denominator) => denominator == 0 ? 0 : decimal.Round((decimal)numerator / denominator * 100, 2);

    private static string Truncate(string? s, int max)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        if (s.Length <= max) return s;
        return s.Substring(0, max) + "...";
    }
}