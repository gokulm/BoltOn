using System;
using BoltOn;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Handler
{
    public class QuerySourceNameRequest : IQuery<ResponseType>
	{
	}

	public class QuerySourceNameHandler : IRequestAsyncHandler<QuerySourceNameRequest, ResponseType>
	{
		public async Task<ResponseType> HandleAsync(QuerySourceNameRequest request, 
			CancellationToken cancellationToken = default(CancellationToken))
		{
			return default(ResponseType);
		}
	}
}
