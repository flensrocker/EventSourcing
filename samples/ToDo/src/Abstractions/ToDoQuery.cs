namespace Flensrocker.ToDo.Abstractions;

[GenerateSerializer]
[Alias(ToDoConstants.ToDoQueryRequest.Base)]
public abstract partial record ToDoQueryRequest
{
  private ToDoQueryRequest() { }
}

[GenerateSerializer]
[Alias(ToDoConstants.ToDoQueryResponse.Base)]
public abstract partial record ToDoQueryResponse
{
  private ToDoQueryResponse() { }
}
