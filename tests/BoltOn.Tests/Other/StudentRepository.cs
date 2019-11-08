using BoltOn.Cqrs;
using BoltOn.Data;
using BoltOn.Data.EF;
using BoltOn.Tests.Other;
using BoltOn.Utilities;

namespace BoltOn.Tests.Other
{
	public interface IStudentRepository : IRepository<Student>
	{
	}

	public class StudentRepository : Repository<Student, SchoolDbContext>, IStudentRepository
	{
		public StudentRepository(IDbContextFactory dbContextFactory, EventBag eventBag, IBoltOnClock boltOnClock)
			: base(dbContextFactory, eventBag, boltOnClock)
		{
		}
	}
}
