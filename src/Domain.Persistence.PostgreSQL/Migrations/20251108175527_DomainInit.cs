using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flensrocker.Domain.Persistence.PostgreSQL.Migrations
{
  /// <inheritdoc />
  public partial class DomainInit : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.EnsureSchema(
        name: "Domain");

      migrationBuilder.CreateSequence(
        name: "GlobalIndex",
        schema: "Domain");

      migrationBuilder.CreateTable(
        name: "DomainEvent",
        schema: "Domain",
        columns: table => new
        {
          EventId = table.Column<Guid>(type: "uuid", nullable: false),
          GlobalIndex = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('\"Domain\".\"GlobalIndex\"')"),
          StreamType = table.Column<string>(type: "text", nullable: false),
          StreamId = table.Column<Guid>(type: "uuid", nullable: false),
          StreamVersion = table.Column<int>(type: "integer", nullable: false),
          TenantId = table.Column<Guid>(type: "uuid", nullable: true),
          CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
          CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
          PayloadJson = table.Column<string>(type: "jsonb", nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_DomainEvent", x => x.EventId);
        });

      migrationBuilder.CreateIndex(
        name: "IX_DomainEvent_GlobalIndex",
        schema: "Domain",
        table: "DomainEvent",
        column: "GlobalIndex",
        unique: true);

      migrationBuilder.CreateIndex(
        name: "IX_DomainEvent_StreamType_StreamId_StreamVersion",
        schema: "Domain",
        table: "DomainEvent",
        columns: new[] { "StreamType", "StreamId", "StreamVersion" },
        unique: true)
        .Annotation("Npgsql:IndexInclude", new[] { "TenantId" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
        name: "DomainEvent",
        schema: "Domain");

      migrationBuilder.DropSequence(
        name: "GlobalIndex",
        schema: "Domain");
    }
  }
}
