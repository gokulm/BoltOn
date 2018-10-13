using System.Collections.Generic;
using System.Reflection;
using BoltOn.Bootstrapping;
using BoltOn.IoC;
using Microsoft.Extensions.Logging;

namespace BoltOn.Logging.NetStandard
{
    public class NetStandardLoggerRegistrationTask : IBootstrapperRegistrationTask
    {
		public void Run(RegistrationTaskContext context)
        {
			var container = context.Container;
            container.RegisterTransient<ILoggerFactory, LoggerFactory>();
            //container.RegisterTransient<IBoltOnLoggerFactory, NetStandardLoggerAdapterFactory>();
			container.RegisterTransient(typeof(IBoltOnLogger<>), typeof(NetStandardLoggerAdapter<>));
        }
	}
}
