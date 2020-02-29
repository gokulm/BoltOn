using BoltOn.Bootstrapping;

namespace BoltOn.Tests.Bootstrapping.Fakes
{
    public class TestBootstrapperPostRegistrationTask : IPostRegistrationTask
    {
        public void Run()
        {
            BootstrapperRegistrationTasksHelper.Tasks.Add($"Executed {GetType().Name}");
        }
    }
}
