using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Npgsql;

namespace Flensrocker.ToDo.IntegrationTests;

public class ToDoWebApplicationFactory : WebApplicationFactory<Program>
{
  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    var databaseName = GetRandomTestDatabaseName();
    var csBuilder = new NpgsqlConnectionStringBuilder(ToDoPostgreSQLContainer.Instance.GetConnectionString()) { Database = databaseName };
    builder.UseSetting("ConnectionStrings:DefaultConnection", csBuilder.ConnectionString);
  }

  private const string _databaseNameChoices = "abcdefghijklmnopqrstuvwxyz";

  private static string GetRandomTestDatabaseName() =>
    $"test_{RandomNumberGenerator.GetString(_databaseNameChoices, 20)}_{Environment.Version.Major}";
}
