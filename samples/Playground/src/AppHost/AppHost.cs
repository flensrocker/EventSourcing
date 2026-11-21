using Playground.ServiceDefaults;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("compose");

// TODO not working anymore?
//var garnetClusteringStorage = builder.AddGarnet(ServiceKeys.OrleansClustering).WithOrleansProviderType("Redis");
var garnetClusteringStorage = builder.AddRedis(ServiceKeys.OrleansClustering)
// We don't need persistent clustering data because we always restart the whole thing and sometimes it tries to connect to the old silo
//.WithDataVolume("playground-garnet-data")
;

var nats = builder.AddNats(ServiceKeys.OrleansStreaming).WithImageTag("2").WithJetStream().WithDataVolume("playground-nats-data");

var postgres = builder
  .AddPostgres("postgres")
  .WithImageTag("18-alpine")
  .WithArgs("-c", "wal_level=logical")
  .WithDataVolume("playground-postgres-data")
  .WithPgWeb();

var db = postgres.AddDatabase(ServiceKeys.PostgresDb, databaseName: "playground");

var dbMigration = builder
  .AddDotnetProject("playground-db-migration", "../Migration")
  //.WithArgs("--force")
  .WithReference(db)
  .WithParentRelationship(db)
  .WaitFor(db);

var orleans = builder
  .AddOrleans("playground-orleans")
  .WithServiceId("playground")
  .WithClusterId("default")
  .WithClustering(garnetClusteringStorage)
  // NOTE does not work yet: .WithStreaming(Flensrocker.EventSourcing.Orleans.Abstractions.OrleansConstants.DomainEventStreamProviderName, nats)
  .WithMemoryGrainStorage("PubSubStore");

IResourceBuilder<IResourceWithServiceDiscovery> webApi;
var hostAllServicesInOneProcess = false;

if (hostAllServicesInOneProcess)
{
  webApi = builder
    .AddDotnetProject("playground-webapi", "../AllInOne")
    .WithReference(nats)
    .WithReference(db)
    .WithReference(orleans)
    .WithExternalHttpEndpoints()
    .WaitFor(nats)
    .WaitFor(garnetClusteringStorage)
    .WaitForCompletion(dbMigration);
}
else
{
  var silo = builder
    .AddDotnetProject("playground-orleans-server", "../Orleans.Server")
    .WithReference(nats)
    .WithReference(db)
    .WithReference(orleans)
    .WaitFor(nats)
    .WaitFor(garnetClusteringStorage)
    .WaitForCompletion(dbMigration);

  var outbox = builder
    .AddDotnetProject("playground-outbox", "../Outbox")
    .WithReference(nats)
    .WithReference(db)
    .WithReference(orleans.AsClient())
    .WaitFor(nats)
    .WaitFor(garnetClusteringStorage)
    .WaitForCompletion(dbMigration)
    .WaitFor(silo);

  webApi = builder
    .AddDotnetProject("playground-webapi", "../WebApi")
    .WithReference(nats)
    .WithReference(db)
    .WithReference(orleans.AsClient())
    .WithExternalHttpEndpoints()
    .WaitFor(nats)
    .WaitFor(garnetClusteringStorage)
    .WaitForCompletion(dbMigration)
    .WaitFor(silo);
}

var frontend = builder
  .AddJavaScriptApp("playground-frontend", "../Frontend", runScriptName: "start")
  .WithPnpm(install: true, installArgs: ["--yes", "--frozen-lockfile"])
  .WithReference(webApi)
  .WaitFor(webApi)
  .WithHttpEndpoint(env: "PORT")
  .WithHttpHealthCheck()
  .WithExternalHttpEndpoints();

builder.Build().Run();
