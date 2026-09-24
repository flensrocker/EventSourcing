using System.Net;
using System.Net.Http.Json;
using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.ToDo.WebApi;
using static Flensrocker.ToDo.WebApi.Tasks.TaskEndpoints;

namespace Flensrocker.ToDo.IntegrationTests.Tasks;

[TestClass]
public class CompleteEndpointTests : BaseEndpointTests<CompleteEndpointTests>
{
  public CompleteEndpointTests(TestContext testContext)
    : base(testContext) { }

  [TestMethod]
  public async Task GivenOpenTaskThenReturnOk()
  {
    var noteRequest = new NoteDto { Title = "Some Title" };

    using var noteResponse = await _client
      .PostAsJsonAsync($"{ToDoRoutes.TaskRoute}/note", noteRequest, _cancellationToken)
      .ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.Created, noteResponse.StatusCode);
    var noteResult = await noteResponse.Content.ReadFromJsonAsync<NotedDto>();
    Assert.IsNotNull(noteResult);
    var taskId = noteResult.TaskId;

    using var testResponse = await _client
      .PutAsJsonAsync($"{ToDoRoutes.TaskRoute}/{taskId}/complete", (object?)null, _cancellationToken)
      .ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.OK, testResponse.StatusCode);
    var testResult = await testResponse.Content.ReadFromJsonAsync<ExecutionResult>(_cancellationToken);
    Assert.IsNotNull(testResult);
    Assert.AreEqual(noteResult.Version + 1, testResult.Version);
  }

  [TestMethod]
  public async Task GivenCompletedTaskThenReturnOkWithSameVersion()
  {
    var noteRequest = new NoteDto { Title = "Some Title" };

    using var noteResponse = await _client
      .PostAsJsonAsync($"{ToDoRoutes.TaskRoute}/note", noteRequest, _cancellationToken)
      .ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.Created, noteResponse.StatusCode);
    var noteResult = await noteResponse.Content.ReadFromJsonAsync<NotedDto>();
    Assert.IsNotNull(noteResult);
    var taskId = noteResult.TaskId;

    using var firstCompleteResponse = await _client
      .PutAsJsonAsync($"{ToDoRoutes.TaskRoute}/{taskId}/complete", (object?)null, _cancellationToken)
      .ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.OK, firstCompleteResponse.StatusCode);
    var firstCompleteResult = await firstCompleteResponse.Content.ReadFromJsonAsync<ExecutionResult>(_cancellationToken);
    Assert.IsNotNull(firstCompleteResult);
    Assert.AreEqual(noteResult.Version + 1, firstCompleteResult.Version);

    using var secondCompleteResponse = await _client
      .PutAsJsonAsync($"{ToDoRoutes.TaskRoute}/{taskId}/complete", (object?)null, _cancellationToken)
      .ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.OK, secondCompleteResponse.StatusCode);
    var secondCompleteResult = await secondCompleteResponse.Content.ReadFromJsonAsync<ExecutionResult>(_cancellationToken);
    Assert.IsNotNull(secondCompleteResult);
    Assert.AreEqual(firstCompleteResult.Version, secondCompleteResult.Version);
  }

  [TestMethod]
  public async Task GivenNotExistingTaskIdThenReturnNotFound()
  {
    var taskId = Guid.CreateVersion7();

    using var testResponse = await _client
      .PutAsJsonAsync($"{ToDoRoutes.TaskRoute}/{taskId}/complete", (object?)null, _cancellationToken)
      .ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.NotFound, testResponse.StatusCode);
  }
}
