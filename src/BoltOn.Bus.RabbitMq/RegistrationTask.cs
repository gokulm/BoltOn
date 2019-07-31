using System;
using BoltOn.Bootstrapping;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Bus.RabbitMq
{
    public class RegistrationTask 
    {
        public void Run(RegistrationTaskContext context)
        {
            var options = context.Get<RabbitMqBusOptions>();
            var busControl = MassTransit.Bus.Factory.CreateUsingRabbitMq(cfg =>
                       {
                           cfg.Host(new Uri(options.HostAddress), h =>
                           {
                               h.Username(options.Username);
                               h.Password(options.Password);
                           });
                       });
            busControl.Start();
            context.Container.AddSingleton(busControl);
        }
    }
}
