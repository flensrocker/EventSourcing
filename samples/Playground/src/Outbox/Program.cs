using Flensrocker.Domain.Abstractions;
using Flensrocker.Domain.Persistence.PostgreSQL;
using Flensrocker.ToDo.Abstractions;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

var globalJsonOptions = DomainJsonSerializerOptions.Default
  .AddToDoJsonOptions();

builder.Services.AddPostgreSQLDomainMessageSubscriber(() =>
{
  return new PostgreSQLDomainMessageSubscriberOptions
  {
    ConnectionName = "playground",
    JsonOptions = globalJsonOptions,
  };
});

builder.Build().Run();
