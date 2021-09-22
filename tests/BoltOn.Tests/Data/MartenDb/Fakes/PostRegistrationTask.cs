using System;
using Microsoft.Extensions.DependencyInjection;
using BoltOn.Bootstrapping;
using BoltOn.Tests.Other;
using Marten;

namespace BoltOn.Tests.Data.MartenDb.Fakes
{
	public class PostRegistrationTask : IPostRegistrationTask
	{
		private readonly IServiceProvider _serviceProvider;

		public PostRegistrationTask(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public void Run()
		{
			if (IntegrationTestHelper.IsSeedMartenDb)
			{
				var documentStore = _serviceProvider.GetService<IDocumentStore>();
				documentStore.Advanced.Clean.CompletelyRemoveAll();
				//documentStore.Advanced.Clean.DeleteAllDocuments();

				using var scope = _serviceProvider.CreateScope();
				var documentSession = scope.ServiceProvider.GetService<IDocumentSession>();
				if (documentSession != null)
				{
					documentSession.Insert(new Student
					{
						Id = 1,
						FirstName = "a",
						LastName = "b"
					});
					var student = new Student
					{
						Id = 2,
						FirstName = "x",
						LastName = "y"
					};
					documentSession.Insert(new Student
					{
						Id = 10,
						FirstName = "record to be deleted",
						LastName = "b"
					});
					documentSession.Insert(new Student
					{
						Id = 11,
						FirstName = "record to be deleted",
						LastName = "b"
					});
					//var address = new Address { Id = Guid.NewGuid(), Street = "Computer Science", Student = student };
					documentSession.Insert(student);
					//documentSession.Insert(address);
					documentSession.SaveChanges();
					documentSession.Dispose();
				}
			}
		}
	}
}
