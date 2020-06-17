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
				var settings = new ConnectionSettings(new Uri("http://127.0.0.1:9200"))
					.DefaultIndex("students");

				var client = new ElasticClient(settings);

				_ = client.IndexDocument(new Student
				{
					Id = 1,
					FirstName = "John",
					LastName = "Smith",
				});
				_ = client.IndexDocument(new Student
				{
					Id = 2,
					FirstName = "x",
					LastName = "y"
				});
				_ = client.IndexDocument(new Student
				{
					Id = 10,
					FirstName = "record to be deleted",
					LastName = "b"
				});
				_ = client.IndexDocument(new Student
				{
					Id = 11,
					FirstName = "record to be updated",
					LastName = "b"
				});
			}
		}
	}
}
