using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions;

[JsonDerivedType(typeof(ListToDos), nameof(ListToDos))]
public abstract partial record ToDoQueryRequest
{
  public sealed record ListToDos : ToDoQueryRequest
  {
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
  public sealed record ListToDos : ToDoQueryResponse
  {
    public sealed record ToDoItem
    {
      public required Guid ToDoId { get; init; }
      public required int Version { get; init; }
      public required DateTimeOffset CreatedAt { get; init; }
      public required string Title { get; init; }
      public required ToDoStatus Status { get; init; }
    }

    public required IReadOnlyCollection<ToDoItem> ToDoItemList { get; init; }
  }
}
