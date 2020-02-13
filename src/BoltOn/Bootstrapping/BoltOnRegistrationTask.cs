using System.Linq;
using BoltOn.Cqrs;
using BoltOn.Logging;
using BoltOn.Mediator.Interceptors;
using BoltOn.Mediator.Pipeline;
using BoltOn.Other;
using BoltOn.UoW;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Bootstrapping
{
	public class BoltOnRegistrationTask : IRegistrationTask
	{
        public void Run(BoltOnOptions boltOnOptions)
        {
			//boltOnOptions.BoltOnAssemblies(GetType().Assembly);
            RegisterOtherTypes(boltOnOptions.ServiceCollection);
            RegisterMediator(boltOnOptions);
        }

		public void Run(RegistrationTaskContext context)
		{
			//RegisterByConvention(context);
			//RegisterOtherTypes(context);
			//RegisterMediator(context);
		}

		private void RegisterOtherTypes(IServiceCollection serviceCollection)
		{
			serviceCollection.AddScoped<IUnitOfWorkManager>(s =>
			{
				return new UnitOfWorkManager(s.GetRequiredService<IBoltOnLogger<UnitOfWorkManager>>(), 
					s.GetRequiredService<IUnitOfWorkFactory>());
			});
			serviceCollection.AddSingleton(typeof(IBoltOnLogger<>), typeof(BoltOnLogger<>));
			serviceCollection.AddSingleton<IBoltOnLoggerFactory, BoltOnLoggerFactory>();
			serviceCollection.AddScoped<EventBag>();

            //foreach (var option in context.Bootstrapper.Options.OtherOptions)
            //{
            //    serviceCollection.AddSingleton(option.GetType(), option);
            //}
		}

		public void RegisterMediator(BoltOnOptions boltOnOptions)
		{
			boltOnOptions.ServiceCollection.AddTransient<IMediator, Mediator.Pipeline.Mediator>();
			boltOnOptions.ServiceCollection.AddSingleton<IUnitOfWorkOptionsBuilder, UnitOfWorkOptionsBuilder>();
		}
	}
}
