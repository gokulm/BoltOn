using System;

namespace BoltOn.Mediator.Pipeline
{
	public class MediatorResponse<TResponse>
	{
		public TResponse Data { get; set; }
		public bool IsSuccessful { get; set; }
		public Exception Exception { get; set; }
	}
}
