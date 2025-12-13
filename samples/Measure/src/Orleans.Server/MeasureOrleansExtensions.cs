namespace Flensrocker.Measure.Orleans.Server;

public static class ToDoOrleansExtensions
{
  extension(ISiloBuilder siloBuilder)
  {
    public ISiloBuilder AddMeasureOrleans()
    {
      return siloBuilder;
    }
  }
}
