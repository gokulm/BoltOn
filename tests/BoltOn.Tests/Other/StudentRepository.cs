using BoltOn.Bootstrapping;
using BoltOn.Cqrs;
using BoltOn.Data;
using BoltOn.Data.EF;

namespace BoltOn.Tests.Other
{
	public interface IStudentRepository : IRepository<Student>
	{
	}

	public class StudentRepository : CqrsRepository<Student, SchoolDbContext>, IStudentRepository
	{
		public StudentRepository(IDbContextFactory dbContextFactory, EventBag eventBag, CqrsOptions cqrsOptions)
			: base(dbContextFactory, eventBag, cqrsOptions)
		{
		}
	}
}
