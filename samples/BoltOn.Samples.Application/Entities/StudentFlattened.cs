using System;
using BoltOn.Samples.Application.Handlers;

namespace BoltOn.Samples.Application.Entities
{
	public class StudentFlattened 
	{
		public Guid StudentId { get; set; }
		public string FirstName { get; private set; }
		public string LastName { get; private set; }
		public string StudentType { get; private set; }
		public int StudentTypeId { get; private set; }

		private StudentFlattened()
		{
		}

		internal StudentFlattened(StudentCreatedEvent @event)
		{
			StudentId = @event.StudentId;
			FirstName = @event.FirstName;
			LastName = @event.LastName;
			StudentType = @event.StudentType;
			StudentTypeId = @event.StudentTypeId;
		}

		internal void Update(StudentUpdatedEvent @event)
		{
			FirstName = @event.FirstName;
			LastName = @event.LastName;
			StudentType = @event.StudentType;
			StudentTypeId = @event.StudentTypeId;
		}
	}
}
