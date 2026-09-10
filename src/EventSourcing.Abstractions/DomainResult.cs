using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Flensrocker.EventSourcing.Abstractions;

[GenerateSerializer]
[Alias(EventSourcingConstants.DomainResult.Base)]
public abstract class DomainResult
{
  private DomainResult() { }

  // for easier pattern matching
  internal interface ISuccess { }

  [GenerateSerializer]
  [Alias($"{EventSourcingConstants.DomainResult.Success}`1")]
  public sealed class Success<T> : DomainResult, ISuccess
  {
    [Id(0)]
    public required T Value { get; init; }
  }

  [GenerateSerializer]
  [Alias(EventSourcingConstants.DomainResult.Success)]
  public sealed class Success : DomainResult, ISuccess
  {
    public static readonly Success Instance = new();
  }

  [GenerateSerializer]
  [Alias(EventSourcingConstants.DomainResult.NotFound)]
  public sealed class NotFound : DomainResult
  {
    [Id(0)]
    public string? Message { get; init; }
  }

  [GenerateSerializer]
  [Alias(EventSourcingConstants.DomainResult.ValidationProblem)]
  public sealed class ValidationProblem : DomainResult
  {
    [Id(0)]
    public required string Title { get; init; }

    [Id(1)]
    public required string Detail { get; init; }

    [Id(2)]
    public required ImmutableDictionary<string, string[]> Errors { get; init; }

    public static readonly ImmutableDictionary<string, string[]> NoErrors = ImmutableDictionary<string, string[]>.Empty;
  }
}

public static class DomainResultExtensions
{
  extension(DomainResult result)
  {
    public bool IsSuccess()
    {
      return result is DomainResult.ISuccess;
    }

    public bool IsSuccess<T>([MaybeNullWhen(false)] out T value)
    {
      if (result is DomainResult.Success<T> success)
      {
        value = success.Value;
        return true;
      }

      value = default!;
      return false;
    }
  }
}
