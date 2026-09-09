using Flensrocker.ToDo.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Flensrocker.ToDo.Persistence.PostgreSQL.EntityTypeConfigurations;

internal class TaskViewListItemConfiguration : IEntityTypeConfiguration<TaskView.ListItem>
{
  public void Configure(EntityTypeBuilder<TaskView.ListItem> builder)
  {
    builder.ToTable(Constants.TaskListItemTableName, Constants.SchemaName);

    builder.HasKey(e => e.TaskId);

    builder.Property(e => e.Version);
    builder.Property(e => e.TenantId);
    builder.Property(e => e.CreatedAt);
    builder.Property(e => e.Title).HasMaxLength(ToDoConstants.TaskTitleMaxLength);
    builder.Property(e => e.Status);

    builder.HasIndex(e => new { e.TenantId, e.TaskId }).IsUnique();
  }
}
