using System;
using BoltOn.Bootstrapping;
using BoltOn.Data;
using BoltOn.Tests.Other;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Tests.Data.CosmosDb
{
	public class PostRegistrationTask : IPostRegistrationTask
	{
		public void Run(PostRegistrationTaskContext context)
		{
			if (IntegrationTestHelper.IsCosmosDbServer && IntegrationTestHelper.IsSeedCosmosDbData)
			{
				var serviceProvider = context.ServiceProvider;
				using (var scope = serviceProvider.CreateScope())
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

					var repository = scope.ServiceProvider.GetService<IRepository<StudentFlattened>>();
					repository.AddAsync(studentFlattened).GetAwaiter().GetResult();
				}

			}
		}
	}
}
