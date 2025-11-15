# Implementing Event Sourcing and CQRS with Microsoft Orleans

This is an experimental playground to explore the possibilities within my requirements.

- **On-Premise:** No "cloud only" features like Azure Tables etc.
- **Cheap:** Low license costs, therefore PostgreSQL as a storage provider
- **Scalable:** If really needed

## Why Orleans?

Grains are single-threaded (non-reentrant)  
=> Queuing of commands sent to aggregates

Grains are kept in memory for some time  
=> Caching of active aggregates/projections

Streaming  
=> Easy In-Memory notifications  
=> Abstract integration with different message brokers

Extensible providers for storage/streaming  
=> Integration with custom storage/message broker

...and I always wanted to try this, so why not? 🤷

## Execute Command

- Frontend sends a command to the WebApi
- After validating the command the WebApi forwards it to the Orleans cluster
- Orleans activates the aggregate grain and restores its state from the event store (PostgreSQL)
- The aggregate handles the command and returns the created events
- Orleans stores the events in the event store and apply them to the aggregate (in memory)
- Success is reported back to the WebApi and Frontend
- After a timeout Orleans deactivates the aggregate grain and releases its resources

```mermaid
sequenceDiagram
  participant Frontend
  participant WebApi
  participant Orleans
  participant PostgreSQL@{ "type": "database" }

  Frontend->>WebApi: send command
  Note over WebApi: validate command
  WebApi->>Orleans: forward command<br/>to cluster
  activate Orleans
  Orleans<<->>PostgreSQL: reconstruct aggregate<br/>from events
  Note over Orleans: execute command<br/>on aggregate
  Orleans->>PostgreSQL: write events
  Note over Orleans: apply events and<br/>keep aggregate in memory
  Orleans->>WebApi: reply to command
  WebApi->>Frontend: reply to command
  Note over Orleans: deactivate after<br/>idle timeout
  deactivate Orleans
```

## Execute Commands In Parallel

While Orleans is executing a command on a aggregate grain, incoming additional commands are queued in the inbox of the grain.
Only after finishing the first command, the next command will be executed.
Since the aggregate stays in memory, restoring from the event store is not needed.

```mermaid
sequenceDiagram
  participant Frontend
  participant WebApi
  participant Orleans
  participant PostgreSQL@{ "type": "database" }

  Frontend->>WebApi: send command 1
  Note over WebApi: validate command 1
  WebApi->>Orleans: forward command 1<br/>to cluster
  activate Orleans
  Orleans<<->>PostgreSQL: reconstruct aggregate<br/>from events
  Frontend->>WebApi: send command 2
  Note over WebApi: validate command 2
  WebApi->>Orleans: forward command 2<br/>to cluster
  Note over Orleans: queue command 2<br/>for aggregate
  Note over Orleans: execute command 1<br/>on aggregate
  Orleans->>PostgreSQL: write events<br/>from command 1
  Note over Orleans: apply events and<br/>keep aggregate in memory
  Orleans->>WebApi: reply to command 1
  WebApi->>Frontend: reply to command 1
  Note over Orleans: execute command 2<br/>on aggregate
  Orleans->>PostgreSQL: write events
  Note over Orleans: apply events and<br/>keep aggregate in memory
  Orleans->>WebApi: reply to command 2
  WebApi->>Frontend: reply to command 2
  Note over Orleans: deactivate after<br/>idle timeout
  deactivate Orleans
```

## Persistent Event Projection

- Orleans stores the event in the event store (PostgreSQL)
- PostgreSQL publishes the event with its logical replication feature
- The Outbox is listening on its replication slot and publishes the events on a message queue (NATS)
- NATS is forwarding the events back into the Orleans cluster
- Orleans activates a projection grain which updates the corresponding row in a database table
- The projection grains stays in memory, speeding up the updates
- Multiple events are processed in order one at the time (like command queuing)
- After a timeout Orleans deactivates the projection grain and releases its resources

```mermaid
sequenceDiagram
  participant Orleans
  participant PostgreSQL@{ "type": "database" }
  participant Outbox
  participant NATS@{ "type": "queue" }

  Orleans->>NATS: subscribe<br/>for events
  Orleans->>PostgreSQL: write events
  PostgreSQL->>Outbox: send events via<br/>logical replication
  Outbox->>NATS: publish events
  Outbox->>PostgreSQL: confirm events
  NATS->>Orleans: stream events<br/>to subscribers
  Orleans->>PostgreSQL: project events<br/>into table
```

## Query Persisted Views

- Frontend sends a query to the WebApi
- After validating the request the WebApi executes the query against the database
- The response is sent back to the Frontend

```mermaid
sequenceDiagram
  participant Frontend
  participant WebApi
  participant PostgreSQL@{ "type": "database" }

  Frontend->>WebApi: send request
  Note over WebApi: validate request
  WebApi<<->>PostgreSQL: execute query
  WebApi->>Frontend: reply with response
```

## Query In-Memory Views

- Frontend sends a query to the WebApi
- After validating the request the WebApi forwards it to the Orleans cluster
- Orleans activates the view grain and restores its state from the event store (PostgreSQL)
- The view grain subscribes for updates on this view on the message queue
- The response is sent back to the WebApi and Frontend
- While the view grain is activated it updates itself in memory
- The next request can be answered directly from memory, speeding up the response time
- After a timeout Orleans deactivates the view grain and releases its resources

```mermaid
sequenceDiagram
  participant Frontend
  participant WebApi
  participant Orleans
  participant PostgreSQL@{ "type": "database" }
  participant NATS@{ "type": "queue" }

  Orleans->>NATS: subscribe for events
  Frontend->>WebApi: send request
  Note over WebApi: validate request
  WebApi->>Orleans: forward request<br/>to cluster
  activate Orleans
  Orleans<<->>PostgreSQL: reconstruct view<br/>from events
  Orleans->>WebApi: reply with response
  WebApi->>Frontend: forward response
  NATS->>Orleans: update view<br/>on new events
  Frontend->>WebApi: send request
  Note over WebApi: validate request
  WebApi->>Orleans: forward request<br/>to cluster
  Orleans->>WebApi: reply with response
  WebApi->>Frontend: forward response
  Note over Orleans: deactivate after<br/>idle timeout
  deactivate Orleans
```
