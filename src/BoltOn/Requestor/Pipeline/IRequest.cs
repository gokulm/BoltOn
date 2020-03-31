namespace BoltOn.Requestor.Pipeline
{
    // ReSharper disable once UnusedTypeParameter
    public interface IRequest<out TResponse>
	{
	}

	public interface IRequest : IRequest<DummyResponse>
	{
	}
}
