using BoltOn.Bootstrapping;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Data.EF
{
	public class RegistrationTask : IRegistrationTask
	{
		public void Run(RegistrationTaskContext context)
		{
			context.ServiceCollection.AddScoped<ChangeTrackerContext>();
			context.AddInterceptor<ChangeTrackerInterceptor>();
		}
	}
}
