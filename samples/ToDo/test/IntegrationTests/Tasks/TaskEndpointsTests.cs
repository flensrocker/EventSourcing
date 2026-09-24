using System.Net;
using System.Net.Http.Json;
using Flensrocker.ToDo.WebApi;
using static Flensrocker.ToDo.WebApi.Tasks.TaskEndpoints;

namespace Flensrocker.ToDo.IntegrationTests.Tasks;

[TestClass]
public class TaskEndpointsTests
{
  private static ToDoWebApplicationFactory _factory = default!;

  private readonly CancellationToken _cancellationToken;
  private HttpClient _client = default!;

  public TaskEndpointsTests(TestContext testContext)
  {
    _cancellationToken = testContext.CancellationTokenSource.Token;
  }

  [ClassInitialize]
  public static async Task ClassInitialize(TestContext testContext)
  {
    _factory = await ToDoWebApplicationFactory.StartAsync(testContext.CancellationToken);
  }

  [ClassCleanup]
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

  [TestMethod]
  public async Task GivenValidNoteRequestThenReturnCreated()
  {
    var request = new NoteDto { Title = "Some Title" };
    using var response = await _client.PostAsJsonAsync($"{ToDoRoutes.TaskRoute}/note", request, _cancellationToken).ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
    var notedResponse = await response.Content.ReadFromJsonAsync<NotedDto>();
    Assert.IsNotNull(notedResponse);
    Assert.AreEqual(1, notedResponse.Version);
    Assert.AreNotEqual(Guid.Empty, notedResponse.TaskId);
  }

  [TestMethod]
  public async Task GivenInvalidNoteRequestThenReturnBadRequest()
  {
    var request = new NoteDto { Title = null };
    using var response = await _client.PostAsJsonAsync($"{ToDoRoutes.TaskRoute}/note", request, _cancellationToken).ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
  }
}
