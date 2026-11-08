using System.Security.Cryptography;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Flensrocker.EventSourcing.Test.Utilities;

public static class TestPostgreSQLContainer
{
  private static readonly SemaphoreSlim _instanceLock = new(1);
  private static PostgreSqlContainer? _instance = null;

  public static async Task InitializeAsync(CancellationToken cancellationToken)
  {
    await _instanceLock.WaitAsync(cancellationToken).ConfigureAwait(false);
    try
    {
      if (_instance is not null)
        return;

      _instance = new PostgreSqlBuilder("postgres:18-alpine").WithCommand("-c", "wal_level=logical").Build();
      await _instance.StartAsync(cancellationToken).ConfigureAwait(false);
    }
    finally
    {
      _instanceLock.Release();
    }
  }

  public static async Task CleanupAsync()
  {
    if (_instance is null)
      return;

    await _instanceLock.WaitAsync().ConfigureAwait(false);
    try
    {
      await _instance.DisposeAsync().ConfigureAwait(false);
      _instance = null;
    }
    finally
    {
      _instanceLock.Release();
    }
  }

  public static string GetConnectionString(string? databaseName = null)
  {
    if (_instance is null)
      throw new InvalidOperationException("Test-Container is not initialized");

    _instanceLock.Wait();
    try
    {
      databaseName ??= GetRandomTestDatabaseName();
      var csBuilder = new NpgsqlConnectionStringBuilder(_instance.GetConnectionString()) { Database = databaseName };
      return csBuilder.ConnectionString;
    }
    finally
    {
      _instanceLock.Release();
    }
  }

  private static string GetRandomTestDatabaseName()
  {
    const string _databaseNameChoices = "abcdefghijklmnopqrstuvwxyz";
    return $"test_{RandomNumberGenerator.GetString(_databaseNameChoices, 20)}_{Environment.Version.Major}";
  }
}
