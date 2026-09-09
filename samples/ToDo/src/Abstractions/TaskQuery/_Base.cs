namespace Flensrocker.ToDo.Abstractions;

[GenerateSerializer]
[Alias(ToDoConstants.TaskQueryRequest.Base)]
public abstract partial record TaskQueryRequest
{
  private TaskQueryRequest() { }
}

[GenerateSerializer]
[Alias(ToDoConstants.TaskQueryResponse.Base)]
public abstract partial record TaskQueryResponse
{
  private TaskQueryResponse() { }
}
