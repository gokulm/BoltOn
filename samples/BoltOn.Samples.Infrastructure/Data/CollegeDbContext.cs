using BoltOn.Data.CosmosDb;
using Microsoft.Extensions.Options;

namespace BoltOn.Samples.Infrastructure.Data
{
    public class CollegeDbContext : BaseCosmosDbContext
    {
        private const string DATABASE_NAME = "College";

        public CollegeDbContext(IOptions<CosmosDbSettings> settings) : base(settings, DATABASE_NAME)
        {
        }
    }
}
