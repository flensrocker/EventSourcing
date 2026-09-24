using System.Net;
using System.Net.Http.Json;
using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.ToDo.Abstractions.Tasks;
using Flensrocker.ToDo.Domain.Tasks;
using Flensrocker.ToDo.WebApi;
using Microsoft.Extensions.DependencyInjection;
using static Flensrocker.ToDo.WebApi.Tasks.TaskEndpoints;

namespace Flensrocker.ToDo.IntegrationTests.Tasks;

[TestClass]
public class NoteEndpointTests
{
  private static ToDoWebApplicationFactory _factory = default!;

  private readonly CancellationToken _cancellationToken;
  private HttpClient _client = default!;

  public NoteEndpointTests(TestContext testContext)
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
    var testRequest = new NoteDto { Title = "Some Title" };

    using var testResponse = await _client
      .PostAsJsonAsync($"{ToDoRoutes.TaskRoute}/note", testRequest, _cancellationToken)
      .ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.Created, testResponse.StatusCode);
    var noteResponse = await testResponse.Content.ReadFromJsonAsync<NotedDto>();
    Assert.IsNotNull(noteResponse);
    Assert.AreEqual(1, noteResponse.Version);
    Assert.AreNotEqual(Guid.Empty, noteResponse.TaskId);

    await using var assertScope = _factory.Services.CreateAsyncScope();
    var eventStore = assertScope.ServiceProvider.GetRequiredService<IDomainEventStorage<TaskEvent>>();

    var (stateVersion, commandState) = await eventStore.ReadStateFromStorage<TaskCommandState>(noteResponse.TaskId, _cancellationToken);

    Assert.AreEqual(noteResponse.Version, stateVersion);
    Assert.AreEqual(noteResponse.Version, commandState.Version);
    Assert.AreEqual(Abstractions.Tasks.TaskStatus.Open, commandState.Status);
    Assert.IsNull(commandState.DismissReason);
  }

  [TestMethod]
  public async Task GivenInvalidNoteRequestThenReturnBadRequest()
  {
    var testRequest = new NoteDto { Title = null };

    using var testResponse = await _client
      .PostAsJsonAsync($"{ToDoRoutes.TaskRoute}/note", testRequest, _cancellationToken)
      .ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.BadRequest, testResponse.StatusCode);
    // TODO inspect Content for ValidationProblems
  }
}
