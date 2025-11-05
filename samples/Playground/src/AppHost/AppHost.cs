var builder = DistributedApplication.CreateBuilder(args);

var redis = builder
  .AddRedis("redis")
  .WithImageRegistry("ghcr.io")
  .WithImage("microsoft/garnet", "1")
  .WithEntrypoint("/app/GarnetServer")
  .WithDataVolume("redis-data");

var nats = builder
  .AddNats("nats")
  .WithJetStream()
  .WithDataVolume("nats-data");

var postgres = builder
  .AddPostgres("postgres")
  .WithImageTag("18-alpine")
  .WithArgs("-c", "wal_level=logical")
  .WithDataVolume("postgres-data");

var tododb = postgres.AddDatabase("todo");

var migrate = builder
  .AddProject<Projects.Playground_Migrate>("migrate")
  .WithReference(tododb)
  .WaitFor(tododb);

var orleans = builder
  .AddOrleans("ToDo")
  .WithServiceId("ToDo")
  .WithClusterId("default")
  .WithClustering(redis)
  .WithMemoryGrainStorage("PubSubStore");
//.WithStreaming(Flensrocker.Domain.Orleans.Abstractions.DomainConstants.StreamProviderDomainEvents, nats); // TODO needs NatsStreamProviderBuilder

var silo = builder
  .AddProject<Projects.Playground_Orleans_Server>("playground-orleans-server")
  .WithReference(nats) // TODO remove when NatsStreamProviderBuilder exist
  .WithReference(tododb)
  .WithReference(orleans)
  //.WithReplicas(3)
  .WaitFor(nats)
  .WaitFor(redis)
  .WaitForCompletion(migrate);

builder
  .AddProject<Projects.Playground_WebApi>("playground-webapi")
  .WithReference(nats) // TODO remove when NatsStreamProviderBuilder exist
  .WithReference(orleans.AsClient())
  .WithExternalHttpEndpoints()
  .WaitFor(nats)
  .WaitFor(redis)
  .WaitFor(silo);

builder.Build().Run();
