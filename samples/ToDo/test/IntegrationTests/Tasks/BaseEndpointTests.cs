namespace Flensrocker.ToDo.IntegrationTests.Tasks;

public abstract class BaseEndpointTests<T>
  where T : BaseEndpointTests<T>
{
  protected static ToDoWebApplicationFactory _factory = default!;

  protected readonly TestContext _testContext;
  protected readonly CancellationToken _cancellationToken;
  protected HttpClient _client = default!;

  protected BaseEndpointTests(TestContext testContext)
  {
    _testContext = testContext;
    _cancellationToken = testContext.CancellationTokenSource.Token;
  }

  [ClassInitialize(InheritanceBehavior.BeforeEachDerivedClass)]
  public static void ClassInitialize(TestContext testContext)
  {
    _factory = new ToDoWebApplicationFactory();
  }

  [ClassCleanup(InheritanceBehavior.BeforeEachDerivedClass)]
  public static async Task ClassCleanup()
  {
    await _factory.DisposeAsync();
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
