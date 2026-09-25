namespace Flensrocker.ToDo.IntegrationTests;

// NOTE BEST PRACTICE
// The generic argument is used to give all the derived classes a unique base class.
// With this they have different static properties and therefore different ToDoWebApplicationFactories with distinct databases.
public abstract class ToDoBaseEndpointTests<T>
  where T : ToDoBaseEndpointTests<T>
{
  protected static ToDoWebApplicationFactory _factory = default!;

  protected readonly TestContext _testContext;
  protected readonly CancellationToken _cancellationToken;
  protected HttpClient _client = default!;

  protected ToDoBaseEndpointTests(TestContext testContext)
  {
    _testContext = testContext;
    _cancellationToken = testContext.CancellationTokenSource.Token;
  }

  [ClassInitialize(InheritanceBehavior.BeforeEachDerivedClass)]
  public static void ClassInitialize(TestContext testContext)
  {
    Assert.IsNull(_factory);
    _factory = new ToDoWebApplicationFactory();
  }

  [ClassCleanup(InheritanceBehavior.BeforeEachDerivedClass)]
  public static async Task ClassCleanup()
  {
    await _factory.DisposeAsync().ConfigureAwait(false);
    _factory = default!;
  }

  [TestInitialize]
  public void TestInitialize()
  {
    _client = _factory.CreateClient();
  }

  [TestCleanup]
  public void TestCleanup()
  {
    _client?.Dispose();
    _client = default!;
  }
}
