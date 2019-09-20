using System;
using BoltOn.Mediator.Pipeline;

namespace BoltOn.Cqrs
{
	public interface ICqrsEvent : IRequest
	{
		Guid Id { get; set; }
		string SourceTypeName { get; set; }
        string SourceId { get; set; }
    }

	public class CqrsEvent : ICqrsEvent
    {
        public Guid Id { get; set; }
        public string SourceTypeName { get; set; }
		public string SourceId { get; set; }
    }
}
