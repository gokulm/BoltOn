using System.Threading;
using System.Threading.Tasks;
using BoltOn.Requestor.Pipeline;
using Hangfire;

namespace BoltOn.Hangfire
{
    public class BoltOnHangfireJobProcessor
    {
        private readonly IRequestor _requestor;

        public BoltOnHangfireJobProcessor(IRequestor requestor)
        {
            _requestor = requestor;
        }

        [JobDisplayName("{0}")]
        public async Task ProcessAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest
        {
            await _requestor.ProcessAsync(request, cancellationToken);
        }
    }
}
