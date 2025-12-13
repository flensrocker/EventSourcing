namespace Flensrocker.Measure.Orleans.Abstractions;

[GenerateSerializer]
public record DummyCommand
{
  [Id(0)]
  public required string Content { get; init; }
}
