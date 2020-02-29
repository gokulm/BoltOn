namespace BoltOn.Tests.Bootstrapping.Fakes
{
    public class ClassWithInjectedDependency
    {
        public ClassWithInjectedDependency(ITestService testService)
        {
            Name = testService.GetName();
        }

        public string Name
        {
            get;
            set;
        }
    }
}
