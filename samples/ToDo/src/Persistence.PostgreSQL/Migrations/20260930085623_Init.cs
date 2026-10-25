using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flensrocker.ToDo.Persistence.PostgreSQL.Migrations
{
  /// <inheritdoc />
  public partial class Init : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.EnsureSchema(name: "ToDo");

      migrationBuilder.CreateTable(
        name: "TaskDailyStats",
        schema: "ToDo",
        columns: table => new
        {
          DailyStatsId = table.Column<string>(type: "character varying(72)", maxLength: 72, nullable: false),
          Day = table.Column<DateOnly>(type: "date", nullable: false),
          TenantId = table.Column<Guid>(type: "uuid", nullable: true),
          UserId = table.Column<Guid>(type: "uuid", nullable: true),
          CountOpened = table.Column<int>(type: "integer", nullable: false),
          CountCompleted = table.Column<int>(type: "integer", nullable: false),
          CountDismissed = table.Column<int>(type: "integer", nullable: false),
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_TaskDailyStats", x => x.DailyStatsId);
        }
      );

      migrationBuilder.CreateTable(
        name: "TaskListItem",
        schema: "ToDo",
        columns: table => new
        {
          TaskId = table.Column<Guid>(type: "uuid", nullable: false),
          Version = table.Column<int>(type: "integer", nullable: false),
          TenantId = table.Column<Guid>(type: "uuid", nullable: true),
          OpenedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
          Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
          Status = table.Column<int>(type: "integer", nullable: false),
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_TaskListItem", x => x.TaskId);
        }
      );

      migrationBuilder.CreateIndex(
        name: "IX_TaskListItem_TenantId_TaskId",
        schema: "ToDo",
        table: "TaskListItem",
        columns: new[] { "TenantId", "TaskId" },
        unique: true
      );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(name: "TaskDailyStats", schema: "ToDo");

      migrationBuilder.DropTable(name: "TaskListItem", schema: "ToDo");
    }
  }
}
