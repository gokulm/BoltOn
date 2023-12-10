using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BoltOn.Tests.Data.CosmosDb.Fakes
{
	public class Student
	{
		[JsonProperty("id")]
		public Guid Id { get; set; }
		public string CourseId { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public int Age { get; set; }
		public List<Address> Addresses { get; set; } = new List<Address>();
	}

	public class Address
	{
		public Guid Id { get; set; }
		public string Street { get; set; }
		public string City { get; set; }
	}
}

