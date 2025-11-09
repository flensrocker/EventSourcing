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
        name: "GlobalMessageIndex",
        schema: "Domain");

      migrationBuilder.CreateTable(
        name: "Message",
        schema: "Domain",
        columns: table => new
        {
          Id = table.Column<Guid>(type: "uuid", nullable: false),
          GlobalIndex = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "nextval('\"Domain\".\"GlobalMessageIndex\"')"),
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
          table.PrimaryKey("PK_Message", x => x.Id);
        });

      migrationBuilder.CreateIndex(
        name: "IX_Message_GlobalIndex",
        schema: "Domain",
        table: "Message",
        column: "GlobalIndex",
        unique: true);

      migrationBuilder.CreateIndex(
        name: "IX_Message_StreamType_StreamId_StreamVersion",
        schema: "Domain",
        table: "Message",
        columns: new[] { "StreamType", "StreamId", "StreamVersion" },
        unique: true)
        .Annotation("Npgsql:IndexInclude", new[] { "TenantId" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
        name: "Message",
        schema: "Domain");

      migrationBuilder.DropSequence(
        name: "GlobalMessageIndex",
        schema: "Domain");
    }
  }
}
