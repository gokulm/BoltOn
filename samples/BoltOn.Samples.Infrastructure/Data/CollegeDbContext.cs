using BoltOn.Data.CosmosDb;

namespace BoltOn.Samples.Infrastructure.Data
{
    public class CollegeDbContext : BaseCosmosDbContext<CollegeDbContext>
    {
        public CollegeDbContext(CosmosDbContextOptions<CollegeDbContext> options) : base(options)
        {
        }
    }
}
