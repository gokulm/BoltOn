using BoltOn.Samples.Infrastructure.Data.Mappings;
using Microsoft.EntityFrameworkCore;
using BoltOn.Data.EF;

namespace BoltOn.Samples.Infrastructure.Data
{
	public class SchoolDbContext : DbContext
	{
		public SchoolDbContext(DbContextOptions<SchoolDbContext> options) : base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
			modelBuilder.ApplyConfigurationsFromNamespaceOfType<StudentMapping>();
			modelBuilder.ApplyConfiguration(new EventStore2Mapping());
		}
    }
}
