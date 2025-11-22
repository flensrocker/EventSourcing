using System.Reflection;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flensrocker.EventSourcing.Orleans.PostgreSQL.Migrations
{
  /// <inheritdoc />
  public partial class OrleansInit : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      string[] sqlScriptNames =
      [
        "Flensrocker.EventSourcing.Orleans.PostgreSQL.SqlScripts.main.sql",
        "Flensrocker.EventSourcing.Orleans.PostgreSQL.SqlScripts.clustering.sql",
        "Flensrocker.EventSourcing.Orleans.PostgreSQL.SqlScripts.persistence.sql",
        "Flensrocker.EventSourcing.Orleans.PostgreSQL.SqlScripts.reminders.sql",
        "Flensrocker.EventSourcing.Orleans.PostgreSQL.SqlScripts.streaming.sql",
      ];

      foreach (var sqlScriptName in sqlScriptNames)
      {
        using var sqlStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(sqlScriptName);
        using var scriptReader = new StreamReader(sqlStream);
        var sqlScript = scriptReader.ReadToEnd();

        migrationBuilder.Sql(sqlScript);
      }
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
  }
}
