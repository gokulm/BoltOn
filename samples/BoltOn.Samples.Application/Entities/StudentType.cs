using BoltOn.Data;

namespace BoltOn.Samples.Application.Entities
{
    public class StudentType : BaseEntity<int>
    {
        public string Description { get; set; }

        private StudentType()
        {
        }

        public StudentType(int id, string description)
        {
            Id = id;
            Description = description;
        }
    }
}
