using BoltOn.Bootstrapping;
using BoltOn.Data.EF;
using BoltOn.Mediator.Interceptors;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Mediator.Data.EF
{
	public class RegistrationTask : IRegistrationTask
	{
		public void Run(RegistrationTaskContext context)
		{
			var container = context.Container;
			container.AddScoped<MediatorDataContext>();
			container.AddTransient<IInterceptor, EFQueryTrackingBehaviorInterceptor>();
		}
	}
}
