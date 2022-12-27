namespace BoltOn.Requestor
{
	public interface IRequest<out TResponse> 
	{
	}

	public interface IRequest : IRequest<DummyResponse>
	{
	}
}
