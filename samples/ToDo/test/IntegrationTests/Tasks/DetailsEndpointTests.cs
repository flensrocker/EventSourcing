using System.Net;
using System.Net.Http.Json;
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
  }

  [TestMethod]
  public async Task GivenInvalidTaskIdThenReturnNotFound()
  {
    var taskId = Guid.CreateVersion7();

    var testResponse = await _client.GetAsync($"{ToDoRoutes.TaskRoute}/{taskId}", _cancellationToken).ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.NotFound, testResponse.StatusCode);
  }
}
