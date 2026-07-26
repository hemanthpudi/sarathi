using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;

namespace Sarathi.API.Controllers;

[ApiController]
[Route("api/diagnostics/gemini")]
public class GeminiDiagnosticsController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeminiDiagnosticsController> _logger;

    public GeminiDiagnosticsController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<GeminiDiagnosticsController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("ping")]
    public async Task<IActionResult> Ping(CancellationToken cancellationToken)
    {
        var model = _configuration["Gemini:Model"] ?? _configuration["GeminiModel"] ?? "gemini-3.5-flash-lite";
        var apiVersion = _configuration["Gemini:ApiVersion"] ?? "v1beta";
        var baseUrl = _configuration["Gemini:BaseUrl"] ?? "https://generativelanguage.googleapis.com";
        var apiKey = _configuration["Gemini:ApiKey"] ?? _configuration["GeminiApiKey"] ?? Environment.GetEnvironmentVariable("GeminiApiKey");
        var accessToken = _configuration["Gemini:AccessToken"] ?? _configuration["GeminiAccessToken"] ?? Environment.GetEnvironmentVariable("GeminiAccessToken");

        var isGemini = model.StartsWith("gemini", StringComparison.OrdinalIgnoreCase) || model.StartsWith("gemini-", StringComparison.OrdinalIgnoreCase);
        var endpoint = isGemini ? "generateContent" : "generateText";
        var requestUri = new Uri($"{baseUrl.TrimEnd('/')}/{apiVersion}/models/{Uri.EscapeDataString(model)}:{endpoint}");

        try
        {
            var client = _httpClientFactory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            if (!string.IsNullOrWhiteSpace(apiKey)) request.Headers.Add("x-goog-api-key", apiKey);
            if (!string.IsNullOrWhiteSpace(accessToken)) request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            if (isGemini)
            {
                request.Content = JsonContent.Create(new
                {
                    contents = new[] { new { parts = new[] { new { text = "Sarathi diagnostic ping" } } } },
                    generationConfig = new { maxOutputTokens = 8 }
                });
            }
            else
            {
                request.Content = JsonContent.Create(new { prompt = new { text = "Sarathi diagnostic ping" }, maxOutputTokens = 8 });
            }

            using var response = await client.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var truncated = Truncate(body, 2000);

            _logger.LogInformation("Gemini diagnostic ping to {RequestUri} returned {StatusCode}", requestUri, response.StatusCode);

            return Ok(new
            {
                RequestUri = requestUri.ToString(),
                StatusCode = (int)response.StatusCode,
                Body = truncated,
                Model = model,
                Endpoint = endpoint
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini diagnostic ping failed");
            return StatusCode(500, new { Error = "Diagnostic ping failed", Message = ex.Message });
        }
    }

    private static string Truncate(string? s, int max)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        if (s.Length <= max) return s;
        return s.Substring(0, max) + "...";
    }
}
