using Flensrocker.EventSourcing.Test.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Flensrocker.ToDo.IntegrationTests;

public class ToDoWebApplicationFactory : WebApplicationFactory<Program>
{
  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder.UseSetting("ConnectionStrings:DefaultConnection", TestPostgreSQLContainer.GetConnectionString());
  }
}
