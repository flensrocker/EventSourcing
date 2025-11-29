var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("compose");

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
  //.WithPgAdmin()
  .WithPgWeb();

var playgroundDb = postgres.AddDatabase("playground");

var migration = builder
  .AddProject<Projects.Playground_Migration>("playground-migration")
  .WithReference(playgroundDb)
  .WithParentRelationship(playgroundDb)
  .WaitFor(playgroundDb);

var orleans = builder
  .AddOrleans("Playground")
  .WithServiceId("playground")
  .WithClusterId("default")
  .WithClustering(redis)
  .WithMemoryStreaming(Flensrocker.EventSourcing.Orleans.Abstractions.OrleansConstants.DomainEventStreamProviderName)
  .WithMemoryGrainStorage("PubSubStore");

//var allInOne = builder
//  .AddProject<Projects.Playground_AllInOne>("playground-all-in-one")
//  .WithReference(nats)
//  .WithReference(playgroundDb)
//  .WithReference(orleans)
//  .WithExternalHttpEndpoints()
//  .WaitFor(nats)
//  .WaitFor(redis)
//  .WaitForCompletion(migration);

var silo = builder
  .AddProject<Projects.Playground_Orleans_Server>("playground-orleans-server")
  .WithReference(nats)
  .WithReference(playgroundDb)
  .WithReference(orleans)
  .WaitFor(nats)
  .WaitFor(redis)
  .WaitForCompletion(migration);

var outbox = builder
  .AddProject<Projects.Playground_Outbox>("playground-outbox")
  .WithReference(nats)
  .WithReference(playgroundDb)
  .WithReference(orleans.AsClient())
  .WaitFor(nats)
  .WaitFor(redis)
  .WaitForCompletion(migration)
  .WaitFor(silo);

var webApi = builder
  .AddProject<Projects.Playground_WebApi>("playground-webapi")
  .WithReference(nats)
  .WithReference(playgroundDb)
  .WithReference(orleans.AsClient())
  .WithExternalHttpEndpoints()
  .WaitFor(nats)
  .WaitFor(redis)
  .WaitForCompletion(migration)
  .WaitFor(silo);

builder.Build().Run();
