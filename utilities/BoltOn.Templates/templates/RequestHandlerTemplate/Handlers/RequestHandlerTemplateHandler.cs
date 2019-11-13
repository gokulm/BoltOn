using System;
using System.Threading.Tasks;

namespace RequestHandlerTemplate
{
	#if(ResponseType == "")
    public class RequestHandlerTemplateRequest : IRequest
	#else
    public class RequestHandlerTemplateRequest : IRequest<ResponseType>
	#endif
	{
	}

	#if(ResponseType == "")
    public class RequestHandlerTemplateHandler : IRequestAsyncHandler<RequestHandlerTemplateRequest>
	#else
    public class RequestHandlerTemplateHandler : IRequestAsyncHandler<RequestHandlerTemplateRequest, ResponseType>
	#endif
	{
		#if(ResponseType == "")
		public async Task HandleAsync(RequestHandlerTemplateRequest request, CancellationToken cancellationToken)
		#else
		public async Task<ResponseType> HandleAsync(RequestHandlerTemplateRequest request, CancellationToken cancellationToken)
		#endif		
		{
			throw new NotImplementedException();
		}
	}
}
