using System;
using BoltOn.Bootstrapping;
using BoltOn.Data;
using BoltOn.Data.CosmosDb;
using BoltOn.Tests.Other;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Tests.Data.CosmosDb
{
    public class RegistrationTask : IRegistrationTask
    {
        public void Run(RegistrationTaskContext context)
        {
            if (IntegrationTestHelper.IsCosmosDbServer)
            {
                var cosmosDbOptions = new TestSchoolCosmosDbOptions
                {
                    Uri = "",
                    AuthorizationKey = "",
                    DatabaseName = ""
                };
                context.Container.AddCosmosDb<TestSchoolCosmosDbOptions>(options =>
                {
                    //fill the cosmosdb details and make sure you have collection and database exists as well inorder to run the cosmosdb integration tests .
                    options.Uri = cosmosDbOptions.Uri;
                    options.AuthorizationKey = cosmosDbOptions.AuthorizationKey;
                    options.DatabaseName = cosmosDbOptions.DatabaseName;
                });
                using(var client = new DocumentClient(new Uri(cosmosDbOptions.Uri), cosmosDbOptions.AuthorizationKey))
                { 
                    client.CreateDatabaseIfNotExistsAsync(new Database { Id = cosmosDbOptions.DatabaseName }).Wait();

                    DocumentCollection documentCollection = new DocumentCollection{ Id = nameof(StudentFlattened).Pluralize() };
                    documentCollection.PartitionKey.Paths.Add("/studentTypeId");
                    client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(cosmosDbOptions.DatabaseName),
                        documentCollection).Wait();    
                }
                context.Container.AddTransient<IRepository<StudentFlattened>, Repository<StudentFlattened, TestSchoolCosmosDbOptions>>();
            }

        }
    }
}
