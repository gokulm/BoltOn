using System;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Mediator.Pipeline;

namespace Handlers
{
	#if(ResponseType == "")
    public class HandlerTemplateRequest : IRequest
	#else
    public class HandlerTemplateRequest : IRequest<ResponseType>
	#endif
	{
	}

	#if(ResponseType == "")
    public class HandlerTemplateHandler : IHandler<HandlerTemplateRequest>
	#else
    public class HandlerTemplateHandler : IHandler<HandlerTemplateRequest, ResponseType>
	#endif
	{
		#if(ResponseType == "")
		public async Task HandleAsync(HandlerTemplateRequest request, CancellationToken cancellationToken)
		#else
		public async Task<ResponseType> HandleAsync(HandlerTemplateRequest request, CancellationToken cancellationToken)
		#endif		
		{
			throw new NotImplementedException();
		}
	}
}
