using System;
using Microsoft.Extensions.DependencyInjection;
using BoltOn.Bootstrapping;

namespace BoltOn.Tests.Common
{
    public class IntegrationTestsStartup
    {
        private static Lazy<IntegrationTestsStartup> _instance = new Lazy<IntegrationTestsStartup>(() => new IntegrationTestsStartup());
        private IServiceProvider _serviceProvider;
		private bool _isInitialized;
		private object _lock = new object();

        private IntegrationTestsStartup()
        {
        }

        internal static IntegrationTestsStartup Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        public void Initialize()
        {
			lock (_lock)
			{
				if (_isInitialized)
					return;
				var serviceCollection = new ServiceCollection();
				serviceCollection.BoltOn();
				_serviceProvider = serviceCollection.BuildServiceProvider();
				_serviceProvider.UseBoltOn();
				_isInitialized = true;
			}
        }

        public TType GetService<TType>()
        {
            return _serviceProvider.GetService<TType>();
        }
    }
}
