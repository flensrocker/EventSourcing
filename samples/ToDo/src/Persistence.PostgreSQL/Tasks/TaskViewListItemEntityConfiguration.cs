using Flensrocker.ToDo.Abstractions.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Flensrocker.ToDo.Persistence.PostgreSQL.Tasks;

internal class TaskViewListItemEntityConfiguration : IEntityTypeConfiguration<TaskView.ListItem>
{
  public void Configure(EntityTypeBuilder<TaskView.ListItem> builder)
  {
    builder.ToTable(ToDoPostgreSQLPersistenceConstants.TaskListItemTableName, ToDoPostgreSQLPersistenceConstants.SchemaName);

    builder.HasKey(e => e.TaskId);

    builder.Property(e => e.Version);
    builder.Property(e => e.TenantId);
    builder.Property(e => e.OpenedAt);
    builder.Property(e => e.Title).HasMaxLength(TaskConstants.TaskTitleMaxLength);
    builder.Property(e => e.Status);

    builder.HasIndex(e => new { e.TenantId, e.TaskId }).IsUnique();
  }
}
