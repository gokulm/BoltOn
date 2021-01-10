using BoltOn.Samples.Infrastructure.Data.Mappings;
using Microsoft.EntityFrameworkCore;

namespace BoltOn.Samples.Infrastructure.Data
{
	public class SchoolReadDbContext : DbContext
    {
        public SchoolReadDbContext(DbContextOptions<SchoolReadDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new StudentFlattenedMapping());
        }
    }
}