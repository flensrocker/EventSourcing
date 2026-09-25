using System.Net.Http.Json;
using System.Text.Json;

namespace Flensrocker.ToDo.IntegrationTests;

public static class TestUtilities
{
  public static readonly JsonSerializerOptions WebJsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

  public class ProblemDetailsWithErrors
  {
    public string? Type { get; init; }
    public string? Title { get; init; }
    public int Status { get; init; }
    public string? TraceId { get; init; }
    public Dictionary<string, string[]>? Errors { get; init; }
  }

  extension(HttpContent httpContent)
  {
    public async Task<ProblemDetailsWithErrors?> ReadAsProblemDetailsWithErrorsAsync(CancellationToken cancellationToken)
    {
      Assert.AreEqual("application/problem+json", httpContent.Headers.ContentType?.MediaType);

      return await httpContent.ReadFromJsonAsync<ProblemDetailsWithErrors>(WebJsonOptions, cancellationToken).ConfigureAwait(false);
    }
  }
}
