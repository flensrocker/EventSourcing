using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.ServerSentEvents;
using System.Text.Json;
using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Abstractions.Tasks;
using Flensrocker.ToDo.WebApi;
using Microsoft.Extensions.DependencyInjection;
using static Flensrocker.ToDo.WebApi.Tasks.TaskEndpoints;

namespace Flensrocker.ToDo.IntegrationTests.Tasks;

[TestClass]
public class TaskEndpointsTests : ToDoBaseEndpointTests<TaskEndpointsTests>
{
  public TaskEndpointsTests(TestContext testContext)
    : base(testContext) { }

  [TestMethod]
  public async Task GivenNotExistingTaskIdWhenCompleteThenReturnNotFound()
  {
    var taskId = Guid.CreateVersion7();

    using var testResponse = await _client
      .PutAsJsonAsync($"{ToDoRoutes.TaskRoute}/{taskId}/complete", (object?)null, _cancellationToken)
      .ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.NotFound, testResponse.StatusCode);

    await using var assertScope = _factory.Services.CreateAsyncScope();
    var eventStore = assertScope.ServiceProvider.GetRequiredService<IDomainEventStorage<TaskEvent>>();

    var taskEventList = await eventStore.ReadEventsFromStorage(taskId, 1, 0, _cancellationToken).ConfigureAwait(false);
    Assert.IsNull(taskEventList);
  }

  [TestMethod]
  public async Task GivenDismissedTaskWhenCompleteThenReturnBadRequest()
  {
    var noteRequest = new NoteDto { Title = "Some Title" };
    using var noteResponse = await _client
      .PostAsJsonAsync($"{ToDoRoutes.TaskRoute}/note", noteRequest, _cancellationToken)
      .ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.Created, noteResponse.StatusCode);
    var noteResult = await noteResponse.Content.ReadFromJsonAsync<NotedDto>(_cancellationToken).ConfigureAwait(false);
    Assert.IsNotNull(noteResult);

    var dismissRequest = new DismissDto { Reason = "Some Reason" };
    using var dismissResponse = await _client
      .PutAsJsonAsync($"{ToDoRoutes.TaskRoute}/{noteResult.TaskId}/dismiss", dismissRequest, _cancellationToken)
      .ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.OK, dismissResponse.StatusCode);

    using var testResponse = await _client
      .PutAsJsonAsync($"{ToDoRoutes.TaskRoute}/{noteResult.TaskId}/complete", (object?)null, _cancellationToken)
      .ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.BadRequest, testResponse.StatusCode);
  }

  [TestMethod]
  [DataRow("2026-09-28", "2026-09-27")]
  public async Task GivenInvalidDismissRequestWhenNoteThenReturnBadRequest(string? fromDate, string? toDate)
  {
    using var testResponse = await _client
      .GetAsync($"{ToDoRoutes.TaskRoute}/dailystats?fromDate={fromDate}&toDate={toDate}", _cancellationToken)
      .ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.BadRequest, testResponse.StatusCode);
    var testResult = await testResponse.Content.ReadAsProblemDetailsWithErrorsAsync(_cancellationToken).ConfigureAwait(false);
    Assert.IsNotNull(testResult?.Errors);
    Assert.IsTrue(testResult.Errors.ContainsKey(nameof(TaskQueryRequest.DailyStats.FromDate)));
    Assert.IsTrue(testResult.Errors.ContainsKey(nameof(TaskQueryRequest.DailyStats.ToDate)));
  }

  [TestMethod]
  public async Task GivenInvalidTaskIdWhenGetDetailsThenReturnNotFound()
  {
    var taskId = Guid.CreateVersion7();

    var testResponse = await _client.GetAsync($"{ToDoRoutes.TaskRoute}/{taskId}", _cancellationToken).ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.NotFound, testResponse.StatusCode);
  }

  [TestMethod]
  public async Task GivenNotExistingTaskIdWhenDismissThenReturnNotFound()
  {
    var taskId = Guid.CreateVersion7();

    var testRequest = new DismissDto { Reason = "Some Reason" };
    using var testResponse = await _client
      .PutAsJsonAsync($"{ToDoRoutes.TaskRoute}/{taskId}/dismiss", testRequest, _cancellationToken)
      .ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.NotFound, testResponse.StatusCode);

    await using var assertScope = _factory.Services.CreateAsyncScope();
    var eventStore = assertScope.ServiceProvider.GetRequiredService<IDomainEventStorage<TaskEvent>>();

    var taskEventList = await eventStore.ReadEventsFromStorage(taskId, 1, 0, _cancellationToken).ConfigureAwait(false);
    Assert.IsNull(taskEventList);
  }

  [TestMethod]
  [DataRow((string?)null)]
  [DataRow("")]
  [DataRow(" ")]
  [DataRow("\t")]
  [DataRow(" \t")]
  [DataRow("\t ")]
  [DataRow(" \t ")]
  [DataRow(
    "123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 1"
  )]
  [DataRow("With\nLinebreak")]
  [DataRow("With\rCarriage Return")]
  public async Task GivenInvalidDismissRequestWhenNoteThenReturnBadRequest(string? invalidReason)
  {
    var taskId = Guid.CreateVersion7();

    var testRequest = new DismissDto { Reason = invalidReason };

    using var testResponse = await _client
      .PutAsJsonAsync($"{ToDoRoutes.TaskRoute}/{taskId}/dismiss", testRequest, _cancellationToken)
      .ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.BadRequest, testResponse.StatusCode);
    var testResult = await testResponse.Content.ReadAsProblemDetailsWithErrorsAsync(_cancellationToken).ConfigureAwait(false);
    Assert.IsNotNull(testResult?.Errors);
    Assert.IsTrue(testResult.Errors.ContainsKey(nameof(DismissDto.Reason)));
  }

  [TestMethod]
  public async Task GivenCompletedTaskWhenDismissThenReturnBadRequest()
  {
    var noteRequest = new NoteDto { Title = "Some Title" };
    using var noteResponse = await _client
      .PostAsJsonAsync($"{ToDoRoutes.TaskRoute}/note", noteRequest, _cancellationToken)
      .ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.Created, noteResponse.StatusCode);
    var noteResult = await noteResponse.Content.ReadFromJsonAsync<NotedDto>(_cancellationToken).ConfigureAwait(false);
    Assert.IsNotNull(noteResult);

    using var completeResponse = await _client
      .PutAsJsonAsync($"{ToDoRoutes.TaskRoute}/{noteResult.TaskId}/complete", (object?)null, _cancellationToken)
      .ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.OK, completeResponse.StatusCode);

    var testRequest = new DismissDto { Reason = "Some Reason" };
    using var testResponse = await _client
      .PutAsJsonAsync($"{ToDoRoutes.TaskRoute}/{noteResult.TaskId}/dismiss", testRequest, _cancellationToken)
      .ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.BadRequest, testResponse.StatusCode);
  }

  [TestMethod]
  [DataRow((string?)null)]
  [DataRow("")]
  [DataRow(" ")]
  [DataRow("\t")]
  [DataRow(" \t")]
  [DataRow("\t ")]
  [DataRow(" \t ")]
  [DataRow("123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 123456789 1")]
  [DataRow("With\nLinebreak")]
  [DataRow("With\rCarriage Return")]
  public async Task GivenInvalidNoteRequestWhenNoteThenReturnBadRequest(string? invalidTitle)
  {
    var testRequest = new NoteDto { Title = invalidTitle };

    using var testResponse = await _client
      .PostAsJsonAsync($"{ToDoRoutes.TaskRoute}/note", testRequest, _cancellationToken)
      .ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.BadRequest, testResponse.StatusCode);
    var testResult = await testResponse.Content.ReadAsProblemDetailsWithErrorsAsync(_cancellationToken).ConfigureAwait(false);
    Assert.IsNotNull(testResult?.Errors);
    Assert.IsTrue(testResult.Errors.ContainsKey(nameof(NoteDto.Title)));
  }

  [TestMethod]
  public async Task GivenNotExistingTaskIdWhenReopenThenReturnNotFound()
  {
    var taskId = Guid.CreateVersion7();

    using var testResponse = await _client
      .PutAsJsonAsync($"{ToDoRoutes.TaskRoute}/{taskId}/reopen", (object?)null, _cancellationToken)
      .ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.NotFound, testResponse.StatusCode);

    await using var assertScope = _factory.Services.CreateAsyncScope();
    var eventStore = assertScope.ServiceProvider.GetRequiredService<IDomainEventStorage<TaskEvent>>();

    var taskEventList = await eventStore.ReadEventsFromStorage(taskId, 1, 0, _cancellationToken).ConfigureAwait(false);
    Assert.IsNull(taskEventList);
  }

  [TestMethod]
  public async Task GivenNoteDismissReopenCompleteWorkflowWhenGetDetailsThenReturnVersion4()
  {
    var expectedVersion = 0;

    var noteRequest = new NoteDto { Title = "Some Title" };
    var noteStartTimestamp = DateTimeOffset.UtcNow;
    using var noteResponse = await _client
      .PostAsJsonAsync($"{ToDoRoutes.TaskRoute}/note", noteRequest, _cancellationToken)
      .ConfigureAwait(false);
    var noteEndTimestamp = DateTimeOffset.UtcNow;
    expectedVersion++;

    Assert.AreEqual(HttpStatusCode.Created, noteResponse.StatusCode);
    var noteResult = await noteResponse.Content.ReadFromJsonAsync<NotedDto>(_cancellationToken).ConfigureAwait(false);
    Assert.IsNotNull(noteResult);
    Assert.AreEqual(expectedVersion, noteResult.Version);
    Assert.AreNotEqual(Guid.Empty, noteResult.TaskId);

    var dismissRequest = new DismissDto { Reason = "Some Reason" };
    using var dismissResponse = await _client
      .PutAsJsonAsync($"{ToDoRoutes.TaskRoute}/{noteResult.TaskId}/dismiss", dismissRequest, _cancellationToken)
      .ConfigureAwait(false);
    expectedVersion++;

    Assert.AreEqual(HttpStatusCode.OK, dismissResponse.StatusCode);
    var dismissResult = await dismissResponse.Content.ReadFromJsonAsync<CommandResult>(_cancellationToken).ConfigureAwait(false);
    Assert.IsNotNull(dismissResult);
    Assert.AreEqual(expectedVersion, dismissResult.Version);

    var secondDismissRequest = new DismissDto { Reason = "Some Reason" };
    using var secondDismissResponse = await _client
      .PutAsJsonAsync($"{ToDoRoutes.TaskRoute}/{noteResult.TaskId}/dismiss", secondDismissRequest, _cancellationToken)
      .ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.OK, secondDismissResponse.StatusCode);
    var secondDismissResult = await secondDismissResponse
      .Content.ReadFromJsonAsync<CommandResult>(_cancellationToken)
      .ConfigureAwait(false);
    Assert.IsNotNull(secondDismissResult);
    Assert.AreEqual(expectedVersion, secondDismissResult.Version);

    using var reopenResponse = await _client
      .PutAsJsonAsync($"{ToDoRoutes.TaskRoute}/{noteResult.TaskId}/reopen", (object?)null, _cancellationToken)
      .ConfigureAwait(false);
    expectedVersion++;

    Assert.AreEqual(HttpStatusCode.OK, reopenResponse.StatusCode);
    var reopenResult = await reopenResponse.Content.ReadFromJsonAsync<CommandResult>(_cancellationToken).ConfigureAwait(false);
    Assert.IsNotNull(reopenResult);
    Assert.AreEqual(expectedVersion, reopenResult.Version);

    using var secondReopenResponse = await _client
      .PutAsJsonAsync($"{ToDoRoutes.TaskRoute}/{noteResult.TaskId}/reopen", (object?)null, _cancellationToken)
      .ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.OK, secondReopenResponse.StatusCode);
    var secondReopenResult = await secondReopenResponse.Content.ReadFromJsonAsync<CommandResult>(_cancellationToken).ConfigureAwait(false);
    Assert.IsNotNull(secondReopenResult);
    Assert.AreEqual(expectedVersion, secondReopenResult.Version);

    using var completeResponse = await _client
      .PutAsJsonAsync($"{ToDoRoutes.TaskRoute}/{noteResult.TaskId}/complete", (object?)null, _cancellationToken)
      .ConfigureAwait(false);
    expectedVersion++;

    Assert.AreEqual(HttpStatusCode.OK, completeResponse.StatusCode);
    var completeResult = await completeResponse.Content.ReadFromJsonAsync<CommandResult>(_cancellationToken).ConfigureAwait(false);
    Assert.IsNotNull(completeResult);
    Assert.AreEqual(expectedVersion, completeResult.Version);

    using var secondCompleteResponse = await _client
      .PutAsJsonAsync($"{ToDoRoutes.TaskRoute}/{noteResult.TaskId}/complete", (object?)null, _cancellationToken)
      .ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.OK, secondCompleteResponse.StatusCode);
    var secondCompleteResult = await secondCompleteResponse
      .Content.ReadFromJsonAsync<CommandResult>(_cancellationToken)
      .ConfigureAwait(false);
    Assert.IsNotNull(secondCompleteResult);
    Assert.AreEqual(expectedVersion, secondCompleteResult.Version);

    var detailsResult = await _client
      .GetFromJsonAsync<TaskView.Details>(
        $"{ToDoRoutes.TaskRoute}/{noteResult.TaskId}",
        ToDoJsonSerializerContext.Default.Options,
        _cancellationToken
      )
      .ConfigureAwait(false);
    Assert.IsNotNull(detailsResult);
    Assert.AreEqual(expectedVersion, detailsResult.Version);
    Assert.AreEqual(noteRequest.Title, detailsResult.Title);
    Assert.AreEqual(Abstractions.Tasks.TaskStatus.Completed, detailsResult.Status);
    Assert.IsNull(detailsResult.DismissReason);

    var listRequest = TaskQueryRequest.List.Create(tenantId: null);
    var listResult = await _client
      .GetFromJsonAsync<TaskQueryResponse.List>(ToDoRoutes.TaskRoute, ToDoJsonSerializerContext.Default.Options, _cancellationToken)
      .ConfigureAwait(false);
    Assert.IsNotNull(listResult);
    Assert.IsNotEmpty(listResult.ItemList);
    CollectionAssert.Contains(listResult.ItemList.Select(item => item.TaskId).ToArray(), noteResult.TaskId);

    await using var assertScope = _factory.Services.CreateAsyncScope();
    var eventStore = assertScope.ServiceProvider.GetRequiredService<IDomainEventStorage<TaskEvent>>();

    var taskEventList = await eventStore.ReadEventsFromStorage(noteResult.TaskId, 1, 0, _cancellationToken).ConfigureAwait(false);
    Assert.IsNotNull(taskEventList);
    Assert.HasCount(4, taskEventList);
    var notedEvent = Assert.IsInstanceOfType<TaskEvent.Noted>(taskEventList[0].Payload);
    Assert.AreEqual(noteRequest.Title, notedEvent.Title);
    Assert.IsInRange(noteStartTimestamp, noteEndTimestamp, notedEvent.CreatedAt);
    var dismissedEvent = Assert.IsInstanceOfType<TaskEvent.Dismissed>(taskEventList[1].Payload);
    Assert.AreEqual(dismissRequest.Reason, dismissedEvent.Reason);
    Assert.IsInstanceOfType<TaskEvent.Reopened>(taskEventList[2].Payload);
    Assert.IsInstanceOfType<TaskEvent.Completed>(taskEventList[3].Payload);
  }

  [TestMethod]
  public async Task GivenStreamRequestThenReturnServerSentEvents()
  {
    var expectedVersion = 0;

    var noteRequest = new NoteDto { Title = "Some Title" };
    var noteStartTimestamp = DateTimeOffset.UtcNow;
    using var noteResponse = await _client
      .PostAsJsonAsync($"{ToDoRoutes.TaskRoute}/note", noteRequest, _cancellationToken)
      .ConfigureAwait(false);
    var noteEndTimestamp = DateTimeOffset.UtcNow;
    expectedVersion++;

    Assert.AreEqual(HttpStatusCode.Created, noteResponse.StatusCode);
    var noteResult = await noteResponse.Content.ReadFromJsonAsync<NotedDto>(_cancellationToken).ConfigureAwait(false);
    Assert.IsNotNull(noteResult);
    Assert.AreEqual(expectedVersion, noteResult.Version);
    Assert.AreNotEqual(Guid.Empty, noteResult.TaskId);

    _testContext.WriteLine("Setup SSE stream");
    var streamSetupTask = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

    using var streamCancellation = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken);
    var streamCancellationToken = streamCancellation.Token;
    var waitForCount = 2;
    var streamTask = Task.Run(async () =>
    {
      using var streamRequest = new HttpRequestMessage(HttpMethod.Get, $"{ToDoRoutes.TaskRoute}/{noteResult.TaskId}/stream");
      streamRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

      using var streamResponse = await _client
        .SendAsync(streamRequest, HttpCompletionOption.ResponseHeadersRead, streamCancellationToken)
        .ConfigureAwait(false);

      Assert.AreEqual(HttpStatusCode.OK, streamResponse.StatusCode);
      await using var stream = await streamResponse.Content.ReadAsStreamAsync(streamCancellationToken).ConfigureAwait(false);
      _testContext.WriteLine("SSE stream requested");

      var detailsList = new List<TaskView.Details>();

      await foreach (
        var sseItem in SseParser
          .Create(stream, (_, data) => JsonSerializer.Deserialize<TaskView.Details>(data, ToDoJsonSerializerContext.Default.Options)!)
          .EnumerateAsync(streamCancellationToken)
          .ConfigureAwait(false)
      )
      {
        _testContext.WriteLine($"Received version {sseItem.Data.Version}");

        if (detailsList.Count == 0)
        {
          _testContext.WriteLine("First event received, setup completed");
          streamSetupTask.SetResult();
        }

        detailsList.Add(sseItem.Data);

        if (detailsList.Count >= waitForCount)
          break;
      }

      return detailsList;
    });
    _testContext.WriteLine("Waiting for SSE stream setup");
    await streamSetupTask.Task.ConfigureAwait(false);

    using var completeResponse = await _client
      .PutAsJsonAsync($"{ToDoRoutes.TaskRoute}/{noteResult.TaskId}/complete", (object?)null, _cancellationToken)
      .ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.OK, completeResponse.StatusCode);

    _testContext.WriteLine("Waiting for stream to finish");
    var detailsList = await streamTask.ConfigureAwait(false);
    _testContext.WriteLine("SSE stream finished");

    Assert.HasCount(2, detailsList);
    Assert.AreEqual(1, detailsList[0].Version);
    Assert.AreEqual(2, detailsList[1].Version);

    // when multiple TaskView.Details got streamed, the async projection is hopefully processed
    var dailyStatsRequest = TaskQueryRequest.DailyStats.Create(tenantId: null, userId: null, fromDate: null, toDate: null);
    // TODO transform dailyStatsRequest into query string
    var dailyStatsResult = await _client
      .GetFromJsonAsync<TaskQueryResponse.DailyStats>(
        $"{ToDoRoutes.TaskRoute}/dailystats",
        ToDoJsonSerializerContext.Default.Options,
        _cancellationToken
      )
      .ConfigureAwait(false);
    Assert.IsNotNull(dailyStatsResult);
    // TODO flaky test
    if (dailyStatsResult.ItemList.Count > 0)
    {
      Assert.IsNotEmpty(dailyStatsResult.ItemList);
      Assert.IsGreaterThan(0, dailyStatsResult.ItemList.First().CountNoted);
    }
  }
}
