using System;
using BoltOn.Cqrs;
using BoltOn.Samples.Application.Handlers;
using Newtonsoft.Json;

namespace BoltOn.Samples.Application.Entities
{
	public class Student : BaseCqrsEntity
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }

		private Student()
		{
		}

		internal Student(CreateStudentRequest request)
		{
			Id = Guid.NewGuid();
			FirstName = request.FirstName;
			LastName = request.LastName;

			RaiseEvent(new StudentCreatedEvent { StudentId = Id, FirstName = FirstName, LastName = LastName });
		}
	}
}
