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
                    options.Uri = "https://bolton2.documents.azure.com:443/";
                    options.AuthorizationKey = "CJNc3RPjK3ACzRBtjOg56rJ774Y3ncyvJKCl5X2pfpMVe5wLPkr2v80pN5wWjhmZXYA0blOEsIDT4MmQifjtrg==";
                    options.DatabaseName = "School";
                });

                context.Container.AddTransient<IRepository<StudentFlattened>, Repository<StudentFlattened, TestSchoolCosmosDbOptions>>();
            }

        }
    }
}
