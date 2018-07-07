using BoltOn.Bootstrapping;
using BoltOn.IoC;
using Microsoft.Extensions.Logging;

namespace BoltOn.Logging.NetStandard
{
    public class NetStandardLoggerRegistrationTask : IBootstrapperRegistrationTask
    {
        public void Run(IBoltOnContainer container)
        {
            container.RegisterTransient<ILoggerFactory, LoggerFactory>();
            container.RegisterTransient<IBoltOnLoggerFactory, NetStandardLoggerAdapterFactory>();
        }
    }
}
