namespace Flensrocker.EventSourcing.Abstractions;

public interface IDbMigration
{
  Task MigrateAsync();
}
