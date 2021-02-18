using System;
using BoltOn.Samples.Application.Handlers;

namespace BoltOn.Samples.Application.Entities
{
	public class Student 
	{
		public Guid StudentId { get; set; }
		public string FirstName { get; private set; }
		public string LastName { get; private set; }
		public string Email { get; private set; }
		public int StudentTypeId { get; private set; }

        private Student()
		{
		}

		internal Student(CreateStudentRequest request)
		{
			StudentId = Guid.NewGuid();
			FirstName = request.FirstName;
			LastName = request.LastName;
			StudentTypeId = request.StudentTypeId;
			Email = request.Email;
		}

		public void Update(UpdateStudentRequest request)
		{
			FirstName = request.FirstName;
			LastName = request.LastName;
			StudentTypeId = request.StudentTypeId;
			Email = request.Email;
		}
	}
}
