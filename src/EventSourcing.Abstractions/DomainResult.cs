using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Flensrocker.EventSourcing.Abstractions;

public static class DomainResult
{
  public struct InternalError
  {
    public string? Message { get; init; }

    public InternalError(string? message)
    {
      Message = message;
    }
  }

  public struct NotFound
  {
    public string? Message { get; init; }

    public NotFound(string? message)
    {
      Message = message;
    }
  }

  public static DomainResult<T> Success<T>(T value) => new DomainResult<T>.Success { Value = value };

  public struct ValidationProblem
  {
    public string Title { get; init; } = "";

    public string Detail { get; init; } = "";

    public ImmutableDictionary<string, string[]> Errors { get; private set; } = ImmutableDictionary<string, string[]>.Empty;

    public bool HasErrors
    {
      get => Errors.Count > 0;
    }

    public ValidationProblem() { }

    public void AddError(string key, string[] messages)
    {
      if (Errors.TryGetValue(key, out var existingMessages))
        Errors = Errors.SetItem(key, existingMessages.Concat(messages).ToArray());
      else
        Errors = Errors.Add(key, messages);
    }

    public void AddError(string key, string message) => AddError(key, [message]);
  }
}

[GenerateSerializer]
[Alias($"{EventSourcingConstants.DomainResult.Base}`1")]
public abstract class DomainResult<T>
{
  private DomainResult() { }

  public abstract DomainResult<TOut> Cast<TOut>();

  [GenerateSerializer]
  [Alias(EventSourcingConstants.DomainResult.InternalError)]
  public sealed class InternalError : DomainResult<T>
  {
    [Id(0)]
    public string? Message { get; init; }

    public override DomainResult<TOut> Cast<TOut>()
    {
      return new DomainResult<TOut>.InternalError { Message = Message };
    }
  }

  [GenerateSerializer]
  [Alias(EventSourcingConstants.DomainResult.NotFound)]
  public sealed class NotFound : DomainResult<T>
  {
    [Id(0)]
    public string? Message { get; init; }

    public override DomainResult<TOut> Cast<TOut>()
    {
      return new DomainResult<TOut>.NotFound { Message = Message };
    }
  }

  [GenerateSerializer]
  [Alias(EventSourcingConstants.DomainResult.Success)]
  public sealed class Success : DomainResult<T>
  {
    [Id(0)]
    public required T Value { get; init; }

    public override DomainResult<TOut> Cast<TOut>()
    {
      return Value is TOut outValue
        ? new DomainResult<TOut>.Success { Value = outValue }
        : new DomainResult<TOut>.Success { Value = default! };
    }
  }

  [GenerateSerializer]
  [Alias(EventSourcingConstants.DomainResult.ValidationProblem)]
  public sealed class ValidationProblem : DomainResult<T>
  {
    [Id(0)]
    public required string Title { get; init; }

    [Id(1)]
    public required string Detail { get; init; }

    [Id(2)]
    public required ImmutableDictionary<string, string[]> Errors { get; init; }

    public override DomainResult<TOut> Cast<TOut>()
    {
      return new DomainResult<TOut>.ValidationProblem
      {
        Title = Title,
        Detail = Detail,
        Errors = Errors,
      };
    }
  }

  public static implicit operator DomainResult<T>(T value) => new Success { Value = value };

  public static implicit operator DomainResult<T>(DomainResult.InternalError error) => new InternalError { Message = error.Message };

  public static implicit operator DomainResult<T>(DomainResult.NotFound error) => new NotFound { Message = error.Message };

  public static implicit operator DomainResult<T>(DomainResult.ValidationProblem error) =>
    new ValidationProblem
    {
      Title = error.Title,
      Detail = error.Detail,
      Errors = error.Errors,
    };
}

public static class DomainResultExtensions
{
  extension<T>(DomainResult<T> result)
  {
    public bool IsSuccess()
    {
      return result is DomainResult<T>.Success;
    }

    public bool IsSuccess([MaybeNullWhen(false)] out T value)
    {
      if (result is DomainResult<T>.Success success)
      {
        value = success.Value;
        return true;
      }

      value = default!;
      return false;
    }

    public DomainResult<TOut> Bind<TOut>(Func<T, DomainResult<TOut>> onSuccess)
    {
      return result.IsSuccess(out var value) ? onSuccess(value) : result.Cast<TOut>();
    }

    public async Task<DomainResult<TOut>> Bind<TOut>(
      Func<T, CancellationToken, Task<DomainResult<TOut>>> onSuccessAsync,
      CancellationToken cancellationToken
    )
    {
      return result.IsSuccess(out var value) ? await onSuccessAsync(value, cancellationToken).ConfigureAwait(false) : result.Cast<TOut>();
    }

    public async Task<DomainResult<TOut>> Bind<TOut>(Func<T, Task<DomainResult<TOut>>> onSuccessAsync)
    {
      return result.IsSuccess(out var value) ? await onSuccessAsync(value).ConfigureAwait(false) : result.Cast<TOut>();
    }
  }

  extension<T>(Task<DomainResult<T>> asyncResult)
  {
    public async Task<DomainResult<TOut>> Bind<TOut>(
      Func<T, CancellationToken, Task<DomainResult<TOut>>> onSuccessAsync,
      CancellationToken cancellationToken
    )
    {
      var result = await asyncResult.ConfigureAwait(false);
      return await result.Bind(onSuccessAsync, cancellationToken).ConfigureAwait(false);
    }

    public async Task<DomainResult<TOut>> Bind<TOut>(Func<T, Task<DomainResult<TOut>>> onSuccessAsync)
    {
      var result = await asyncResult.ConfigureAwait(false);
      return await result.Bind(onSuccessAsync).ConfigureAwait(false);
    }
  }
}
