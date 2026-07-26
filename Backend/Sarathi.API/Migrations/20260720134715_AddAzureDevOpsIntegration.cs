using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sarathi.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAzureDevOpsIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sprints_ProjectId",
                table: "Sprints");

            migrationBuilder.AddColumn<string>(
                name: "AzureIterationId",
                table: "Sprints",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "IntervalMinutes",
                table: "AutomationSchedules",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "AutomationSchedules",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AzureDevOpsPatCipherText",
                table: "AdminConfigurations",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "AzureDevOpsPatUpdatedAtUtc",
                table: "AdminConfigurations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AzureDevOpsSyncEnabled",
                table: "AdminConfigurations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_WorkItems_AzureWorkItemId",
                table: "WorkItems",
                column: "AzureWorkItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sprints_ProjectId_AzureIterationId",
                table: "Sprints",
                columns: new[] { "ProjectId", "AzureIterationId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkItems_AzureWorkItemId",
                table: "WorkItems");

            migrationBuilder.DropIndex(
                name: "IX_Sprints_ProjectId_AzureIterationId",
                table: "Sprints");

            migrationBuilder.DropColumn(
                name: "AzureIterationId",
                table: "Sprints");

            migrationBuilder.DropColumn(
                name: "IntervalMinutes",
                table: "AutomationSchedules");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "AutomationSchedules");

            migrationBuilder.DropColumn(
                name: "AzureDevOpsPatCipherText",
                table: "AdminConfigurations");

            migrationBuilder.DropColumn(
                name: "AzureDevOpsPatUpdatedAtUtc",
                table: "AdminConfigurations");

            migrationBuilder.DropColumn(
                name: "AzureDevOpsSyncEnabled",
                table: "AdminConfigurations");

            migrationBuilder.CreateIndex(
                name: "IX_Sprints_ProjectId",
                table: "Sprints",
                column: "ProjectId");
        }
    }
}
