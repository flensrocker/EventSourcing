using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flensrocker.ToDo.Persistence.PostgreSQL.Migrations
{
  /// <inheritdoc />
  public partial class ToDoInit : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.EnsureSchema(name: "ToDo");

      migrationBuilder.CreateTable(
        name: "TaskListItem",
        schema: "ToDo",
        columns: table => new
        {
          TaskId = table.Column<Guid>(type: "uuid", nullable: false),
          Version = table.Column<int>(type: "integer", nullable: false),
          TenantId = table.Column<Guid>(type: "uuid", nullable: true),
          CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
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
      migrationBuilder.DropTable(name: "TaskListItem", schema: "ToDo");
    }
  }
}
