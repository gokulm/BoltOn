using System;
using BoltOn.Bootstrapping;
using BoltOn.Data.CosmosDb;
using BoltOn.Tests.Other;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Tests.Data.CosmosDb.Fakes
{
	public static class Extensions 
	{
		public static void RegisterCosmosdbFakes(this BoltOnOptions boltOnOptions)
		{
			var cosmosDbOptions = new TestSchoolCosmosDbOptions
			{
				Uri = "",
				AuthorizationKey = "==",
				DatabaseName = "studentsdb"
			};
			boltOnOptions.ServiceCollection.AddSingleton(cosmosDbOptions);

			if (IntegrationTestHelper.IsCosmosDbServer)
			{
				boltOnOptions.ServiceCollection.AddCosmosDb<TestSchoolCosmosDbOptions>(options =>
				{
					options.Uri = cosmosDbOptions.Uri;
					options.AuthorizationKey = cosmosDbOptions.AuthorizationKey;
					options.DatabaseName = cosmosDbOptions.DatabaseName;
				});

                using var client = new DocumentClient(new Uri(cosmosDbOptions.Uri), cosmosDbOptions.AuthorizationKey);
                client.CreateDatabaseIfNotExistsAsync(new Database { Id = cosmosDbOptions.DatabaseName }).GetAwaiter().GetResult();

                var documentCollection = new DocumentCollection { Id = nameof(StudentFlattened).Pluralize() };
                documentCollection.PartitionKey.Paths.Add("/studentTypeId");
                client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(cosmosDbOptions.DatabaseName),
                    documentCollection).GetAwaiter().GetResult();
			}
		}
	}
}
