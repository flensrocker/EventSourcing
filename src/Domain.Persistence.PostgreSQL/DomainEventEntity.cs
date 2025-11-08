using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Flensrocker.Domain.Persistence.PostgreSQL;

internal class DomainEventEntity
{
  // Primary Key
  internal Guid EventId { get; set; }
  // Unique
  internal long GlobalIndex { get; set; }
  // Unique (StreamType, StreamId, StreamVersion)
  // StreamType = typeof(TEventBase).GetTypeName()
  internal string StreamType { get; set; } = "";
  internal Guid StreamId { get; set; }
  // > 0
  internal int StreamVersion { get; set; }
  internal Guid? TenantId { get; set; }

  internal DateTimeOffset CreatedAt { get; set; }
  internal Guid? CreatedByUserId { get; set; }

  // DomainEvent<TEventBase>.Payload as JSON
  internal string PayloadJson { get; set; } = "";
}

internal class DomainEventEntiyConfiguration(ModelBuilder modelBuilder) : IEntityTypeConfiguration<DomainEventEntity>
{
  private readonly ModelBuilder _modelBuilder = modelBuilder;

  public void Configure(EntityTypeBuilder<DomainEventEntity> builder)
  {
    builder.ToTable(Constants.DomainEventTableName, Constants.SchemaName);

    builder.HasKey(e => e.EventId);

    _modelBuilder
      .HasSequence(Constants.GlobalIndexSequenceName, Constants.SchemaName)
      .StartsAt(1L);

    builder
      .Property(e => e.GlobalIndex)
      .HasDefaultValueSql($"nextval('\"{Constants.SchemaName}\".\"{Constants.GlobalIndexSequenceName}\"')");

    builder.Property(e => e.TenantId);
    builder.Property(e => e.CreatedAt);
    builder.Property(e => e.CreatedByUserId);

    builder
      .Property(e => e.PayloadJson)
      .HasColumnType("jsonb");

    builder
      .HasIndex(e => e.GlobalIndex)
      .IsUnique();

    builder
      .HasIndex(e => new { e.StreamType, e.StreamId, e.StreamVersion })
      .IncludeProperties(e => new { e.TenantId })
      .IsUnique();
  }
}
