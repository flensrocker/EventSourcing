using Flensrocker.EventSourcing.Abstractions;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// An instance of this interface lives in the DI scope of the <c>IDomainEventStorage</c>.
/// This can be the scope of an Orleans grain, which can last for several minutes.
/// Keep this in mind when implementing this.
/// </summary>
/// <typeparam name="TDbContext"></typeparam>
public interface IDbStrongConsistentProjection<TEventBase, TDbContext>
  where TEventBase : class
  where TDbContext : DbContext
{
  Task ApplyEvents(IDomainEventStorage<TEventBase> eventStore, TDbContext dbContext, IReadOnlyList<DomainEvent> domainEvents);
}
