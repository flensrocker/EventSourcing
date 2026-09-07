using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flensrocker.EventSourcing.Persistence.PostgreSQL.Migrations
{
  /// <inheritdoc />
  public partial class Init : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.EnsureSchema(name: "EventSourcing");

      migrationBuilder.CreateSequence(name: "GlobalMessageIndex", schema: "EventSourcing");

      migrationBuilder.CreateTable(
        name: "Message",
        schema: "EventSourcing",
        columns: table => new
        {
          Id = table.Column<Guid>(type: "uuid", nullable: false),
          GlobalIndex = table.Column<long>(
            type: "bigint",
            nullable: false,
            defaultValueSql: "nextval('\"EventSourcing\".\"GlobalMessageIndex\"')"
          ),
          StreamType = table.Column<string>(
            type: "character varying(250)",
            maxLength: 250,
            nullable: false
          ),
          StreamId = table.Column<Guid>(type: "uuid", nullable: false),
          StreamVersion = table.Column<int>(type: "integer", nullable: false),
          TenantId = table.Column<Guid>(type: "uuid", nullable: true),
          CreatedAt = table.Column<DateTimeOffset>(
            type: "timestamp with time zone",
            nullable: false
          ),
          CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
          TraceId = table.Column<byte[]>(type: "bytea", maxLength: 16, nullable: true),
          SpanId = table.Column<byte[]>(type: "bytea", maxLength: 8, nullable: true),
          PayloadJson = table.Column<string>(type: "jsonb", nullable: false),
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Message", x => x.Id);
        }
      );

      migrationBuilder.CreateIndex(
        name: "IX_Message_GlobalIndex",
        schema: "EventSourcing",
        table: "Message",
        column: "GlobalIndex",
        unique: true
      );

      migrationBuilder.CreateIndex(
        name: "IX_Message_StreamType_GlobalIndex",
        schema: "EventSourcing",
        table: "Message",
        columns: new[] { "StreamType", "GlobalIndex" }
      );

      migrationBuilder
        .CreateIndex(
          name: "IX_Message_StreamType_StreamId_StreamVersion",
          schema: "EventSourcing",
          table: "Message",
          columns: new[] { "StreamType", "StreamId", "StreamVersion" },
          unique: true
        )
        .Annotation("Npgsql:IndexInclude", new[] { "TenantId" });

      migrationBuilder.CreatePublication();
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropPublication();

      migrationBuilder.DropTable(name: "Message", schema: "EventSourcing");

      migrationBuilder.DropSequence(name: "GlobalMessageIndex", schema: "EventSourcing");
    }
  }
}
