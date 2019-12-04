using BoltOn.Cqrs;

namespace BoltOn.Tests.Cqrs
{
    public class StudentFlattened : BaseCqrsEntity
    {
		public string FirstName { get; set; }

		public virtual string LastName { get; internal set; }

        public virtual string Input2Property1 { get; internal set; }

        public virtual int Input2Property2 { get; internal set; }

		public StudentFlattened()
		{
		}

		public StudentFlattened(StudentCreatedEvent @event)
		{
			ProcessEvent(@event, e =>
			{
				e.Input = @event.Input;
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
