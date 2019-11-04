using System;
using BoltOn.Cqrs;
using BoltOn.Data;
using BoltOn.Data.CosmosDb;
using BoltOn.Tests.Other;
using BoltOn.Utilities;

namespace BoltOn.Tests.Data.CosmosDb
{
	public interface IStudentRepository : IRepository<Student>
	{
	}

	public class StudentRepository : Repository<Student, SchoolCosmosDbOptions>, IStudentRepository
	{
		public StudentRepository(SchoolCosmosDbOptions options, EventBag eventBag, IBoltOnClock boltOnClock)
			: base(options, eventBag, boltOnClock)
		{
		}
	}
}
