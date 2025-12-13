namespace Flensrocker.Measure.Orleans.Abstractions;

public interface IDummyGrain : IGrainWithGuidKey
{
  Task ExecuteNoop(DummyCommand command);
}
