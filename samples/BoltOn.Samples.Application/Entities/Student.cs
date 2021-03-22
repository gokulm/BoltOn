using System;
using System.Collections.Generic;
using BoltOn.Samples.Application.Handlers;

namespace BoltOn.Samples.Application.Entities
{
	public class Student
	{
		private List<StudentCourse> _courses = new List<StudentCourse>();

		public virtual Guid StudentId { get; private set; }
		public virtual string FirstName { get; private set; }
		public virtual string LastName { get; private set; }
		public virtual string Email { get; private set; }
		public virtual int StudentTypeId { get; private set; }
		public virtual IReadOnlyCollection<StudentCourse> Courses { get { return _courses.AsReadOnly(); } }

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
