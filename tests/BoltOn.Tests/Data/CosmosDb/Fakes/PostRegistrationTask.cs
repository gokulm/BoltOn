using System;
using System.Collections.Generic;
using Azure.Core;
using BoltOn.Bootstrapping;
using BoltOn.Tests.Other;
using Microsoft.Azure.Cosmos;
using Nest;

namespace BoltOn.Tests.Data.CosmosDb.Fakes
{
	public class PostRegistrationTask : IPostRegistrationTask
	{
		private readonly CosmosClient _cosmosClient;

		public PostRegistrationTask(CosmosClient cosmosClient)
		{
			_cosmosClient = cosmosClient;
		}

		public void Run()
		{
			if (IntegrationTestHelper.IsCosmosDbServer)
			{
				var clientOptions = _cosmosClient.ClientOptions;
				var container = _cosmosClient.GetContainer(clientOptions.ApplicationName, "Students");
				container.DeleteContainerAsync().GetAwaiter().GetResult();
				var database = _cosmosClient.GetDatabase(clientOptions.ApplicationName);
				List<string> keyPaths = new()
				{
					"/CourseId"
				};
				ContainerProperties properties = new(id: "Students", partitionKeyPaths: keyPaths);
				database.CreateContainerIfNotExistsAsync(containerProperties: properties).GetAwaiter().GetResult();
			}
		}
	}
}
