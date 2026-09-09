using Flensrocker.EventSourcing.Orleans.EntityFrameworkCore;

namespace Flensrocker.ToDo.Orleans.Server;

public static class ToDoOrleansServerExtensions
{
  extension(ISiloBuilder siloBuilder)
  {
    public ISiloBuilder AddToDoOrleans()
    {
      siloBuilder.Services.AddOrleansEntityFrameworkCoreProjections();

      return siloBuilder;
    }
  }
}
