using Flensrocker.Domain.Abstractions;
using Flensrocker.ToDo.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Serialization;

namespace Flensrocker.ToDo.Orleans.Abstractions;

public static class ToDoOrleansExtensions
{
  public static IServiceCollection AddToDoSerializer(this IServiceCollection services)
  {
    services.AddSerializer(serializerBuilder =>
    {
      serializerBuilder.AddJsonSerializer(isSupported: type => type.IsAssignableTo(typeof(ToDoCommand)) || type.IsAssignableTo(typeof(DomainEvent<ToDoEvent>)), ToDoJsonSerializerContext.Default.Options);
    });

    return services;
  }
}
