namespace Flensrocker.EventSourcing.Abstractions;

[GenerateSerializer]
[Alias("Flensrocker_EventSourcing_UserClaim")]
public sealed record UserClaim
{
  [Id(0)]
  public required string Type { get; init; }

  [Id(1)]
  public required string Value { get; init; }
}

[GenerateSerializer]
[Alias("Flensrocker_EventSourcing_User")]
public sealed record User
{
  [Id(0)]
  public required Guid TenantId { get; init; }

  [Id(1)]
  public required Guid UserId { get; init; }

  [Id(2)]
  public IReadOnlyCollection<UserClaim> Claims { get; init; } = [];
}
