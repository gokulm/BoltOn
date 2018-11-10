using System.Collections.Generic;
using System.Reflection;
using BoltOn.Bootstrapping;
using BoltOn.IoC;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Logging.NetStandard
{
    public class NetStandardLoggerRegistrationTask : IBootstrapperRegistrationTask
    {
		public void Run(RegistrationTaskContext context)
        {
			var container = context.ServiceCollection;
			container.AddSingleton<ILoggerFactory, LoggerFactory>();
            //container.RegisterTransient<IBoltOnLoggerFactory, NetStandardLoggerAdapterFactory>();
			container.AddSingleton(typeof(IBoltOnLogger<>), typeof(NetStandardLoggerAdapter<>));
        }
	}
}
