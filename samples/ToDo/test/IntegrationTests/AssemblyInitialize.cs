using Flensrocker.EventSourcing.Test.Utilities;

[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.ClassLevel)]

namespace Flensrocker.ToDo.IntegrationTests;

[TestClass]
public static class AssemblyInitialize
{
  [AssemblyInitialize]
  public static async Task InitializeAsync(TestContext testContext)
  {
    await TestPostgreSQLContainer.InitializeAsync(testContext.CancellationToken).ConfigureAwait(false);
  }

  [AssemblyCleanup]
  public static async Task CleanupAsync()
  {
    await TestPostgreSQLContainer.CleanupAsync().ConfigureAwait(false);
  }
}
