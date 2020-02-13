using System;
using BoltOn.Bootstrapping;
using BoltOn.Data;
using BoltOn.Tests.Other;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Tests.Data.CosmosDb
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
			if (IntegrationTestHelper.IsCosmosDbServer && IntegrationTestHelper.IsSeedCosmosDbData)
			{
				using (var scope = _serviceProvider.CreateScope())
				{
					var guid = Guid.Parse("eda6ac19-0b7c-4698-a1f7-88279339d9ff");
					var studentFlattened = new StudentFlattened
					{
						Id = guid,
						StudentTypeId = 1,
						FirstName = "john",
						LastName = "smith",
						StudentType = "Grad"
					};

					var recordToBeDeleted = new StudentFlattened
					{
						Id = Guid.Parse("ff96d626-3911-4c78-b337-00d7ecd2eadd"),
						StudentTypeId = 1,
						FirstName = "record to be deleted",
						LastName = "smith",
						StudentType = "Grad"
					};

					var repository = scope.ServiceProvider.GetService<IRepository<StudentFlattened>>();
					repository.AddAsync(studentFlattened).GetAwaiter().GetResult();
					repository.AddAsync(recordToBeDeleted).GetAwaiter().GetResult();
				}
			}
		}
	}
}
