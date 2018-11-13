using BoltOn.Bootstrapping;
using BoltOn.Context;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Mediator
{
    public class MediatorPostRegistrationTask : IBootstrapperPostRegistrationTask
    {
        private readonly IContextRetriever _contextRetriever;

        public MediatorPostRegistrationTask(IContextRetriever contextRetriever)
        {
            _contextRetriever = contextRetriever;
        }

        public void Run(RegistrationTaskContext context)
        {
            var options = context.GetOptions<MediatorOptions>();
			var mediatorContext = new MediatorContext
			{
				DefaultCommandIsolationLevel = options.UnitOfWorkOptions.DefaultCommandIsolationLevel,
				DefaultQueryIsolationLevel = options.UnitOfWorkOptions.DefaultQueryIsolationLevel,
				DefaultIsolationLevel = options.UnitOfWorkOptions.DefaultIsolationLevel
            };
            _contextRetriever.Set(mediatorContext, ContextScope.App);
        }
    }

	public class MediatorPreRegistrationTask : IBootstrapperPreRegistrationTask
	{
		public void Run(PreRegistrationTaskContext context)
		{
			context.Configure<MediatorOptions>(m =>
			{
				m.ClearMiddlewares();
				m.RegisterMiddleware<StopwatchMiddleware>();
				m.RegisterMiddleware<UnitOfWorkMiddleware>();
			});


		}
	}
}
