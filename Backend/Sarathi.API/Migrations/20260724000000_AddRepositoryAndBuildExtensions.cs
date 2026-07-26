using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Sarathi.API.Persistence;

#nullable disable

namespace Sarathi.API.Migrations;

/// <summary>
/// Adds LastUpdatedUtc to Repositories and TriggerType to Builds
/// to support enhanced pipeline and repository tracking.
/// </summary>
[DbContext(typeof(AppDbContext))]
[Migration("20260724000000_AddRepositoryAndBuildExtensions")]
public partial class AddRepositoryAndBuildExtensions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_NAME = 'Repositories' AND COLUMN_NAME = 'LastUpdatedUtc')
            BEGIN
                ALTER TABLE [dbo].[Repositories]
                ADD [LastUpdatedUtc] datetime2 NULL;
            END;
            """);

        migrationBuilder.Sql("""
            IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_NAME = 'Builds' AND COLUMN_NAME = 'TriggerType')
            BEGIN
                ALTER TABLE [dbo].[Builds]
                ADD [TriggerType] nvarchar(100) NULL;
            END;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_NAME = 'Repositories' AND COLUMN_NAME = 'LastUpdatedUtc')
            BEGIN
                ALTER TABLE [dbo].[Repositories]
                DROP COLUMN [LastUpdatedUtc];
            END;
            """);

        migrationBuilder.Sql("""
            IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_NAME = 'Builds' AND COLUMN_NAME = 'TriggerType')
            BEGIN
                ALTER TABLE [dbo].[Builds]
                DROP COLUMN [TriggerType];
            END;
            """);
    }
}
