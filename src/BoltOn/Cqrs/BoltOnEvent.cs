using System;

namespace BoltOn.Cqrs
{
	public interface IEvent
	{
		Guid Id { get; set; }
		string SourceTypeName { get; set; }
	}

	public class BoltOnEvent : IEvent
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string SourceTypeName { get; set; }
    }
}
