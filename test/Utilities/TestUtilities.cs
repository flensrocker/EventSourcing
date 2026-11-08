using System.Text.Json;
using Flensrocker.EventSourcing.Abstractions;

namespace Flensrocker.EventSourcing.Test.Utilities;

public static class TestUtilities
{
  public static void AssertJsonSerializable<TBase, TValue>(DomainResult<TValue> result, JsonSerializerOptions jsonOptions)
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

  public static void AssertJsonSerializable<TValue>(DomainResult<TValue> result, JsonSerializerOptions jsonOptions)
  {
    var expected = Assert.IsInstanceOfType<DomainResult<TValue>.Success>(result);

    var jsonResult = expected.Pipe(value => DomainResult.Success(JsonSerializer.Serialize(value, jsonOptions)));
    var expectedJson = Assert.IsInstanceOfType<DomainResult<string>.Success>(jsonResult);

    var testResult = jsonResult.Pipe(valueJson => DomainResult.Success(JsonSerializer.Deserialize<TValue>(valueJson, jsonOptions)));

    var actual = Assert.IsInstanceOfType<DomainResult<TValue>.Success>(testResult);
    var actualJson = JsonSerializer.Serialize(actual.Value, jsonOptions);
    Assert.AreEqual(expectedJson.Value, actualJson);
  }

  public static object TestGetKey<T>(DomainEvent domainEvent)
    where T : IWhereKey<T>
  {
    return T.GetKey(domainEvent);
  }

  public static bool TestWhereKey<T>(T item, DomainEvent domainEvent)
    where T : IWhereKey<T>
  {
    return T.GetWhereKey(domainEvent).Compile().Invoke(item);
  }
}
