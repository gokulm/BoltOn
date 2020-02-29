using BoltOn.Bootstrapping;

namespace BoltOn.Tests.Bootstrapping.Fakes
{
    public class TestBootstrapperPostRegistrationTaskWithDependency : IPostRegistrationTask
    {
        private readonly ITestService _testService;

        public TestBootstrapperPostRegistrationTaskWithDependency(ITestService testService)
        {
            _testService = testService;
        }

        public void Run()
        {
            BootstrapperRegistrationTasksHelper.Tasks.Add($"Executed {GetType().Name}");
            BootstrapperRegistrationTasksHelper.Tasks.Add($"Executed {_testService.GetName()} service");
        }
    }
}
