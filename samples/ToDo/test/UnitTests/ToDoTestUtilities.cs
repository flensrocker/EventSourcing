using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.EventSourcing.Test.Utilities;
using Flensrocker.ToDo.Abstractions;

namespace Flensrocker.ToDo.UnitTests;

internal static class ToDoTestUtilities
{
  internal static EventSourcingJsonSerializerOptions JsonOptions { get; } =
    new EventSourcingJsonSerializerOptions().AddToDoJsonOptions().MakeReadOnly();

  internal static void AssertJsonSerializable<TBase, TValue>(DomainResult<TValue> result)
    where TValue : TBase => TestUtilities.AssertJsonSerializable<TBase, TValue>(result, JsonOptions.Options);

  internal static void AssertJsonSerializable<TValue>(DomainResult<TValue> result) =>
    TestUtilities.AssertJsonSerializable(result, JsonOptions.Options);
}
