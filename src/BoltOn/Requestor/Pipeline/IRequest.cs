using BoltOn.Requestor.Interceptors;

namespace BoltOn.Requestor.Pipeline
{
	public interface IRequest<out TResponse> : IEnableInterceptor<StopwatchInterceptor>
	{
	}

	public interface IRequest : IRequest<DummyResponse>
	{
	}
}
