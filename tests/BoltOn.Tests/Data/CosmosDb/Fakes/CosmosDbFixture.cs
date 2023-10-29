using System;
using BoltOn.Data.CosmosDb;
using BoltOn.Tests.Other;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BoltOn.Tests.Data.CosmosDb.Fakes
{
	public class CosmosDbFixture : IDisposable
    {
        public CosmosDbFixture()
        {
            IntegrationTestHelper.IsCosmosDbServer = true;
            var serviceCollection = new ServiceCollection();
            serviceCollection
                .BoltOn(options =>
                {
                    options.BoltOnCosmosDbModule();
                });

			serviceCollection.AddSingleton((provider) =>
			{
				var endpointUri = "https://bolton-cosmos.documents.azure.com:443/";
				var primaryKey = "ld5FYhdYKrfS6jujmMMpTC7j35tbYb2g68tJ9WJgFY2vn2zyQQLtKBJdd6dQqm5VGXT0tUYm4B0RACDbx6n4BA==";
				var databaseName = "bolton";

				var cosmosClientOptions = new CosmosClientOptions
				{
					ApplicationName = databaseName,
					ConnectionMode = ConnectionMode.Gateway,

					//ServerCertificateCustomValidationCallback = (request, certificate, chain) =>
					//{
					//    // Always return true to ignore certificate validation errors
					//    return true; //not for production
					//}
				};

				var loggerFactory = LoggerFactory.Create(builder =>
				{
					builder.AddConsole();
				});

				var cosmosClient = new CosmosClient(endpointUri, primaryKey, cosmosClientOptions);
				return cosmosClient;
			});
			serviceCollection.AddScoped<IRepository<Student>, Repository<Student>>();


			ServiceProvider = serviceCollection.BuildServiceProvider();
            ServiceProvider.TightenBolts();
            SubjectUnderTest = ServiceProvider.GetService<IRepository<Student>>();
        }

        public void Dispose()
        {
			ServiceProvider.LoosenBolts();
		}

        public IServiceProvider ServiceProvider { get; set; }

        public IRepository<Student> SubjectUnderTest { get; set; }
    }
}
