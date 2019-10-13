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

		public Student(Guid id, string firstName, string lastName)
		{
			Id = id;
			FirstName = firstName;
			LastName = lastName;

			RaiseEvent(new StudentCreatedEvent { Body = JsonConvert.SerializeObject(this) });
		}
	}

	public class StudentFlattened : BaseCqrsEntity
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }

		private StudentFlattened()
		{
		}

		public StudentFlattened(StudentCreatedEvent @event)
		{
			ProcessEvent(@event, e =>
			{
				var student = JsonConvert.DeserializeObject<Student>(e.Body);
				Id = student.Id;
				FirstName = student.FirstName;
				LastName = student.LastName;
			});
		}
	}
}
