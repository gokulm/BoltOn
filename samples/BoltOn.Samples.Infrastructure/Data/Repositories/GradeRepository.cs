using BoltOn.Data.Cosmos;
using BoltOn.Samples.Application.Abstractions.Data;
using BoltOn.Samples.Domain.Entities;

namespace BoltOn.Samples.Infrastructure.Data.Repositories
{
    public class GradeRepository : BaseCosmosRepository<Grade, CollegeDbContext>, IGradeRepository
    {
        public GradeRepository(ICosmosContextFactory cosmosContextFactory) : base(cosmosContextFactory)
        {
        }
    }
}
