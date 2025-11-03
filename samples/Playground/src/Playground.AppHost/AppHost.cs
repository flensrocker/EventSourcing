using Flensrocker.Domain.Orleans.Abstractions;

var builder = DistributedApplication.CreateBuilder(args);

var nats = builder
  .AddNats("nats")
  .WithJetStream()
  .WithDataVolume();

var redis = builder
  .AddRedis("redis")
  .WithImageRegistry("ghcr.io")
  .WithImage("microsoft/garnet", "1")
  .WithEntrypoint("/app/GarnetServer")
  .WithDataVolume();

var orleans = builder
  .AddOrleans("ToDo")
  .WithServiceId("ToDo")
  .WithClusterId("default")
  .WithClustering(redis)
  .WithMemoryGrainStorage("PubSubStore")
  .WithMemoryStreaming(DomainConstants.StreamProviderDomainEvents);
  //.WithStreaming(DomainConstants.StreamProviderDomainEvents, nats);

var silo = builder
  .AddProject<Projects.Playground_Orleans_Server>("playground-orleans-server")
  .WithReference(orleans)
  //.WithReplicas(3)
  .WaitFor(nats)
  .WaitFor(redis);

builder
  .AddProject<Projects.Playground_WebApi>("playground-webapi")
  .WithReference(orleans.AsClient())
  .WithExternalHttpEndpoints()
  .WaitFor(nats)
  .WaitFor(redis)
  .WaitFor(silo);

builder.Build().Run();
