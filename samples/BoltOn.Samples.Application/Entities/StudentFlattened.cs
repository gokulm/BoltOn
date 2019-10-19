using BoltOn.Cqrs;
using BoltOn.Samples.Application.Handlers;

namespace BoltOn.Samples.Application.Entities
{
	public class StudentFlattened : BaseCqrsEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
		public string StudentType { get; set; }

		private StudentFlattened()
        {
        }

        internal StudentFlattened(StudentCreatedEvent @event)
        {
            ProcessEvent(@event, e =>
            {
                Id = e.StudentId;
                FirstName = e.FirstName;
                LastName = e.LastName;
				StudentType = e.StudentType;
            });
        }

		public void Update(StudentUpdatedEvent @event)
		{
			ProcessEvent(@event, e =>
			{
				Id = e.StudentId;
				FirstName = e.FirstName;
				LastName = e.LastName;
				StudentType = e.StudentType;
			});
		}
	}
}
