using System;
using BoltOn.Bus;

namespace BoltOn.Samples.Application.Messages
{
	public class CreateStudent : IMessage
	{
		public string FirstName { get; set; }
		public Guid CorrelationId { get; set; } = Guid.NewGuid();
	}
}
