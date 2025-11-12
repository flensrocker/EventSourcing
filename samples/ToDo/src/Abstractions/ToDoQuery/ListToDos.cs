using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions;

[JsonDerivedType(typeof(ListToDosRequest), nameof(ListToDosRequest))]
[JsonDerivedType(typeof(ListToDosResponse), nameof(ListToDosResponse))]
public abstract partial record ToDoQuery
{
  public sealed record ListToDosRequest : ToDoQuery
  {
    public Guid? TenantId { get; init; }
    // TODO add filter, order, page-size etc.

    [JsonConstructor]
    internal ListToDosRequest() { }

    public static ListToDosRequest? Create(Guid? tenantId)
    {
      return new ListToDosRequest
      {
        TenantId = tenantId,
      };
    }
  }

  public sealed record ListToDosResponse : ToDoQuery
  {
    public required IReadOnlyCollection<ToDoView.ToDoListItem> ToDoItemList { get; init; }
  }
}
