using BoltOn.Cqrs;
using BoltOn.Samples.Application.Handlers;

namespace BoltOn.Samples.Application.Entities
{
	public class Student : BaseCqrsEntity
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }

		private Student()
		{
		}

		public Student(int id, string firstName, string lastName)
		{
			Id = id.ToString();
			FirstName = firstName;
			LastName = lastName;

			RaiseEvent(new StudentCreatedEvent { Student = this });
		}
	}

	public class StudentQueryEntity : BaseCqrsEntity
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }

		private StudentQueryEntity()
		{
		}

		public StudentQueryEntity(StudentCreatedEvent @event)
		{
			ProcessEvent(@event, e =>
			{
				Id = e.Student.Id;
				FirstName = e.Student.FirstName;
				LastName = e.Student.LastName;
			});
		}
	}
}
