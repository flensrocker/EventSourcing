using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace Flensrocker.ToDo.IntegrationTests;

internal class ToDoWebApplicationFactory(PostgreSqlContainer postgres) : WebApplicationFactory<Program>
{
  internal static async Task<ToDoWebApplicationFactory> StartAsync(CancellationToken cancellationToken)
  {
    var databaseName = GetRandomTestDatabaseName();

    var postgres = new PostgreSqlBuilder("postgres:18-alpine").WithCommand("-c", "wal_level=logical").WithDatabase(databaseName).Build();
    await postgres.StartAsync(cancellationToken);

    return new ToDoWebApplicationFactory(postgres);
  }

  private readonly PostgreSqlContainer _postgres = postgres;

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder.UseSetting("ConnectionStrings:DefaultConnection", _postgres.GetConnectionString());
  }

  public override async ValueTask DisposeAsync()
  {
    await base.DisposeAsync();
    await _postgres.DisposeAsync();
  }

  private const string _databaseNameChoices = "abcdefghijklmnopqrstuvwxyz";

  private static string GetRandomTestDatabaseName() =>
    $"test_{RandomNumberGenerator.GetString(_databaseNameChoices, 20)}_{Environment.Version.Major}";
}
