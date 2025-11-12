var builder = DistributedApplication.CreateBuilder(args);

var redis = builder
  .AddRedis("redis")
  .WithImageRegistry("ghcr.io")
  .WithImage("microsoft/garnet", "1")
  .WithEntrypoint("/app/GarnetServer")
  .WithDataVolume("playground-redis-data");

var nats = builder
  .AddNats("nats")
  .WithJetStream()
  .WithDataVolume("playground-nats-data");

var postgres = builder
  .AddPostgres("postgres")
  .WithImageTag("18-alpine")
  .WithArgs("-c", "wal_level=logical")
  .WithDataVolume("playground-postgres-data")
  .WithPgAdmin();

var playgroundDb = postgres.AddDatabase("playground");

var migrate = builder
  .AddProject<Projects.Playground_Migrate>("migrate")
  .WithReference(playgroundDb)
  .WithParentRelationship(playgroundDb)
  .WaitFor(playgroundDb);

var orleans = builder
  .AddOrleans("Playground")
  .WithServiceId("playground")
  .WithClusterId("default")
  .WithClustering(redis);

var silo = builder
  .AddProject<Projects.Playground_Orleans_Server>("playground-orleans-server")
  .WithReference(nats)
  .WithReference(playgroundDb)
  .WithReference(orleans)
  .WaitFor(nats)
  .WaitFor(redis)
  .WaitForCompletion(migrate);

builder
  .AddProject<Projects.Playground_WebApi>("playground-webapi")
  .WithReference(nats)
  .WithReference(playgroundDb)
  .WithReference(orleans.AsClient())
  .WithExternalHttpEndpoints()
  .WaitFor(nats)
  .WaitFor(redis)
  .WaitFor(silo);

builder.Build().Run();
