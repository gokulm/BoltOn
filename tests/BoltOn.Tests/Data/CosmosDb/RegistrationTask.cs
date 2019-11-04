using BoltOn.Bootstrapping;
using BoltOn.Data;
using BoltOn.Data.CosmosDb;
using BoltOn.Tests.Other;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Tests.Data.CosmosDb
{
    public class RegistrationTask : IRegistrationTask
    {
        public void Run(RegistrationTaskContext context)
        {
            if (IntegrationTestHelper.IsCosmosDbServer)
            {
                context.Container.AddCosmosDb<TestSchoolCosmosDbOptions>(options =>
                {
                    //fill the cosmosdb details and make sure you have collection and database exists as well inorder to run the cosmosdb integration tests .
                    options.Uri = "";
                    options.AuthorizationKey = "";
                    options.DatabaseName = "";
                });

                context.Container.AddTransient<IRepository<StudentFlattened>, Repository<StudentFlattened, TestSchoolCosmosDbOptions>>();
            }

        }
    }
}
