namespace Flensrocker.Measure.Orleans.Abstractions;

[GenerateSerializer]
[Alias("Flensrocker.Measure.DummyCommand")]
public record DummyCommand
{
  [Id(0)]
  public required string Content { get; init; }
}
