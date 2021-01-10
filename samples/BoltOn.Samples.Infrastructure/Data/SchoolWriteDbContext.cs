using BoltOn.Samples.Infrastructure.Data.Mappings;
using Microsoft.EntityFrameworkCore;

namespace BoltOn.Samples.Infrastructure.Data
{
	public class SchoolWriteDbContext : DbContext
	{
		public SchoolWriteDbContext(DbContextOptions<SchoolWriteDbContext> options) : base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
			modelBuilder.ApplyConfiguration(new StudentMapping());
			modelBuilder.ApplyConfiguration(new StudentTypeMapping());
		}
    }
}
