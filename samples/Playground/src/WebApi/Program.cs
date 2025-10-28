using Flensrocker.ToDo.Orleans.Abstractions;
using Flensrocker.ToDo.WebApi;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseOrleansClient(static clientBuilder =>
{
  clientBuilder.UseLocalhostClustering();

  clientBuilder.Services.AddToDoSerializer();
});

using var app = builder.Build();

app.MapToDo();

app.Run();
