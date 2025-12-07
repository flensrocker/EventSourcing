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
  .WithPgWeb();

var db = postgres.AddDatabase("playground-db", databaseName: "playground");

var dbMigration = builder
  .AddProject<Projects.Playground_Migration>("playground-db-migration")
  .WithReference(db)
  .WithParentRelationship(db)
  .WaitFor(db);

var orleans = builder
  .AddOrleans("playground-orleans")
  .WithServiceId("playground")
  .WithClusterId("default")
  .WithClustering(redis)
  .WithMemoryGrainStorage("PubSubStore");

IResourceBuilder<IResourceWithServiceDiscovery> webApi;
var hostAllServicesInOneProcess = false;

if (hostAllServicesInOneProcess)
{
  webApi = builder
    .AddProject<Projects.Playground_AllInOne>("playground-webapi")
    .WithReference(nats)
    .WithReference(db)
    .WithReference(orleans)
    .WithExternalHttpEndpoints()
    .WaitFor(nats)
    .WaitFor(redis)
    .WaitForCompletion(dbMigration);
}
else
{
  var silo = builder
    .AddProject<Projects.Playground_Orleans_Server>("playground-orleans-server")
    .WithReference(nats)
    .WithReference(db)
    .WithReference(orleans)
    .WaitFor(nats)
    .WaitFor(redis)
    .WaitForCompletion(dbMigration);

  var outbox = builder
    .AddProject<Projects.Playground_Outbox>("playground-outbox")
    .WithReference(nats)
    .WithReference(db)
    .WithReference(orleans.AsClient())
    .WaitFor(nats)
    .WaitFor(redis)
    .WaitForCompletion(dbMigration)
    .WaitFor(silo);

  webApi = builder
    .AddProject<Projects.Playground_WebApi>("playground-webapi")
    .WithReference(nats)
    .WithReference(db)
    .WithReference(orleans.AsClient())
    .WithExternalHttpEndpoints()
    .WaitFor(nats)
    .WaitFor(redis)
    .WaitForCompletion(dbMigration)
    .WaitFor(silo);
}

var frontend = builder.AddJavaScriptApp("playground-frontend", "../Frontend", runScriptName: "start")
  .WithReference(webApi)
  .WaitFor(webApi)
  .WithHttpEndpoint(env: "PORT")
  .WithExternalHttpEndpoints();

builder.Build().Run();
