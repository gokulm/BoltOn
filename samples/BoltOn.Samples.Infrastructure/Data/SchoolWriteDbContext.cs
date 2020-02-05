using BoltOn.Data.EF;
using BoltOn.Samples.Infrastructure.Data.Mappings;
using Microsoft.EntityFrameworkCore;

namespace BoltOn.Samples.Infrastructure.Data
{
	public class SchoolWriteDbContext : BaseDbContext<SchoolWriteDbContext>
	{
		public SchoolWriteDbContext(DbContextOptions<SchoolWriteDbContext> options) : base(options)
		{
		}

        protected override void ApplyConfigurations(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new StudentMapping());
            modelBuilder.ApplyConfiguration(new StudentTypeMapping());
        }
    }
}
