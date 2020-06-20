using System;
using BoltOn.Bootstrapping;
using BoltOn.Tests.Other;
using Microsoft.Azure.Documents.Client;
using BoltOn.Data.CosmosDb;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Tests.Data.CosmosDb.Fakes
{
	public class CleanupTask : ICleanupTask
    {
		private readonly IServiceProvider _serviceProvider;

		public CleanupTask(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

        public void Run()
        {
            if (IntegrationTestHelper.IsCosmosDbServer && IntegrationTestHelper.IsSeedCosmosDbData)
            {
                var cosmosDbOptions = _serviceProvider.GetService<TestSchoolCosmosDbOptions>();
                using var client = new DocumentClient(new Uri(cosmosDbOptions.Uri), cosmosDbOptions.AuthorizationKey);
                client.DeleteDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(cosmosDbOptions.DatabaseName, nameof(StudentFlattened).Pluralize())).GetAwaiter().GetResult();
            }
        }
    }
}
