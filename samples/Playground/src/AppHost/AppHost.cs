using Playground.ServiceDefaults;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("compose");

var garnetClusteringStorage = builder
  .AddGarnet(ServiceKeys.OrleansClustering)
  .WithOrleansProviderType("Redis")
  .WithDataVolume("playground-garnet-data");

var nats = builder
  .AddNats("nats")
  .WithImageTag("2")
  .WithJetStream()
  .WithDataVolume("playground-nats-data");

var azure = builder
  .AddAzureStorage("azure")
  .RunAsEmulator();
var azureQueuesStreams = azure
  .AddQueues(ServiceKeys.OrleansStreaming);

var postgres = builder
  .AddPostgres("postgres")
  .WithImageTag("18-alpine")
  .WithArgs("-c", "wal_level=logical")
  .WithDataVolume("playground-postgres-data")
  .WithPgWeb();

var db = postgres.AddDatabase(ServiceKeys.PostgresDb, databaseName: "playground");

var dbMigration = builder
  .AddProject<Projects.Playground_Migration>("playground-db-migration")
  .WithReference(db)
  .WithParentRelationship(db)
  .WaitFor(db);

var orleans = builder
  .AddOrleans("playground-orleans")
  .WithServiceId("playground")
  .WithClusterId("default")
  .WithClustering(garnetClusteringStorage)
  .WithStreaming(Flensrocker.EventSourcing.Orleans.Abstractions.OrleansConstants.DomainEventStreamProviderName, azureQueuesStreams)
  .WithMemoryGrainStorage("PubSubStore");

IResourceBuilder<IResourceWithServiceDiscovery> webApi;
var hostAllServicesInOneProcess = false;

if (hostAllServicesInOneProcess)
{
  webApi = builder
    .AddProject<Projects.Playground_AllInOne>("playground-webapi")
    .WithReference(nats)
    .WithReference(azureQueuesStreams)
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
    .AddProject<Projects.Playground_Orleans_Server>("playground-orleans-server")
    .WithReference(nats)
    .WithReference(azureQueuesStreams)
    .WithReference(db)
    .WithReference(orleans)
    .WaitFor(nats)
    .WaitFor(garnetClusteringStorage)
    .WaitForCompletion(dbMigration);

  var outbox = builder
    .AddProject<Projects.Playground_Outbox>("playground-outbox")
    .WithReference(nats)
    .WithReference(azureQueuesStreams)
    .WithReference(db)
    .WithReference(orleans.AsClient())
    .WaitFor(nats)
    .WaitFor(garnetClusteringStorage)
    .WaitForCompletion(dbMigration)
    .WaitFor(silo);

  webApi = builder
    .AddProject<Projects.Playground_WebApi>("playground-webapi")
    .WithReference(nats)
    .WithReference(azureQueuesStreams)
    .WithReference(db)
    .WithReference(orleans.AsClient())
    .WithExternalHttpEndpoints()
    .WaitFor(nats)
    .WaitFor(garnetClusteringStorage)
    .WaitForCompletion(dbMigration)
    .WaitFor(silo);
}

var frontend = builder.AddJavaScriptApp("playground-frontend", "../Frontend", runScriptName: "start")
  .WithPnpm()
  .WithReference(webApi)
  .WaitFor(webApi)
  .WithHttpEndpoint(env: "PORT")
  .WithHttpHealthCheck()
  .WithExternalHttpEndpoints();

builder.Build().Run();
