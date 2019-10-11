using BoltOn.Data;

namespace BoltOn.Samples.Application.Entities
{
	public class Student : BaseEntity<int>
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
	}
}
