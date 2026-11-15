using Flensrocker.EventSourcing.Test.Utilities;

[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]

namespace Flensrocker.EventSourcing.UnitTests;

[TestClass]
public static class TestSetup
{
  [AssemblyCleanup]
  public static async Task CleanupAsync()
  {
    await TestPostgreSQLContainer.CleanupAsync().ConfigureAwait(false);
  }
}
