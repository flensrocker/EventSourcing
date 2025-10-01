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
