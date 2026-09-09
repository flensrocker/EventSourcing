namespace Flensrocker.ToDo.Abstractions.Tasks;

[GenerateSerializer]
[Alias(TaskConstants.TaskQueryRequest.Base)]
public abstract partial record TaskQueryRequest
{
  private TaskQueryRequest() { }
}

[GenerateSerializer]
[Alias(TaskConstants.TaskQueryResponse.Base)]
public abstract partial record TaskQueryResponse
{
  private TaskQueryResponse() { }
}
