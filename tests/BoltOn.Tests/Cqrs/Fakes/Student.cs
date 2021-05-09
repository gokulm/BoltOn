using System;
using BoltOn.Cqrs;

namespace BoltOn.Tests.Cqrs.Fakes
{
	public class Student : BaseDomainEntity
	{
		public Guid StudentId { get; set; }

		public string Name { get; internal set; }

		public override string DomainEntityId => StudentId.ToString();

		public Student()
		{
		}

		public Student(string name, Guid? studentId = null, Guid? eventId = null, bool purgeEvents = true)
		{
			Name = name;
			if (studentId.HasValue)
				StudentId = studentId.Value;

			PurgeEvents = purgeEvents;

			var @event = new StudentCreatedEvent
			{
				StudentId = StudentId,
				Name = name
			};

			if (eventId.HasValue)
				@event.EventId = eventId.Value;
			else
				@event.EventId = CqrsConstants.Event1Id;

			RaiseEvent(@event);
		}
	}
}
