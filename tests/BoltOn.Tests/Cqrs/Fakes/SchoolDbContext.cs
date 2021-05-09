using BoltOn.Data.EF;
using Microsoft.EntityFrameworkCore;

namespace BoltOn.Tests.Cqrs.Fakes
{
    public class SchoolDbContext : DbContext
    {
        public SchoolDbContext(DbContextOptions<SchoolDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new StudentMapping());
            modelBuilder.ApplyConfiguration(new StudentFlattenedMapping());
            modelBuilder.ApplyConfiguration(new EventStoreMapping());
        }
    }
}
