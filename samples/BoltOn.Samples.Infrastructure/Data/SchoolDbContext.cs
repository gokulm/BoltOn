using BoltOn.Data.EF;
using Microsoft.EntityFrameworkCore;

namespace BoltOn.Samples.Infrastructure.Data
{
	public class SchoolDbContext : BaseDbContext<SchoolDbContext>
	{
		public SchoolDbContext(DbContextOptions<SchoolDbContext> options) : base(options)
		{
		}
	}
}
