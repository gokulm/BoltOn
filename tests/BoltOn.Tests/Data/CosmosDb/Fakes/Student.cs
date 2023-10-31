using System;
using System.Collections.Generic;
using BoltOn.Tests.Other;
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
		public List<Address> Addresses { get; set; } = new List<Address>();
	}
}

