using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Sarathi.API.Application.Interfaces;
using Sarathi.API.Application.Services;
using Sarathi.API.Infrastructure.AzureDevOps;
using Sarathi.API.Infrastructure.Authentication;
using Sarathi.API.Infrastructure.Notifications;
using Sarathi.API.Infrastructure.Repositories;
using Sarathi.API.Persistence;

// ── Serilog bootstrap logger ──────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var contentRoot = Directory.GetCurrentDirectory();
    var envFile = Path.Combine(contentRoot, ".env");
    if (File.Exists(envFile))
    {
        foreach (var line in File.ReadAllLines(envFile))
        {
            var trimmed = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#"))
            {
                continue;
            }

            var parts = trimmed.Split('=', 2);
            if (parts.Length != 2)
            {
                continue;
            }

            var key = parts[0].Trim();
            var value = parts[1].Trim();
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }

    var builder = WebApplication.CreateBuilder(args);
    builder.Configuration.AddEnvironmentVariables();

    // ── Serilog ──────────────────────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, lc) =>
        lc.ReadFrom.Configuration(ctx.Configuration));

    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(
            Path.Combine(builder.Environment.ContentRootPath, "DataProtectionKeys")));

    // ── Entity Framework Core ─────────────────────────────────────────────────
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            sql => sql.EnableRetryOnFailure()));
    builder.Services.AddHttpClient();
    // ── JWT Bearer Authentication ─────────────────────────────────────────────
    var jwtSecret = builder.Configuration["Jwt:Secret"]
        ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
    var jwtIssuer = builder.Configuration["Jwt:Issuer"]
        ?? throw new InvalidOperationException("Jwt:Issuer is not configured.");
    var jwtAudience = builder.Configuration["Jwt:Audience"]
        ?? throw new InvalidOperationException("Jwt:Audience is not configured.");

    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = true,
                ValidAudience = jwtAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/notifications"))
                    {
                        context.Token = accessToken;
                    }

                    return Task.CompletedTask;
                }
            };
        });

    builder.Services.AddAuthorization();

    // ── CORS ──────────────────────────────────────────────────────────────────
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    });

    // ── Application Services ──────────────────────────────────────────────────
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IAdminService, AdminService>();
    builder.Services.AddScoped<IAzureDevOpsIntegrationService, AzureDevOpsIntegrationService>();
    builder.Services.AddScoped<IAzureDevOpsSynchronizationRuntimeService, AzureDevOpsSynchronizationRuntimeService>();
    builder.Services.AddScoped<IDashboardAnalyticsService, DashboardAnalyticsService>();
    builder.Services.AddScoped<IItAdminService, ItAdminService>();
    builder.Services.AddScoped<INotificationService, NotificationService>();
    builder.Services.AddScoped<INotificationRealtimeService, NotificationRealtimeService>();
    builder.Services.AddScoped<IProjectManagerService, ProjectManagerService>();
    builder.Services.AddScoped<IProjectContextBuilder, ProjectContextBuilder>();
    builder.Services.AddHttpClient<IProjectManagerAiService, GeminiProjectManagerAiService>(client => client.Timeout = TimeSpan.FromSeconds(30));
    builder.Services.AddScoped<IReportsService, ReportsService>();
    builder.Services.AddScoped<IAuditLogService, AuditLogService>();
    builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
    builder.Services.AddScoped<IMicrosoftTokenValidator, MicrosoftTokenValidator>();
    builder.Services.AddScoped<IAdminConfigurationRepository, AdminConfigurationRepository>();
    builder.Services.AddScoped<IAdminProjectStatisticsRepository, AdminProjectStatisticsRepository>();
    builder.Services.AddScoped<IAzureDevOpsIntegrationRepository, AzureDevOpsIntegrationRepository>();
    builder.Services.AddScoped<IDashboardAnalyticsRepository, DashboardAnalyticsRepository>();
    builder.Services.AddScoped<IItAdminRepository, ItAdminRepository>();
    builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
    builder.Services.AddScoped<IProjectManagerRepository, ProjectManagerRepository>();
    builder.Services.AddScoped<IReportsRepository, ReportsRepository>();
    builder.Services.AddHttpClient<IAzureDevOpsClient, AzureDevOpsClient>();
    builder.Services.AddHostedService<AzureDevOpsSyncBackgroundService>();
    builder.Services.AddSignalR();

    // ── Controllers ───────────────────────────────────────────────────────────
    builder.Services.AddControllers();

    // ── Swagger / OpenAPI ─────────────────────────────────────────────────────
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Sarathi AI API",
            Version = "v1",
            Description = "AI-Powered Delivery Governance & KPI Dashboard"
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter your JWT token. Example: Bearer {token}"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    // ── Problem Details ───────────────────────────────────────────────────────
    builder.Services.AddProblemDetails();

    var app = builder.Build();

    // Apply EF Core migrations only when explicitly enabled. This is suitable for
    // a newly provisioned Azure SQL database; established environments should run
    // the reviewed migration bundle from the deployment pipeline instead.
    if (builder.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
    {
        using var migrationScope = app.Services.CreateScope();
        var dbContext = migrationScope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    // Startup banner
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine();
        Console.WriteLine("==============================================");
        Console.WriteLine("🚀 Sarathi AI API Started Successfully!");
        Console.WriteLine($"🌍 Environment : {app.Environment.EnvironmentName}");

        foreach (var url in app.Urls)
        {
            Console.WriteLine($"🌐 Listening   : {url}");
        }

        Console.WriteLine("📄 Swagger     : /swagger");
        Console.WriteLine("==============================================");
        Console.ResetColor();
    });

    // ── Middleware Pipeline ───────────────────────────────────────────────────
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sarathi AI API v1"));
    }

    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
            var schemaNotInitialized = exception is SqlException { Number: 208 };
            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = schemaNotInitialized
                ? StatusCodes.Status503ServiceUnavailable
                : StatusCodes.Status500InternalServerError;

            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7807",
                title = schemaNotInitialized ? "Database schema is not initialized" : "Internal Server Error",
                status = context.Response.StatusCode,
                detail = schemaNotInitialized
                    ? "Sarathi governance tables are not available. Apply the database bootstrap before using dashboards or synchronization."
                    : "An unexpected error occurred. Please try again later.",
                instance = context.Request.Path.ToString()
            });
        });
    });

    var httpsPortConfigured = !string.IsNullOrWhiteSpace(builder.Configuration["HTTPS_PORT"])
        || !string.IsNullOrWhiteSpace(builder.Configuration["ASPNETCORE_HTTPS_PORT"]);

    if (httpsPortConfigured)
    {
        app.UseHttpsRedirection();
    }
    app.UseCors("AllowFrontend");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHub<NotificationHub>("/hubs/notifications");

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Sarathi API terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}
