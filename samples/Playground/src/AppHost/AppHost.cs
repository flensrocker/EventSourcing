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
  .WithDataVolume("postgres-data")
  .WithPgAdmin();

var tododb = postgres.AddDatabase("todo");

var migrate = builder
  .AddProject<Projects.Playground_Migrate>("migrate")
  .WithReference(tododb)
  .WithParentRelationship(tododb)
  .WaitFor(tododb);

var orleans = builder
  .AddOrleans("ToDo")
  .WithServiceId("ToDo")
  .WithClusterId("default")
  .WithClustering(redis)
  .WithMemoryGrainStorage("PubSubStore");

var silo = builder
  .AddProject<Projects.Playground_Orleans_Server>("playground-orleans-server")
  .WithReference(nats)
  .WithReference(tododb)
  .WithReference(orleans)
  //.WithReplicas(3)
  .WaitFor(nats)
  .WaitFor(redis)
  .WaitForCompletion(migrate);

builder
  .AddProject<Projects.Playground_WebApi>("playground-webapi")
  .WithReference(nats)
  .WithReference(orleans.AsClient())
  .WithExternalHttpEndpoints()
  .WaitFor(nats)
  .WaitFor(redis)
  .WaitFor(silo);

builder.Build().Run();
