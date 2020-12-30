using System;
using Microsoft.Extensions.DependencyInjection;
using BoltOn.Bootstrapping;
using BoltOn.Tests.Other;

namespace BoltOn.Tests.Data.EF.Fakes
{
	public class PostRegistrationTask : IPostRegistrationTask
	{
		private readonly IServiceProvider _serviceProvider;

		public PostRegistrationTask(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public void Run()
		{
			if (IntegrationTestHelper.IsSeedData)
			{
				using var scope = _serviceProvider.CreateScope();
				var schoolDbContext = scope.ServiceProvider.GetService<SchoolDbContext>();
				if (schoolDbContext != null)
				{
					schoolDbContext.Database.EnsureDeleted();
					schoolDbContext.Database.EnsureCreated();

					schoolDbContext.Set<Student>().Add(new Student
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
					schoolDbContext.Set<Student>().Add(new Student
					{
						Id = 10,
						FirstName = "record to be deleted",
						LastName = "b"
					});
					schoolDbContext.Set<Student>().Add(new Student
					{
						Id = 11,
						FirstName = "record to be deleted",
						LastName = "b"
					});
					var address = new Address { Id = Guid.NewGuid(), Street = "Computer Science", Student = student };
					schoolDbContext.Set<Student>().Add(student);
					schoolDbContext.Set<Address>().Add(address);
					schoolDbContext.SaveChanges();
					schoolDbContext.Dispose();
				}
			}
		}
	}
}
