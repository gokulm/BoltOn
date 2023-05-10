using BoltOn.Data;
using BoltOn.Data.EF;

namespace BoltOn.Tests.Other
{
	public interface IStudentRepository : IRepository<Student>
	{
	}

	public class StudentRepository : Repository<Student, SchoolDbContext>, IStudentRepository
	{
		public StudentRepository(SchoolDbContext schoolDbContext)
			: base(schoolDbContext)
		{
		}
	}
}
