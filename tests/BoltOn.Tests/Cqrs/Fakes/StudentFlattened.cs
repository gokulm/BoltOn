using System;

namespace BoltOn.Tests.Cqrs.Fakes
{
	public class StudentFlattened 
    {
        public Guid StudentId { get; set; }

        public virtual string Name { get; internal set; }

        public StudentFlattened()
        {
        }

        public StudentFlattened(StudentCreatedEvent @event)
        {
            StudentId = @event.StudentId;
            Name = @event.Name;
        }
    }
}
