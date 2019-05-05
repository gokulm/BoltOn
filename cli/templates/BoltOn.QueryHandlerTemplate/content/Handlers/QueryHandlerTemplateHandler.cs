using System;
using BoltOn;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Handler
{
    public class QueryHandlerTemplateRequest : IQuery<ResponseType>
	{
	}

	public class QueryHandlerTemplateHandler : IRequestAsyncHandler<QueryHandlerTemplateRequest, ResponseType>
	{
		public async Task<ResponseType> HandleAsync(QueryHandlerTemplateRequest request, 
			CancellationToken cancellationToken = default(CancellationToken))
		{
			return default(ResponseType);
		}
	}
}
