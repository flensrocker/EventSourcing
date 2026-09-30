using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Orleans.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Orleans.Streams.Core;

namespace Flensrocker.EventSourcing.Orleans.Server;

public abstract partial class DomainEventRouterGrain<TProjection> : Grain, IStreamSubscriptionObserver, IAsyncObserver<DomainEvent>
{
  private readonly ILogger _logger;

  protected DomainEventRouterGrain()
  {
    _logger = ServiceProvider.GetRequiredService<ILogger<DomainEventRouterGrain<TProjection>>>();
  }

  // TODO change return type to union(string, Guid) in C# 15/.NET 11
  protected abstract object GetTargetKey(DomainEvent domainEvent);

  public async Task OnSubscribed(IStreamSubscriptionHandleFactory handleFactory)
  {
    var handle = handleFactory.Create<DomainEvent>();
    await handle.ResumeAsync(this);
  }

  public async Task OnNextAsync(DomainEvent domainEvent, StreamSequenceToken? token = null)
  {
    var targetKey = GetTargetKey(domainEvent);

    IUpdateDomainEventProjectionGrain<TProjection>? targetGrain = targetKey switch
    {
      string stringKey => GrainFactory.GetGrain<IUpdateDomainEventProjectionGrainWithStringKey<TProjection>>(stringKey),
      Guid guidKey => GrainFactory.GetGrain<IUpdateDomainEventProjectionGrainWithGuidKey<TProjection>>(guidKey),
      _ => null,
    };

    if (targetGrain is null)
      LogTargetKeyError(_logger, typeof(TProjection), targetKey.GetType(), targetKey);
    else
      await targetGrain.UpdateProjection(domainEvent);
  }

  public Task OnErrorAsync(Exception ex)
  {
    LogStreamError(_logger, ex);

    return Task.CompletedTask;
  }

  [LoggerMessage(LogLevel.Error, Message = "Unsupported target key type for projection {ProjectionType}: {TargetKeyType} ({targetKey})")]
  internal static partial void LogTargetKeyError(ILogger logger, Type projectionType, Type targetKeyType, object targetKey);

  [LoggerMessage(LogLevel.Error, Message = "Unexpected error from the Orleans Stream")]
  internal static partial void LogStreamError(ILogger logger, Exception ex);
}
