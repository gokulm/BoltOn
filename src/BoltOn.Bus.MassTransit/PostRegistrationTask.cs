using System;
using BoltOn.Bootstrapping;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Bus.MassTransit
{
	public class PostRegistrationTask : IPostRegistrationTask
    {
		private readonly IServiceProvider _serviceProvider;

		public PostRegistrationTask(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public void Run(PostRegistrationTaskContext context)
        {
            var busControl = _serviceProvider.GetService<IBusControl>();
            busControl?.Start();
        }
    }
}
