using System.Text.Json;
using Flensrocker.EventSourcing.Abstractions;

namespace Flensrocker.EventSourcing.Persistence.EntityFrameworkCore;

public interface IDomainEventStorageDbProvider
{
  (string InsertScript, IEnumerable<object> Parameters) GenerateInsertSql(
    IReadOnlyList<DomainEvent> domainEvents,
    JsonSerializerOptions jsonOptions
  );
}
