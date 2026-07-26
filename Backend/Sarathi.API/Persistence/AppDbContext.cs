using Microsoft.EntityFrameworkCore;
using Sarathi.API.Domain.Entities;
using Sarathi.API.Domain.Enums;
namespace Sarathi.API.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<LoginAudit> LoginAudits => Set<LoginAudit>();
    public DbSet<AdminConfiguration> AdminConfigurations => Set<AdminConfiguration>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<KpiSnapshot> KpiSnapshots => Set<KpiSnapshot>();
    public DbSet<ProjectRiskAnalysis> ProjectRiskAnalyses => Set<ProjectRiskAnalysis>();
    public DbSet<ProjectManagerAssignment> ProjectManagerAssignments => Set<ProjectManagerAssignment>();
    public DbSet<Sprint> Sprints => Set<Sprint>();
    public DbSet<WorkItem> WorkItems => Set<WorkItem>();
    public DbSet<Repository> Repositories => Set<Repository>();
    public DbSet<Build> Builds => Set<Build>();
    public DbSet<AzureDevOpsSyncJob> AzureDevOpsSyncJobs => Set<AzureDevOpsSyncJob>();
    public DbSet<AdminLogEntry> AdminLogEntries => Set<AdminLogEntry>();
    public DbSet<MaintenanceTask> MaintenanceTasks => Set<MaintenanceTask>();
    public DbSet<AutomationSchedule> AutomationSchedules => Set<AutomationSchedule>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

       modelBuilder.Entity<User>(entity =>
{
    entity.ToTable("Loginusers");

    entity.HasKey(u => u.UserId);

    entity.Property(u => u.UserId)
        .HasColumnName("Id");

    entity.Property(u => u.Name)
        .HasColumnName("UserName")
        .IsRequired()
        .HasMaxLength(256);

    entity.Property(u => u.Email)
        .HasColumnName("Email")
        .IsRequired()
        .HasMaxLength(256);

    entity.HasIndex(u => u.Email)
        .IsUnique();

    entity.Property(u => u.EntraObjectId)
        .HasColumnName("EntraObjectId")
        .IsRequired()
        .HasMaxLength(100);

    entity.HasIndex(u => u.EntraObjectId)
        .IsUnique();

    entity.Property(u => u.Role)
        .HasColumnName("roles")
        .HasConversion(
            v => v.ToString().ToUpper(),
            v => Enum.Parse<UserRole>(v, true))
        .IsRequired();
});
        modelBuilder.Entity<LoginAudit>(entity =>
        {
            entity.HasKey(a => a.AuditId);

            entity.Property(a => a.AuditId)
                .HasDefaultValueSql("NEWID()");

            entity.Property(a => a.LoginTime)
                .IsRequired();

            entity.Property(a => a.IPAddress)
                .IsRequired()
                .HasMaxLength(64);

            entity.Property(a => a.Status)
                .IsRequired()
                .HasMaxLength(64);

            entity.HasOne(a => a.User)
                .WithMany(u => u.LoginAudits)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AdminConfiguration>(entity =>
        {
            entity.ToTable("AdminConfigurations");

            entity.HasKey(item => item.Id);

            entity.Property(item => item.AzureDevOpsOrganizationUrl)
                .HasMaxLength(500);

            entity.Property(item => item.AzureDevOpsProjectFilter)
                .HasMaxLength(256);

            entity.Property(item => item.AzureDevOpsPatCipherText)
                .HasMaxLength(4000);

            entity.Property(item => item.SprintVelocityTarget).HasPrecision(18, 2);
            entity.Property(item => item.CompletionRateTarget).HasPrecision(18, 2);
            entity.Property(item => item.DefectDensityThreshold).HasPrecision(18, 2);
            entity.Property(item => item.BacklogHealthThreshold).HasPrecision(18, 2);
            entity.Property(item => item.ReleaseSuccessRateTarget).HasPrecision(18, 2);

            entity.Property(item => item.AzureDevOpsSyncEnabled)
                .IsRequired();

            entity.Property(item => item.UpdatedAtUtc)
                .IsRequired();
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("Projects");

            entity.HasKey(item => item.ProjectId);

            entity.Property(item => item.ProjectName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(item => item.AzureProjectId)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(item => item.Visibility)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(item => item.Description)
                .HasMaxLength(2000);

            entity.HasIndex(item => item.AzureProjectId)
                .IsUnique();
        });

        modelBuilder.Entity<ProjectManagerAssignment>(entity =>
        {
            entity.ToTable("ProjectManagerAssignments");

            entity.HasKey(item => item.Id);

            entity.Property(item => item.ProjectRole)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(item => item.AllocationPercent)
                .HasColumnType("decimal(5,2)");

            entity.Property(item => item.DeliveryHealth)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(item => item.AssignedAtUtc)
                .IsRequired();

            entity.HasIndex(item => new { item.ProjectManagerUserId, item.ProjectId })
                .IsUnique();

            entity.HasOne(item => item.Project)
                .WithMany(project => project.ProjectManagerAssignments)
                .HasForeignKey(item => item.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Sprint>(entity =>
        {
            entity.ToTable("Sprints");

            entity.HasKey(item => item.SprintId);

            entity.Property(item => item.SprintName)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(item => item.AzureIterationId)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(item => item.Status)
                .IsRequired()
                .HasMaxLength(30);

            entity.HasIndex(item => new { item.ProjectId, item.AzureIterationId })
                .IsUnique();

            entity.HasOne(item => item.Project)
                .WithMany(project => project.Sprints)
                .HasForeignKey(item => item.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WorkItem>(entity =>
        {
            entity.ToTable("WorkItems");

            entity.HasKey(item => item.WorkItemId);

            entity.Property(item => item.AzureWorkItemId)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(item => item.Title)
                .IsRequired()
                .HasMaxLength(300);

            entity.Property(item => item.WorkItemType)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(item => item.State)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(item => item.Priority)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(item => item.AssignedToName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(item => item.StoryPoints)
                .HasColumnType("decimal(10,2)");

            entity.Property(item => item.LastUpdatedUtc)
                .IsRequired();

            entity.HasIndex(item => item.AzureWorkItemId)
                .IsUnique();

            entity.HasOne(item => item.Project)
                .WithMany(project => project.WorkItems)
                .HasForeignKey(item => item.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(item => item.Sprint)
                .WithMany(sprint => sprint.WorkItems)
                .HasForeignKey(item => item.SprintId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Repository>(entity =>
        {
            entity.ToTable("Repositories");
            entity.HasKey(item => item.RepositoryId);
            entity.Property(item => item.AzureRepoId).IsRequired().HasMaxLength(100);
            entity.Property(item => item.RepositoryName).IsRequired().HasMaxLength(200);
            entity.Property(item => item.DefaultBranch).HasMaxLength(100);
            entity.Property(item => item.Url).HasMaxLength(500);
            entity.HasIndex(item => item.AzureRepoId).IsUnique();
            entity.HasOne(item => item.Project)
                .WithMany(project => project.Repositories)
                .HasForeignKey(item => item.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Build>(entity =>
        {
            entity.ToTable("Builds");
            entity.HasKey(item => item.BuildId);
            entity.Property(item => item.DefinitionName).IsRequired().HasMaxLength(200);
            entity.Property(item => item.BuildNumber).IsRequired().HasMaxLength(100);
            entity.Property(item => item.Status).IsRequired().HasMaxLength(50);
            entity.Property(item => item.Result).HasMaxLength(50);
            entity.Property(item => item.SourceBranch).IsRequired().HasMaxLength(200);
            entity.HasIndex(item => item.AzureBuildId).IsUnique();
            entity.HasOne(item => item.Project)
                .WithMany(project => project.Builds)
                .HasForeignKey(item => item.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AzureDevOpsSyncJob>(entity =>
        {
            entity.ToTable("AzureDevOpsSyncJobs");

            entity.HasKey(item => item.Id);

            entity.Property(item => item.SyncType)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(item => item.ScopeName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(item => item.Status)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(item => item.Source)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(item => item.TriggeredByDisplayName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(item => item.ErrorMessage)
                .HasMaxLength(2000);
        });

        modelBuilder.Entity<AdminLogEntry>(entity =>
        {
            entity.ToTable("AdminLogEntries");

            entity.HasKey(item => item.Id);

            entity.Property(item => item.Severity)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(item => item.Category)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(item => item.Message)
                .IsRequired()
                .HasMaxLength(2000);

            entity.Property(item => item.SourceSystem)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(item => item.CorrelationId)
                .HasMaxLength(100);
        });

        modelBuilder.Entity<MaintenanceTask>(entity =>
        {
            entity.ToTable("MaintenanceTasks");

            entity.HasKey(item => item.Id);

            entity.Property(item => item.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(item => item.EnvironmentName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(item => item.Status)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(item => item.OwnerName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(item => item.Notes)
                .HasMaxLength(2000);
        });

        modelBuilder.Entity<AutomationSchedule>(entity =>
        {
            entity.ToTable("AutomationSchedules");

            entity.HasKey(item => item.Id);

            entity.Property(item => item.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(item => item.JobType)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(item => item.ScopeName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(item => item.Source)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(item => item.CronExpression)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(item => item.IntervalMinutes)
                .IsRequired();

            entity.Property(item => item.TimeZoneId)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(item => item.RunWindow)
                .IsRequired()
                .HasMaxLength(100);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("Notifications");

            entity.HasKey(item => item.Id);

            entity.Property(item => item.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(item => item.Message)
                .IsRequired()
                .HasMaxLength(2000);

            entity.Property(item => item.Category)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(item => item.Severity)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(item => item.ActionUrl)
                .HasMaxLength(500);

            entity.HasIndex(item => new { item.UserId, item.IsRead, item.CreatedAtUtc });

            entity.HasOne(item => item.User)
                .WithMany(user => user.Notifications)
                .HasForeignKey(item => item.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<KpiSnapshot>(entity =>
        {
            entity.ToTable("KPISnapshots");

            entity.HasKey(item => item.Id);

            entity.Property(item => item.SnapshotDate)
                .IsRequired();

            entity.Property(item => item.SprintVelocity)
                .HasColumnType("decimal(10,2)");

            entity.Property(item => item.CompletionRate)
                .HasColumnType("decimal(10,2)");

            entity.Property(item => item.DefectDensity)
                .HasColumnType("decimal(10,2)");

            entity.Property(item => item.BacklogHealth)
                .HasColumnType("decimal(10,2)");

            entity.Property(item => item.ReleaseSuccessRate)
                .HasColumnType("decimal(10,2)");

            entity.HasOne(item => item.Project)
                .WithMany(project => project.KpiSnapshots)
                .HasForeignKey(item => item.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProjectRiskAnalysis>(entity =>
        {
            entity.ToTable("RiskAnalysis");

            entity.HasKey(item => item.Id);

            entity.Property(item => item.RiskScore)
                .HasColumnType("decimal(10,2)");

            entity.Property(item => item.RiskLevel)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(item => item.RiskSummary)
                .HasMaxLength(2000);

            entity.Property(item => item.GeneratedDate)
                .IsRequired();

            entity.HasOne(item => item.Project)
                .WithMany(project => project.RiskAnalyses)
                .HasForeignKey(item => item.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
