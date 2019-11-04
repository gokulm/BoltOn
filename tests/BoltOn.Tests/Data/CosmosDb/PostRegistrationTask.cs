using System;
using BoltOn.Bootstrapping;
using BoltOn.Cqrs;
using BoltOn.Data.CosmosDb;
using BoltOn.Tests.Other;
using Newtonsoft.Json;

namespace BoltOn.Tests.Data.CosmosDb
{
	public class PostRegistrationTask : IPostRegistrationTask
	{
		public void Run(PostRegistrationTaskContext context)
		{
			if (IntegrationTestHelper.IsCosmosDbServer && IntegrationTestHelper.IsSeedCosmosDbData)
			{
				// seeding goes here 
			}
		}
	}

	public class StudentFlattened : BaseCqrsEntity
	{
		[JsonProperty("id")]
		public override Guid Id { get; set; }

		[JsonProperty("firstName")]
		public string FirstName { get; private set; }

		[JsonProperty("lastName")]
		public string LastName { get; private set; }

		[JsonProperty("studentType")]
		public string StudentType { get; private set; }

		[JsonProperty("studentTypeId")]
		public int StudentTypeId { get; private set; }
	}

	public class TestSchoolCosmosDbOptions : BaseCosmosDbOptions
	{
	}
}
