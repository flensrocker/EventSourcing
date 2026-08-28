namespace Flensrocker.ToDo.Abstractions;

[GenerateSerializer]
[Alias("Flensrocker.ToDo.ToDoQueryRequest")]
public abstract partial record ToDoQueryRequest
{
  private ToDoQueryRequest() { }
}

[GenerateSerializer]
[Alias("Flensrocker.ToDo.ToDoQueryResponse")]
public abstract partial record ToDoQueryResponse
{
  private ToDoQueryResponse() { }
}
