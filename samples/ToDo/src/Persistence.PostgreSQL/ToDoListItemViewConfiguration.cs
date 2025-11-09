using Flensrocker.ToDo.Abstractions;
using Flensrocker.ToDo.Abstractions.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Flensrocker.ToDo.Persistence.PostgreSQL;

internal class ToDoListItemViewConfiguration : IEntityTypeConfiguration<ToDoListItemView>
{
  public void Configure(EntityTypeBuilder<ToDoListItemView> builder)
  {
    builder.ToTable(Constants.ListItemTableName, Constants.SchemaName);

    builder.HasKey(e => e.ToDoId);

    builder.Property(e => e.Version);
    builder.Property(e => e.TenantId);
    builder.Property(e => e.CreatedAt);
    builder.Property(e => e.Title).HasMaxLength(ToDoConstants.TitleMaxLength);
    builder.Property(e => e.Status);

    builder.HasIndex(e => new { e.TenantId, e.ToDoId }).IsUnique();
  }
}
