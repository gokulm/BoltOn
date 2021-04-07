using System;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Cqrs;
using BoltOn.Requestor.Pipeline;
using BoltOn.Samples.Application.Entities;

namespace BoltOn.Samples.Application.Handlers
{
	public class StudentCreatedEvent : ICqrsEvent
	{
		public Guid Id { get; set; }
		public string EntityType { get; set; }
		public string EntityId { get; set; }
		public DateTimeOffset? CreatedDate { get; set; }
		public DateTimeOffset? ProcessedDate { get; set; }
		public string FirstName { get; private set; }
		public string LastName { get; private set; }

		public StudentCreatedEvent(string entityId, string firstName,
			string lastName)
		{
			Id = Guid.NewGuid();
			EntityType = typeof(Student).Name;
			EntityId = entityId;
			CreatedDate = DateTimeOffset.Now;
			FirstName = firstName;
			LastName = lastName;
		}
	}

	public class StudentCreatedEventHandler : IHandler<StudentCreatedEvent>
    {
        public Task HandleAsync(StudentCreatedEvent request, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
