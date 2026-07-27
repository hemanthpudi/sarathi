using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Sarathi.API.Persistence;

#nullable disable

namespace Sarathi.API.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260722120000_AddRepositoriesAndBuilds")]
public partial class AddRepositoriesAndBuilds : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Builds",
            columns: table => new
            {
                BuildId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                AzureBuildId = table.Column<int>(type: "int", nullable: false),
                ProjectId = table.Column<int>(type: "int", nullable: false),
                DefinitionName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                BuildNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Result = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                SourceBranch = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                FinishTime = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Builds", x => x.BuildId);
                table.ForeignKey(
                    name: "FK_Builds_Projects_ProjectId",
                    column: x => x.ProjectId,
                    principalTable: "Projects",
                    principalColumn: "ProjectId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Repositories",
            columns: table => new
            {
                RepositoryId = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                AzureRepoId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                ProjectId = table.Column<int>(type: "int", nullable: false),
                RepositoryName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                DefaultBranch = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                Size = table.Column<long>(type: "bigint", nullable: true),
                Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Repositories", x => x.RepositoryId);
                table.ForeignKey(
                    name: "FK_Repositories_Projects_ProjectId",
                    column: x => x.ProjectId,
                    principalTable: "Projects",
                    principalColumn: "ProjectId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Builds_AzureBuildId",
            table: "Builds",
            column: "AzureBuildId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Builds_ProjectId",
            table: "Builds",
            column: "ProjectId");

        migrationBuilder.CreateIndex(
            name: "IX_Repositories_AzureRepoId",
            table: "Repositories",
            column: "AzureRepoId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Repositories_ProjectId",
            table: "Repositories",
            column: "ProjectId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Builds");
        migrationBuilder.DropTable(name: "Repositories");
    }
}
