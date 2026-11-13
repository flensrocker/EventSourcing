using Flensrocker.EventSourcing.Persistence.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Flensrocker.EventSourcing.Persistence.PostgreSQL;

internal class MessageEntiyConfiguration(ModelBuilder modelBuilder, bool excludeFromMigrations) : IEntityTypeConfiguration<MessageEntity>
{
  private readonly ModelBuilder _modelBuilder = modelBuilder;
  private readonly bool _excludeFromMigrations = excludeFromMigrations;

  public void Configure(EntityTypeBuilder<MessageEntity> builder)
  {
    builder.ToTable(EntityFrameworkCoreConstants.MessageTableName, EntityFrameworkCoreConstants.SchemaName, tableBuilder =>
      tableBuilder.ExcludeFromMigrations(_excludeFromMigrations)
    );

    builder.HasKey(e => e.Id);

    if (!_excludeFromMigrations)
    {
      _modelBuilder
        .HasSequence(EntityFrameworkCoreConstants.GlobalMessageIndexSequenceName, EntityFrameworkCoreConstants.SchemaName)
        .StartsAt(1L);
    }

    builder
      .Property(e => e.GlobalIndex)
      .HasDefaultValueSql(
        $"nextval('\"{EntityFrameworkCoreConstants.SchemaName}\".\"{EntityFrameworkCoreConstants.GlobalMessageIndexSequenceName}\"')"
      );

    builder.Property(e => e.StreamType).HasMaxLength(EntityFrameworkCoreConstants.MaxLengthStreamType);

    builder.Property(e => e.TenantId);
    builder.Property(e => e.CreatedAt);
    builder.Property(e => e.CreatedByUserId);

    builder.Property(e => e.TraceId).HasMaxLength(16);
    builder.Property(e => e.SpanId).HasMaxLength(8);

    builder.Property(e => e.PayloadJson).HasColumnType("jsonb");

    builder.HasIndex(e => e.GlobalIndex).IsUnique();

    builder.HasIndex(e => new { e.StreamType, e.GlobalIndex });

    builder
      .HasIndex(e => new
      {
        e.StreamType,
        e.StreamId,
        e.StreamVersion,
      })
      .IncludeProperties(e => new { e.TenantId })
      .IsUnique();
  }
}
