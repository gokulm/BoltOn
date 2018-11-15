using BoltOn.Bootstrapping;

namespace BoltOn.Mediator
{
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
