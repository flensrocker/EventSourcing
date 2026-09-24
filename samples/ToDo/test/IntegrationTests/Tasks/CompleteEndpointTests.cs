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
    var noted = await noteResponse.Content.ReadFromJsonAsync<NotedDto>();
    Assert.IsNotNull(noted);
    var taskId = noted.TaskId;

    using var testResponse = await _client
      .PutAsJsonAsync($"{ToDoRoutes.TaskRoute}/{taskId}/complete", (object?)null, _cancellationToken)
      .ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.OK, testResponse.StatusCode);
    var result = await testResponse.Content.ReadFromJsonAsync<ExecutionResult>(_cancellationToken);
    Assert.IsNotNull(result);
    Assert.AreEqual(noted.Version + 1, result.Version);
  }

  [TestMethod]
  public async Task GivenCompletedTaskThenReturnOkWithSameVersion()
  {
    var noteRequest = new NoteDto { Title = "Some Title" };

    using var noteResponse = await _client
      .PostAsJsonAsync($"{ToDoRoutes.TaskRoute}/note", noteRequest, _cancellationToken)
      .ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.Created, noteResponse.StatusCode);
    var noted = await noteResponse.Content.ReadFromJsonAsync<NotedDto>();
    Assert.IsNotNull(noted);
    var taskId = noted.TaskId;

    using var completeResponse = await _client
      .PutAsJsonAsync($"{ToDoRoutes.TaskRoute}/{taskId}/complete", (object?)null, _cancellationToken)
      .ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.OK, completeResponse.StatusCode);
    var completeResult = await completeResponse.Content.ReadFromJsonAsync<ExecutionResult>(_cancellationToken);
    Assert.IsNotNull(completeResult);
    Assert.AreEqual(noted.Version + 1, completeResult.Version);

    using var testResponse = await _client
      .PutAsJsonAsync($"{ToDoRoutes.TaskRoute}/{taskId}/complete", (object?)null, _cancellationToken)
      .ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.OK, testResponse.StatusCode);
    var result = await testResponse.Content.ReadFromJsonAsync<ExecutionResult>(_cancellationToken);
    Assert.IsNotNull(result);
    Assert.AreEqual(completeResult.Version, result.Version);
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
