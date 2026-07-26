using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sarathi.API.Migrations;

/// <summary>
/// Repairs environments where the original repository/build migration was
/// recorded but its tables were not created. Every statement is conditional,
/// making the migration safe for databases where the tables already exist.
/// </summary>
public partial class EnsureRepositoriesAndBuildsExist : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF OBJECT_ID(N'[dbo].[Repositories]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[Repositories] (
                    [RepositoryId] int NOT NULL IDENTITY,
                    [AzureRepoId] nvarchar(100) NOT NULL,
                    [ProjectId] int NOT NULL,
                    [RepositoryName] nvarchar(200) NOT NULL,
                    [DefaultBranch] nvarchar(100) NULL,
                    [Size] bigint NULL,
                    [Url] nvarchar(500) NULL,
                    CONSTRAINT [PK_Repositories] PRIMARY KEY ([RepositoryId]),
                    CONSTRAINT [FK_Repositories_Projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[Projects] ([ProjectId]) ON DELETE CASCADE
                );
            END;
            IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Repositories_AzureRepoId' AND object_id = OBJECT_ID(N'[dbo].[Repositories]'))
                CREATE UNIQUE INDEX [IX_Repositories_AzureRepoId] ON [dbo].[Repositories] ([AzureRepoId]);
            IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Repositories_ProjectId' AND object_id = OBJECT_ID(N'[dbo].[Repositories]'))
                CREATE INDEX [IX_Repositories_ProjectId] ON [dbo].[Repositories] ([ProjectId]);
            """);

        migrationBuilder.Sql("""
            IF OBJECT_ID(N'[dbo].[Builds]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[Builds] (
                    [BuildId] int NOT NULL IDENTITY,
                    [AzureBuildId] int NOT NULL,
                    [ProjectId] int NOT NULL,
                    [DefinitionName] nvarchar(200) NOT NULL,
                    [BuildNumber] nvarchar(100) NOT NULL,
                    [Status] nvarchar(50) NOT NULL,
                    [Result] nvarchar(50) NULL,
                    [SourceBranch] nvarchar(200) NOT NULL,
                    [StartTime] datetime2 NOT NULL,
                    [FinishTime] datetime2 NULL,
                    CONSTRAINT [PK_Builds] PRIMARY KEY ([BuildId]),
                    CONSTRAINT [FK_Builds_Projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[Projects] ([ProjectId]) ON DELETE CASCADE
                );
            END;
            IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Builds_AzureBuildId' AND object_id = OBJECT_ID(N'[dbo].[Builds]'))
                CREATE UNIQUE INDEX [IX_Builds_AzureBuildId] ON [dbo].[Builds] ([AzureBuildId]);
            IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Builds_ProjectId' AND object_id = OBJECT_ID(N'[dbo].[Builds]'))
                CREATE INDEX [IX_Builds_ProjectId] ON [dbo].[Builds] ([ProjectId]);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // This is a repair migration; dropping tables during a rollback would
        // remove synchronized production data, so rollback is intentionally a no-op.
    }
}
