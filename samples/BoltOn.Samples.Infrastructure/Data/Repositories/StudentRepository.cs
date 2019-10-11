using BoltOn.Data.EF;
using BoltOn.Samples.Application.Abstractions.Data;
using BoltOn.Samples.Application.Entities;

namespace BoltOn.Samples.Infrastructure.Data.Repositories
{
	public class StudentRepository : EFRepository<Student, SchoolDbContext>, IStudentRepository
	{
		public StudentRepository(IDbContextFactory dbContextFactory) : base(dbContextFactory)
		{
		}
	}
}
