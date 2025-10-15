using Orleans;

namespace Flensrocker.Domain.Abstractions;

[GenerateSerializer]
public sealed record DomainUserClaim
{
  [Id(0)]
  public required string Type { get; init; }
  [Id(1)]
  public required string Value { get; init; }
}

[GenerateSerializer]
public sealed record DomainUser
{
  [Id(0)]
  public required Guid TenantId { get; init; }
  [Id(1)]
  public required Guid UserId { get; init; }
  [Id(2)]
  public IReadOnlyCollection<DomainUserClaim> Claims { get; init; } = [];
}
