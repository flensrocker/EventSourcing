using System.Net;
using System.Net.Http.Json;
using Flensrocker.ToDo.WebApi;

namespace Flensrocker.ToDo.IntegrationTests.Tasks;

[TestClass]
public class CompleteEndpointTests : BaseEndpointTests<CompleteEndpointTests>
{
  public CompleteEndpointTests(TestContext testContext)
    : base(testContext) { }

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
