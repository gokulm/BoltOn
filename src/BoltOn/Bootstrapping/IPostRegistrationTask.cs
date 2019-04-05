namespace BoltOn.Bootstrapping
{
    public interface IPostRegistrationTask
    {
        void Run(PostRegistrationTaskContext context);
    }
}