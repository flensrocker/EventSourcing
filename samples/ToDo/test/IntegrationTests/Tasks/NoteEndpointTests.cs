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
public class NoteEndpointTests : BaseEndpointTests<NoteEndpointTests>
{
  public NoteEndpointTests(TestContext testContext)
    : base(testContext) { }

  [TestMethod]
  public async Task GivenValidNoteRequestThenReturnCreated()
  {
    var testRequest = new NoteDto { Title = "Some Title" };

    using var testResponse = await _client
      .PostAsJsonAsync($"{ToDoRoutes.TaskRoute}/note", testRequest, _cancellationToken)
      .ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.Created, testResponse.StatusCode);
    var noteResult = await testResponse.Content.ReadFromJsonAsync<NotedDto>(_cancellationToken);
    Assert.IsNotNull(noteResult);
    Assert.AreEqual(1, noteResult.Version);
    Assert.AreNotEqual(Guid.Empty, noteResult.TaskId);

    var detailsResult = await _client.GetFromJsonAsync<TaskView.Details>(
      $"{ToDoRoutes.TaskRoute}/{noteResult.TaskId}",
      ToDoJsonSerializerContext.Default.Options,
      _cancellationToken
    );
    Assert.IsNotNull(detailsResult);
    Assert.AreEqual(1, detailsResult.Version);
    Assert.AreEqual(testRequest.Title, detailsResult.Title);
    Assert.AreEqual(Abstractions.Tasks.TaskStatus.Open, detailsResult.Status);
    Assert.IsNull(detailsResult.DismissReason);

    await using var assertScope = _factory.Services.CreateAsyncScope();
    var eventStore = assertScope.ServiceProvider.GetRequiredService<IDomainEventStorage<TaskEvent>>();

    var taskEventList = await eventStore.ReadEventsFromStorage(noteResult.TaskId, 1, 0, _cancellationToken);
    Assert.IsNotNull(taskEventList);
    Assert.HasCount(1, taskEventList);
    var notedMetadata = taskEventList[0].Metadata;
    var notedEvent = Assert.IsInstanceOfType<TaskEvent.Noted>(taskEventList[0].Payload);
    Assert.AreEqual(testRequest.Title, notedEvent.Title);
    Assert.AreEqual(1, notedMetadata.StreamVersion);
  }

  [TestMethod]
  public async Task GivenInvalidNoteRequestThenReturnBadRequest()
  {
    var testRequest = new NoteDto { Title = null };

    using var testResponse = await _client
      .PostAsJsonAsync($"{ToDoRoutes.TaskRoute}/note", testRequest, _cancellationToken)
      .ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.BadRequest, testResponse.StatusCode);
    var testResult = await testResponse.Content.ReadAsProblemDetailsWithErrorsAsync(_cancellationToken);
    Assert.IsNotNull(testResult);
    Assert.IsNotNull(testResult.Errors);
    Assert.IsTrue(testResult.Errors.ContainsKey("Title"));
  }
}
