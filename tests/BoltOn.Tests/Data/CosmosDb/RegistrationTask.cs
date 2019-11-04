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
                context.Container.AddCosmosDb<SchoolCosmosDbOptions>(options =>
                {
                    options.Uri = "";
                    options.AuthorizationKey = "";
                    options.DatabaseName = "School";
                });

                context.Container.AddTransient<IRepository<StudentFlattened>, Repository<StudentFlattened, SchoolCosmosDbOptions>>();
            }
        }
    }
}
