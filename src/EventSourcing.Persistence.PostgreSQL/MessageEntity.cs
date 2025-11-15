using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Flensrocker.EventSourcing.Persistence.PostgreSQL;

internal class MessageEntity
{
  // Primary Key
  internal Guid Id { get; set; }
  // Unique
  internal long GlobalIndex { get; set; }
  // Unique (StreamType, StreamId, StreamVersion)
  // StreamType = typeof(TEventBase).GetTypeName() for a DomainEvent
  // Namespace of the Orleans Stream
  internal string StreamType { get; set; } = "";
  internal Guid StreamId { get; set; }
  // if > 0 => DomainEvent
  // if == 0 => public message
  internal int StreamVersion { get; set; }
  internal Guid? TenantId { get; set; }

  internal DateTimeOffset CreatedAt { get; set; }
  internal Guid? CreatedByUserId { get; set; }

  // Either DomainEvent<TEventBase>.Payload as JSON
  // Or just the public message as JSON
  internal string PayloadJson { get; set; } = "";
}

internal class MessageEntiyConfiguration(ModelBuilder modelBuilder) : IEntityTypeConfiguration<MessageEntity>
{
  private readonly ModelBuilder _modelBuilder = modelBuilder;

  public void Configure(EntityTypeBuilder<MessageEntity> builder)
  {
    builder.ToTable(Constants.MessageTableName, Constants.SchemaName);

    builder.HasKey(e => e.Id);

    _modelBuilder
      .HasSequence(Constants.GlobalMessageIndexSequenceName, Constants.SchemaName)
      .StartsAt(1L);

    builder
      .Property(e => e.GlobalIndex)
      .HasDefaultValueSql($"nextval('\"{Constants.SchemaName}\".\"{Constants.GlobalMessageIndexSequenceName}\"')");

    builder
      .Property(e => e.StreamType)
      .HasMaxLength(Constants.MaxLengthStreamType);

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
