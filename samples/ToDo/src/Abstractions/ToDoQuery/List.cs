using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions;

[JsonDerivedType(typeof(List), ToDoConstants.ToDoQueryRequest.Base)]
public abstract partial record ToDoQueryRequest
{
  [GenerateSerializer]
  [Alias(ToDoConstants.ToDoQueryRequest.List)]
  public sealed record List : ToDoQueryRequest
  {
    [Id(0)]
    public Guid? TenantId { get; init; }

    // TODO add filter, order, page-size etc.

    [JsonConstructor]
    internal List() { }

    public static List? Create(Guid? tenantId)
    {
      return new List { TenantId = tenantId };
    }
  }
}

[JsonDerivedType(typeof(List), ToDoConstants.ToDoQueryResponse.List)]
public abstract partial record ToDoQueryResponse
{
  [GenerateSerializer]
  [Alias(ToDoConstants.ToDoQueryResponse.List)]
  public sealed record List : ToDoQueryResponse
  {
    [GenerateSerializer]
    [Alias(ToDoConstants.ToDoQueryResponse.List_Item)]
    public sealed record Item
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
    public required IReadOnlyCollection<Item> ItemList { get; init; }
  }
}
