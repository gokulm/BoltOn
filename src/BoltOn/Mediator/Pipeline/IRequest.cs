namespace BoltOn.Mediator.Pipeline
{
	public interface IRequest<out TResponse>
	{
	}

	public interface IRequest : IRequest<DummyResponse>
	{
	}
}
