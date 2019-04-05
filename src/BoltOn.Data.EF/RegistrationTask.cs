using BoltOn.Bootstrapping;
using BoltOn.Data.EF.Mediator;
using BoltOn.Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Data.EF
{
	public class RegistrationTask : IRegistrationTask
	{
		public void Run(RegistrationTaskContext context)
		{
			var container = context.Container;
			container.AddScoped<MediatorDataContext>();
			container.AddInterceptor<EFQueryTrackingBehaviorInterceptor>();
		}
	}
}
