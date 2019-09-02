using System;
using BoltOn.Bootstrapping;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BoltOn.Bus.RabbitMq
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
