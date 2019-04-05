using BoltOn.Data;

namespace BoltOn.Samples.Domain.Entities
{
	public class Student : BaseEntity<int>
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
	}
}
