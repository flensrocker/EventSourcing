using System.Reflection;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flensrocker.Domain.Orleans.PostgreSQL.Migrations
{
  /// <inheritdoc />
  public partial class OrleansInit : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      string[] sqlScriptNames =
      [
        "Flensrocker.Domain.Orleans.PostgreSQL.SqlScripts.main.sql",
        "Flensrocker.Domain.Orleans.PostgreSQL.SqlScripts.clustering.sql",
        "Flensrocker.Domain.Orleans.PostgreSQL.SqlScripts.persistence.sql",
        "Flensrocker.Domain.Orleans.PostgreSQL.SqlScripts.reminders.sql",
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
