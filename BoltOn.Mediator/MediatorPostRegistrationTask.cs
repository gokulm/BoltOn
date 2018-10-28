using BoltOn.Bootstrapping;
using BoltOn.Context;

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
}
