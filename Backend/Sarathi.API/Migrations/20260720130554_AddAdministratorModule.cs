using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sarathi.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAdministratorModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Reconcile deployments where Loginusers was created before EF ran
            // this migration, preserving users and login-audit relationships.
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'[dbo].[Loginusers]', N'U') IS NULL
                BEGIN
                    ALTER TABLE [dbo].[LoginAudits] DROP CONSTRAINT [FK_LoginAudits_Users_UserId];
                    ALTER TABLE [dbo].[Users] DROP CONSTRAINT [PK_Users];
                    ALTER TABLE [dbo].[Users] DROP COLUMN [IsActive];
                    EXEC sp_rename N'[dbo].[Users]', N'Loginusers';
                    EXEC sp_rename N'[dbo].[Loginusers].[Role]', N'roles', N'COLUMN';
                    EXEC sp_rename N'[dbo].[Loginusers].[Name]', N'UserName', N'COLUMN';
                    EXEC sp_rename N'[dbo].[Loginusers].[UserId]', N'Id', N'COLUMN';
                    EXEC sp_rename N'[dbo].[Loginusers].[IX_Users_Email]', N'IX_Loginusers_Email', N'INDEX';
                    ALTER TABLE [dbo].[Loginusers] ALTER COLUMN [roles] nvarchar(max) NOT NULL;
                    ALTER TABLE [dbo].[Loginusers] ADD [EntraObjectId] nvarchar(100) NOT NULL CONSTRAINT [DF_Loginusers_EntraObjectId] DEFAULT N'';
                    ALTER TABLE [dbo].[Loginusers] ADD CONSTRAINT [PK_Loginusers] PRIMARY KEY ([Id]);
                END
                ELSE
                BEGIN
                    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_LoginAudits_Users_UserId')
                        ALTER TABLE [dbo].[LoginAudits] DROP CONSTRAINT [FK_LoginAudits_Users_UserId];

                    IF OBJECT_ID(N'[dbo].[Users]', N'U') IS NOT NULL
                    BEGIN
                        UPDATE audit
                        SET [UserId] = existing.[Id]
                        FROM [dbo].[LoginAudits] AS audit
                        INNER JOIN [dbo].[Users] AS legacy ON legacy.[UserId] = audit.[UserId]
                        INNER JOIN [dbo].[Loginusers] AS existing ON existing.[Email] = legacy.[Email];

                        INSERT INTO [dbo].[Loginusers] ([Id], [UserName], [Email], [EntraObjectId], [roles])
                        SELECT legacy.[UserId], legacy.[Name], legacy.[Email], CONVERT(nvarchar(100), legacy.[UserId]),
                            CASE legacy.[Role] WHEN 0 THEN N'ADMINISTRATOR' WHEN 1 THEN N'PROJECTMANAGER' ELSE N'ITADMIN' END
                        FROM [dbo].[Users] AS legacy
                        WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Loginusers] AS existing WHERE existing.[Email] = legacy.[Email]);

                        DROP TABLE [dbo].[Users];
                    END
                END
                """);

            migrationBuilder.CreateTable(
                name: "AdminConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KpiRefreshIntervalMinutes = table.Column<int>(type: "int", nullable: false),
                    SprintVelocityTarget = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CompletionRateTarget = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DefectDensityThreshold = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BacklogHealthThreshold = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReleaseSuccessRateTarget = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AzureDevOpsOrganizationUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AzureDevOpsProjectFilter = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    ProjectId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AzureProjectId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProjectName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Visibility = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.ProjectId);
                });

            migrationBuilder.CreateTable(
                name: "KPISnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    SnapshotDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SprintVelocity = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    CompletionRate = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    DefectDensity = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    BacklogHealth = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    ReleaseSuccessRate = table.Column<decimal>(type: "decimal(10,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KPISnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KPISnapshots_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RiskAnalysis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    RiskScore = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    RiskLevel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RiskSummary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    GeneratedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskAnalysis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RiskAnalysis_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Loginusers_EntraObjectId",
                table: "Loginusers",
                column: "EntraObjectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KPISnapshots_ProjectId",
                table: "KPISnapshots",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_AzureProjectId",
                table: "Projects",
                column: "AzureProjectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RiskAnalysis_ProjectId",
                table: "RiskAnalysis",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_LoginAudits_Loginusers_UserId",
                table: "LoginAudits",
                column: "UserId",
                principalTable: "Loginusers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoginAudits_Loginusers_UserId",
                table: "LoginAudits");

            migrationBuilder.DropTable(
                name: "AdminConfigurations");

            migrationBuilder.DropTable(
                name: "KPISnapshots");

            migrationBuilder.DropTable(
                name: "RiskAnalysis");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Loginusers",
                table: "Loginusers");

            migrationBuilder.DropIndex(
                name: "IX_Loginusers_EntraObjectId",
                table: "Loginusers");

            migrationBuilder.DropColumn(
                name: "EntraObjectId",
                table: "Loginusers");

            migrationBuilder.RenameTable(
                name: "Loginusers",
                newName: "Users");

            migrationBuilder.RenameColumn(
                name: "roles",
                table: "Users",
                newName: "Role");

            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "Users",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Users",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Loginusers_Email",
                table: "Users",
                newName: "IX_Users_Email");

            migrationBuilder.AlterColumn<int>(
                name: "Role",
                table: "Users",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Users",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_LoginAudits_Users_UserId",
                table: "LoginAudits",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
