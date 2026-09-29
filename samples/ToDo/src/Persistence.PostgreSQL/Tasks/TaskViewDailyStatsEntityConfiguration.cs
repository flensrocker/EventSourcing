using Flensrocker.ToDo.Abstractions.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Flensrocker.ToDo.Persistence.PostgreSQL.Tasks;

internal class TaskViewDailyStatsEntityConfiguration : IEntityTypeConfiguration<TaskView.DailyStats>
{
  public void Configure(EntityTypeBuilder<TaskView.DailyStats> builder)
  {
    builder.ToTable(ToDoPostgreSQLPersistenceConstants.TaskDailyStatsTableName, ToDoPostgreSQLPersistenceConstants.SchemaName);

    builder.Property(e => e.DailyStatsId).IsRequired();
    builder.Property(e => e.Day);
    builder.Property(e => e.TenantId);
    builder.Property(e => e.UserId);
    builder.Property(e => e.CountNoted);
    builder.Property(e => e.CountCompleted);
    builder.Property(e => e.CountDismissed);

    builder.HasKey(e => e.DailyStatsId);
    builder
      .HasIndex(e => new
      {
        e.Day,
        e.TenantId,
        e.UserId,
      })
      .IsUnique();
  }
}
