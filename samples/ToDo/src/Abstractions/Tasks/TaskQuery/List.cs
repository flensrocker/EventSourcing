using System.Text.Json.Serialization;

namespace Flensrocker.ToDo.Abstractions.Tasks;

[JsonDerivedType(typeof(List), TaskConstants.TaskQueryRequest.Base)]
public abstract partial record TaskQueryRequest
{
  [GenerateSerializer]
  [Alias(TaskConstants.TaskQueryRequest.List)]
  public sealed record List : TaskQueryRequest
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

[JsonDerivedType(typeof(List), TaskConstants.TaskQueryResponse.List)]
public abstract partial record TaskQueryResponse
{
  [GenerateSerializer]
  [Alias(TaskConstants.TaskQueryResponse.List)]
  public sealed record List : TaskQueryResponse
  {
    [GenerateSerializer]
    [Alias(TaskConstants.TaskQueryResponse.List_Item)]
    public sealed record Item
    {
      [Id(0)]
      public required Guid TaskId { get; init; }

      [Id(1)]
      public required int Version { get; init; }

      [Id(2)]
      public required DateTimeOffset CreatedAt { get; init; }

      [Id(3)]
      public required string Title { get; init; }

      [Id(4)]
      public required TaskStatus Status { get; init; }
    }

    [Id(0)]
    public required IReadOnlyCollection<Item> ItemList { get; init; }
  }
}
