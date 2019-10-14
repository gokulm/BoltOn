using System;
using BoltOn.Cqrs;
using BoltOn.Samples.Application.Handlers;

namespace BoltOn.Samples.Application.Entities
{
	public class Student : BaseCqrsEntity
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public int StudentTypeId { get; set; }

		private Student()
		{
		}

		internal Student(CreateStudentRequest request, string studentType)
		{
			Id = Guid.NewGuid();
			FirstName = request.FirstName;
			LastName = request.LastName;
			StudentTypeId = request.StudentTypeId;

			RaiseEvent(new StudentCreatedEvent
			{
				StudentId = Id,
				FirstName = FirstName,
				LastName = LastName,
				StudentType = studentType
			});
		}
	}
}
