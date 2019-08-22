using BoltOn.Bootstrapping;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BoltOn.Bus.RabbitMq
{
    public class PostRegistrationTask : IPostRegistrationTask
    {
        public void Run(PostRegistrationTaskContext context)
        {
            var busControl = context.ServiceProvider.GetService<IBusControl>();
            busControl.Start();
        }
    }
}
