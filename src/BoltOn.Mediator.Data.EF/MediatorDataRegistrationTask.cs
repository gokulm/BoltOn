using BoltOn.Bootstrapping;
using BoltOn.Data.EF;
using BoltOn.Mediator.Middlewares;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Mediator.Data.EF
{
	public class MediatorDataRegistrationTask : IBootstrapperRegistrationTask
	{
		public void Run(RegistrationTaskContext context)
		{
			var container = context.Container;
			container.AddTransient<IDbContextFactory, MediatorDbContextFactory>();
			container.AddScoped<IMediatorDataContext, MediatorDataContext>();
			container.AddTransient<IMediatorMiddleware, EFAutoDetectChangesDisablingMiddleware>();
		}
	}
}
