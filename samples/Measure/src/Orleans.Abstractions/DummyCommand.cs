namespace Flensrocker.Measure.Orleans.Abstractions;

[GenerateSerializer]
[Alias("Measure.DummyCommand")]
public record DummyCommand
{
  [Id(0)]
  public required string Content { get; init; }
}
