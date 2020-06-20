using System;
using BoltOn.Bootstrapping;
using BoltOn.Tests.Other;
using Microsoft.Azure.Documents.Client;
using BoltOn.Data.CosmosDb;

namespace BoltOn.Tests.Data.CosmosDb.Fakes
{
	public class CleanupTask : ICleanupTask
    {
		private readonly TestSchoolCosmosDbOptions _cosmosDbOptions;

		public CleanupTask(TestSchoolCosmosDbOptions cosmosDbOptions)
		{
			_cosmosDbOptions = cosmosDbOptions;
		}

        public void Run()
        {
            if (IntegrationTestHelper.IsCosmosDbServer && IntegrationTestHelper.IsSeedCosmosDbData)
            {
                using var client = new DocumentClient(new Uri(_cosmosDbOptions.Uri), _cosmosDbOptions.AuthorizationKey);
                client.DeleteDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(_cosmosDbOptions.DatabaseName, nameof(StudentFlattened).Pluralize())).GetAwaiter().GetResult();
            }
        }
    }
}
