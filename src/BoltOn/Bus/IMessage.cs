using System;
using BoltOn.Mediator.Pipeline;

namespace BoltOn.Bus
{
    public interface IMessage : IRequest
    {
        Guid CorrelationId { get; set; }
    }
}
