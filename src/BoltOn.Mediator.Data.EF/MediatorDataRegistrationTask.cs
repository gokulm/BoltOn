using BoltOn.Bootstrapping;
using BoltOn.Data.EF;
using BoltOn.Mediator.Interceptors;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Mediator.Data.EF
{
	public class MediatorDataRegistrationTask : IBootstrapperRegistrationTask
	{
		public void Run(RegistrationTaskContext context)
		{
			var container = context.Container;
			container.AddTransient<IDbContextFactory, MediatorDbContextFactory>();
			container.AddScoped<MediatorDataContext>();
			container.AddTransient<IInterceptor, EFQueryTrackingBehaviorInterceptor>();
		}
	}
}
