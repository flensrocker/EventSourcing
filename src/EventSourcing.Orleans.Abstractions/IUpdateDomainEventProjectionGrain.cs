using Flensrocker.EventSourcing.Abstractions;

namespace Flensrocker.EventSourcing.Orleans.Abstractions;

public interface IUpdateDomainEventProjectionGrain<TProjection>
{
  Task UpdateProjection(DomainEvent domainEvent);
}

public interface IUpdateDomainEventProjectionGrainWithGuidKey<TProjection>
  : IGrainWithGuidKey,
    IUpdateDomainEventProjectionGrain<TProjection> { }

public interface IUpdateDomainEventProjectionGrainWithStringKey<TProjection>
  : IGrainWithStringKey,
    IUpdateDomainEventProjectionGrain<TProjection> { }
