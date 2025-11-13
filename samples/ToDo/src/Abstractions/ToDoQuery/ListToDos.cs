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
    // TODO Make own type for items without TenantId
    public required IReadOnlyCollection<ToDoView.ToDoListItem> ToDoItemList { get; init; }
  }
}
