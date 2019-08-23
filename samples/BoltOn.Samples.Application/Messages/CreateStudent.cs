using System;
using BoltOn.Mediator.Pipeline;

namespace BoltOn.Samples.Application.Messages
{
	public class CreateStudent : IRequest
	{
		public string FirstName { get; set; }
		public Guid CorrelationId { get; set; } = Guid.NewGuid();
	}
}
