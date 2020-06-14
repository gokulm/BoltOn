using System;
using BoltOn.Data;

namespace BoltOn.Tests.Data.Elasticsearch.Fakes
{
	public class Person : BaseEntity<int>
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
	}
}
