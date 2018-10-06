using System;
using BoltOn.Bootstrapping;
using Microsoft.Extensions.Logging;

namespace BoltOn.Logging.NetStandard
{
    public static class NetStandardLoggerExtensions
    {
        public static Bootstrapper BoltOnNetStandardLogger(this Bootstrapper bootstrapper,
                                                           Action<BoltOnLoggerOptions> action = null)
        {
            var options = new BoltOnLoggerOptions(bootstrapper);
            action?.Invoke(options);
            bootstrapper.Container.RegisterTransient<ILoggerFactory, LoggerFactory>();
            bootstrapper.Container.RegisterTransient(typeof(IBoltOnLogger<>), typeof(NetStandardLoggerAdapter<>));
            return bootstrapper;
        }
    }
}
