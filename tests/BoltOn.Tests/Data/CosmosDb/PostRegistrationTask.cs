using System;
using BoltOn.Bootstrapping;
using BoltOn.Cqrs;
using BoltOn.Data;
using BoltOn.Data.CosmosDb;
using BoltOn.Tests.Other;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace BoltOn.Tests.Data.CosmosDb
{
    public class PostRegistrationTask : IPostRegistrationTask
    {
        public void Run(PostRegistrationTaskContext context)
        {
            if (IntegrationTestHelper.IsCosmosDbServer && IntegrationTestHelper.IsSeedCosmosDbData)
            {
                var serviceProvider = context.ServiceProvider;
                using (var scope = serviceProvider.CreateScope())
                {
                    var guid = Guid.Parse("eda6ac19-0b7c-4698-a1f7-88279339d9ff");
                    var studentFlattened = new StudentFlattened { Id = guid, StudentTypeId = 1, FirstName = "john", LastName = "smith", StudentType = "Grad" };

                    var repository = scope.ServiceProvider.GetService<IRepository<StudentFlattened>>();

                    repository.Add(studentFlattened);
                }

            }
        }
    }

    public class StudentFlattened : BaseCqrsEntity
    {
        [JsonProperty("id")]
        public override Guid Id { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("studentType")]
        public string StudentType { get; set; }

        [JsonProperty("studentTypeId")]
        public int StudentTypeId { get; set; }
    }

    public class TestSchoolCosmosDbOptions : BaseCosmosDbOptions
    {
    }
}
