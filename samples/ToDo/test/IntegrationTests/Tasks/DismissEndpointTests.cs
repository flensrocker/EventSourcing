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
public class DismissEndpointTests : ToDoBaseEndpointTests<DismissEndpointTests>
{
  public DismissEndpointTests(TestContext testContext)
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

    var testRequest = new DismissDto { Reason = "Some Reason" };
    using var testResponse = await _client
      .PutAsJsonAsync($"{ToDoRoutes.TaskRoute}/{noteResult.TaskId}/dismiss", testRequest, _cancellationToken)
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
    Assert.AreEqual(Abstractions.Tasks.TaskStatus.Dismissed, detailsResult.Status);
    Assert.AreEqual(testRequest.Reason, detailsResult.DismissReason);

    await using var assertScope = _factory.Services.CreateAsyncScope();
    var eventStore = assertScope.ServiceProvider.GetRequiredService<IDomainEventStorage<TaskEvent>>();

    var taskEventList = await eventStore.ReadEventsFromStorage(noteResult.TaskId, 1, 0, _cancellationToken);
    Assert.IsNotNull(taskEventList);
    Assert.HasCount(2, taskEventList);
    Assert.IsInstanceOfType<TaskEvent.Noted>(taskEventList[0].Payload);
    var dismissedEvent = Assert.IsInstanceOfType<TaskEvent.Dismissed>(taskEventList[1].Payload);
    Assert.AreEqual(testRequest.Reason, dismissedEvent.Reason);
  }

  [TestMethod]
  public async Task GivenDismissedTaskThenReturnOkWithSameVersion()
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

    var testRequest = new DismissDto { Reason = "Some Reason" };
    using var firstDismissResponse = await _client
      .PutAsJsonAsync($"{ToDoRoutes.TaskRoute}/{noteResult.TaskId}/dismiss", testRequest, _cancellationToken)
      .ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.OK, firstDismissResponse.StatusCode);
    var firstDismissResult = await firstDismissResponse.Content.ReadFromJsonAsync<ExecutionResult>(_cancellationToken);
    Assert.IsNotNull(firstDismissResult);
    Assert.AreEqual(expectedVersion, firstDismissResult.Version);

    using var secondDismissResponse = await _client
      .PutAsJsonAsync($"{ToDoRoutes.TaskRoute}/{noteResult.TaskId}/dismiss", testRequest, _cancellationToken)
      .ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.OK, secondDismissResponse.StatusCode);
    var secondDismissResult = await secondDismissResponse.Content.ReadFromJsonAsync<ExecutionResult>(_cancellationToken);
    Assert.IsNotNull(secondDismissResult);
    Assert.AreEqual(expectedVersion, secondDismissResult.Version);

    var detailsResult = await _client.GetFromJsonAsync<TaskView.Details>(
      $"{ToDoRoutes.TaskRoute}/{noteResult.TaskId}",
      ToDoJsonSerializerContext.Default.Options,
      _cancellationToken
    );
    Assert.IsNotNull(detailsResult);
    Assert.AreEqual(expectedVersion, detailsResult.Version);
    Assert.AreEqual(noteRequest.Title, detailsResult.Title);
    Assert.AreEqual(Abstractions.Tasks.TaskStatus.Dismissed, detailsResult.Status);
    Assert.AreEqual(testRequest.Reason, detailsResult.DismissReason);

    await using var assertScope = _factory.Services.CreateAsyncScope();
    var eventStore = assertScope.ServiceProvider.GetRequiredService<IDomainEventStorage<TaskEvent>>();

    var taskEventList = await eventStore.ReadEventsFromStorage(noteResult.TaskId, 1, 0, _cancellationToken);
    Assert.IsNotNull(taskEventList);
    Assert.HasCount(2, taskEventList);
    Assert.IsInstanceOfType<TaskEvent.Noted>(taskEventList[0].Payload);
    var dismissedEvent = Assert.IsInstanceOfType<TaskEvent.Dismissed>(taskEventList[1].Payload);
    Assert.AreEqual(testRequest.Reason, dismissedEvent.Reason);
  }

  [TestMethod]
  public async Task GivenNotExistingTaskIdThenReturnNotFound()
  {
    var taskId = Guid.CreateVersion7();

    var testRequest = new DismissDto { Reason = "Some Reason" };
    using var testResponse = await _client
      .PutAsJsonAsync($"{ToDoRoutes.TaskRoute}/{taskId}/dismiss", testRequest, _cancellationToken)
      .ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.NotFound, testResponse.StatusCode);

    await using var assertScope = _factory.Services.CreateAsyncScope();
    var eventStore = assertScope.ServiceProvider.GetRequiredService<IDomainEventStorage<TaskEvent>>();

    var taskEventList = await eventStore.ReadEventsFromStorage(taskId, 1, 0, _cancellationToken);
    Assert.IsNull(taskEventList);
  }
}
