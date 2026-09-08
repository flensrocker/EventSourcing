using Flensrocker.ToDo.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Flensrocker.ToDo.Persistence.PostgreSQL.EntityTypeConfigurations;

internal class ToDoViewListItemConfiguration : IEntityTypeConfiguration<ToDoView.ListItem>
{
  public void Configure(EntityTypeBuilder<ToDoView.ListItem> builder)
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
