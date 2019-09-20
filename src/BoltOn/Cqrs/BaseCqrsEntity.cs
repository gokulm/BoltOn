using System;
using System.Collections.Generic;
using BoltOn.Data;

namespace BoltOn.Cqrs
{
	public interface ICqrsEntity
	{
		List<CqrsEvent> Events { get; set; }
		bool IsDisbursed { get; set; }
	}

	public abstract class BaseCqrsEntity : BaseEntity<string>, ICqrsEntity
	{
		public List<CqrsEvent> Events { get; set; } = new List<CqrsEvent>();

		public bool IsDisbursed { get; set; }

		protected void RaiseEvent(CqrsEvent @event)
		{
			IsDisbursed = true;

			if (@event.Id == Guid.Empty)
				@event.Id = Guid.NewGuid();

			@event.SourceTypeName = GetType().AssemblyQualifiedName;
			Events.Add(@event);
		}
	}
}
