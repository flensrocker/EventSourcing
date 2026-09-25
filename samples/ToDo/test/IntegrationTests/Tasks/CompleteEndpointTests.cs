using System.Net;
using System.Net.Http.Json;
using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Abstractions.Tasks;
using Flensrocker.ToDo.WebApi;
using Microsoft.Extensions.DependencyInjection;
using static Flensrocker.ToDo.WebApi.Tasks.TaskEndpoints;

namespace Flensrocker.ToDo.IntegrationTests.Tasks;

[TestClass]
public class CompleteEndpointTests : ToDoBaseEndpointTests<CompleteEndpointTests>
{
  public CompleteEndpointTests(TestContext testContext)
    : base(testContext) { }

  [TestMethod]
  public async Task GivenOpenTaskThenReturnOk()
  {
    var expectedVersion = 1;

    var noteRequest = new NoteDto { Title = "Some Title" };
    using var noteResponse = await _client
      .PostAsJsonAsync($"{ToDoRoutes.TaskRoute}/note", noteRequest, _cancellationToken)
      .ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.Created, noteResponse.StatusCode);
    var noteResult = await noteResponse.Content.ReadFromJsonAsync<NotedDto>();
    Assert.IsNotNull(noteResult);
    Assert.AreEqual(expectedVersion, noteResult.Version);
    expectedVersion++;

    using var testResponse = await _client
      .PutAsJsonAsync($"{ToDoRoutes.TaskRoute}/{noteResult.TaskId}/complete", (object?)null, _cancellationToken)
      .ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.OK, testResponse.StatusCode);
    var testResult = await testResponse.Content.ReadFromJsonAsync<ExecutionResult>(_cancellationToken);
    Assert.IsNotNull(testResult);
    Assert.AreEqual(expectedVersion, testResult.Version);

    var detailsResult = await _client.GetFromJsonAsync<TaskView.Details>(
      $"{ToDoRoutes.TaskRoute}/{noteResult.TaskId}",
      ToDoJsonSerializerContext.Default.Options,
      _cancellationToken
    );
    Assert.IsNotNull(detailsResult);
    Assert.AreEqual(expectedVersion, detailsResult.Version);
    Assert.AreEqual(noteRequest.Title, detailsResult.Title);
    Assert.AreEqual(Abstractions.Tasks.TaskStatus.Completed, detailsResult.Status);
    Assert.IsNull(detailsResult.DismissReason);

    await using var assertScope = _factory.Services.CreateAsyncScope();
    var eventStore = assertScope.ServiceProvider.GetRequiredService<IDomainEventStorage<TaskEvent>>();

    var taskEventList = await eventStore.ReadEventsFromStorage(noteResult.TaskId, 1, 0, _cancellationToken);
    Assert.IsNotNull(taskEventList);
    Assert.HasCount(2, taskEventList);
    Assert.IsInstanceOfType<TaskEvent.Noted>(taskEventList[0].Payload);
    Assert.IsInstanceOfType<TaskEvent.Completed>(taskEventList[1].Payload);
  }

  [TestMethod]
  public async Task GivenCompletedTaskThenReturnOkWithSameVersion()
  {
    var expectedVersion = 1;

    var noteRequest = new NoteDto { Title = "Some Title" };
    using var noteResponse = await _client
      .PostAsJsonAsync($"{ToDoRoutes.TaskRoute}/note", noteRequest, _cancellationToken)
      .ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.Created, noteResponse.StatusCode);
    var noteResult = await noteResponse.Content.ReadFromJsonAsync<NotedDto>();
    Assert.IsNotNull(noteResult);
    Assert.AreEqual(expectedVersion, noteResult.Version);
    expectedVersion++;

    using var firstCompleteResponse = await _client
      .PutAsJsonAsync($"{ToDoRoutes.TaskRoute}/{noteResult.TaskId}/complete", (object?)null, _cancellationToken)
      .ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.OK, firstCompleteResponse.StatusCode);
    var firstCompleteResult = await firstCompleteResponse.Content.ReadFromJsonAsync<ExecutionResult>(_cancellationToken);
    Assert.IsNotNull(firstCompleteResult);
    Assert.AreEqual(expectedVersion, firstCompleteResult.Version);

    using var secondCompleteResponse = await _client
      .PutAsJsonAsync($"{ToDoRoutes.TaskRoute}/{noteResult.TaskId}/complete", (object?)null, _cancellationToken)
      .ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.OK, secondCompleteResponse.StatusCode);
    var secondCompleteResult = await secondCompleteResponse.Content.ReadFromJsonAsync<ExecutionResult>(_cancellationToken);
    Assert.IsNotNull(secondCompleteResult);
    Assert.AreEqual(expectedVersion, secondCompleteResult.Version);

    var detailsResult = await _client.GetFromJsonAsync<TaskView.Details>(
      $"{ToDoRoutes.TaskRoute}/{noteResult.TaskId}",
      ToDoJsonSerializerContext.Default.Options,
      _cancellationToken
    );
    Assert.IsNotNull(detailsResult);
    Assert.AreEqual(expectedVersion, detailsResult.Version);
    Assert.AreEqual(noteRequest.Title, detailsResult.Title);
    Assert.AreEqual(Abstractions.Tasks.TaskStatus.Completed, detailsResult.Status);
    Assert.IsNull(detailsResult.DismissReason);

    await using var assertScope = _factory.Services.CreateAsyncScope();
    var eventStore = assertScope.ServiceProvider.GetRequiredService<IDomainEventStorage<TaskEvent>>();

    var taskEventList = await eventStore.ReadEventsFromStorage(noteResult.TaskId, 1, 0, _cancellationToken);
    Assert.IsNotNull(taskEventList);
    Assert.HasCount(2, taskEventList);
    Assert.IsInstanceOfType<TaskEvent.Noted>(taskEventList[0].Payload);
    Assert.IsInstanceOfType<TaskEvent.Completed>(taskEventList[1].Payload);
  }

  [TestMethod]
  public async Task GivenNotExistingTaskIdThenReturnNotFound()
  {
    var taskId = Guid.CreateVersion7();

    using var testResponse = await _client
      .PutAsJsonAsync($"{ToDoRoutes.TaskRoute}/{taskId}/complete", (object?)null, _cancellationToken)
      .ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.NotFound, testResponse.StatusCode);

    await using var assertScope = _factory.Services.CreateAsyncScope();
    var eventStore = assertScope.ServiceProvider.GetRequiredService<IDomainEventStorage<TaskEvent>>();

    var taskEventList = await eventStore.ReadEventsFromStorage(taskId, 1, 0, _cancellationToken);
    Assert.IsNull(taskEventList);
  }
}
