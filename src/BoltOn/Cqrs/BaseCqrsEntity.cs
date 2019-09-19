using System;
using System.Collections.Generic;
using BoltOn.Data;

namespace BoltOn.Cqrs
{
	public interface ICqrsEntity
	{
		List<BoltOnEvent> Events { get; set; }
		bool IsDisbursed { get; set; }
	}

	public abstract class BaseCqrsEntity : BaseEntity<string>, ICqrsEntity
	{
		public List<BoltOnEvent> Events { get; set; } = new List<BoltOnEvent>();

		public bool IsDisbursed { get; set; }

		protected void RaiseEvent(BoltOnEvent @event)
		{
			IsDisbursed = true;

			if (@event.Id == Guid.Empty)
				@event.Id = Guid.NewGuid();

			@event.SourceTypeName = GetType().AssemblyQualifiedName;

			Events.Add(@event);
		}
	}

	//public abstract class BaseCqrsEntity<TIdType> : BaseEntity<TIdType>, ICqrsEntity
	//  {
	//public List<BoltOnEvent> Events { get; set; } = new List<BoltOnEvent>();

	//     public bool IsDisbursed { get; set; }

	//     protected void RaiseEvent(BoltOnEvent @event)
	//     {
	//IsDisbursed = true;

	//if (@event.Id != Guid.Empty)
	//@event.Id = Guid.NewGuid();

	//        Events.Add(@event);
	//    }
	//}
}
