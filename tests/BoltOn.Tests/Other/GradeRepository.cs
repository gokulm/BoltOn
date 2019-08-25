using System;
using BoltOn.Data;
using BoltOn.Data.CosmosDb;

namespace BoltOn.Tests.Other
{
    public interface IGradeRepository : IRepository<Grade>
    {
    }

    public class GradeRepository : BaseCosmosDbRepository<Grade, CollegeDbContext>, IGradeRepository
    {
        public GradeRepository(CollegeDbContext collegeDbContext) : base(collegeDbContext)
        {
        }
    }
}
