using System;
using BoltOn.Bootstrapping;
using BoltOn.Data;
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
			if (IntegrationTestHelper.IsCosmosDbServer)
			{
				var cosmosDbOptions = new TestSchoolCosmosDbOptions
				{
					Uri = "https://bolton-cosmosdb.documents.azure.com:443/",
					AuthorizationKey = "55Ta2AiEedmWAR69AwvVlwbnGbKJAj93AXXSNehOsi7pBGlFPjSp3xDf0d7GfO9mb47Xfj4fWhaDUks4xFvkuw==",
					DatabaseName = "studentsdb"
				};
				boltOnOptions.ServiceCollection.AddCosmosDb<TestSchoolCosmosDbOptions>(options =>
				{
					options.Uri = cosmosDbOptions.Uri;
					options.AuthorizationKey = cosmosDbOptions.AuthorizationKey;
					options.DatabaseName = cosmosDbOptions.DatabaseName;
				});
				boltOnOptions.ServiceCollection.AddSingleton(cosmosDbOptions);

                using var client = new DocumentClient(new Uri(cosmosDbOptions.Uri), cosmosDbOptions.AuthorizationKey);
                client.CreateDatabaseIfNotExistsAsync(new Database { Id = cosmosDbOptions.DatabaseName }).GetAwaiter().GetResult();

                var documentCollection = new DocumentCollection { Id = nameof(StudentFlattened).Pluralize() };
                documentCollection.PartitionKey.Paths.Add("/studentTypeId");
                client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(cosmosDbOptions.DatabaseName),
                    documentCollection).GetAwaiter().GetResult();
            }

			boltOnOptions.ServiceCollection.AddTransient<IRepository<StudentFlattened>, Repository<StudentFlattened, TestSchoolCosmosDbOptions>>();
		}
	}
}
