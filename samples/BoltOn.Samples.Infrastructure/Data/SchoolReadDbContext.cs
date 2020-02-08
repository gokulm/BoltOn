using BoltOn.Data.EF;
using BoltOn.Samples.Infrastructure.Data.Mappings;
using Microsoft.EntityFrameworkCore;

namespace BoltOn.Samples.Infrastructure.Data
{
    public class SchoolReadDbContext : BaseDbContext<SchoolReadDbContext>
    {
        public SchoolReadDbContext(DbContextOptions<SchoolReadDbContext> options) : base(options)
        {
        }

        protected override void ApplyConfigurations(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new StudentFlattenedMapping());
        }
    }
}