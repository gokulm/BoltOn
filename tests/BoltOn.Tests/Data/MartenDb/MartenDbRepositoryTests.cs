using System;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BoltOn.Tests.Data.MartenDb
{
	public class MartenDbRepositoryTests
	{
		[Fact]
		public void Test()
		{
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddMarten("host=localhost;database=bolton;password=bolton;username=bolton");

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var store = DocumentStore
    .For("host=localhost;database=bolton;password=bolton;username=bolton");

            using (var session = store.LightweightSession())
            using (var session2 = store.QuerySession())
            using (var session3 = store.DirtyTrackedSession())
            using (var session4 = store.OpenSession())
            {
                var user = new User { FirstName = "Han", LastName = "Solo" };
                session2.Store(user);

                session.SaveChanges();
            }

            //serviceCollection.AddElasticsearch<TestElasticsearchOptions>(
            //    t => t.ConnectionSettings = new Nest.ConnectionSettings(new Uri("http://127.0.0.1:9200")));
            //ServiceProvider = serviceCollection.BuildServiceProvider();
            //ServiceProvider.TightenBolts();
            //SubjectUnderTest = ServiceProvider.GetService<IRepository<Student>>();
        }
	}

    public class User
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool Internal { get; set; }
        public string UserName { get; set; }
        public string Department { get; set; }
    }
}
