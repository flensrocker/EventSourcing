using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flensrocker.ToDo.Persistence.PostgreSQL.Migrations
{
  /// <inheritdoc />
  public partial class ToDoDailyStats : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
        name: "TaskDailyStats",
        schema: "ToDo",
        columns: table => new
        {
          Day = table.Column<DateOnly>(type: "date", nullable: false),
          TenantId = table.Column<Guid>(type: "uuid", nullable: false),
          UserId = table.Column<Guid>(type: "uuid", nullable: false),
          CountNoted = table.Column<int>(type: "integer", nullable: false),
          CountCompleted = table.Column<int>(type: "integer", nullable: false),
          CountDismissed = table.Column<int>(type: "integer", nullable: false),
        },
        constraints: table =>
        {
          table.PrimaryKey(
            "PK_TaskDailyStats",
            x => new
            {
              x.Day,
              x.TenantId,
              x.UserId,
            }
          );
        }
      );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(name: "TaskDailyStats", schema: "ToDo");
    }
  }
}
