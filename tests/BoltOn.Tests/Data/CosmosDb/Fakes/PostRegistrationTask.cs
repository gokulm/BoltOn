using System;
using BoltOn.Bootstrapping;
using BoltOn.Tests.Other;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Tests.Data.CosmosDb.Fakes
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
			if (IntegrationTestHelper.IsCosmosDbServer)
            {
                using var scope = _serviceProvider.CreateScope();
                var guid = Guid.Parse("eda6ac19-0b7c-4698-a1f7-88279339d9ff");
                var studentFlattened = new StudentFlattened
                {
                    StudentTypeId = 1,
                    FirstName = "john",
                    LastName = "smith",
                    StudentType = "Grad"
                };

                var recordToBeDeleted = new StudentFlattened
                {
                    StudentTypeId = 1,
                    FirstName = "record to be deleted",
                    LastName = "smith",
                    StudentType = "Grad"
                };

                var repository = scope.ServiceProvider.GetService<BoltOn.Data.CosmosDb.IRepository<StudentFlattened>>();
                repository?.AddAsync(studentFlattened).GetAwaiter().GetResult();
                repository?.AddAsync(recordToBeDeleted).GetAwaiter().GetResult();
            }
		}
	}
}
