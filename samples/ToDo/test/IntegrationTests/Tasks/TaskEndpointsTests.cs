using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using static Flensrocker.ToDo.WebApi.Tasks.TaskEndpoints;

namespace Flensrocker.ToDo.IntegrationTests.Tasks;

[TestClass]
public class TaskEndpointsTests
{
  private static WebApplicationFactory<Program> _factory = default!;

  private readonly CancellationToken _cancellationToken;
  private HttpClient _client = default!;

  public TaskEndpointsTests(TestContext testContext)
  {
    _cancellationToken = testContext.CancellationTokenSource.Token;
  }

  [ClassInitialize]
  public static void ClassInitialize(TestContext testContext)
  {
    _factory = new WebApplicationFactory<Program>();
  }

  [ClassCleanup]
  public static void ClassCleanup()
  {
    _factory.Dispose();
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
  public async Task GivenInvalidNoteRequestThenReturnBadRequest()
  {
    var request = new NoteDto { Title = null };
    using var response = await _client.PostAsJsonAsync($"/api/todo/task/note", request, _cancellationToken).ConfigureAwait(false);

    Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
  }
}
