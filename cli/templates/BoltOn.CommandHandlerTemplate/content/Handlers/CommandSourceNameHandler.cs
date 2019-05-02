using System;
using BoltOn;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Handler
{
    public class CommandSourceNameRequest : ICommand<ResponseType>
	{
	}

	public class CommandSourceNameHandler : IRequestAsyncHandler<CommandSourceNameRequest, ResponseType>
	{
		public async Task<ResponseType> HandleAsync(CommandSourceNameRequest request, 
			CancellationToken cancellationToken = default(CancellationToken))
		{
			return default(ResponseType);
		}
	}
}
