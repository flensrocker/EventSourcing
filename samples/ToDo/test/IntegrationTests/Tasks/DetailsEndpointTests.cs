using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.ServerSentEvents;
using System.Text.Json;
using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Abstractions.Tasks;
using Flensrocker.ToDo.WebApi;
using static Flensrocker.ToDo.WebApi.Tasks.TaskEndpoints;

namespace Flensrocker.ToDo.IntegrationTests.Tasks;

[TestClass]
public class DetailsEndpointTests : ToDoBaseEndpointTests<DetailsEndpointTests>
{
  public DetailsEndpointTests(TestContext testContext)
    : base(testContext) { }

  [TestMethod]
  public async Task GivenValidTaskIdThenReturnDetails()
  {
    var noteRequest = new NoteDto { Title = "Some Title" };

    using var noteResponse = await _client
      .PostAsJsonAsync($"{ToDoRoutes.TaskRoute}/note", noteRequest, _cancellationToken)
      .ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.Created, noteResponse.StatusCode);
    var noteResult = await noteResponse.Content.ReadFromJsonAsync<NotedDto>(_cancellationToken).ConfigureAwait(false);
    Assert.IsNotNull(noteResult);
    Assert.AreEqual(1, noteResult.Version);
    Assert.AreNotEqual(Guid.Empty, noteResult.TaskId);

    var testResult = await _client
      .GetFromJsonAsync<TaskView.Details>(
        $"{ToDoRoutes.TaskRoute}/{noteResult.TaskId}",
        ToDoJsonSerializerContext.Default.Options,
        _cancellationToken
      )
      .ConfigureAwait(false);
    Assert.IsNotNull(testResult);
    Assert.AreEqual(1, testResult.Version);
    Assert.AreEqual(noteRequest.Title, testResult.Title);
    Assert.AreEqual(Abstractions.Tasks.TaskStatus.Open, testResult.Status);
    Assert.IsNull(testResult.DismissReason);

    // DRAFT interleave streaming and modifing requests with each other
    // Hangs sometimes when executing all tests at once
    // using var streamRequest = new HttpRequestMessage(HttpMethod.Get, $"{ToDoRoutes.TaskRoute}/{noteResult.TaskId}/stream");
    // streamRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

    // using var streamResponse = await _client
    //   .SendAsync(streamRequest, HttpCompletionOption.ResponseHeadersRead, _cancellationToken)
    //   .ConfigureAwait(false);

    // Assert.AreEqual(HttpStatusCode.OK, streamResponse.StatusCode);
    // await using var stream = await streamResponse.Content.ReadAsStreamAsync(_cancellationToken).ConfigureAwait(false);

    // var taskEventStream = SseParser
    //   .Create(stream, (_, data) => JsonSerializer.Deserialize<TaskView.Details>(data, ToDoJsonSerializerContext.Default.Options)!)
    //   .EnumerateAsync(_cancellationToken)
    //   .ConfigureAwait(false);

    // await using var taskEventEnumerator = taskEventStream.GetAsyncEnumerator();
    // var hasNext = await taskEventEnumerator.MoveNextAsync();
    // Assert.IsTrue(hasNext);
    // Assert.AreEqual(1, taskEventEnumerator.Current.Data.Version);
    // Assert.AreEqual(noteRequest.Title, taskEventEnumerator.Current.Data.Title);
    // Assert.AreEqual(Abstractions.Tasks.TaskStatus.Open, taskEventEnumerator.Current.Data.Status);
    // Assert.IsNull(taskEventEnumerator.Current.Data.DismissReason);

    // using var completeResponse = await _client
    //   .PutAsJsonAsync($"{ToDoRoutes.TaskRoute}/{noteResult.TaskId}/complete", (object?)null, _cancellationToken)
    //   .ConfigureAwait(false);
    // Assert.AreEqual(HttpStatusCode.OK, completeResponse.StatusCode);

    // hasNext = await taskEventEnumerator.MoveNextAsync();
    // Assert.IsTrue(hasNext);
    // Assert.AreEqual(2, taskEventEnumerator.Current.Data.Version);
    // Assert.AreEqual(noteRequest.Title, taskEventEnumerator.Current.Data.Title);
    // Assert.AreEqual(Abstractions.Tasks.TaskStatus.Completed, taskEventEnumerator.Current.Data.Status);
    // Assert.IsNull(taskEventEnumerator.Current.Data.DismissReason);

    // using var reopenResponse = await _client
    //   .PutAsJsonAsync($"{ToDoRoutes.TaskRoute}/{noteResult.TaskId}/reopen", (object?)null, _cancellationToken)
    //   .ConfigureAwait(false);
    // Assert.AreEqual(HttpStatusCode.OK, reopenResponse.StatusCode);

    // hasNext = await taskEventEnumerator.MoveNextAsync();
    // Assert.IsTrue(hasNext);
    // Assert.AreEqual(3, taskEventEnumerator.Current.Data.Version);
    // Assert.AreEqual(noteRequest.Title, taskEventEnumerator.Current.Data.Title);
    // Assert.AreEqual(Abstractions.Tasks.TaskStatus.Open, taskEventEnumerator.Current.Data.Status);
    // Assert.IsNull(taskEventEnumerator.Current.Data.DismissReason);

    // var dismissRequest = new DismissDto { Reason = "Some Reason" };
    // using var dismissResponse = await _client
    //   .PutAsJsonAsync($"{ToDoRoutes.TaskRoute}/{noteResult.TaskId}/dismiss", dismissRequest, _cancellationToken)
    //   .ConfigureAwait(false);
    // Assert.AreEqual(HttpStatusCode.OK, dismissResponse.StatusCode);

    // hasNext = await taskEventEnumerator.MoveNextAsync();
    // Assert.IsTrue(hasNext);
    // Assert.AreEqual(4, taskEventEnumerator.Current.Data.Version);
    // Assert.AreEqual(noteRequest.Title, taskEventEnumerator.Current.Data.Title);
    // Assert.AreEqual(Abstractions.Tasks.TaskStatus.Dismissed, taskEventEnumerator.Current.Data.Status);
    // Assert.AreEqual(dismissRequest.Reason, taskEventEnumerator.Current.Data.DismissReason);
  }

  [TestMethod]
  public async Task GivenInvalidTaskIdThenReturnNotFound()
  {
    var taskId = Guid.CreateVersion7();

    var testResponse = await _client.GetAsync($"{ToDoRoutes.TaskRoute}/{taskId}", _cancellationToken).ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.NotFound, testResponse.StatusCode);
  }
}
