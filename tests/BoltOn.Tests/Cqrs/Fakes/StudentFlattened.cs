using System;
using BoltOn.Cqrs;

namespace BoltOn.Tests.Cqrs.Fakes
{
    public class StudentFlattened : BaseCqrsEntity
    {
		public virtual string FirstName { get; internal set; }

		public virtual string LastName { get; internal set; }

        public virtual string Input2Property1 { get; internal set; }

        public virtual int Input2Property2 { get; internal set; }

		public StudentFlattened()
		{
		}

		public StudentFlattened(StudentCreatedEvent @event)
		{
			@event.ProcessedDate = DateTime.UtcNow.AddSeconds(-3);
			ProcessEvent(@event, e =>
			{
				Id = @event.SourceId;
				FirstName = @event.Input;
				LastName = @event.Input;
			});
		}

		public bool UpdateInput(StudentUpdatedEvent @event)
        {
            return ProcessEvent(@event, e =>
			{
				FirstName = e.Name;
				LastName = e.Name;
                Input2Property1 = e.Input2.Property1;
                Input2Property2 = e.Input2.Property2;
            });
        }
    }
}
