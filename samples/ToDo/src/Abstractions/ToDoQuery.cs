namespace Flensrocker.ToDo.Abstractions;

[GenerateSerializer]
[Alias("ToDo.ToDoQueryRequest")]
public abstract partial record ToDoQueryRequest
{
  private ToDoQueryRequest() { }
}

[GenerateSerializer]
[Alias("ToDo.ToDoQueryResponse")]
public abstract partial record ToDoQueryResponse
{
  private ToDoQueryResponse() { }
}
