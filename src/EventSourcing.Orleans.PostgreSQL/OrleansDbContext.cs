using Microsoft.EntityFrameworkCore;

namespace Flensrocker.EventSourcing.Orleans.PostgreSQL;

// only used for migration management
internal class OrleansDbContext : DbContext
{
  public OrleansDbContext(DbContextOptions<OrleansDbContext> options)
    : base(options) { }
}
