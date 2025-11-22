using Flensrocker.EventSourcing.Orleans.EntityFrameworkCore;

namespace Flensrocker.ToDo.Orleans.Server;

public static class ToDoOrleansExtensions
{
  public static ISiloBuilder AddToDoOrleans(this ISiloBuilder siloBuilder)
  {
    siloBuilder.Services.AddOrleansEntityFrameworkCoreProjections();

    return siloBuilder;
  }
}
