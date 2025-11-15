namespace Flensrocker.ToDo.Abstractions;

[GenerateSerializer]
public abstract partial record ToDoQueryRequest
{
  private ToDoQueryRequest() { }
}

[GenerateSerializer]
public abstract partial record ToDoQueryResponse
{
  private ToDoQueryResponse() { }
}
