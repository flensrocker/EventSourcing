using Testcontainers.PostgreSql;

[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.ClassLevel)]

namespace Flensrocker.ToDo.IntegrationTests;

[TestClass]
public static class ToDoPostgreSQLContainer
{
  public static PostgreSqlContainer Instance = default!;

  [AssemblyInitialize]
  public static async Task InitializeAsync(TestContext testContext)
  {
    Assert.IsNull(Instance);
    Instance = new PostgreSqlBuilder("postgres:18-alpine").WithCommand("-c", "wal_level=logical").Build();
    await Instance.StartAsync(testContext.CancellationToken).ConfigureAwait(false);
  }

  [AssemblyCleanup]
  public static async Task CleanupAsync()
  {
    await Instance.DisposeAsync().ConfigureAwait(false);
    Instance = default!;
  }
}
