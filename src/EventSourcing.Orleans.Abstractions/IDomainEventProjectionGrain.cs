using Flensrocker.EventSourcing.Abstractions;

namespace Flensrocker.EventSourcing.Orleans.Abstractions;

public interface IDomainEventProjectionGrain<TProjection> : IDomainEventProjection<TProjection>, IGrainWithGuidKey { }
