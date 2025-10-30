using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Orleans.Abstractions;
using Flensrocker.ToDo.WebApi;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Host.UseOrleansClient(static clientBuilder =>
{
  clientBuilder.UseLocalhostClustering();

  clientBuilder.Services.AddToDoSerializer();
});

builder.Services.ConfigureHttpJsonOptions(jsonOptions =>
{
  jsonOptions.SerializerOptions.AllowOutOfOrderMetadataProperties = ToDoJsonSerializerContext.Default.Options.AllowOutOfOrderMetadataProperties;
  jsonOptions.SerializerOptions.PropertyNamingPolicy = ToDoJsonSerializerContext.Default.Options.PropertyNamingPolicy;
  jsonOptions.SerializerOptions.TypeInfoResolverChain.Add(ToDoJsonSerializerContext.Default);
});

using var app = builder.Build();

app.MapDefaultEndpoints();

app.MapToDo();

app.Run();
