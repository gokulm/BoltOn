using BoltOn.Logging;

namespace BoltOn.Tests.Bootstrapping.Fakes
{
    public class Employee
    {
        private readonly IAppLogger<Employee> _logger;

        public Employee(IAppLogger<Employee> logger)
        {
            _logger = logger;
            _logger.Info("Employee instantiated...");
        }

        public string GetName()
        {
            _logger.Debug("getting name...");
            return "John";
        }
    }
}
