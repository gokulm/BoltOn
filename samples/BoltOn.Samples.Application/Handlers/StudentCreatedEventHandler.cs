using System;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Cqrs;
using BoltOn.Requestor.Pipeline;
using BoltOn.Samples.Application.Entities;

namespace BoltOn.Samples.Application.Handlers
{
	public class StudentCreatedEvent : BaseDomainEvent<Student>, IDomainEvent
	{
		public string FirstName { get; private set; }
		public string LastName { get; private set; }

		public StudentCreatedEvent(string entityId, string firstName,
			string lastName) 
		{
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
