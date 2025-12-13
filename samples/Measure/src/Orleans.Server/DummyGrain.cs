using Flensrocker.Measure.Orleans.Abstractions;

namespace Flensrocker.Measure.Orleans.Server;

public class DummyGrain : IDummyGrain
{
  public Task ExecuteNoop(DummyCommand command)
  {
    return Task.CompletedTask;
  }
}
