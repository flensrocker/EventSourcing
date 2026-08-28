using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions;

[JsonDerivedType(typeof(ListToDos), nameof(ListToDos))]
public abstract partial record ToDoQueryRequest
{
  [GenerateSerializer]
  [Alias("Flensrocker.ToDo.ToDoQueryRequest.ListToDos")]
  public sealed record ListToDos : ToDoQueryRequest
  {
    [Id(0)]
    public Guid? TenantId { get; init; }
    // TODO add filter, order, page-size etc.

    [JsonConstructor]
    internal ListToDos() { }

    public static ListToDos? Create(Guid? tenantId)
    {
      return new ListToDos
      {
        TenantId = tenantId,
      };
    }
  }
}


[JsonDerivedType(typeof(ListToDos), nameof(ListToDos))]
public abstract partial record ToDoQueryResponse
{
  [GenerateSerializer]
  [Alias("Flensrocker.ToDo.ToDoQueryResponse.ListToDos")]
  public sealed record ListToDos : ToDoQueryResponse
  {
    [GenerateSerializer]
    [Alias("Flensrocker.ToDo.ToDoQueryResponse.ListToDos.ToDoItem")]
    public sealed record ToDoItem
    {
      [Id(0)]
      public required Guid ToDoId { get; init; }
      [Id(1)]
      public required int Version { get; init; }
      [Id(2)]
      public required DateTimeOffset CreatedAt { get; init; }
      [Id(3)]
      public required string Title { get; init; }
      [Id(4)]
      public required ToDoStatus Status { get; init; }
    }

    [Id(0)]
    public required IReadOnlyCollection<ToDoItem> ToDoItemList { get; init; }
  }
}
