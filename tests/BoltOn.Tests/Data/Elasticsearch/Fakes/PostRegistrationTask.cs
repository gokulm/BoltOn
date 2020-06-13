using System;
using BoltOn.Bootstrapping;
using BoltOn.Tests.Other;
using Elasticsearch.Net;
using Nest;

namespace BoltOn.Tests.Data.Elasticsearch.Fakes
{
    public class PostRegistrationTask : IPostRegistrationTask
    {
        public void Run()
        {
            if (IntegrationTestHelper.IsSeedElasticsearch)
            {
                var uris = new[] { new Uri("http://127.0.0.1:9200") };
                //var connectionPool = new SniffingConnectionPool(uris);
                var settings = new ConnectionSettings(new Uri("http://127.0.0.1:9200"))
                    .DefaultIndex("people");

                var client = new ElasticClient(settings);
                var person1 = new Person
                {
                    Id = Guid.Parse("1a8f88e2-6c40-4e12-9fbc-f39ccab490c1"),
                    FirstName = "Martijn",	
                    LastName = "Laarman"
                };
                var person2 = new Person
                {
                    Id = Guid.Parse("5aa44d39-f89c-446b-90ab-53cdd3c280e1"),
                    FirstName = "John",
                    LastName = "Smith"
                };
				var person3 = new Person
                {
                    Id = Guid.Parse("84d19d30-a395-4983-b0fa-4caff052eacb"),
                    FirstName = "record to be deleted",
                    LastName = "test"
                };
				//var test = client.IndexDocument(person1);
				//_ = client.IndexDocument(person2);
				_ = client.IndexDocument(person3);
			}
        }
    }
}
