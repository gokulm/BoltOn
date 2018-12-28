using BoltOn.Bootstrapping;
using BoltOn.Data.EF;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Mediator.Data
{
    public class MediatorDataRegistrationTask : IBootstrapperRegistrationTask
    {
        public void Run(RegistrationTaskContext context)
        {
            context.Container.AddTransient<IDbContextFactory, MediatorDbContextFactory>();
        }
    }
}
