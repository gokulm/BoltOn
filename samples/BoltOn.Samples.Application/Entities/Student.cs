using System;
using System.Collections.Generic;
using BoltOn.Samples.Application.Handlers;
using System.Linq;
using BoltOn.Exceptions;
using BoltOn.Cqrs;

namespace BoltOn.Samples.Application.Entities
{
	public class Student : BaseDomainEntity
	{
		private List<StudentCourse> _courses = new List<StudentCourse>();

		public virtual Guid StudentId { get; private set; }
		public virtual string FirstName { get; private set; }
		public virtual string LastName { get; private set; }
		public virtual string Email { get; private set; }
		public virtual int StudentTypeId { get; private set; }
		public virtual IReadOnlyCollection<StudentCourse> Courses { get { return _courses.AsReadOnly(); } }

		public override bool PurgeEvents { get; set; } = false;

		public override string DomainEntityId => StudentId.ToString();

		private Student()
		{
		}

		internal Student(CreateStudentRequest request, string studentType)
		{
			StudentId = Guid.NewGuid();
			FirstName = request.FirstName;
			LastName = request.LastName;
			StudentTypeId = request.StudentTypeId;
			Email = request.Email;

			RaiseEvent(new StudentCreatedEvent(StudentId, FirstName, LastName,
				Email, StudentTypeId, studentType));
		}

		internal void Update(UpdateStudentRequest request, string studentType)
		{
			FirstName = request.FirstName;
			LastName = request.LastName;
			StudentTypeId = request.StudentTypeId;
			Email = request.Email;

			RaiseEvent(new StudentUpdatedEvent
			{
				StudentId = StudentId,
				FirstName = FirstName,
				LastName = LastName,
				StudentType = studentType,
				StudentTypeId = StudentTypeId
			});
		}

		internal void EnrollCourse(Guid courseId)
		{
			if (_courses.Any(a => a.CourseId == courseId))
				throw new BusinessValidationException("Course already enrolled");

			_courses.Add(new StudentCourse(StudentId, courseId));
		}
	}
}
