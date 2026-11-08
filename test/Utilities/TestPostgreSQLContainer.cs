using System.Security.Cryptography;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Flensrocker.EventSourcing.Test.Utilities;

public static class TestPostgreSQLContainer
{
  private const string _databaseNameChoices = "abcdefghijklmnopqrstuvwxyz";

  public static PostgreSqlContainer Instance = new PostgreSqlBuilder("postgres:18-alpine").WithCommand("-c", "wal_level=logical").Build();

  public static async Task InitializeAsync(CancellationToken cancellationToken)
  {
    await Instance.StartAsync(cancellationToken).ConfigureAwait(false);
  }

  public static async Task CleanupAsync()
  {
    await Instance.DisposeAsync().ConfigureAwait(false);
  }

  public static string GetRandomTestDatabaseName()
  {
    return $"test_{RandomNumberGenerator.GetString(_databaseNameChoices, 20)}_{Environment.Version.Major}";
  }

  public static string GetConnectionString(string? databaseName = null)
  {
    databaseName ??= GetRandomTestDatabaseName();
    var csBuilder = new NpgsqlConnectionStringBuilder(Instance.GetConnectionString()) { Database = databaseName };
    return csBuilder.ConnectionString;
  }
}
