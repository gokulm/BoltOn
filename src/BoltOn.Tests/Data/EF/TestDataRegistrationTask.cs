using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using BoltOn.Bootstrapping;

namespace BoltOn.Tests.Data.EF
{
    public class TestDataRegistrationTask : IBootstrapperRegistrationTask
    {
        public void Run(RegistrationTaskContext context)
        {
            context.Container.AddDbContext<SchoolDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbForTesting");
            });
        }
    }
}
