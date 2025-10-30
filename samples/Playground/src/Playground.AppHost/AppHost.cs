var builder = DistributedApplication.CreateBuilder(args);

var orleans = builder
  .AddOrleans("ToDo")
  .WithDevelopmentClustering();

var silo = builder
  .AddProject<Projects.Playground_Orleans_Server>("playground-orleans-server")
  .WithReference(orleans)
  .WithReplicas(3);

builder
  .AddProject<Projects.Playground_WebApi>("playground-webapi")
  .WithReference(orleans.AsClient())
  .WithExternalHttpEndpoints()
  .WaitFor(silo);

builder.Build().Run();
