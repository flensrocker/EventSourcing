using System.Text.Json;
using Flensrocker.EventSourcing.Abstractions;
using Flensrocker.ToDo.Abstractions;

internal static class ToDoTestUtilities
{
  internal static EventSourcingJsonSerializerOptions JsonOptions { get; } =
    EventSourcingJsonSerializerOptions.Default.AddToDoJsonOptions().MakeReadOnly();

  internal static void AssertJsonSerializable<TBase, TValue>(DomainResult<TValue> result)
    where TValue : TBase => AssertJsonSerializable<TBase, TValue>(result, JsonOptions.Options);

  internal static void AssertJsonSerializable<TValue>(DomainResult<TValue> result) => AssertJsonSerializable(result, JsonOptions.Options);

  // TODO move to some global helper class
  internal static void AssertJsonSerializable<TBase, TValue>(DomainResult<TValue> result, JsonSerializerOptions jsonOptions)
    where TValue : TBase
  {
    var expected = Assert.IsInstanceOfType<DomainResult<TValue>.Success>(result);

    var jsonResult = expected.Cast<TBase>().Pipe(value => DomainResult.Success(JsonSerializer.Serialize(value, jsonOptions)));
    var expectedJson = Assert.IsInstanceOfType<DomainResult<string>.Success>(jsonResult);

    var testResult = jsonResult.Pipe(valueJson => DomainResult.Success(JsonSerializer.Deserialize<TBase>(valueJson, jsonOptions)));

    var actual = Assert.IsInstanceOfType<DomainResult<TBase>.Success>(testResult);
    var actualValue = Assert.IsInstanceOfType<TValue>(actual.Value);
    var actualJson = JsonSerializer.Serialize((TBase)actualValue, jsonOptions);
    Assert.AreEqual(expectedJson.Value, actualJson);
  }

  internal static void AssertJsonSerializable<TValue>(DomainResult<TValue> result, JsonSerializerOptions jsonOptions)
  {
    var expected = Assert.IsInstanceOfType<DomainResult<TValue>.Success>(result);

    var jsonResult = expected.Pipe(value => DomainResult.Success(JsonSerializer.Serialize(value, jsonOptions)));
    var expectedJson = Assert.IsInstanceOfType<DomainResult<string>.Success>(jsonResult);

    var testResult = jsonResult.Pipe(valueJson => DomainResult.Success(JsonSerializer.Deserialize<TValue>(valueJson, jsonOptions)));

    var actual = Assert.IsInstanceOfType<DomainResult<TValue>.Success>(testResult);
    var actualJson = JsonSerializer.Serialize(actual.Value, jsonOptions);
    Assert.AreEqual(expectedJson.Value, actualJson);
  }

  internal static bool TestWhereId<T>(T item, Guid id)
    where T : IWhereId<T>
  {
    return T.GetWhereId(id).Compile().Invoke(item);
  }
}
