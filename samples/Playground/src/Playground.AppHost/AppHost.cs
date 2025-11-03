var builder = DistributedApplication.CreateBuilder(args);

var redis = builder
  .AddRedis("redis")
  .WithImageRegistry("ghcr.io")
  .WithImage("microsoft/garnet", "1")
  .WithEntrypoint("/app/GarnetServer");

var orleans = builder
  .AddOrleans("ToDo")
  .WithClustering(redis)
  .WithMemoryGrainStorage("PubSubStore");

var silo = builder
  .AddProject<Projects.Playground_Orleans_Server>("playground-orleans-server")
  .WithReference(orleans)
  //.WithReplicas(3)
  .WaitFor(redis);

builder
  .AddProject<Projects.Playground_WebApi>("playground-webapi")
  .WithReference(orleans.AsClient())
  .WithExternalHttpEndpoints()
  .WaitFor(silo)
  .WaitFor(redis);

builder.Build().Run();
