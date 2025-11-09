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
      migrationBuilder.EnsureSchema(
        name: "ToDo");

      migrationBuilder.CreateTable(
        name: "ListItem",
        schema: "ToDo",
        columns: table => new
        {
          ToDoId = table.Column<Guid>(type: "uuid", nullable: false),
          Version = table.Column<int>(type: "integer", nullable: false),
          TenantId = table.Column<Guid>(type: "uuid", nullable: true),
          CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
          Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
          Status = table.Column<int>(type: "integer", nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_ListItem", x => x.ToDoId);
        });

      migrationBuilder.CreateIndex(
        name: "IX_ListItem_TenantId_ToDoId",
        schema: "ToDo",
        table: "ListItem",
        columns: new[] { "TenantId", "ToDoId" },
        unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
        name: "ListItem",
        schema: "ToDo");
    }
  }
}
