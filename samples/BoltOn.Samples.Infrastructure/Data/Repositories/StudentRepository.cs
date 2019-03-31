using BoltOn.Data.EF;
using BoltOn.Samples.Application.Abstractions.Data;
using BoltOn.Samples.Domain.Entities;

namespace BoltOn.Samples.Infrastructure.Data.Repositories
{
	public class StudentRepository : BaseEFRepository<Student, SchoolDbContext>, IStudentRepository
	{
		public StudentRepository(IDbContextFactory dbContextFactory) : base(dbContextFactory)
		{
		}
	}
}
