using System.Net.Http.Headers;
using System.Net;
using System.Text;
using System.Text.Json;
using Sarathi.API.Application.Interfaces;

namespace Sarathi.API.Infrastructure.AzureDevOps;

public class AzureDevOpsClient : IAzureDevOpsClient
{
    private const int MaxRetryAttempts = 3;
    private readonly HttpClient _httpClient;

    public AzureDevOpsClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<AzureDevOpsProjectData>> GetProjectsAsync(string organizationUrl, string personalAccessToken, CancellationToken cancellationToken = default)
    {
        var items = new List<AzureDevOpsProjectData>();
        string? continuationToken = null;

        do
        {
            var url = AppendContinuationToken($"{organizationUrl.TrimEnd('/')}/_apis/projects?$top=100&api-version=7.1", continuationToken);
            using var response = await SendAsyncWithRetry(() => CreateRequest(HttpMethod.Get, url, personalAccessToken), cancellationToken);
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            foreach (var element in document.RootElement.GetProperty("value").EnumerateArray())
            {
                items.Add(new AzureDevOpsProjectData
                {
                    Id = element.GetProperty("id").GetString() ?? string.Empty,
                    Name = element.GetProperty("name").GetString() ?? string.Empty,
                    Visibility = element.TryGetProperty("visibility", out var visibility) ? visibility.GetString() ?? "private" : "private",
                    Description = element.TryGetProperty("description", out var description) ? description.GetString() : null,
                });
            }

            continuationToken = GetContinuationToken(response);
        }
        while (!string.IsNullOrWhiteSpace(continuationToken));

        return items;
    }

    public async Task<IReadOnlyList<AzureDevOpsIterationData>> GetSprintsAsync(string organizationUrl, string projectId, string projectName, string personalAccessToken, CancellationToken cancellationToken = default)
    {
        var teams = await GetTeamsAsync(organizationUrl, projectId, personalAccessToken, cancellationToken);
        var itemsById = new Dictionary<string, AzureDevOpsIterationData>(StringComparer.OrdinalIgnoreCase);

        foreach (var team in teams.DefaultIfEmpty(new AzureDevOpsTeamData { Name = projectName }))
        {
            using var response = await SendAsyncWithRetry(
                () => CreateRequest(HttpMethod.Get, $"{organizationUrl.TrimEnd('/')}/{Uri.EscapeDataString(projectName)}/{Uri.EscapeDataString(team.Name)}/_apis/work/teamsettings/iterations?api-version=7.1", personalAccessToken),
                cancellationToken);

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            foreach (var element in document.RootElement.GetProperty("value").EnumerateArray())
            {
                var attributes = element.TryGetProperty("attributes", out var attr) ? attr : default;
                var iteration = new AzureDevOpsIterationData
                {
                    Id = element.GetProperty("id").GetString() ?? string.Empty,
                    Name = element.GetProperty("name").GetString() ?? string.Empty,
                    StartDate = attributes.ValueKind != JsonValueKind.Undefined && attributes.TryGetProperty("startDate", out var startDate) && startDate.TryGetDateTime(out var start) ? start : DateTime.UtcNow,
                    EndDate = attributes.ValueKind != JsonValueKind.Undefined && attributes.TryGetProperty("finishDate", out var finishDate) && finishDate.TryGetDateTime(out var finish) ? finish : DateTime.UtcNow,
                    Status = attributes.ValueKind != JsonValueKind.Undefined && attributes.TryGetProperty("timeFrame", out var timeFrame) ? NormalizeTimeFrame(timeFrame.GetString()) : "Planned",
                };

                itemsById[iteration.Id] = iteration;
            }
        }

        return itemsById.Values
            .OrderBy(item => item.StartDate)
            .ThenBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<IReadOnlyList<AzureDevOpsWorkItemData>> GetWorkItemsAsync(string organizationUrl, string projectName, string personalAccessToken, CancellationToken cancellationToken = default)
    {
        var ids = await GetRecentWorkItemIdsAsync(organizationUrl, projectName, personalAccessToken, cancellationToken);
        Console.WriteLine($"Work Item IDs Found: {ids.Count}");
        if (ids.Count == 0)
        {
            return Array.Empty<AzureDevOpsWorkItemData>();
        }

        var fields = string.Join(',', new[]
        {
            "System.Id",
            "System.Title",
            "System.WorkItemType",
            "System.State",
            "Microsoft.VSTS.Common.Priority",
            "System.AssignedTo",
            "Microsoft.VSTS.Scheduling.StoryPoints",
            "Microsoft.VSTS.Scheduling.Effort",
            "Microsoft.VSTS.Scheduling.RemainingWork",
            "System.IterationPath",
            "System.CreatedDate",
            "System.ChangedDate",
            "Microsoft.VSTS.Common.ClosedDate",
            "Microsoft.VSTS.CMMI.Blocked"
        });

        var items = new List<AzureDevOpsWorkItemData>();

        foreach (var batch in ids.Chunk(200))
        {
            using var response = await SendAsyncWithRetry(
                () => CreateRequest(
                    HttpMethod.Get,
                    $"{organizationUrl.TrimEnd('/')}/_apis/wit/workitems?ids={string.Join(',', batch)}&fields={Uri.EscapeDataString(fields)}&api-version=7.1",
                    personalAccessToken),
                cancellationToken);

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            foreach (var element in document.RootElement.GetProperty("value").EnumerateArray())
            {
                var fieldsElement = element.GetProperty("fields");
                items.Add(new AzureDevOpsWorkItemData
                {
                    Id = element.GetProperty("id").ToString(),
                    Title = GetString(fieldsElement, "System.Title"),
                    WorkItemType = GetString(fieldsElement, "System.WorkItemType"),
                    State = GetString(fieldsElement, "System.State"),
                    Priority = GetString(fieldsElement, "Microsoft.VSTS.Common.Priority"),
                    AssignedToName = GetAssignedTo(fieldsElement),
                    StoryPoints = GetDecimal(fieldsElement, "Microsoft.VSTS.Scheduling.StoryPoints"),
                    Effort = GetDecimal(fieldsElement, "Microsoft.VSTS.Scheduling.Effort"),
                    RemainingWork = GetDecimal(fieldsElement, "Microsoft.VSTS.Scheduling.RemainingWork"),
                    IsBlocked = GetString(fieldsElement, "Microsoft.VSTS.CMMI.Blocked").Equals("Yes", StringComparison.OrdinalIgnoreCase),
                    SprintName = GetSprintName(GetString(fieldsElement, "System.IterationPath"), projectName),
                    CreatedDateUtc = GetDateTime(fieldsElement, "System.CreatedDate"),
                    LastUpdatedUtc = GetDateTime(fieldsElement, "System.ChangedDate") ?? DateTime.UtcNow,
                    ClosedDateUtc = GetDateTime(fieldsElement, "Microsoft.VSTS.Common.ClosedDate"),
                });
            }
        }

        return items;
    }

    public async Task<IReadOnlyList<AzureDevOpsRepositoryData>> GetRepositoriesAsync(string organizationUrl, string projectName, string personalAccessToken, CancellationToken cancellationToken = default)
    {
        var url = $"{organizationUrl.TrimEnd('/')}/{Uri.EscapeDataString(projectName)}/_apis/git/repositories?api-version=7.1";
        using var response = await SendAsyncWithRetry(() => CreateRequest(HttpMethod.Get, url, personalAccessToken), cancellationToken);
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        return document.RootElement.GetProperty("value")
            .EnumerateArray()
            .Select(element => new AzureDevOpsRepositoryData
            {
                Id = element.GetProperty("id").GetString() ?? string.Empty,
                Name = element.GetProperty("name").GetString() ?? string.Empty,
                DefaultBranch = element.TryGetProperty("defaultBranch", out var defaultBranch) ? defaultBranch.GetString() : null,
                Size = element.TryGetProperty("size", out var size) && size.TryGetInt64(out var repositorySize) ? repositorySize : null,
                Url = element.TryGetProperty("webUrl", out var webUrl) ? webUrl.GetString() : null,
            })
            .ToList();
    }

    public async Task<IReadOnlyList<AzureDevOpsBuildData>> GetBuildsAsync(string organizationUrl, string projectName, string personalAccessToken, CancellationToken cancellationToken = default)
    {
        var items = new List<AzureDevOpsBuildData>();
        string? continuationToken = null;

        do
        {
            var url = AppendContinuationToken(
                $"{organizationUrl.TrimEnd('/')}/{Uri.EscapeDataString(projectName)}/_apis/build/builds?$top=100&queryOrder=finishTimeDescending&api-version=7.1",
                continuationToken);

            using var response = await SendAsyncWithRetry(() => CreateRequest(HttpMethod.Get, url, personalAccessToken), cancellationToken);
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            foreach (var element in document.RootElement.GetProperty("value").EnumerateArray())
            {
                items.Add(new AzureDevOpsBuildData
                {
                    Id = element.TryGetProperty("id", out var id) ? id.GetInt32() : 0,
                    BuildNumber = element.TryGetProperty("buildNumber", out var buildNumber) ? buildNumber.GetString() ?? string.Empty : string.Empty,
                    DefinitionName = element.TryGetProperty("definition", out var definition) && definition.TryGetProperty("name", out var definitionName)
                        ? definitionName.GetString() ?? string.Empty
                        : string.Empty,
                    Status = element.TryGetProperty("status", out var status) ? status.GetString() ?? string.Empty : string.Empty,
                    Result = element.TryGetProperty("result", out var result) ? result.GetString() ?? string.Empty : string.Empty,
                    SourceBranch = element.TryGetProperty("sourceBranch", out var sourceBranch) ? sourceBranch.GetString() ?? string.Empty : string.Empty,
                    StartTimeUtc = element.TryGetProperty("startTime", out var startTime) ? ParseDateTime(startTime) : null,
                    FinishTimeUtc = element.TryGetProperty("finishTime", out var finishTime) ? ParseDateTime(finishTime) : null,
                    TriggerType = element.TryGetProperty("reason", out var reason) ? reason.GetString() : null,
                });
            }

            continuationToken = GetContinuationToken(response);
        }
        while (!string.IsNullOrWhiteSpace(continuationToken));

        return items;
    }

    public async Task<IReadOnlyList<AzureDevOpsReleaseData>> GetReleasesAsync(string organizationUrl, string projectName, string personalAccessToken, CancellationToken cancellationToken = default)
    {
        var organizationName = GetOrganizationName(organizationUrl);
        var items = new List<AzureDevOpsReleaseData>();
        string? continuationToken = null;

        do
        {
            var url = AppendContinuationToken(
                $"https://vsrm.dev.azure.com/{Uri.EscapeDataString(organizationName)}/{Uri.EscapeDataString(projectName)}/_apis/release/releases?$top=100&queryOrder=descending&api-version=7.1",
                continuationToken);

            using var response = await SendAsyncWithRetry(() => CreateRequest(HttpMethod.Get, url, personalAccessToken), cancellationToken);
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            foreach (var element in document.RootElement.GetProperty("value").EnumerateArray())
            {
                items.Add(new AzureDevOpsReleaseData
                {
                    Id = element.TryGetProperty("id", out var id) ? id.GetInt32() : 0,
                    Name = element.TryGetProperty("name", out var name) ? name.GetString() ?? string.Empty : string.Empty,
                    Status = element.TryGetProperty("status", out var status) ? status.GetString() ?? string.Empty : string.Empty,
                    CreatedOnUtc = element.TryGetProperty("createdOn", out var createdOn) ? ParseDateTime(createdOn) : null,
                    ModifiedOnUtc = element.TryGetProperty("modifiedOn", out var modifiedOn) ? ParseDateTime(modifiedOn) : null,
                });
            }

            continuationToken = GetContinuationToken(response);
        }
        while (!string.IsNullOrWhiteSpace(continuationToken));

        return items;
    }

    private async Task<IReadOnlyList<AzureDevOpsTeamData>> GetTeamsAsync(string organizationUrl, string projectId, string personalAccessToken, CancellationToken cancellationToken)
    {
        var items = new List<AzureDevOpsTeamData>();
        string? continuationToken = null;

        do
        {
            var url = AppendContinuationToken(
                $"{organizationUrl.TrimEnd('/')}/_apis/projects/{Uri.EscapeDataString(projectId)}/teams?$top=100&api-version=7.1",
                continuationToken);

            using var response = await SendAsyncWithRetry(() => CreateRequest(HttpMethod.Get, url, personalAccessToken), cancellationToken);
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            foreach (var element in document.RootElement.GetProperty("value").EnumerateArray())
            {
                items.Add(new AzureDevOpsTeamData
                {
                    Id = element.TryGetProperty("id", out var id) ? id.GetString() ?? string.Empty : string.Empty,
                    Name = element.TryGetProperty("name", out var name) ? name.GetString() ?? string.Empty : string.Empty,
                });
            }

            continuationToken = GetContinuationToken(response);
        }
        while (!string.IsNullOrWhiteSpace(continuationToken));

        return items;
    }

    private async Task<List<int>> GetRecentWorkItemIdsAsync(string organizationUrl, string projectName, string personalAccessToken, CancellationToken cancellationToken)
    {
        using var response = await SendAsyncWithRetry(() =>
        {
            var request = CreateRequest(HttpMethod.Post, $"{organizationUrl.TrimEnd('/')}/{Uri.EscapeDataString(projectName)}/_apis/wit/wiql?api-version=7.1", personalAccessToken);
            request.Content = new StringContent("{\"query\":\"Select [System.Id] From WorkItems Where [System.TeamProject] = @project Order By [System.ChangedDate] Desc\"}", Encoding.UTF8, "application/json");
            return request;
        }, cancellationToken);

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        return document.RootElement.GetProperty("workItems")
            .EnumerateArray()
            .Select(item => item.GetProperty("id").GetInt32())
            .ToList();
    }

    private async Task<HttpResponseMessage> SendAsyncWithRetry(Func<HttpRequestMessage> requestFactory, CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= MaxRetryAttempts; attempt++)
        {
            using var request = requestFactory();

            try
            {
                var response = await _httpClient.SendAsync(request, cancellationToken);
                if (!IsTransient(response.StatusCode) || attempt == MaxRetryAttempts)
                {
                    response.EnsureSuccessStatusCode();
                    return response;
                }

                response.Dispose();
            }
            catch (HttpRequestException) when (attempt < MaxRetryAttempts)
            {
            }

            await Task.Delay(TimeSpan.FromSeconds(attempt * 2), cancellationToken);
        }

        throw new HttpRequestException("Azure DevOps request failed after retries.");
    }

    private static HttpRequestMessage CreateRequest(HttpMethod method, string url, string personalAccessToken)
    {
        var request = new HttpRequestMessage(method, url);
        var token = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{personalAccessToken}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", token);
        return request;
    }

    private static string NormalizeTimeFrame(string? timeFrame)
    {
        return timeFrame?.ToLowerInvariant() switch
        {
            "past" => "Completed",
            "current" => "Active",
            "future" => "Planned",
            _ => "Planned",
        };
    }

    private static string GetString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) ? value.ToString() : string.Empty;
    }

    private static decimal? GetDecimal(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.Number when value.TryGetDecimal(out var decimalValue) => decimalValue,
            JsonValueKind.String when decimal.TryParse(value.GetString(), out var decimalValue) => decimalValue,
            _ => null,
        };
    }

    private static DateTime? GetDateTime(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value))
        {
            return null;
        }

        return ParseDateTime(value);
    }

    private static DateTime? ParseDateTime(JsonElement value)
    {
        if (value.TryGetDateTime(out var dateTime))
        {
            return dateTime;
        }

        return DateTime.TryParse(value.ToString(), out var parsed) ? parsed : null;
    }

    private static string GetAssignedTo(JsonElement element)
    {
        if (!element.TryGetProperty("System.AssignedTo", out var assignedTo))
        {
            return string.Empty;
        }

        if (assignedTo.ValueKind == JsonValueKind.Object && assignedTo.TryGetProperty("displayName", out var displayName))
        {
            return displayName.GetString() ?? string.Empty;
        }

        return assignedTo.ToString();
    }

    private static string GetSprintName(string iterationPath, string projectName)
    {
        if (string.IsNullOrWhiteSpace(iterationPath))
        {
            return string.Empty;
        }

        var parts = iterationPath.Split('\\', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
        {
            return string.Empty;
        }

        if (parts.Length == 1)
        {
            return parts[0].Equals(projectName, StringComparison.OrdinalIgnoreCase) ? string.Empty : parts[0];
        }

        return parts[^1];
    }

    private static bool IsTransient(HttpStatusCode statusCode)
    {
        return statusCode == HttpStatusCode.RequestTimeout
               || statusCode == (HttpStatusCode)429
               || (int)statusCode >= 500;
    }

    private static string? GetContinuationToken(HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues("x-ms-continuationtoken", out var values))
        {
            return values.FirstOrDefault();
        }

        return null;
    }

    private static string AppendContinuationToken(string url, string? continuationToken)
    {
        if (string.IsNullOrWhiteSpace(continuationToken))
        {
            return url;
        }

        return $"{url}&continuationToken={Uri.EscapeDataString(continuationToken)}";
    }

    private static string GetOrganizationName(string organizationUrl)
    {
        var uri = new Uri(organizationUrl);
        var pathSegments = uri.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (pathSegments.Length == 0)
        {
            throw new InvalidOperationException("Azure DevOps organization URL must include the organization name.");
        }

        return pathSegments[0];
    }
}