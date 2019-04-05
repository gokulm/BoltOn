using BoltOn.Logging;

namespace BoltOn.Tests.Other
{
    public class Employee
    {
        private readonly IBoltOnLogger<Employee> _logger;

        public Employee(IBoltOnLogger<Employee> logger)
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
