using BoltOn.Data;

namespace BoltOn.Samples.Domain.Entities
{
    public class Grade : BaseEntity<string>
    {
        public string CourseName { get; set; }
        public int Year { get; set; }
        public string Score { get; set; }
    }
}
