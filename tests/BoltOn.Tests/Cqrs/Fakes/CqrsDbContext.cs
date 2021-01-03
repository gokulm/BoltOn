using Microsoft.EntityFrameworkCore;

namespace BoltOn.Tests.Cqrs.Fakes
{
	public class CqrsDbContext : DbContext
    {
        public CqrsDbContext(DbContextOptions<CqrsDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new StudentMapping());
            modelBuilder.ApplyConfiguration(new StudentFlattenedMapping());
        }
    }
}
