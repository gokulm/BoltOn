using System.Threading.Tasks;
using BoltOn.Bootstrapping;
using BoltOn.Mediator.Pipeline;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Bus.RabbitMq
{
    public static partial class Extensions
    {
        public class MassTransitRequestConsumer<TRequest> : IConsumer<TRequest> where TRequest : class, IMessage
        {
            public async Task Consume(ConsumeContext<TRequest> context)
            {
                var request = context.Message;
                using (var scope = BoltOnServiceProvider.Current.CreateScope())
                {
                    var mediator = scope.ServiceProvider.GetService<IMediator>();
                    await mediator.ProcessAsync(request);
                }
            }
        }
    }
}
