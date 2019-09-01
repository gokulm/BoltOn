namespace BoltOn.Bootstrapping
{
    public class BoltOnPostRegistrationTask : IPostRegistrationTask
    {
        public void Run(PostRegistrationTaskContext context)
        {
            BoltOnServiceLocator.Current = context.ServiceProvider;
        }
    }
}
