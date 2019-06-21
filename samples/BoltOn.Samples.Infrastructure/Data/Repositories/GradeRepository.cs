using BoltOn.Data.CosmosDb;
using BoltOn.Samples.Application.Abstractions.Data;
using BoltOn.Samples.Domain.Entities;

namespace BoltOn.Samples.Infrastructure.Data.Repositories
{
    public class GradeRepository : BaseCosmosDbRepository<Grade, CollegeDbContext>, IGradeRepository
    {
        public GradeRepository(CollegeDbContext collegeDbContext) : base(collegeDbContext)
        {
        }
    }
}
