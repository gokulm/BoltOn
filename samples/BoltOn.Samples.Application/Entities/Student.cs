using System;
using BoltOn.Cqrs;
using BoltOn.Samples.Application.Handlers;

namespace BoltOn.Samples.Application.Entities
{
	public class Student : BaseCqrsEntity
	{
		public string FirstName { get; private set; }
		public string LastName { get; private set; }
		public int StudentTypeId { get; private set; }

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
				StudentType = studentType,
				StudentTypeId = StudentTypeId
			});
		}

		public void Update(UpdateStudentRequest request, string studentType)
		{
			FirstName = request.FirstName;
			LastName = request.LastName;
			StudentTypeId = request.StudentTypeId;

			RaiseEvent(new StudentUpdatedEvent
			{
				StudentId = Id,
				FirstName = FirstName,
				LastName = LastName,
				StudentType = studentType,
				StudentTypeId = StudentTypeId
			});
		}
	}
}
