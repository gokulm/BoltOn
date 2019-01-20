using System;
using Microsoft.Extensions.DependencyInjection;
using BoltOn.Bootstrapping;
using BoltOn.Tests.Mediator;

namespace BoltOn.Tests.Data.EF
{
    public class TestDataPostRegistrationTask : IBootstrapperPostRegistrationTask
    {
        public void Run(PostRegistrationTaskContext context)
        {
			if (MediatorTestHelper.IsSeedData)
			{
				var serviceProvider = context.ServiceProvider;
				var testDbContext = serviceProvider.GetService<SchoolDbContext>();
				testDbContext.Set<Student>().Add(new Student
				{
					Id = 1,
					FirstName = "a",
					LastName = "b"
				});
				var student = new Student
				{
					Id = 2,
					FirstName = "x",
					LastName = "y"
				};
				var address = new Address { Id = Guid.NewGuid(), Street = "Computer Science", Student = student };
				testDbContext.Set<Student>().Add(student);
				testDbContext.Set<Address>().Add(address);
				testDbContext.SaveChanges();
			}
        }
    }
}
