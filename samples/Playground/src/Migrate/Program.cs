using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.AddNpgsqlDataSource("tododb");

var host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();

logger.LogInformation("Migration starting");

for (var tick = 1; tick <= 5; tick++)
{
  await Task.Delay(TimeSpan.FromSeconds(2));
  logger.LogInformation($"Migrating {tick * 20}%");
}

logger.LogInformation("Migration finished");
