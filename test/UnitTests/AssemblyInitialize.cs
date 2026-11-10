using Flensrocker.EventSourcing.Test.Utilities;

[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.ClassLevel)]

namespace Flensrocker.EventSourcing.UnitTests;

[TestClass]
public static class AssemblyInitialize
{
  [AssemblyCleanup]
  public static async Task CleanupAsync()
  {
    await TestPostgreSQLContainer.CleanupAsync().ConfigureAwait(false);
  }
}
